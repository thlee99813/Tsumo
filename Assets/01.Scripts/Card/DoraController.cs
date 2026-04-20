using UnityEngine;

public class DoraController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DeckController _deckController;
    [SerializeField] private GameObject _doraRootObject;
    [SerializeField] private CardView _doraCardView;

    [Header("Rule")]
    [SerializeField] private int _doraUnlockStageIndex = 3;

    [Header("Debug")]
    [SerializeField] private bool _isUnlocked;
    [SerializeField] private CardData _currentDoraCard;

    public bool IsUnlocked => _isUnlocked;
    public CardData CurrentDoraCard => _currentDoraCard;

    public void ApplyStage(int stageIndex)
    {
        bool shouldUnlock = stageIndex >= _doraUnlockStageIndex;
        _isUnlocked = shouldUnlock;

        if (!shouldUnlock)
        {
            _currentDoraCard = null;
            _doraRootObject.SetActive(false);
            SyncDoraVisualState();
            return;
        }

        _doraRootObject.SetActive(true);

        if (_currentDoraCard == null)
        {
            RollNewDoraCard();
        }

        SyncDoraVisualState();
    }


    public void RollAfterFire()
    {
        if (!_isUnlocked) return;
        RollNewDoraCard();
        SyncDoraVisualState();
    }


    private void RollNewDoraCard()
    {
        _currentDoraCard = _deckController.GetRandomCardDefinition();
        _doraCardView.SetCardData(_currentDoraCard);
    }
    private void SyncDoraVisualState()
    {
        CardView.SetDoraState(_currentDoraCard, _isUnlocked);
        _deckController.RefreshAllCardViews();
    }

        

    
}
