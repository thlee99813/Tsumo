using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] private int _maxHp = 100;

    public int MaxHp => _maxHp;
}
