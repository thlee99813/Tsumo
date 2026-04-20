using System;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Enemy _enemy;

    public event Action OnBattleComplete;
    public event Action OnEnemyDead;

    private void Start()
    {
        _enemy.OnEnemyDead += HandleEnemyDead;
        _player.OnHitFrame += HandlePlayerHitFrame;
    }

    private void OnDestroy()
    {
        _enemy.OnEnemyDead -= HandleEnemyDead;
        _player.OnHitFrame -= HandlePlayerHitFrame;
    }

    private void HandlePlayerHitFrame()
    {
        if (_enemy != null) _enemy.PlayHitEffect();
    }

    public void ExecuteBattle(int finalDamage)
    {
        if(_enemy == null || _enemy.IsDead) return;

        _enemy.TakeDamage(finalDamage);

        //적이 살아있으면 반격
        if(!_enemy.IsDead)
        {
            _player.TakeDamage(_enemy.CounterDamage);
        }

        _enemy.OnDamageProcessed();
        OnBattleComplete?.Invoke();
    }

    private void HandleEnemyDead()
    {
        OnEnemyDead?.Invoke();
    }

    
    public void SetEnemy(Enemy enemy)
    {
        
        _enemy.OnEnemyDead -= HandleEnemyDead;
        _enemy = enemy;
        _enemy.OnEnemyDead += HandleEnemyDead;
    }

}
