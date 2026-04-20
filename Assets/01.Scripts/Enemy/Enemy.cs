using System;
using DG.Tweening;
using UnityEngine;
using System.Collections;
[RequireComponent(typeof(EnemyStats))]
public class Enemy : MonoBehaviour
    {
        private EnemyStats _stats;

    [SerializeField] private SpriteRenderer _enemySpriteRenderer;
    [SerializeField] private Transform _headPoint;
    [SerializeField] private float _moveDistance = 2f;

    [SerializeField] private float _moveDelay = 3f;
    [SerializeField] private float _moveDuration = 0.5f;

    [SerializeField] private int _currentHp;
    [SerializeField] private int _runtimeMaxHp;
    [SerializeField] private int _runtimeCounterDamage;
    private Vector3 _startPosition;

    public bool IsDead => _currentHp <= 0;
    public int CurrentHp => _currentHp;
    public int CounterDamage => _runtimeCounterDamage;
    public Transform HeadPoint => _headPoint != null ? _headPoint : transform;



    public event Action OnEnemyDead;
    public event Action OnEnemyReady;   //왼쪽 이동 완료 시 발행

    private void Awake()
    {
        _stats = GetComponent<EnemyStats>();
        _startPosition = transform.position;
        _runtimeMaxHp = _stats.MaxHp;
        _runtimeCounterDamage = _stats.CounterDamage;
        _currentHp = _runtimeMaxHp;
    }



    private void Start()
    {
        StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
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

    private IEnumerator ReturnAndLoop()
    {
        yield return MoveToOffset(0f);

        StartCoroutine(MoveLoop());
    }
    private IEnumerator MoveToOffset(float offsetX)
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
    public void ApplyBattleStats(int maxHp, int counterDamage, bool resetHp)
    {
        _runtimeMaxHp = Mathf.Max(1, maxHp);
        _runtimeCounterDamage = Mathf.Max(0, counterDamage);

        if (resetHp)
        {
            _currentHp = _runtimeMaxHp;
        }
    }

    public void ApplyStageStats(int stageIndex, bool resetHp)
    {
        _stats.GetStatsByStage(stageIndex, out int maxHp, out int counterDamage, out Sprite sprite);
        ApplyBattleStats(maxHp, counterDamage, resetHp);
        if (sprite != null) _enemySpriteRenderer.sprite = sprite;
    }


    public void ResetEnemy()
    {
        gameObject.SetActive(true);
        transform.DOKill();
        StopAllCoroutines();
        transform.position = _startPosition;
        _currentHp = _runtimeMaxHp;
        StartCoroutine(MoveLoop());
    }

}
