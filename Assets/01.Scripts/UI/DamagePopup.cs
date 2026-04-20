using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _riseDuration = 0.8f;
    [SerializeField] private float _riseHeight = 1.5f;
    [SerializeField] private Color _baseScroeColor = Color.white;
    [SerializeField] private Color _finalScoreColor = Color.yellow;

    public void ShowBaseScore(int score, Vector3 worldPos)
    {
        Show(score.ToString(), worldPos, _baseScroeColor, 1f);
    } 

    public void ShowFinalScore(int score, Vector3 worldPos)
    {
        Show($"Total : {score}", worldPos, _finalScoreColor, 1.5f);
    }

    private void Show(string message, Vector3 worldPos, Color color, float scale)
    {
        Debug.Log($"[DamagePopup] Show 호출 : {message} at {worldPos}");
        transform.position = worldPos;
        transform.localScale = Vector3.one * scale;
        _text.text = message;
        _text.color = color;

        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(worldPos.y + _riseHeight, _riseDuration))
            .SetEase(Ease.OutQuad);
        seq.Join(_text.DOFade(0f, _riseDuration));
        seq.OnComplete(() => gameObject.SetActive(false));
    }
}
