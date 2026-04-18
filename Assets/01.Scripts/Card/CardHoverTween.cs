using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class CardHoverTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField, Min(1f)] private float _hoverScaleMultiplier = 1.12f;
    [SerializeField, Min(0f)] private float _duration = 0.12f;
    [SerializeField] private Ease _ease = Ease.OutQuad;
    [SerializeField] private bool _useUnscaledTime = true;

    private Vector3 _baseScale;
    private Tween _scaleTween;

    private void Awake()
    {
        if (_targetTransform == null)
        {
            _targetTransform = transform;
        }

        _baseScale = _targetTransform.localScale;
    }

    private void OnEnable()
    {
        if (_targetTransform != null)
        {
            _targetTransform.localScale = _baseScale;
        }
    }

    private void OnDisable()
    {
        KillTween();

        if (_targetTransform != null)
        {
            _targetTransform.localScale = _baseScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayScale(_baseScale * _hoverScaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScale(_baseScale);
    }

    public void ForceNormal()
    {
        KillTween();
        if (_targetTransform != null)
        {
            _targetTransform.localScale = _baseScale;
        }
    }

    private void PlayScale(Vector3 targetScale)
    {
        if (_targetTransform == null)
        {
            return;
        }

        KillTween();

        _scaleTween = _targetTransform
            .DOScale(targetScale, _duration)
            .SetEase(_ease)
            .SetUpdate(_useUnscaledTime);
    }

    private void KillTween()
    {
        if (_scaleTween != null && _scaleTween.IsActive())
        {
            _scaleTween.Kill();
        }

        _scaleTween = null;
    }
}
