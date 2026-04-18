using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text _rerollCountText;

    public void SetRerollCount(int remainingCount, int maxCount)
    {
        if (_rerollCountText == null)
        {
            return;
        }

        int clampedRemaining = Mathf.Clamp(remainingCount, 0, maxCount);
        _rerollCountText.text = $"{clampedRemaining} / {maxCount}";
    }
}
