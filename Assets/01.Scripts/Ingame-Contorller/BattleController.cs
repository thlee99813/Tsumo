using System;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Enemy _enemy;

    public event Action OnBattleComplete;

    private void Start()
    {
        _enemy.OnEnemyDead += HandleEnemyDead;
    }

    private void OnDestroy()
    {
        _enemy.OnEnemyDead -= HandleEnemyDead;
    }

    //InGameController에서 호출 + COmboResolver 완성 후 파라미터로 최종 데미지 수신
    public void ExecuteBattle(int finalDamage)
    {
        if(_enemy == null || _enemy.IsDead) return;

        _enemy.TakeDamage(finalDamage);

        //적이 살아있으면 반격
        if(!_enemy.IsDead)
        {
            _player.TakeDamage(_enemy.CounterDamage);
        }

        OnBattleComplete?.Invoke();
    }

    private void HandleEnemyDead()
    {
        //TODO IngameController 혹은 StakeFlowController에 적 사망 통보 
    }
    
    public void SetEnemy(Enemy enemy)
    {
        if(_enemy != null)
            _enemy.OnEnemyDead -= HandleEnemyDead;
        
        _enemy = enemy;
        _enemy.OnEnemyDead += HandleEnemyDead;
    }
}
