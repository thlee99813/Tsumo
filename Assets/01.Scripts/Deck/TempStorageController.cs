using System.Collections.Generic;
using UnityEngine;

public class TempStorageController : MonoBehaviour
{
    [SerializeField, Min(1)] private int _maxCardCount = 2;
    [SerializeField] private List<CardView> _slotViews = new List<CardView>();
    [SerializeField] private Transform _blockedSlotCardFront;
    [SerializeField] private float _blockedSlotXScaleMultiplier = 0.5f;

    [Header("Debug")]
    [SerializeField] private List<CardData> _storedCards = new List<CardData>();
    [SerializeField] private int _usableSlotCount;

    private Vector3 _defaultBlockedSlotScale;
    private Vector3 _defaultBlockedSlotLocalPosition;


    public bool HasEmptySlot()
    {
        return _storedCards.Count < _usableSlotCount;
    }

    public bool HasCardAt(int tempIndex)
    {
        return tempIndex >= 0 && tempIndex < _storedCards.Count && tempIndex < _usableSlotCount;
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

    public void ClearStoredCardsForRestart()
    {
        _storedCards.Clear();
        RefreshViews();
    }

    private void Awake()
    {
        _usableSlotCount = _maxCardCount;
        _defaultBlockedSlotScale = _blockedSlotCardFront.localScale;
        _defaultBlockedSlotLocalPosition = _blockedSlotCardFront.localPosition;
    }


    public void SetUsableSlotCount(int usableSlotCount, bool showBlockedVisual)
    {
        _usableSlotCount = Mathf.Clamp(usableSlotCount, 1, _maxCardCount);
        ApplyBlockedSlotVisual(showBlockedVisual);

        if (_storedCards.Count > _usableSlotCount)
        {
            _storedCards.RemoveRange(_usableSlotCount, _storedCards.Count - _usableSlotCount);
        }

        RefreshViews();
    }
    private void ApplyBlockedSlotVisual(bool isBlocked)
    {
        if (!isBlocked)
        {
            _blockedSlotCardFront.localScale = _defaultBlockedSlotScale;
            _blockedSlotCardFront.localPosition = _defaultBlockedSlotLocalPosition;
            return;
        }

        Vector3 blockedScale = _defaultBlockedSlotScale;
        blockedScale.x = _defaultBlockedSlotScale.x * _blockedSlotXScaleMultiplier;

        float reducedAmountX = (_defaultBlockedSlotScale.x - 0.25f);

        Vector3 blockedLocalPosition = _defaultBlockedSlotLocalPosition;
        blockedLocalPosition.x -= reducedAmountX;

        _blockedSlotCardFront.localScale = blockedScale;
        _blockedSlotCardFront.localPosition = blockedLocalPosition;
    }


    public void RestoreDefaultSlotCount()
    {
        SetUsableSlotCount(_maxCardCount, false);
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

            if (i >= _usableSlotCount)
            {
                slotView.gameObject.SetActive(false);
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
