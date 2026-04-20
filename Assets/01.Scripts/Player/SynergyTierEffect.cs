using System.Collections;
using UnityEngine;

// Player 자식 오브젝트에 부착 — 최종 배율 티어에 따라 공격 전 이펙트 재생
public class SynergyTierEffect : MonoBehaviour
{
    [Header("Tier Sprites (배율 기준)")]
    [SerializeField] private Sprite[] _tier0Sprites;   // 1.0 ~ 1.5
    [SerializeField] private Sprite[] _tier1Sprites;   // 1.5 ~ 2.5
    [SerializeField] private Sprite[] _tier2Sprites;   // 2.5 ~ 4.5
    [SerializeField] private Sprite[] _tier3Sprites;   // 4.5 ~ 7.0
    [SerializeField] private Sprite[] _tier4Sprites;   // 7.0 이상

    [Header("Frame Settings")]
    [SerializeField] private float _fps = 12f;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    // StageFlowController에서 yield return Play(multiplier) 로 호출
    public IEnumerator Play(float multiplier)
    {
        Sprite[] sprites = GetTierSprites(multiplier);
        if (sprites == null || sprites.Length == 0) yield break;

        _spriteRenderer.enabled = true;
        float interval = 1f / _fps;
        foreach (Sprite sprite in sprites)
        {
            _spriteRenderer.sprite = sprite;
            yield return new WaitForSecondsRealtime(interval);
        }
        _spriteRenderer.enabled = false;
    }

    private Sprite[] GetTierSprites(float multiplier)
    {
        if (multiplier >= 7.0f) return _tier4Sprites;
        if (multiplier >= 4.5f) return _tier3Sprites;
        if (multiplier >= 2.5f) return _tier2Sprites;
        if (multiplier >= 1.5f) return _tier1Sprites;
        return _tier0Sprites;
    }
}
