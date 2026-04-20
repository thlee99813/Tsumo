using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class AttackAnimationTiming
{
    public int HitFrameIndex = 2;
    public float HitStopDuration = 0.3f;
    public float EffectDelay = 0.3f;
}

public class PlayerAnimator : MonoBehaviour
{
    [Header("Attack Sprite")]
    [SerializeField] private Sprite[] _swordSprites;
    [SerializeField] private Sprite[] _shurikenSprites;
    [SerializeField] private Sprite[] _spellSprites;
    [SerializeField] private Sprite[] _teleportSprites;

    [Header("Movement Sprites")]
    [SerializeField] private Sprite[] _idleSprites;
    [SerializeField] private Sprite[] _runSpites;
    [SerializeField] private Sprite[] _runStopSprites;

    [Header("Frame Settings")]
    [SerializeField] private float _idleFps = 8f;
    [SerializeField] private float _swordFps = 12f;
    [SerializeField] private float _shurikenFps = 12f;
    [SerializeField] private float _spellFps = 12f;
    [SerializeField] private float _runFps = 12f;
    [SerializeField] private float _runStopFps = 12f;
    [SerializeField] private float _teleportFps = 12f;

    [Header("Attack Timing")]
    [SerializeField] private AttackAnimationTiming _swordTiming = new AttackAnimationTiming();
    [SerializeField] private AttackAnimationTiming _shurikenTiming = new AttackAnimationTiming();
    [SerializeField] private AttackAnimationTiming _spellTiming = new AttackAnimationTiming();

    [Header("Reference")]
    [SerializeField] private BattleImpulseEmitter _impulseEmitter;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentAnim;
    private int _loopFrameIndex;

    public event Action OnAnimationComplete;
    public event Action OnHitFrame;

    public void PlaySword() => PlayAttack(_swordSprites, _swordFps, _swordTiming);
    public void PlayShuriken() => PlayAttack(_shurikenSprites, _shurikenFps, _shurikenTiming);
    public void PlaySpell() => PlayAttack(_spellSprites, _spellFps, _spellTiming);
    public void PlayCustomAttack(AttackType attackType, Sprite[] sprites, float fps) => PlayAttack(sprites, fps, GetTiming(attackType));
    public void HideSprite() => _spriteRenderer.enabled = false;
    public void ShowSprite() => _spriteRenderer.enabled = true;

    public void PlayIdle(float fpsOverride = -1f)
    {
        float fps = fpsOverride > 0f ? fpsOverride : _idleFps;
        PlayLoop(_idleSprites, fps);
    }

    public void PlayRun(float fpsOverride = -1f)
    {
        float fps = fpsOverride > 0f ? fpsOverride : _runFps;
        PlayLoop(_runSpites, fps);
    }

    public void PlayRunStop(Action onComplete = null)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayOnceCoroutine(_runStopSprites, _runStopFps, onComplete));
    }

    public void PlayTeleport(Action onComplete = null)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayOnceCoroutine(_teleportSprites, _teleportFps, onComplete));
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_impulseEmitter == null)
            _impulseEmitter = GetComponent<BattleImpulseEmitter>();
    }

    private void Start()
    {
        PlayRun();
    }

    public void StopAnimation()
    {
        if (_currentAnim != null)
        {
            StopCoroutine(_currentAnim);
            _currentAnim = null;
        }
    }

    public float GetEffectDelay(AttackType attackType)
    {
        return GetTiming(attackType).EffectDelay;
    }

    private AttackAnimationTiming GetTiming(AttackType attackType)
    {
        return attackType switch
        {
            AttackType.Sword => _swordTiming,
            AttackType.Shuriken => _shurikenTiming,
            AttackType.Spell => _spellTiming,
            _ => _swordTiming
        };
    }

    private void PlayAttack(Sprite[] sprites, float fps, AttackAnimationTiming timing)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayOnceCoroutine(sprites, fps, () =>
        {
            OnAnimationComplete?.Invoke();
        }, timing));
    }

    private void PlayLoop(Sprite[] sprites, float fps)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayLoopCoroutine(sprites, fps));
    }

    private IEnumerator PlayOnceCoroutine(Sprite[] sprites, float fps, Action onComplete, AttackAnimationTiming timing = null)
    {
        if (sprites == null || sprites.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float interval = 1f / fps;
        int hitFrameIndex = -1;

        if (timing != null)
            hitFrameIndex = Mathf.Clamp(timing.HitFrameIndex, 0, sprites.Length - 1);

        for (int i = 0; i < sprites.Length; i++)
        {
            _spriteRenderer.sprite = sprites[i];

            bool isHitFrame = timing != null && i == hitFrameIndex;
            if (isHitFrame)
            {
                _impulseEmitter?.EmitHitImpulse();
                OnHitFrame?.Invoke();
            }

            if (i < sprites.Length - 1)
            {
                if (isHitFrame)
                    yield return new WaitForSeconds(timing.HitStopDuration);
                else
                    yield return new WaitForSeconds(interval);
            }
            else if (isHitFrame)
            {
                yield return new WaitForSeconds(timing.HitStopDuration);
            }
        }

        _currentAnim = null;
        onComplete?.Invoke();
    }

    private IEnumerator PlayLoopCoroutine(Sprite[] sprites, float fps)
    {
        if (sprites == null || sprites.Length == 0)
            yield break;

        float interval = 1f / fps;
        int i = _loopFrameIndex % sprites.Length;

        while (true)
        {
            _spriteRenderer.sprite = sprites[i];
            int next = (i + 1) % sprites.Length;
            _loopFrameIndex = next;
            yield return new WaitForSeconds(interval);
            i = next;
        }
    }
}