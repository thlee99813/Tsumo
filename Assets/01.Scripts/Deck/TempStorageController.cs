using System.Collections.Generic;
using UnityEngine;

public class TempStorageController : MonoBehaviour
{
    [SerializeField, Min(1)] private int _maxCardCount = 2;
    [SerializeField] private List<CardView> _slotViews = new List<CardView>();

    [Header("Debug")]
    [SerializeField] private List<CardData> _storedCards = new List<CardData>();

    public bool HasEmptySlot()
    {
        return _storedCards.Count < _maxCardCount;
    }

    public bool HasCardAt(int tempIndex)
    {
        return tempIndex >= 0 && tempIndex < _storedCards.Count;
    }

    public bool TryStoreCard(CardData cardData)
    {
        if (cardData == null || !HasEmptySlot())
        {
            return false;
        }

        _storedCards.Add(cardData);
        RefreshViews();
        return true;
    }

    public bool TryRegisterCardToSquad(int tempIndex, SquadDropZone squadZone)
    {
        if (!HasCardAt(tempIndex) || squadZone == null)
        {
            return false;
        }

        CardData cardData = _storedCards[tempIndex];
        if (!squadZone.TryRegisterCard(cardData))
        {
            return false;
        }

        _storedCards.RemoveAt(tempIndex);
        RefreshViews();
        return true;
    }

    public bool TryDiscardCard(int tempIndex, DeckController deckController)
    {
        if (!HasCardAt(tempIndex) || deckController == null)
        {
            return false;
        }

        CardData cardData = _storedCards[tempIndex];
        _storedCards.RemoveAt(tempIndex);
        deckController.AddToDiscard(cardData);
        RefreshViews();
        return true;
    }

    public void ReturnAllTo(List<CardData> deckCards)
    {
        if (deckCards == null || _storedCards.Count == 0)
        {
            return;
        }

        deckCards.AddRange(_storedCards);
        _storedCards.Clear();
        RefreshViews();
    }

    private void OnValidate()
    {
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

            if (i < _storedCards.Count)
            {
                slotView.gameObject.SetActive(true);
                slotView.SetCardData(_storedCards[i]);
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
