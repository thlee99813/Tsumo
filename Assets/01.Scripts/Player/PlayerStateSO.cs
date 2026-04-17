using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateSO", menuName = "Data/PlayerStats")]
public class PlayerStateSO : ScriptableObject
{
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _penaltyDamage = 20;

    public int MaxHp => _maxHp;
    public int PenaltyDamage => _penaltyDamage;

}
