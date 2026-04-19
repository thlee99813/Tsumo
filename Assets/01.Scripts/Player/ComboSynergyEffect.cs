using System.Collections;
using UnityEngine;

[System.Serializable]
public struct ComboEffectSet
{
    public Sprite[] SwordSprites;
    public Sprite[] KunaiSprites;
    public Sprite[] FoxSpiritSprites;
}

public class ComboSynergyEffect : MonoBehaviour
{
    [Header("Normal Combo Effect")]
    [SerializeField] private ComboEffectSet _normalComboEffect;

    [Header("Enhanced Combo Effect")]
    [SerializeField] private ComboEffectSet _enhancedComboEffect;

    [Header("Synergy Effect")]
    [SerializeField] private ComboEffectSet _synergyEffect;

    [Header("Frame Settings")]
    [SerializeField] private float _effectFps = 12f;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentEffect;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;
    }

    // 시너지가 있으면 시너지 이펙트 우선, 없으면 콤보 종류에 맞는 이펙트 재생
    public void PlayComboEffect(SquadResultType resultType, CardType cardType, bool hasSynergy)
    {
        Sprite[] sprites = hasSynergy
            ? GetSprites(_synergyEffect, cardType)
            : resultType switch
            {
                SquadResultType.NormalCombo   => GetSprites(_normalComboEffect, cardType),
                SquadResultType.EnhancedCombo => GetSprites(_enhancedComboEffect, cardType),
                _                             => null
            };

        Play(sprites);
    }

    private Sprite[] GetSprites(ComboEffectSet effectSet, CardType cardType) => cardType switch
    {
        CardType.Sword     => effectSet.SwordSprites,
        CardType.Kunai     => effectSet.KunaiSprites,
        CardType.FoxSpirit => effectSet.FoxSpiritSprites,
        _                  => null
    };

    private void Play(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;
        StopCurrentEffect();
        _currentEffect = StartCoroutine(PlayCoroutine(sprites));
    }

    public void StopCurrentEffect()
    {
        if (_currentEffect != null)
        {
            StopCoroutine(_currentEffect);
            _currentEffect = null;
        }
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;
    }

    private IEnumerator PlayCoroutine(Sprite[] sprites)
    {
        _spriteRenderer.enabled = true;
        float interval = 1f / _effectFps;
        foreach (Sprite sprite in sprites)
        {
            _spriteRenderer.sprite = sprite;
            yield return new WaitForSeconds(interval);
        }
        _spriteRenderer.enabled = false;
        _currentEffect = null;
    }
}
