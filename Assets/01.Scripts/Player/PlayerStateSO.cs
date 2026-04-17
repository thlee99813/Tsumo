using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateSO", menuName = "Data/PlayerStats")]
public class PlayerStateSO : ScriptableObject
{
    [SerializeField] private int _maxHp = 100;
    
    public int MaxHp => _maxHp;

}
