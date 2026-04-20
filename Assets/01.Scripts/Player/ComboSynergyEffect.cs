using UnityEngine;

[System.Serializable]
public struct ComboEffectSet
{
    public Sprite[] SwordSprites;
    public Sprite[] KunaiSprites;
    public Sprite[] FoxSpiritSprites;
}

// Player 오브젝트에 부착 — 강화 콤보일 때 공격 애니메이션을 교체할 스프라이트 제공
public class ComboSynergyEffect : MonoBehaviour
{
    [Header("Enhanced Combo Effect")]
    [SerializeField] private ComboEffectSet _enhancedComboEffect;

    [Header("Frame Settings")]
    [SerializeField] private float _effectFps = 12f;

    public float Fps => _effectFps;

    // EnhancedCombo일 때만 스프라이트 반환, 그 외 null → PlayerAnimator 기본 애니 사용
    public Sprite[] GetAttackSprites(SquadResultType resultType, CardType cardType)
    {
        if (resultType != SquadResultType.EnhancedCombo) return null;
        return cardType switch
        {
            CardType.Sword     => _enhancedComboEffect.SwordSprites,
            CardType.Kunai     => _enhancedComboEffect.KunaiSprites,
            CardType.FoxSpirit => _enhancedComboEffect.FoxSpiritSprites,
            _                  => null
        };
    }
}
