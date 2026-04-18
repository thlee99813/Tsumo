using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private int _maxHp = 50;
    [SerializeField] private int _countDamage = 10;

    public int MaxHp => _maxHp;
    public int CounterDamage => _countDamage;
}
