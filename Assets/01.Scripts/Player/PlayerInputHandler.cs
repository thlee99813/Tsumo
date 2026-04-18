using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private IngameController _ingameController;

    private void Update()
    {

        if(Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            _player.AddAttack(AttackType.Sword);
        }

        if(Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            _player.AddAttack(AttackType.Shuriken);
        }

        if(Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            _player.AddAttack(AttackType.Spell);
        }
    }
}
