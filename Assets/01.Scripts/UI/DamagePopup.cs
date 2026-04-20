using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _riseDuration = 0.8f;
    [SerializeField] private float _riseHeight = 1.5f;
    [SerializeField] private float _tallyStepDuration = 0.35f;
    [SerializeField] private float _tallyFadeDuration = 0.4f;
    [SerializeField] private Color _baseScroeColor = Color.white;
    [SerializeField] private Color _finalScoreColor = Color.yellow;
    [SerializeField] private Color _bonusColor = Color.cyan;

    public void ShowBaseScore(int score, Vector3 worldPos, Action onComplete = null)
    {
        Show(score.ToString(), worldPos, _baseScroeColor, 1f, onComplete);
    }

    // +25% / 족보명 표시 후 사라지고 onComplete 호출
    public void ShowYakuBonus(string yakuName, float bonusMultiplier, Vector3 worldPos, Action onComplete)
    {
        _text.DOKill();
        transform.DOKill();

        transform.position = worldPos;
        transform.localScale = Vector3.one * 1.3f;

        int percent = Mathf.RoundToInt(bonusMultiplier * 100f);
        Color c = _bonusColor;
        c.a = 1f;
        _text.color = c;
        _text.text = $"+{percent}%";
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(worldPos.y + _riseHeight * 0.5f, _tallyFadeDuration * 1.5f)
            .SetEase(Ease.OutQuad));
        seq.AppendInterval(0.3f);
        seq.Append(_text.DOFade(0f, _tallyFadeDuration));
        seq.OnComplete(() =>
        {
            transform.DOKill();
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // 0부터 최종 결과값까지 카운트업
    public void ShowFinalScore(int score, Vector3 worldPos, Action onComplete = null)
    {
        _text.DOKill();
        transform.DOKill();

        transform.position = worldPos;
        transform.localScale = Vector3.one * 1.5f;

        Color c = _finalScoreColor;
        c.a = 1f;
        _text.color = c;
        _text.text = "Total : 0";
        gameObject.SetActive(true);

        float current = 0f;
        float baseScale = 0.5f;
        float maxScale = 1.5f;
        int milestone = 5000;
        int lastMilestone = 0;
        transform.localScale = Vector3.one * baseScale;

        Sequence seq = DOTween.Sequence();

        // 카운트업 + 크기 증가
        seq.Append(DOTween.To(() => current, x =>
        {
            current = x;
            _text.text = $"Total : {(int)current}";

            int currentMilestone = ((int)current / milestone) * milestone;
            if (currentMilestone > lastMilestone && currentMilestone > 0)
            {
                lastMilestone = currentMilestone;

                int step = currentMilestone / milestone;
                float targetScale = Mathf.Min(baseScale + step * 0.3f, maxScale);
                transform.localScale = Vector3.one * targetScale;

                float punchStrength = 0.2f + step * 0.1f;
                transform.DOPunchScale(
                    Vector3.one * punchStrength,
                    0.3f,
                    6,
                    0.5f
                ).SetUpdate(true);
            }

        }, score, 0.8f + score / 20000f)
            .SetEase(Ease.OutCubic));

        seq.AppendCallback(() =>
        {
            transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.5f);
        });

        seq.AppendInterval(0.8f);
        seq.Append(transform.DOMoveY(worldPos.y + _riseHeight, _riseDuration).SetEase(Ease.OutQuad));
        seq.Join(_text.DOFade(0f, _riseDuration));
        seq.OnComplete(() =>
        {
            transform.DOKill();
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void Show(string message, Vector3 worldPos, Color color, float scale, Action onComplete = null)
    {
        _text.DOKill();
        transform.DOKill();

        transform.position = worldPos;
        transform.localScale = Vector3.one * scale;

        Color c = color;
        c.a = 1f;
        _text.color = c;
        _text.text = message;
        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(worldPos.y + _riseHeight, _riseDuration).SetEase(Ease.OutQuad));
        seq.Join(_text.DOFade(0f, _riseDuration));
        seq.OnComplete(() =>
        {
            transform.DOKill();
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}