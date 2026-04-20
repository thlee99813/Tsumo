using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class TutorialFlowController : MonoBehaviour
{
    private enum TutorialAction
    {
        None,
        SetupBlue123Blue6Hand,
        AutoPlaceBlue123AndBlue6,
        SetupBlue66Green4Hand,
        AutoPlaceBlueTripleAndRed4,
        AutoPlaceRed5Red6,
        ShowYakuPartialFlush,
        ShowYakuFullFlush,
        ShowYakuDoubleSequence,
        ShowYakuTripleSequence,
        ShowYakuSanshokuDojun,
        ShowYakuStraightOneToNine,
        ShowYakuDoubleTriple,
        ShowYakuAllTriples,
        ShowYakuSanshokuDoukou,
        HideYakuGuide
    }


    [System.Serializable]
    private class TutorialStep
    {
        [TextArea(2, 4)]
        [SerializeField] private string _line;
        [SerializeField] private TutorialAction _onEnter;
        [SerializeField] private TutorialAction _onAdvance;

        public string Line => _line;
        public TutorialAction OnEnter => _onEnter;
        public TutorialAction OnAdvance => _onAdvance;

        public TutorialStep(string line, TutorialAction onEnter = TutorialAction.None, TutorialAction onAdvance = TutorialAction.None)
        {
            _line = line;
            _onEnter = onEnter;
            _onAdvance = onAdvance;
        }
    }

    [Header("UI")]
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private float _typingInterval = 0.04f;

    [Header("Yaku Guide UI")]
    [SerializeField] private GameObject _yakuGuideRoot;
    [SerializeField] private TMP_Text _yakuGuideTitleText;
    [SerializeField] private List<CardView> _yakuGuideCardViews = new List<CardView>();

    [Header("End")]
    [SerializeField] private string _mainSceneName = "01.MainScene";
    [SerializeField] private float _moveMainDelay = 2f;


    [Header("Refs")]
    [SerializeField] private Player _player;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private Enemy _enemy;
    [SerializeField] private DeckController _deckController;
    [SerializeField] private TempStorageController _tempStorageController;
    [SerializeField] private List<SquadDropZone> _squadZones = new List<SquadDropZone>();

    [Header("Steps")]
    [SerializeField] private List<TutorialStep> _steps = new List<TutorialStep>();

    private int _currentStepIndex = -1;
    private bool _isTyping;
    private bool _skipTyping;
    private Coroutine _typingCoroutine;
    private bool _isEnding;

    private readonly struct CardRef
    {
        public readonly CardType Type;
        public readonly int Number;

        public CardRef(CardType type, int number)
        {
            Type = type;
            Number = number;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        _player.StopMovement();
        _playerAnimator.PlayRun();
        _enemy.PauseMovement();

        _tempStorageController.RestoreDefaultSlotCount();
        _deckController.BuildDeckAndDrawHand();

        BuildDefaultSteps();
        HideYakuGuide();


        _nextButton.onClick.AddListener(OnClickNext);
        OnClickNext();
    }

    private void OnDestroy()
    {
        _nextButton.onClick.RemoveListener(OnClickNext);
    }

    public void OnClickNext()
    {
        if (_isTyping)
        {
            _skipTyping = true;
            return;
        }

        if (_currentStepIndex >= 0 && _currentStepIndex < _steps.Count)
        {
            RunAction(_steps[_currentStepIndex].OnAdvance);
        }

        _currentStepIndex++;
        if (_currentStepIndex >= _steps.Count)
        {
            return;
        }


        RunAction(_steps[_currentStepIndex].OnEnter);

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(TypeLine(_steps[_currentStepIndex].Line));
    }

    private IEnumerator TypeLine(string line)
    {
        _isTyping = true;
        _skipTyping = false;
        _dialogueText.text = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            if (_skipTyping)
            {
                _dialogueText.text = line;
                break;
            }

            _dialogueText.text += line[i];

            if (_typingInterval > 0f)
            {
                yield return new WaitForSecondsRealtime(_typingInterval);
            }
        }

        _isTyping = false;
        OnStepLineCompleted();

    }
    private void OnStepLineCompleted()
    {
        if (_currentStepIndex != _steps.Count - 1)
        {
            return;
        }

        if (_isEnding)
        {
            return;
        }

        _isEnding = true;
        _nextButton.interactable = false;
        StartCoroutine(MoveToMainSceneAfterDelay());
    }

    private IEnumerator MoveToMainSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(_moveMainDelay);
        UnitySceneManager.LoadScene(_mainSceneName);
    }

    private void RunAction(TutorialAction action)
    {
        switch (action)
        {
            case TutorialAction.SetupBlue123Blue6Hand:
                SetupBlue123Blue6Hand();
                break;
            case TutorialAction.AutoPlaceBlue123AndBlue6:
                AutoPlaceBlue123AndBlue6();
                break;
            case TutorialAction.SetupBlue66Green4Hand:
                SetupBlue66Green4Hand();
                break;
            case TutorialAction.AutoPlaceBlueTripleAndRed4:
                AutoPlaceBlueTripleAndRed4();
                break;
            case TutorialAction.AutoPlaceRed5Red6:
                AutoPlaceRed5Red6();
                break;
                case TutorialAction.ShowYakuPartialFlush:
                SetYakuGuide("편일문",
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Sword, 7), new CardRef(CardType.Sword, 7), new CardRef(CardType.Sword, 7),
                    new CardRef(CardType.Kunai, 4), new CardRef(CardType.Kunai, 5), new CardRef(CardType.Kunai, 6));
                break;

            case TutorialAction.ShowYakuFullFlush:
                SetYakuGuide("일문오의",
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Sword, 4), new CardRef(CardType.Sword, 5), new CardRef(CardType.Sword, 6),
                    new CardRef(CardType.Sword, 9), new CardRef(CardType.Sword, 9), new CardRef(CardType.Sword, 9));
                break;

            case TutorialAction.ShowYakuDoubleSequence:
                SetYakuGuide("이연격",
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Kunai, 8), new CardRef(CardType.Kunai, 8), new CardRef(CardType.Kunai, 8));
                break;

            case TutorialAction.ShowYakuTripleSequence:
                SetYakuGuide("삼연극의",
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3));
                break;

            case TutorialAction.ShowYakuSanshokuDojun:
                SetYakuGuide("삼문연격",
                    new CardRef(CardType.Sword, 4), new CardRef(CardType.Sword, 5), new CardRef(CardType.Sword, 6),
                    new CardRef(CardType.Kunai, 4), new CardRef(CardType.Kunai, 5), new CardRef(CardType.Kunai, 6),
                    new CardRef(CardType.FoxSpirit, 4), new CardRef(CardType.FoxSpirit, 5), new CardRef(CardType.FoxSpirit, 6));
                break;

            case TutorialAction.ShowYakuStraightOneToNine:
                SetYakuGuide("구단일섬",
                    new CardRef(CardType.Sword, 1), new CardRef(CardType.Sword, 2), new CardRef(CardType.Sword, 3),
                    new CardRef(CardType.Kunai, 4), new CardRef(CardType.Kunai, 5), new CardRef(CardType.Kunai, 6),
                    new CardRef(CardType.FoxSpirit, 7), new CardRef(CardType.FoxSpirit, 8), new CardRef(CardType.FoxSpirit, 9));
                break;

            case TutorialAction.ShowYakuDoubleTriple:
                SetYakuGuide("이중타",
                    new CardRef(CardType.Sword, 7), new CardRef(CardType.Sword, 7), new CardRef(CardType.Sword, 7),
                    new CardRef(CardType.Kunai, 1), new CardRef(CardType.Kunai, 1), new CardRef(CardType.Kunai, 1),
                    new CardRef(CardType.FoxSpirit, 4), new CardRef(CardType.FoxSpirit, 5), new CardRef(CardType.FoxSpirit, 6));
                break;

            case TutorialAction.ShowYakuAllTriples:
                SetYakuGuide("전면집결",
                    new CardRef(CardType.Sword, 4), new CardRef(CardType.Sword, 4), new CardRef(CardType.Sword, 4),
                    new CardRef(CardType.Kunai, 8), new CardRef(CardType.Kunai, 8), new CardRef(CardType.Kunai, 8),
                    new CardRef(CardType.FoxSpirit, 3), new CardRef(CardType.FoxSpirit, 3), new CardRef(CardType.FoxSpirit, 3));
                break;

            case TutorialAction.ShowYakuSanshokuDoukou:
                SetYakuGuide("삼문집결",
                    new CardRef(CardType.Sword, 7), new CardRef(CardType.Sword, 7), new CardRef(CardType.Sword, 7),
                    new CardRef(CardType.Kunai, 7), new CardRef(CardType.Kunai, 7), new CardRef(CardType.Kunai, 7),
                    new CardRef(CardType.FoxSpirit, 7), new CardRef(CardType.FoxSpirit, 7), new CardRef(CardType.FoxSpirit, 7));
                break;

            case TutorialAction.HideYakuGuide:
                HideYakuGuide();
                break;

        }
    }
    private void SetYakuGuide(string yakuName, params CardRef[] cardRefs)
    {
        _yakuGuideRoot.SetActive(true);
        _yakuGuideTitleText.text = yakuName;

        for (int i = 0; i < _yakuGuideCardViews.Count; i++)
        {
            CardView cardView = _yakuGuideCardViews[i];

            if (i < cardRefs.Length)
            {
                cardView.gameObject.SetActive(true);
                cardView.SetCardData(GetCard(cardRefs[i].Type, cardRefs[i].Number));
            }
            else
            {
                cardView.gameObject.SetActive(false);
            }
        }
    }

    private void HideYakuGuide()
    {
        _yakuGuideRoot.SetActive(false);
    }


    private void SetupBlue123Blue6Hand()
    {
        ClearAllZones();
        SetHand(
            new CardRef(CardType.FoxSpirit, 1),
            new CardRef(CardType.FoxSpirit, 2),
            new CardRef(CardType.FoxSpirit, 3),
            new CardRef(CardType.FoxSpirit, 6));
    }

    private void AutoPlaceBlue123AndBlue6()
    {
        ClearAllZones();

        _squadZones[0].TryRegisterCard(GetCard(CardType.FoxSpirit, 1));
        _squadZones[0].TryRegisterCard(GetCard(CardType.FoxSpirit, 2));
        _squadZones[0].TryRegisterCard(GetCard(CardType.FoxSpirit, 3));

        _tempStorageController.TryStoreCard(GetCard(CardType.FoxSpirit, 6));
        SetHand();
    }

    private void SetupBlue66Green4Hand()
    {
        SetHand(
            new CardRef(CardType.FoxSpirit, 6),
            new CardRef(CardType.FoxSpirit, 6),
            new CardRef(CardType.Kunai, 4));
    }

    private void AutoPlaceBlueTripleAndRed4()
    {
        _tempStorageController.TryRegisterCardToSquad(0, _squadZones[1]);
        _squadZones[1].TryRegisterCard(GetCard(CardType.FoxSpirit, 6));
        _squadZones[1].TryRegisterCard(GetCard(CardType.FoxSpirit, 6));

        _squadZones[2].TryRegisterCard(GetCard(CardType.Sword, 4));
        SetHand();
    }

    private void AutoPlaceRed5Red6()
    {
        _squadZones[2].TryRegisterCard(GetCard(CardType.Sword, 5));
        _squadZones[2].TryRegisterCard(GetCard(CardType.Sword, 6));
        SetHand();
    }

    private void ClearAllZones()
    {
        for (int i = 0; i < _squadZones.Count; i++)
        {
            _squadZones[i].ClearRegisteredCardsForRestart();
        }

        _tempStorageController.ClearStoredCardsForRestart();
    }

    private void SetHand(params CardRef[] cardRefs)
    {
        List<CardData> cards = new List<CardData>(cardRefs.Length);

        for (int i = 0; i < cardRefs.Length; i++)
        {
            cards.Add(GetCard(cardRefs[i].Type, cardRefs[i].Number));
        }

        _deckController.ReplaceHandForDebug(cards);
    }

    private CardData GetCard(CardType cardType, int number)
    {
        _deckController.TryGetCardDefinition(cardType, number, out CardData cardData);
        return cardData;
    }

    private void BuildDefaultSteps()
    {
        _steps = new List<TutorialStep>
        {
            new TutorialStep("안녕! 난 허수아비야."),
            new TutorialStep("오늘은 네게 기본\n비급을 알려줄거야."),
            new TutorialStep("각각의 스쿼드에는\n같은색의 숫자들만 들어가!"),
            new TutorialStep("아래 카드 3장을 넣어서\n연속되는 숫자를 만들 수 있어!", TutorialAction.SetupBlue123Blue6Hand),
            new TutorialStep("오른쪽 임시저장소에는,\n두개의 카드를 넣을 수 있어.", TutorialAction.None, TutorialAction.AutoPlaceBlue123AndBlue6),
            new TutorialStep("이건 일반콤보야!\n1000점을 줘."),
            new TutorialStep("한번 공격하기 전에\n리롤을 세번 돌릴 수 있어!", TutorialAction.SetupBlue66Green4Hand),
            new TutorialStep("임시 저장소에 있는걸 스쿼드로\n가져다가끌어서 등록할 수 있어!"),
            new TutorialStep("임시 저장소에 있는걸\n허공에 던지면 사라져.", TutorialAction.None, TutorialAction.AutoPlaceBlueTripleAndRed4),
            new TutorialStep("이건 강화콤보야!\n1500점을 줘."),
            new TutorialStep("나머지 칸을 채울게!", TutorialAction.None, TutorialAction.AutoPlaceRed5Red6),
            new TutorialStep("공격키를 눌러 적을 공격하면 돼!"),
            new TutorialStep("각 콤보의 수치 합과 시너지를 곱해\n적에게 데미지를 입혀!"),
            new TutorialStep("적은 한대를 맞고\n바로 죽지 않으면 반격해."),
            new TutorialStep("시너지를 간단하게 설명해줄게.", TutorialAction.HideYakuGuide),
            new TutorialStep("같은색 두 블럭인 편일문이 있어.", TutorialAction.ShowYakuPartialFlush),
            new TutorialStep("모두 같은색 블럭인 일문오의가 있어.", TutorialAction.ShowYakuFullFlush),
            new TutorialStep("같은색 연속블럭 두개인 이연격이 있어.", TutorialAction.ShowYakuDoubleSequence),
            new TutorialStep("같은색 연속블럭 세개인 삼연극의가 있어.", TutorialAction.ShowYakuTripleSequence),
            new TutorialStep("모두 다른색 연속블럭인 삼문연격이 있어.", TutorialAction.ShowYakuSanshokuDojun),
            new TutorialStep("1~9 타일을 모두 사용하는 구단일섬이 있어.", TutorialAction.ShowYakuStraightOneToNine),
            new TutorialStep("트리플 블럭을 두쌍 만들면 이중타야.", TutorialAction.ShowYakuDoubleTriple),
            new TutorialStep("트리플 블럭을 세쌍 만들면 전면집결이야.", TutorialAction.ShowYakuAllTriples),
            new TutorialStep("트리플 블럭이 모두 같은 숫자면 삼문집결이야!", TutorialAction.ShowYakuSanshokuDoukou),
            new TutorialStep("위 족보들은 왼쪽위 기술일람을 클릭하면 확인할 수 있어.", TutorialAction.HideYakuGuide),
            new TutorialStep("그럼 즐거운 수련되길 바래!")

        };
    }
}
