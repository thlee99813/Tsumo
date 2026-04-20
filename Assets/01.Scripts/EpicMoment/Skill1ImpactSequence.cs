using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Skill1ImpactSequence : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image _bgFillImage;
    [SerializeField] private RectTransform[] _textRects;

    [Header("Timing")]
    [SerializeField] private float _bgFillTime = 0.18f;
    [SerializeField] private float _textStartDelay = 0.04f;
    [SerializeField] private float _textStagger = 0.05f;
    [SerializeField] private float _textHitTime = 0.08f;
    [SerializeField] private float _textSettleTime = 0.06f;
    [SerializeField] private float _textFadeInTime = 0.05f;

    [Header("Motion")]
    [SerializeField] private float _textSpawnYOffset = 80f;
    [SerializeField] private float _textHitOvershootY = 18f;
    [SerializeField] private float _textStartScale = 0.88f;
    [SerializeField] private float _textPunchAmount = 0.18f;
    [SerializeField] private float _textPunchTime = 0.12f;

    [Header("Option")]
    [SerializeField] private bool _autoPlayOnEnable = false;
    [SerializeField] private bool _useUnscaledTime = true;

    private Vector2[] _textBasePositions;
    private Vector3[] _textBaseScales;
    private Graphic[] _textGraphics;
    private Sequence _playSequence;

    private void Awake()
    {
        Cache();
        ResetToStartState();
    }

    private void OnEnable()
    {
        if (_autoPlayOnEnable)
        {
            Play();
        }
    }

    public void Play()
    {
        KillSequence();
        Cache();
        ResetToStartState();

        _playSequence = DOTween.Sequence().SetUpdate(_useUnscaledTime);

        if (_bgFillImage != null)
        {
            _playSequence.Insert(
                0f,
                DOTween.To(
                    () => _bgFillImage.fillAmount,
                    value => _bgFillImage.fillAmount = value,
                    1f,
                    _bgFillTime
                ).SetEase(Ease.OutCubic)
            );
        }

        for (int i = 0; i < _textRects.Length; i++)
        {
            RectTransform textRect = _textRects[i];
            if (textRect == null)
            {
                continue;
            }

            float startAt = _textStartDelay + (_textStagger * i);
            float targetY = _textBasePositions[i].y;

            Sequence hitSequence = DOTween.Sequence().SetUpdate(_useUnscaledTime);

            hitSequence.Append(
                textRect.DOAnchorPosY(targetY - _textHitOvershootY, _textHitTime).SetEase(Ease.InQuad)
            );
            hitSequence.Join(
                textRect.DOScale(_textBaseScales[i], _textHitTime).SetEase(Ease.OutQuad)
            );

            if (_textGraphics[i] != null)
            {
                hitSequence.Join(_textGraphics[i].DOFade(1f, _textFadeInTime));
            }

            hitSequence.Append(
                textRect.DOAnchorPosY(targetY, _textSettleTime).SetEase(Ease.OutBack)
            );
            hitSequence.Join(
                textRect.DOPunchScale(Vector3.one * _textPunchAmount, _textPunchTime, 10, 0.6f)
            );

            _playSequence.Insert(startAt, hitSequence);
        }
    }

    public void HideImmediate()
    {
        KillSequence();
        ResetToStartState();
    }

    private void Cache()
    {
        if (_textRects == null)
        {
            _textRects = Array.Empty<RectTransform>();
        }

        if (_textBasePositions != null && _textBasePositions.Length == _textRects.Length)
        {
            return;
        }

        _textBasePositions = new Vector2[_textRects.Length];
        _textBaseScales = new Vector3[_textRects.Length];
        _textGraphics = new Graphic[_textRects.Length];

        for (int i = 0; i < _textRects.Length; i++)
        {
            RectTransform textRect = _textRects[i];
            if (textRect == null)
            {
                continue;
            }

            _textBasePositions[i] = textRect.anchoredPosition;
            _textBaseScales[i] = textRect.localScale;
            _textGraphics[i] = textRect.GetComponent<Graphic>();
        }
    }

    private void ResetToStartState()
    {
        if (_bgFillImage != null)
        {
            _bgFillImage.fillAmount = 0f;
        }

        for (int i = 0; i < _textRects.Length; i++)
        {
            RectTransform textRect = _textRects[i];
            if (textRect == null)
            {
                continue;
            }

            Vector2 pos = _textBasePositions[i];
            pos.y += _textSpawnYOffset;
            textRect.anchoredPosition = pos;
            textRect.localScale = _textBaseScales[i] * _textStartScale;

            if (_textGraphics[i] != null)
            {
                Color color = _textGraphics[i].color;
                color.a = 0f;
                _textGraphics[i].color = color;
            }
        }
    }

    private void KillSequence()
    {
        if (_playSequence != null && _playSequence.IsActive())
        {
            _playSequence.Kill();
        }
    }

    private void OnDestroy()
    {
        KillSequence();
    }
}
