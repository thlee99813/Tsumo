using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageIntroDialogueController : MonoBehaviour
{
    [SerializeField] private GameObject _dialogueRoot;
    [SerializeField] private GameObject _scarecrowObject;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private float _typingInterval = 0.04f;

    public IEnumerator Play(params string[] lines)
    {
        _dialogueRoot.SetActive(true);
        _scarecrowObject.SetActive(true);

        for (int i = 0; i < lines.Length; i++)
        {
            yield return ShowSingleLine(lines[i]);
        }

        _scarecrowObject.SetActive(false);
        _dialogueRoot.SetActive(false);
    }

    private IEnumerator ShowSingleLine(string line)
    {
        _dialogueText.text = string.Empty;
        int index = 0;
        bool isTyping = true;

        yield return WaitForLeftClickRelease();

        while (isTyping)
        {
            if (IsLeftClickPressed())
            {
                _dialogueText.text = line;
                isTyping = false;
                break;
            }

            if (index >= line.Length)
            {
                isTyping = false;
                break;
            }

            _dialogueText.text += line[index];
            index++;

            if (_typingInterval > 0f)
            {
                yield return new WaitForSecondsRealtime(_typingInterval);
            }
            else
            {
                yield return null;
            }
        }

        yield return WaitForLeftClickRelease();
        yield return new WaitUntil(IsLeftClickPressed);
    }
    private IEnumerator WaitForLeftClickRelease()
    {
        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            yield return null;
        }
    }



    private bool IsLeftClickPressed()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }
}
