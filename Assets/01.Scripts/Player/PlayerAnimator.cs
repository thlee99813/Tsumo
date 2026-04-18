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

    [Header("Movement Sprites")]
    [SerializeField] private Sprite[] _runSpites;
    [SerializeField] private Sprite[] _runStopSprites;

    [Header("Frame Settings")]
    [SerializeField] private float _swordFps = 12f;
    [SerializeField] private float _shurikenFps = 12f;
    [SerializeField] private float _spellFps = 12f;
    [SerializeField] private float _runFps = 12f;
    [SerializeField] private float _runStopFps = 12f;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentAnim;

    public event Action OnAnimationComplete;        // 공격 애니메이션 완료 시 발행


    //외부 호출용 - 공격
    public void PlaySword() => PlayAttack(_swordSprites, _swordFps);
    public void PlayShuriken() => PlayAttack(_shurikenSprites, _shurikenFps);
    public void PlaySpell() => PlayAttack(_spellSprites, _spellFps);

    //외부 호출용 - 이동
    public void PlayRun() => PlayLoop(_runSpites, _runFps);
    public void PlayRunStop() => PlayOnce(_runStopSprites, _runStopFps);

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
        if(sprites == null || sprites.Length == 0) yield break;

        float interval = 1f / fps;
        foreach(Sprite sprite in sprites)
        {
            _spriteRenderer.sprite = sprite;
            yield return new WaitForSecondsRealtime(interval);
        }
        onComplete?.Invoke();
        _currentAnim = null;
    }

    private IEnumerator PlayLoopCoroutine(Sprite[] sprites, float fps)
    {
        if(sprites == null || sprites.Length == 0) yield break;

        float interval = 1f / fps;
        while(true)
        {
            foreach(Sprite sprite in sprites)
            {
                _spriteRenderer.sprite = sprite;
                yield return new WaitForSecondsRealtime(interval);
            }
        }
    }

}
