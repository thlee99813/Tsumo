using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerHpBar : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Player _player;
    [SerializeField] private PlayerState _playerState;
    [SerializeField] private Slider _slider;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Follow")]
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, -1.2f, 0f);
    [SerializeField] private bool _hideWhenOffScreen = true;

    [Header("Tween - Value")]
    [SerializeField] private float _decreaseDuration = 0.2f;
    [SerializeField] private float _increaseDuration = 0.12f;

    [Header("Tween - Punch")]
    [SerializeField] private Vector3 _decreasePunch = new Vector3(0.12f, 0.12f, 0f);
    [SerializeField] private float _decreasePunchDuration = 0.2f;
    [SerializeField] private int _decreasePunchVibrato = 8;
    [SerializeField] private float _decreasePunchElasticity = 0.9f;

    private RectTransform _rectTransform;
    private Tween _valueTween;
    private Tween _punchTween;
    private int _lastHp;
    private bool _isBound;

    private void Reset()
    {
        _rectTransform = transform as RectTransform;
        _slider = GetComponent<Slider>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
        BindPlayer();
        RefreshBarImmediate();
        UpdateFollow();
    }

    private void OnDisable()
    {
        UnbindPlayer();
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void LateUpdate()
    {
        UpdateFollow();
    }

    // 필요한 참조를 캐시한다.
    private void CacheReferences()
    {
        if (_rectTransform == null)
            _rectTransform = transform as RectTransform;

        if (_slider == null)
        {
            _slider = GetComponent<Slider>();
        }

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_playerState == null && _player != null)
            _playerState = _player.GetComponent<PlayerState>();
    }

    // 플레이어 HP 변경 이벤트를 구독한다.
    private void BindPlayer()
    {
        if (_player == null || _isBound)
            return;

        _player.OnHpChanged += HandleHpChanged;
        _isBound = true;
    }

    // 플레이어 HP 변경 이벤트 구독을 해제한다.
    private void UnbindPlayer()
    {
        if (_player == null || !_isBound)
            return;

        _player.OnHpChanged -= HandleHpChanged;
        _isBound = false;
    }

    // 현재 상태를 기준으로 HP 바를 즉시 동기화한다.
    private void RefreshBarImmediate()
    {
        if (_player == null || _playerState == null || _slider == null)
            return;

        KillTweens();

        _slider.minValue = 0f;
        _slider.maxValue = _playerState.MaxHp;
        _slider.value = _playerState.MaxHp;
        _lastHp = _playerState.MaxHp;

        transform.localScale = Vector3.one;
    }

    // 플레이어 체력이 변했을 때 슬라이더 값을 갱신한다.
    private void HandleHpChanged(int currentHp)
    {
        if (_slider == null || _playerState == null)
            return;

        if (!Mathf.Approximately(_slider.maxValue, _playerState.MaxHp))
            _slider.maxValue = _playerState.MaxHp;

        bool isDecrease = currentHp < _lastHp;
        _lastHp = currentHp;

        _valueTween?.Kill();

        if (isDecrease)
        {
            _valueTween = _slider
                .DOValue(currentHp, _decreaseDuration)
                .SetEase(Ease.OutCubic);

            PlayDecreasePunch();
        }
        else
        {
            _valueTween = _slider
                .DOValue(currentHp, _increaseDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    // 플레이어 월드 좌표를 화면 좌표로 변환해 HP 바 위치를 갱신한다.
    private void UpdateFollow()
    {
        if (_player == null || _rectTransform == null || Camera.main == null)
            return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_player.transform.position + _worldOffset);

        bool isVisible =
            screenPosition.z > 0f &&
            screenPosition.x >= 0f &&
            screenPosition.x <= Screen.width &&
            screenPosition.y >= 0f &&
            screenPosition.y <= Screen.height;

        if (_hideWhenOffScreen && _canvasGroup != null)
            _canvasGroup.alpha = isVisible ? 1f : 0f;

        if (!isVisible)
            return;

        _rectTransform.position = screenPosition;
    }

    // 체력이 줄어들 때 HP 바에 펀치 모션을 준다.
    private void PlayDecreasePunch()
    {
        _punchTween?.Kill();

        transform.localScale = Vector3.one;

        _punchTween = transform.DOPunchScale(
            _decreasePunch,
            _decreasePunchDuration,
            _decreasePunchVibrato,
            _decreasePunchElasticity);
    }

    // 진행 중인 트윈을 정리한다.
    private void KillTweens()
    {
        _valueTween?.Kill();
        _valueTween = null;

        _punchTween?.Kill();
        _punchTween = null;
    }
}