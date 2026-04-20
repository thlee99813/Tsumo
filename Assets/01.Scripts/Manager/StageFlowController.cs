using System.Collections;
using UnityEngine;

public class StageFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngameController _ingameController;
    [SerializeField] private DeckController _deckController;
    [SerializeField] private BattleController _battleController;
    [SerializeField] private UIController _uiController;
    [SerializeField] private BattleResultPresenter _battleResultPresenter;
    [SerializeField] private Player _player;
    [SerializeField] private ComboSynergyJudge _comboSynergyJudge;

    [Header("Enemy Spawn")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private Transform _enemySpawnPoint;
    [SerializeField] private Transform _enemyParent;

    [Header("Stage")]
    [SerializeField] private int _currentStageIndex = 0;

    private readonly string[] _stageCodes = { "1-1", "1-2", "1-3", "2-1", "2-2", "2-3" };

    private int _checkpointStageIndex = 0;
    private bool _isRunning;
    private bool _enemyDefeatedThisTurn;
    private Enemy _currentEnemy;

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        _ingameController.OnPlayerDead += HandlePlayerDead;
        _battleController.OnEnemyDead += HandleEnemyDead;
        _uiController.OnRestartClicked += HandleRestartClicked;

        _currentStageIndex = Mathf.Clamp(_currentStageIndex, 0, _stageCodes.Length - 1);

        _currentEnemy = _ingameController.CurrentEnemy;
        if (_currentEnemy == null)
        {
            Debug.LogError("[StageFlowController] IngameController Enemy 참조가 비어 있습니다.");
            enabled = false;
            return;
        }

        if (_enemyPrefab == null)
        {
            _enemyPrefab = _currentEnemy;
        }

        _battleController.SetEnemy(_currentEnemy);
        _ingameController.SetEnemy(_currentEnemy);
       _player.SetPopupTarget(_currentEnemy.HeadPoint);
        ApplyEnemyStatsForCurrentStage();

        _uiController.HideGameOver();
        _uiController.HideBattleResultPanelImmediate();
        _uiController.SetStageText(_stageCodes[_currentStageIndex]);


        _isRunning = true;
        StartCoroutine(StageTurnLoop());
    }

    private void OnDestroy()
    {
        if (_ingameController != null)
        {
            _ingameController.OnPlayerDead -= HandlePlayerDead;
        }

        if (_battleController != null)
        {
            _battleController.OnEnemyDead -= HandleEnemyDead;
        }

        if (_uiController != null)
        {
            _uiController.OnRestartClicked -= HandleRestartClicked;
        }
    }

    private bool ValidateReferences()
    {
        if (_ingameController == null || _deckController == null || _battleController == null || _uiController == null || _battleResultPresenter == null || _player == null)
        {
            Debug.LogError("필수참조해제");
            return false;
        }

        return true;
    }

    public void StopFlow()
    {
        _isRunning = false;
        StopAllCoroutines();
        Time.timeScale = 1f;
    }

    private IEnumerator StageTurnLoop()
    {
        while (_isRunning && _ingameController.IsRunning)
        {
            _enemyDefeatedThisTurn = false;

            yield return _ingameController.RunIdlePhase();
            if (!_isRunning || !_ingameController.IsRunning)
            {
                yield break;
            }

            _ingameController.BeginDeckBuildPhase();

            while (_isRunning && _ingameController.IsRunning && !_ingameController.TickDeckBuildPhase())
            {
                yield return null;
            }


            bool isFirePressed = _ingameController.ConsumeFireRequest();
            FireExecutionData fireExecutionData = _deckController.BuildFireExecutionData(isFirePressed);

            if (isFirePressed && fireExecutionData.ScoreResult != null)
            {
                if (_comboSynergyJudge == null)
                {
                    Debug.LogWarning("[StageFlowController] ComboSynergyJudge 참조 없음 — Inspector 확인 필요");
                }
                else
                {
                    var judgements = _comboSynergyJudge.Evaluate(fireExecutionData.ScoreResult);
                    _player.PrepareComboOverrides(judgements);
                }
            }

            bool hasCardAttack = fireExecutionData.IsFirePressed
                && fireExecutionData.ScoreResult != null
                && fireExecutionData.ScoreResult.SquadResults.Exists(s => s.IsValid);

            if (hasCardAttack)
            {
                _player.ClearCombo();
                foreach (var squad in fireExecutionData.ScoreResult.SquadResults)
                {
                    if (squad.IsValid)
                        _player.AddAttack(CardTypeToAttackType(squad.CardType));
                }
            }

            int enemyHpBeforeBattle = _currentEnemy.CurrentHp;
            int playerHpBeforeBattle = _player.CurrentHp;

            yield return _ingameController.RunFireProcessPhase(fireExecutionData);

            if (!_isRunning || !_ingameController.IsRunning)
            {
                yield break;
            }

            int enemyRemainHp = _currentEnemy.CurrentHp;
            int playerRemainHp = _player.CurrentHp;
            int enemyDamageToPlayer = Mathf.Max(0, playerHpBeforeBattle - playerRemainHp);

            yield return _battleResultPresenter.PlayBattleResultPanel(fireExecutionData, enemyHpBeforeBattle, enemyRemainHp, playerHpBeforeBattle, enemyDamageToPlayer, playerRemainHp);


            if (!_isRunning || !_ingameController.IsRunning)
            {
                yield break;
            }

            _deckController.CompleteTurnAfterFire();

                yield return _ingameController.RunTurnResultPhase();
                if (!_isRunning || !_ingameController.IsRunning)
                {
                    yield break;
                }

                if (_enemyDefeatedThisTurn)
                {
                    if (!MoveToNextStage())
                    {
                        yield break;
                    }
                }
            }
    }

    private void HandleEnemyDead()
    {
        _enemyDefeatedThisTurn = true;
    }

    private bool MoveToNextStage()
    {
        _currentStageIndex++;

        if (_currentStageIndex >= _stageCodes.Length)
        {
            StopFlow();
            Debug.Log("[StageFlowController] All Stage Clear");
            return false;
        }

        if (_currentStageIndex >= 3)
        {
            _checkpointStageIndex = 3;
        }

        SpawnNewEnemy();
        _deckController.BuildDeckAndDrawHand();
        _ingameController.ResetForRespawn();
        _uiController.SetStageText(_stageCodes[_currentStageIndex]);

        return true;
    }

    private void HandlePlayerDead()
    {
        StopFlow();

        _checkpointStageIndex = GetCheckpointIndex(_currentStageIndex);
        _uiController.HideBattleResultPanelImmediate();
        _uiController.ShowGameOver(_stageCodes[_checkpointStageIndex]);
    }

    private int GetCheckpointIndex(int stageIndex)
    {
        return stageIndex < 3 ? 0 : 3;
    }

    private AttackType CardTypeToAttackType(CardType cardType) => cardType switch
    {
        CardType.Sword     => AttackType.Sword,
        CardType.Kunai     => AttackType.Shuriken,
        CardType.FoxSpirit => AttackType.Spell,
        _                  => AttackType.Sword
    };

    private void HandleRestartClicked()
    {
        StopFlow();
        Time.timeScale = 1f;

        _currentStageIndex = _checkpointStageIndex;

        SpawnNewEnemy();
        RunRespawn(_player, _currentEnemy);
        _deckController.BuildDeckAndDrawHand();
        _ingameController.ResetForRespawn();

        _uiController.HideGameOver();
        _uiController.HideBattleResultPanelImmediate();
        _uiController.SetStageText(_stageCodes[_currentStageIndex]);


        _isRunning = true;
        StartCoroutine(StageTurnLoop());
    }

    private void SpawnNewEnemy()
    {
        Vector3 spawnPosition = _enemySpawnPoint != null ? _enemySpawnPoint.position : _currentEnemy.transform.position;
        Quaternion spawnRotation = _enemySpawnPoint != null ? _enemySpawnPoint.rotation : _currentEnemy.transform.rotation;
        Transform spawnParent = _enemyParent != null ? _enemyParent : _currentEnemy.transform.parent;

        if (_currentEnemy != null)
        {
            Destroy(_currentEnemy.gameObject);
        }

        _currentEnemy = Instantiate(_enemyPrefab, spawnPosition, spawnRotation, spawnParent);

        _battleController.SetEnemy(_currentEnemy);
        _ingameController.SetEnemy(_currentEnemy);
        _player.SetPopupTarget(_currentEnemy.HeadPoint);
        ApplyEnemyStatsForCurrentStage();

    }

    private void ApplyEnemyStatsForCurrentStage()
    {
        _currentEnemy.ApplyStageStats(_currentStageIndex, true);
    }


    public void RunRespawn(Player player, Enemy enemy)
    {
        player.ResetHp();
        enemy.ResetEnemy();
        
    }
}
