using UnityEngine;

[System.Serializable]
public struct ComboEffectSet
{
    public Sprite[] SwordSprites;
    public Sprite[] KunaiSprites;
    public Sprite[] FoxSpiritSprites;
}

// Player 오브젝트에 부착 — 강화 콤보일 때 공격 애니메이션 및 이펙트 스프라이트 제공
public class ComboSynergyEffect : MonoBehaviour
{
    [Header("Enhanced Attack Animation (플레이어 본체 스프라이트)")]
    [SerializeField] private ComboEffectSet _enhancedAttackSprites;

    [Header("Enhanced Effect (별도 이펙트 SpriteRenderer)")]
    [SerializeField] private ComboEffectSet _enhancedEffectSprites;

    [Header("Frame Settings")]
    [SerializeField] private float _attackFps = 12f;
    [SerializeField] private float _effectFps = 12f;

    public float Fps => _attackFps;
    public float EffectFps => _effectFps;

    // EnhancedCombo일 때만 반환, null → PlayerAnimator 기본 애니 사용
    public Sprite[] GetAttackSprites(SquadResultType resultType, CardType cardType)
    {
        if (resultType != SquadResultType.EnhancedCombo) return null;
        return cardType switch
        {
            CardType.Sword     => _enhancedAttackSprites.SwordSprites,
            CardType.Kunai     => _enhancedAttackSprites.KunaiSprites,
            CardType.FoxSpirit => _enhancedAttackSprites.FoxSpiritSprites,
            _                  => null
        };
    }

    // EnhancedCombo일 때만 반환, null → PlayerEffect 기본 이펙트 사용
    public Sprite[] GetEffectSprites(SquadResultType resultType, CardType cardType)
    {
        if (resultType != SquadResultType.EnhancedCombo) return null;
        return cardType switch
        {
            CardType.Sword     => _enhancedEffectSprites.SwordSprites,
            CardType.Kunai     => _enhancedEffectSprites.KunaiSprites,
            CardType.FoxSpirit => _enhancedEffectSprites.FoxSpiritSprites,
            _                  => null
        };
    }
}