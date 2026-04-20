using System.Collections;
using UnityEngine;

// Player의 자식 오브젝트에 부착 — 시너지 발동 시 독립적으로 오버레이 재생
public class SynergyOverlayEffect : MonoBehaviour
{
    [Header("Synergy Overlay Sprites")]
    [SerializeField] private ComboEffectSet _synergyEffect;

    [Header("Frame Settings")]
    [SerializeField] private float _effectFps = 12f;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentEffect;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    public void PlaySynergy(CardType cardType)
    {
        Sprite[] sprites = cardType switch
        {
            CardType.Sword     => _synergyEffect.SwordSprites,
            CardType.Kunai     => _synergyEffect.KunaiSprites,
            CardType.FoxSpirit => _synergyEffect.FoxSpiritSprites,
            _                  => null
        };
        Play(sprites);
    }

    public void Stop()
    {
        if (_currentEffect != null)
        {
            StopCoroutine(_currentEffect);
            _currentEffect = null;
        }
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;
    }

    private void Play(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;
        Stop();
        _currentEffect = StartCoroutine(PlayCoroutine(sprites));
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
