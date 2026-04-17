using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStateSO _stats;
    [SerializeField] private float _knockBackHeight = 1.5f;
    [SerializeField] private float _knockBackDuration = 0.4f;

    private int _currentHp;
    private Vector3 _startPosition;
    private bool _isKnockBack;

    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;
    public bool IsKnockBack => _isKnockBack;

    public event Action OnPlayerDead;
    public event Action<int> OnHpChanged;

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
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

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(_startPosition.y + _knockBackHeight, _knockBackDuration * 0.4f))
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
        
        seq.Append(transform.DOMoveY(_startPosition.y, _knockBackDuration * 0.6f)
            .SetEase(Ease.InQuad)
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
