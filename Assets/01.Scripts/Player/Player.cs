using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStateSO _stats;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _knockBackHeight = 1.5f;
    [SerializeField] private float _knockBackDuration = 0.4f;

    private int _currentHp;
    private Vector3 _startPosition;
    private bool _isKnockBack;
    private bool _isReached;        //접근 중인가

    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;
    public bool IsKnockBack => _isKnockBack;
    public bool IsReached => _isReached;

    public event Action OnPlayerDead;
    public event Action<int> OnHpChanged;

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
    }

    //IdlePhase 에서 호출 -> 적 위치까지 이동
    public void MoveToEnemy(float targetX)
    {
        _isReached = false;
        transform.DOKill();     // 복귀 트윈 등 기존 트윈 제거
        float distance = Mathf.Abs(transform.position.x - targetX);
        float duration = distance / _moveSpeed;

        transform.DOMoveX(targetX, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => _isReached = true);
    }

    public void ReturnToStart()
    {
        _isReached = false;
        transform.DOMove(_startPosition, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    public void TakeDamage(int damage)
    {
        if(_isKnockBack) return;
        ApplyDamage(damage);
    }

    public void TakePenaltyDamage()
    {
        if(_isKnockBack) return;
        ApplyDamage(_stats.PenaltyDamage);
    }

    private void ApplyDamage(int damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage);
        OnHpChanged?.Invoke(_currentHp);

        PlayerKnockBack();      //때린뒤, 혹은 맞은 뒤에 원래 자리로 돌아가기
    }

    private void PlayerKnockBack()
    {
        if(_isKnockBack) return;
        _isKnockBack = true;

        transform.DOKill();   

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        // 1단계: Y축 위로 튕김
        seq.Append(transform.DOMoveY(_startPosition.y + _knockBackHeight, _knockBackDuration * 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true));
        // 2단계: Y축 아래로 낙하
        seq.Append(transform.DOMoveY(_startPosition.y, _knockBackDuration * 0.6f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true));
        // 3단계: X축 원래 자리로 복귀
        seq.Append(transform.DOMoveX(_startPosition.x, 0.5f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true));
        seq.OnComplete(() =>
        {
            transform.position = _startPosition;
            _isKnockBack = false;

            if(IsDead)
                OnPlayerDead?.Invoke();
        });
    }

    public void ResetHp()
    {
        _currentHp = _stats.MaxHp;
        OnHpChanged?.Invoke(_currentHp);
    }

    
}
