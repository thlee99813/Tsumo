using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _borderRenderer;
    [SerializeField] private TMP_Text _effectText;
    [SerializeField] private CardData _cardData;

    [SerializeField] private GameObject _doraEffect;
    [SerializeField] private GameObject _curseDoraEffect;

    private static bool _isDoraEnabled;
    private static bool _isCursedDoraMode;
    private static readonly List<CardData> _activeDoraCards = new List<CardData>();



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

    public static void SetDoraState(List<CardData> doraCards, bool isDoraEnabled, bool isCursedDoraMode)
    {
        _isDoraEnabled = isDoraEnabled;
        _isCursedDoraMode = isCursedDoraMode;
        _activeDoraCards.Clear();

        if (doraCards == null) return;

        for (int i = 0; i < doraCards.Count; i++)
        {
            CardData card = doraCards[i];
            if (card != null)
            {
                _activeDoraCards.Add(card);
            }
        }
    }



    private void ApplyDoraEffect()
    {
        bool isMatch = false;

        if (_isDoraEnabled && _cardData != null)
        {
            for (int i = 0; i < _activeDoraCards.Count; i++)
            {
                CardData dora = _activeDoraCards[i];
                if (dora != null && _cardData.Type == dora.Type && _cardData.Number == dora.Number)
                {
                    isMatch = true;
                    break;
                }
            }
        }

        _doraEffect.SetActive(isMatch && !_isCursedDoraMode);
        _curseDoraEffect.SetActive(isMatch && _isCursedDoraMode);
    }



    public void RefreshView()
    {
        if (_cardData == null)
        {
            _doraEffect.SetActive(false);
            _curseDoraEffect.SetActive(false);
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
