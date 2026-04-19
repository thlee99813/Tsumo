using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerComboTestInput : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Transform _targetEnemy;

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            _player.AddAttack(AttackType.Sword);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            _player.AddAttack(AttackType.Shuriken);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            _player.AddAttack(AttackType.Spell);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _player.StopMovement();
            _player.ExecuteCombo(_targetEnemy.position.x);
        }
    }
}
