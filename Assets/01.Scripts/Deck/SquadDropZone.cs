using System.Collections.Generic;
using UnityEngine;

public class SquadDropZone : MonoBehaviour
{
    [SerializeField] private int _maxCardCount = 3;
    [SerializeField] private List<CardView> _slotViews = new List<CardView>();

    [Header("Debug")]
    [SerializeField] private List<CardData> _registeredCards = new List<CardData>();
    private CardSorter _cardSorter = new CardSorter();

    public bool TryRegisterCard(CardData cardData)
    {
        if (cardData == null)
        {
            return false;
        }

        if (_registeredCards.Count >= _maxCardCount)
        {
            return false;
        }

        if (_registeredCards.Count > 0)
        {
            CardType lockedType = _registeredCards[0].Type;
            if (cardData.Type != lockedType)
            {
                return false;
            }
        }

        _registeredCards.Add(cardData);
        _cardSorter.Sort(_registeredCards);
        RefreshViews();
        return true;
    }
    public List<CardData> GetRegisteredCardsSnapshot()
    {
        return new List<CardData>(_registeredCards);
    }

    public void ClearRegisteredCardsForRestart()
    {
        _registeredCards.Clear();
        RefreshViews();
    }

    public void ReturnAllTo(List<CardData> deckCards)
    {
        if (deckCards == null || _registeredCards.Count == 0)
        {
            return;
        }

        deckCards.AddRange(_registeredCards);
        _registeredCards.Clear();
        RefreshViews();
    }


    private void OnValidate()
    {
        _cardSorter.Sort(_registeredCards);
        RefreshViews();
    }

    private void RefreshViews()
    {
        for (int i = 0; i < _slotViews.Count; i++)
        {
            CardView slotView = _slotViews[i];
            if (slotView == null)
            {
                continue;
            }

            if (i < _registeredCards.Count)
            {
                slotView.gameObject.SetActive(true);
                slotView.SetCardData(_registeredCards[i]);
            }
            else
            {
                slotView.gameObject.SetActive(false);
            }
        }
    }
    public void RefreshAllSlotViews()
    {
        for (int i = 0; i < _slotViews.Count; i++)
        {
            CardView slotView = _slotViews[i];
            if (slotView.gameObject.activeSelf)
            {
                slotView.RefreshView();
            }
        }
    }

}
