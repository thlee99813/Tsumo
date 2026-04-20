using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerComboTestInput : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private IngameController _ingameController;

    private void Start()
    {
        if (_player == null) Debug.LogError("[TestInput] _player 미할당");
        if (_ingameController == null) Debug.LogError("[TestInput] _ingameController 미할당");
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            Debug.LogError("[TestInput] Keyboard.current가 null - Input System 미설정");
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("[TestInput] 1 입력");
            _player.AddAttack(AttackType.Sword);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("[TestInput] 2 입력");
            _player.AddAttack(AttackType.Shuriken);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            Debug.Log("[TestInput] 3 입력");
            _player.AddAttack(AttackType.Spell);
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[TestInput] Space 입력");
            _ingameController.HandleFireInput();
        }
    }
}
