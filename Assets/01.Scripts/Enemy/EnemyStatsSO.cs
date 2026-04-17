using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatsSO", menuName = "Data/EnemyStats")]
public class EnemyStatsSO : ScriptableObject
{
    [SerializeField] private int _maxHp = 50;
    [SerializeField] private int _countDamage = 10;
    
    public int MaxHp => _maxHp;
    public int CounterDamage => _countDamage;
}
