using System;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyStats _stats;
    [SerializeField] private float _moveDistance = 2f;
    [SerializeField] private float _moveDelay = 3f;
    [SerializeField] private float _moveDuration = 0.5f;

    [SerializeField] private int _currentHp;
    private Vector3 _startPosition;

    public bool IsDead => _currentHp <= 0;
    public int CurrentHp => _currentHp;
    public int CounterDamage => _stats.CounterDamage;


    public event Action OnEnemyDead;
    public event Action OnEnemyReady;   //왼쪽 이동 완료 시 발행

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
    }

    private void Start()
    {
        StartCoroutine(MoveLoop());
    }

    private System.Collections.IEnumerator MoveLoop()
    {
        while(!IsDead)
        {
            //3초 대기 후 왼쪽으로 이동
            yield return new WaitForSeconds(_moveDelay);
            if(IsDead) yield break;

            yield return MoveToOffset(-_moveDistance);

            OnEnemyReady?.Invoke();
        }
    }

    public void OnDamageProcessed()
    {
        if(IsDead) return;
        StopAllCoroutines();
        StartCoroutine(ReturnAndLoop());
    }

    private System.Collections.IEnumerator ReturnAndLoop()
    {
        yield return MoveToOffset(0f);

        StartCoroutine(MoveLoop());
    }

    private System.Collections.IEnumerator MoveToOffset(float offsetX)
    {
        float targetX = _startPosition.x + offsetX;
        transform.DOMoveX(targetX, _moveDuration).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(_moveDuration);
    }


    public void TakeDamage(int damage)
    {
        if(IsDead) return;
        _currentHp = Mathf.Max(0, _currentHp - damage);

        if (IsDead)
        {
            gameObject.SetActive(false);
            OnEnemyDead?.Invoke();
        }

    }

    // 슬로우모션 진입 시 적 이동 중단 (Time.timeScale 비의존)
    public void PauseMovement()
    {
        StopAllCoroutines();
        transform.DOKill();
    }

    public void ResetEnemy()
    {
        gameObject.SetActive(true);
        transform.DOKill();
        StopAllCoroutines();
        transform.position = _startPosition;
        _currentHp = _stats.MaxHp;
        StartCoroutine(MoveLoop());
    }

}
