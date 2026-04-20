using UnityEngine;
using TMPro;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _borderRenderer;
    [SerializeField] private TMP_Text _effectText;
    [SerializeField] private CardData _cardData;

    [SerializeField] private GameObject _doraEffect;

    private static CardData _currentDoraCard;
    private static bool _isDoraEnabled;


    [Header("Border Sprites")]
    [SerializeField] private Sprite _swordBorderSprite;
    [SerializeField] private Sprite _kunaiBorderSprite;
    [SerializeField] private Sprite _foxSpiritBorderSprite;

    [Header("Text Colors")]
    [SerializeField] private Color _swordTextColor = Color.red;
    [SerializeField] private Color _kunaiTextColor = Color.green;
    [SerializeField] private Color _foxSpiritTextColor = Color.cyan;


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

    public static void SetDoraState(CardData doraCardData, bool isDoraEnabled)
    {
        _currentDoraCard = doraCardData;
        _isDoraEnabled = isDoraEnabled;
    }

    private void ApplyDoraEffect()
    {
        bool isMatch = _isDoraEnabled
                    && _currentDoraCard != null
                    && _cardData != null
                    && _cardData.Type == _currentDoraCard.Type
                    && _cardData.Number == _currentDoraCard.Number;

        _doraEffect.SetActive(isMatch);
    }


    public void RefreshView()
    {
        if (_cardData == null)
        {
            _doraEffect.SetActive(false);
            return;
        }

        ApplyBorderSpriteByType();
        ApplyTexts();
        ApplyTextColorByType();
        ApplyDoraEffect();
    }


    

    private void ApplyBorderSpriteByType()
    {
        _borderRenderer.sprite = GetBorderSpriteByType(_cardData.Type);
        _borderRenderer.color = Color.white;
    }

    private void ApplyTextColorByType()
    {
        Color typeColor = GetTextColorByType(_cardData.Type);
        _effectText.color = typeColor;
    }

    private void ApplyTexts()
    {
        _effectText.text = _cardData.NumberDisplayName;
    }

    private Sprite GetBorderSpriteByType(CardType cardType)
    {
        return cardType switch
        {
            CardType.Sword => _swordBorderSprite,
            CardType.Kunai => _kunaiBorderSprite,
            CardType.FoxSpirit => _foxSpiritBorderSprite,
            _ => _swordBorderSprite
        };
    }

    private Color GetTextColorByType(CardType cardType)
    {
        return cardType switch
        {
            CardType.Sword => _swordTextColor,
            CardType.Kunai => _kunaiTextColor,
            CardType.FoxSpirit => _foxSpiritTextColor,
            _ => _swordTextColor
        };
    }

}
