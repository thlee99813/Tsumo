using UnityEngine;
using System.Collections.Generic;

public class DoraController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DeckController _deckController;
    [SerializeField] private GameObject _normalDoraRootObject;
    [SerializeField] private CardView _normalDoraCardView;
    [SerializeField] private List<GameObject> _cursedDoraRootObjects = new List<GameObject>();
    [SerializeField] private List<CardView> _cursedDoraCardViews = new List<CardView>();
    [SerializeField] private int _cursedDoraStageIndex = 5; // 2-3
    [SerializeField] private int _cursedDoraCount = 6;
    [SerializeField] private bool _isCursedDoraMode;
    [SerializeField] private List<CardData> _currentCursedDoraCards = new List<CardData>();

    public bool IsCursedDoraMode => _isCursedDoraMode;
    public List<CardData> CurrentCursedDoraCards => _currentCursedDoraCards;



    [Header("Rule")]
    [SerializeField] private int _doraUnlockStageIndex = 3;

    [Header("Debug")]
    [SerializeField] private bool _isUnlocked;
    [SerializeField] private CardData _currentDoraCard;

    public bool IsUnlocked => _isUnlocked;
    public CardData CurrentDoraCard => _currentDoraCard;

    public void ApplyStage(int stageIndex)
    {
        _isUnlocked = stageIndex >= _doraUnlockStageIndex;
        _isCursedDoraMode = _isUnlocked && stageIndex == _cursedDoraStageIndex;

        if (!_isUnlocked)
        {
            _currentDoraCard = null;
            _currentCursedDoraCards.Clear();
            _normalDoraRootObject.SetActive(false);
            SetCursedDoraRootsActive(0);
            SyncDoraVisualState();
            return;
        }

        if (_isCursedDoraMode)
        {
            _normalDoraRootObject.SetActive(false);
            int visibleCount = GetCursedDoraVisibleCount();
            SetCursedDoraRootsActive(visibleCount);
            RollNewCursedDoraCards(visibleCount);
        }
        else
        {
            _currentCursedDoraCards.Clear();
            SetCursedDoraRootsActive(0);
            _normalDoraRootObject.SetActive(true);

            if (_currentDoraCard == null)
            {
                RollNewDoraCard();
            }
        }

        SyncDoraVisualState();
    }




    public void RollAfterFire()
    {
        if (!_isUnlocked) return;

        if (_isCursedDoraMode)
        {
            int visibleCount = GetCursedDoraVisibleCount();
            RollNewCursedDoraCards(visibleCount);
        }
        else
        {
            RollNewDoraCard();
        }

        SyncDoraVisualState();
    }



    private void RollNewDoraCard()
    {
        _currentDoraCard = _deckController.GetRandomCardDefinition();
        _normalDoraCardView.SetCardData(_currentDoraCard);
    }

    private void RollNewCursedDoraCards(int count)
    {
        _currentCursedDoraCards.Clear();
        HashSet<int> usedCardKeys = new HashSet<int>();

        for (int i = 0; i < count; i++)
        {
            CardData cardData = GetUniqueCursedDoraCard(usedCardKeys);
            _currentCursedDoraCards.Add(cardData);
            _cursedDoraCardViews[i].SetCardData(cardData);
        }
    }

    private CardData GetUniqueCursedDoraCard(HashSet<int> usedCardKeys)
    {
        for (int tryCount = 0; tryCount < 64; tryCount++)
        {
            CardData cardData = _deckController.GetRandomCardDefinition();
            int key = ((int)cardData.Type * 10) + cardData.Number;

            if (usedCardKeys.Add(key))
            {
                return cardData;
            }
        }

        for (int typeIndex = 0; typeIndex < 3; typeIndex++)
        {
            CardType cardType = (CardType)typeIndex;
            for (int number = 1; number <= 9; number++)
            {
                int key = (typeIndex * 10) + number;
                if (usedCardKeys.Contains(key))
                {
                    continue;
                }

                if (_deckController.TryGetCardDefinition(cardType, number, out CardData cardData))
                {
                    usedCardKeys.Add(key);
                    return cardData;
                }
            }
        }

        return _deckController.GetRandomCardDefinition();
    }


    private int GetCursedDoraVisibleCount()
    {
        return Mathf.Min(_cursedDoraCount, Mathf.Min(_cursedDoraRootObjects.Count, _cursedDoraCardViews.Count));
    }

    private void SetCursedDoraRootsActive(int activeCount)
    {
        for (int i = 0; i < _cursedDoraRootObjects.Count; i++)
        {
            _cursedDoraRootObjects[i].SetActive(i < activeCount);
        }
    }

    private void SyncDoraVisualState()
    {
        List<CardData> activeDoraCards = new List<CardData>();

        if (_isCursedDoraMode)
        {
            for (int i = 0; i < _currentCursedDoraCards.Count; i++)
            {
                CardData cardData = _currentCursedDoraCards[i];
                if (cardData != null)
                {
                    activeDoraCards.Add(cardData);
                }
            }
        }
        else if (_currentDoraCard != null)
        {
            activeDoraCards.Add(_currentDoraCard);
        }

        CardView.SetDoraState(activeDoraCards, _isUnlocked, _isCursedDoraMode);

        _normalDoraCardView.RefreshView();
        for (int i = 0; i < _cursedDoraCardViews.Count; i++)
        {
            _cursedDoraCardViews[i].RefreshView();
        }

        _deckController.RefreshAllCardViews();
    }



        

    
}
