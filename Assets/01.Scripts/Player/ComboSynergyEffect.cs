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
    [Header("Enhanced Combo Effect")]
    [SerializeField] private ComboEffectSet _enhancedComboEffect;

    [Header("Synergy Effect")]
    [SerializeField] private ComboEffectSet _synergyEffect;

    [Header("Frame Settings")]
    [SerializeField] private float _effectFps = 12f;

    public float Fps => _effectFps;

    // 일반 콤보는 null 반환 → PlayerAnimator 기본 애니메이션 사용
    public Sprite[] GetSprites(SquadResultType resultType, CardType cardType, bool hasSynergy)
    {
        if (hasSynergy)
            return GetFromSet(_synergyEffect, cardType);

        if (resultType == SquadResultType.EnhancedCombo)
            return GetFromSet(_enhancedComboEffect, cardType);

        return null;
    }

    private Sprite[] GetFromSet(ComboEffectSet effectSet, CardType cardType) => cardType switch
    {
        CardType.Sword     => effectSet.SwordSprites,
        CardType.Kunai     => effectSet.KunaiSprites,
        CardType.FoxSpirit => effectSet.FoxSpiritSprites,
        _                  => null
    };
}
