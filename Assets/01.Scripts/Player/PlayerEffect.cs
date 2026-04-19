using System;
using UnityEngine;
using System.Collections;

public class PlayerEffect : MonoBehaviour
{
    [Header("Effect Sprites")]
    [SerializeField] private Sprite[] _swordEffectSprites;
    [SerializeField] private Sprite[] _shurikenEffectSprits;
    [SerializeField] private Sprite[] _spellEffectSprits;

    [Header("Frame Settings")]
    [SerializeField] private float _effectFps = 12f;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentEffect;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    public void PlaySwordEffect() => PlayEffect(_swordEffectSprites);
    public void PlayShurikenEffect() => PlayEffect(_shurikenEffectSprits);
    public void PlaySpellEffect() => PlayEffect(_spellEffectSprits);

    public void StopEffect()
    {
        if(_currentEffect != null)
        {
            StopCoroutine(_currentEffect);
            _currentEffect = null;
        }
        _spriteRenderer.enabled = false;
    }

    private void PlayEffect(Sprite[] sprites)
    {
        StopEffect();
        _currentEffect = StartCoroutine(PlayEffectCoroutine(sprites));
    }

    private IEnumerator PlayEffectCoroutine(Sprite[] sprites)
    {
        if(sprites == null || sprites.Length == 0) yield break;
        Debug.Log("Effect Start");
        _spriteRenderer.enabled = true;
        float interval = 1f / _effectFps;

        foreach(Sprite sprite in sprites)
        {
            _spriteRenderer.sprite = sprite;
            yield return new WaitForSecondsRealtime(interval);
        }

        _spriteRenderer.enabled = false;
        _currentEffect = null;
    }
}
