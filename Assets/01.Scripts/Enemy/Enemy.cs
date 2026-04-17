using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyStatsSO _stats;

    private int _currentHp;
    public bool IsDead => _currentHp <= 0;
    public int CounterDamage => _stats.CounterDamage;

    public event Action OnEnemyDead;

    private void Awake()
    {
        _currentHp = _stats.MaxHp;
    }
    public void TakeDamage(int damage)
    {
        if(IsDead) return;
        _currentHp = Mathf.Max(0, _currentHp - damage);

        if(IsDead)
            OnEnemyDead?.Invoke();
    }

    public void ResetEnemy()
    {
        _currentHp = _stats.MaxHp;
    }
}
