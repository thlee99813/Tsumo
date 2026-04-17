using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _borderRenderer;
    [SerializeField] private CardData _cardData;

    [SerializeField] private Color _swordColor = Color.red;
    [SerializeField] private Color _kunaiColor = Color.green;
    [SerializeField] private Color _foxSpiritColor = Color.cyan;

    public void ApplyBorderColorByType()
    {
        if (_borderRenderer == null || _cardData == null) return;

        _borderRenderer.color = _cardData.Type 
        switch
        {
            CardType.Sword => _swordColor,
            CardType.Kunai => _kunaiColor,
            CardType.FoxSpirit => _foxSpiritColor,
            _ => _borderRenderer.color
        };
    }
    public void Start()
    {
        ApplyBorderColorByType();
    }
}
