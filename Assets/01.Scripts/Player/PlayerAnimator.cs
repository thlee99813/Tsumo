using System;
using UnityEngine;
using System.Collections;
using UnityEngine.LowLevel;


public class PlayerAnimator : MonoBehaviour
{
    [Header("Attack Sprite")]
    [SerializeField] private Sprite[] _swordSprites;
    [SerializeField] private Sprite[] _shurikenSprites;
    [SerializeField] private Sprite[] _spellSprites;
    [SerializeField] private Sprite[] _teleportSprites;

    [Header("Movement Sprites")]
    [SerializeField] private Sprite[] _runSpites;
    [SerializeField] private Sprite[] _runStopSprites;

    [Header("Frame Settings")]
    [SerializeField] private float _swordFps = 12f;
    [SerializeField] private float _shurikenFps = 12f;
    [SerializeField] private float _spellFps = 12f;
    [SerializeField] private float _runFps = 12f;
    [SerializeField] private float _runStopFps = 12f;
    [SerializeField] private float _teleportFps = 12f;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentAnim;
    private int _runFrameIndex = 0;

    public event Action OnAnimationComplete;        // 공격 애니메이션 완료 시 발행


    //외부 호출용 - 공격
    public void PlaySword() => PlayAttack(_swordSprites, _swordFps);
    public void PlayShuriken() => PlayAttack(_shurikenSprites, _shurikenFps);
    public void PlaySpell() => PlayAttack(_spellSprites, _spellFps);
    public void HideSprite() => _spriteRenderer.enabled = false;
    public void ShowSprite() => _spriteRenderer.enabled = true;

    //외부 호출용 - 이동
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
    }

    private void Start()
    {
        PlayRun();
    }


    public void StopAnimation()
    {
        if(_currentAnim != null)
        {
            StopCoroutine(_currentAnim);
            _currentAnim = null;
        }
    }
    // 공격 1회 재생 후 OnAnimationComplete 발행
    private void PlayAttack(Sprite[] sprites, float fps)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayOnceCoroutine(sprites, fps, () =>
        {
            OnAnimationComplete?.Invoke();
        }));
    }

    // 1회 재생
    private void PlayOnce(Sprite[] sprites, float fps)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayOnceCoroutine(sprites, fps, null));
    }

    private void PlayLoop(Sprite[] sprites, float fps)
    {
        StopAnimation();
        _currentAnim = StartCoroutine(PlayLoopCoroutine(sprites, fps));
    }

    private IEnumerator PlayOnceCoroutine(Sprite[] sprites, float fps, Action onComplete)
    {
        if(sprites == null || sprites.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float interval = 1f / fps;
        for(int i = 0; i < sprites.Length; i++)
        {
            _spriteRenderer.sprite = sprites[i];
            if(i < sprites.Length - 1)
                yield return new WaitForSeconds(interval);
        }
        //yield return null;  // 마지막 프레임을 1 렌더 프레임만 유지 후 전환
        _currentAnim = null;   // onComplete가 PlayRun 등을 호출해 _currentAnim을 덮어쓰기 전에 먼저 null로 비워야 함
        onComplete?.Invoke();
    }

    private IEnumerator PlayLoopCoroutine(Sprite[] sprites, float fps)
    {
        if(sprites == null || sprites.Length == 0) yield break;

        float interval = 1f / fps;
        int i = _runFrameIndex % sprites.Length;
        while(true)
        {
            _spriteRenderer.sprite = sprites[i];
            int next = (i + 1) % sprites.Length;
            _runFrameIndex = next;
            yield return new WaitForSeconds(interval);
            i = next;
        }
    }

}
