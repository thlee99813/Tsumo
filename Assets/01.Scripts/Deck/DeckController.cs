using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    [Header("Card SO")]
    [SerializeField] private List<CardData> _cardDefinitions = new List<CardData>();

    [Header("Deck Settings")]
    [SerializeField] private int _copiesPerCard = 4;
    [SerializeField] private int _drawCount = 6;
    [SerializeField] private int _maxRerollPerTurn = 2;
    [SerializeField] private int _rerollUsedThisTurn = 0;

    [Header("UI")]
    [SerializeField] private UIController _uiController;


    [Header("Hand Slots")]
    [SerializeField] private List<CardView> _handSlots = new List<CardView>();

    [Header("Squad Zones")]
    [SerializeField] private List<SquadDropZone> _squadZones = new List<SquadDropZone>();

    [Header("Temp")]
    [SerializeField] private TempStorageController _tempStorageController;
    [Header("Score")]
    [SerializeField] private FireScoreCalculator _fireScoreCalculator;


    [Header("Debug")]

    [SerializeField] private bool _handSort = false; 
    [SerializeField] private List<CardData> _deckCards = new List<CardData>();
    [SerializeField] private List<CardData> _handCards = new List<CardData>();
    [SerializeField] private List<CardData> _discardCards = new List<CardData>();

    private CardSorter _cardSorter = new CardSorter();


    private void Start()
    {
        BuildDeckAndDrawHand();
    }

    public void BuildDeckAndDrawHand()
    {
        BuildDeck();
        DrawHand();
    }

    public void BuildDeck()
    {
        _deckCards.Clear();
        _handCards.Clear();
        _discardCards.Clear();

        foreach (CardData cardData in _cardDefinitions)
        {
            if (cardData == null)
            {
                continue;
            }

            for (int i = 0; i < _copiesPerCard; i++)
            {
                _deckCards.Add(cardData);
            }
        }

        ShuffleDeck();
        _rerollUsedThisTurn = 0;
        RefreshRerollCountUI();

    }


    public void DrawHand()
    {
        _handCards.Clear();

        int drawAmount = Mathf.Min(_drawCount, _deckCards.Count);
        for (int i = 0; i < drawAmount; i++)
        {
            int lastIndex = _deckCards.Count - 1;
            CardData drawnCard = _deckCards[lastIndex];
            _deckCards.RemoveAt(lastIndex);
            _handCards.Add(drawnCard);
        }
        if(_handSort)
        {
            _cardSorter.Sort(_handCards);
        }
        
        ApplyHandToSlots();
    }

    public bool HasHandCardAt(int handIndex)
    {
        return handIndex >= 0 && handIndex < _handCards.Count;
    }

    public bool IsHandSlotOwner(int handIndex, CardView cardView)
    {
        if (cardView == null) return false;
        if (handIndex < 0 || handIndex >= _handSlots.Count) return false;
        return _handSlots[handIndex] == cardView;
    }


    public bool TryRegisterHandCardToSquad(int handIndex, SquadDropZone squadZone)
    {
        if (!HasHandCardAt(handIndex) || squadZone == null)
        {
            return false;
        }

        CardData cardData = _handCards[handIndex];
        if (!squadZone.TryRegisterCard(cardData))
        {
            return false;
        }

        _handCards.RemoveAt(handIndex);
        ApplyHandToSlots();
        return true;
    }
    public bool TryMoveHandCardToTemp(int handIndex, TempStorageController tempStorageController)
    {
        if (!HasHandCardAt(handIndex) || tempStorageController == null)
        {
            return false;
        }

        CardData cardData = _handCards[handIndex];
        if (!tempStorageController.TryStoreCard(cardData))
        {
            return false;
        }

        _handCards.RemoveAt(handIndex);
        ApplyHandToSlots();
        return true;
    }

    public void AddToDiscard(CardData cardData)
    {
        if (cardData == null)
        {
            return;
        }

        _discardCards.Add(cardData);
    }

    public void OnClickReroll()
    {
        if (_rerollUsedThisTurn >= _maxRerollPerTurn)
        {
            return;
        }

        MoveHandToDiscard();
        DrawHand();
        _rerollUsedThisTurn++;
        RefreshRerollCountUI();
    }

    public FireScoreResult CalculateFireScore()
    {
<<<<<<< HEAD
        if (_fireScoreCalculator == null)
        {
            Debug.LogWarning("[DeckController] FireScoreCalculator 참조가 비어있습니다.");
            return new FireScoreResult();
        }

        return _fireScoreCalculator.Calculate(_squadZones);
    }

    public void CompleteTurnAfterFire()
    {
        ReturnAllCardsToDeckForNextTurn();
        ShuffleDeck();
        DrawHand();
        _rerollUsedThisTurn = 0;
        RefreshRerollCountUI();
=======
        FireExecutionData fireData = BuildFireExecutionData(true);
        Debug.Log($"[DeckController] Fire Debug FinalDamage: {fireData.FinalDamage}");
        CompleteTurnAfterFire();
>>>>>>> feature/deck-controller
    }

    public void OnClickFire()
    {
        Debug.LogWarning("[DeckController] Fire 버튼은 IngameController.HandleFireInput에만 연결하세요.");
    }




    private void MoveHandToDiscard()
    {
        if (_handCards.Count == 0)
        {
            return;
        }

        _discardCards.AddRange(_handCards);
        _handCards.Clear();
    }

    public FireExecutionData BuildFireExecutionData(bool isFirePressed)
    {
        FireExecutionData data = new FireExecutionData
        {
            IsFirePressed = isFirePressed,
            FinalDamage = 0,
            ScoreResult = null
        };

        if (!isFirePressed)
        {
            Debug.Log("[FireScore] 타임아웃: 플레이어 공격 0");
            return data;
        }

        if (_fireScoreCalculator == null)
        {
            Debug.LogWarning("[FireScore] FireScoreCalculator 참조가 비어있습니다.");
            return data;
        }

        FireScoreResult result = _fireScoreCalculator.Calculate(_squadZones);
        data.ScoreResult = result;
        data.FinalDamage = Mathf.Max(0, result.FinalScore);

        Debug.Log(result.BuildDebugText());
        return data;
    }

    public void CompleteTurnAfterFire()
    {
        ReturnAllCardsToDeckForNextTurn();
        ShuffleDeck();
        DrawHand();
        _rerollUsedThisTurn = 0;
        RefreshRerollCountUI();
    }



    private void ReturnAllCardsToDeckForNextTurn()
    {
        if (_handCards.Count > 0)
        {
            _deckCards.AddRange(_handCards);
            _handCards.Clear();
        }

        if (_discardCards.Count > 0)
        {
            _deckCards.AddRange(_discardCards);
            _discardCards.Clear();
        }

        for (int i = 0; i < _squadZones.Count; i++)
        {
            SquadDropZone squadZone = _squadZones[i];
            if (squadZone == null)
            {
                continue;
            }

            squadZone.ReturnAllTo(_deckCards);
        }

        if (_tempStorageController != null)
        {
            _tempStorageController.ReturnAllTo(_deckCards);
        }

    }



    private void ApplyHandToSlots()
    {
        for (int i = 0; i < _handSlots.Count; i++)
        {
            CardView slotView = _handSlots[i];
            if (slotView == null)
            {
                continue;
            }

            if (i < _handCards.Count)
            {
                slotView.gameObject.SetActive(true);
                slotView.SetCardData(_handCards[i]);
            }

            else
            {
                slotView.gameObject.SetActive(false);
            }
        }
    }

    private void ShuffleDeck()
    {
        for (int i = _deckCards.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);

            CardData temp = _deckCards[i];
            _deckCards[i] = _deckCards[swapIndex];
            _deckCards[swapIndex] = temp;
        }
    }
    private void RefreshRerollCountUI()
    {
        if (_uiController == null)
        {
            return;
        }

        int remainingCount = _maxRerollPerTurn - _rerollUsedThisTurn;
        _uiController.SetRerollCount(remainingCount, _maxRerollPerTurn);
    }



}
