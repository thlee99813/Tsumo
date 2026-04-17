using UnityEngine;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _borderRenderer;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _effectText;
    [SerializeField] private CardData _cardData;

    [SerializeField] private Color _swordColor = Color.red;
    [SerializeField] private Color _kunaiColor = Color.green;
    [SerializeField] private Color _foxSpiritColor = Color.cyan;

    private void Start()
    {
        RefreshView();
    }
     private void OnValidate()
    {
        RefreshView();
    }

    public void SetCardData(CardData cardData)
    {
        _cardData = cardData;
        RefreshView();
    }

    public void RefreshView()
    {
        ApplyBorderColorByType();
        ApplyTexts();
    }
    

    public void ApplyBorderColorByType()
    {
        if (_borderRenderer == null || _cardData == null) return;

        _borderRenderer.color = _cardData.Type switch
        {
            CardType.Sword => _swordColor,
            CardType.Kunai => _kunaiColor,
            CardType.FoxSpirit => _foxSpiritColor,
            _ => _borderRenderer.color
        };
    }

    private void ApplyTexts()
    {
        if (_cardData == null) return;

        if (_titleText != null)
        {
            _titleText.text = _cardData.TypeDisplayName;
        }

        if (_effectText != null)
        {
            _effectText.text = _cardData.SkillDisplayName;
        }
    }
}
