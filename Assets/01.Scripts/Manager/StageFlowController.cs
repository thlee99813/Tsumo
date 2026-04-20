using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class StageFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngameController _ingameController;
    [SerializeField] private DeckController _deckController;
    [SerializeField] private BattleController _battleController;
    [SerializeField] private UIController _uiController;
    [SerializeField] private DoraController _doraController;
    [SerializeField] private TempStorageController _tempStorageController;


    [SerializeField] private Player _player;
    [SerializeField] private ComboSynergyJudge _comboSynergyJudge;
    [SerializeField] private YakuEpicMomentController _yakuEpicMomentController;
    [SerializeField] private SynergyTierEffect _synergyTierEffect;


    [Header("Enemy Spawn")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private Transform _enemySpawnPoint;
    [SerializeField] private Transform _enemyParent;

    [Header("Stage")]
    [SerializeField] private int _currentStageIndex = 0;

    private readonly string[] _stageCodes = { "1-1", "1-2", "1-3", "2-1", "2-2", "2-3" };
    [SerializeField] private int _stage1BossIndex = 2;      // 1-3
    [SerializeField] private int _stage2StartIndex = 3;     // 2-1
    [SerializeField] private int _bossTempUsableSlotCount = 1;
    [SerializeField] private int _stage2BossIndex = 5; // 2-3


    private int _checkpointStageIndex = 0;
    private bool _isRunning;
    private bool _enemyDefeatedThisTurn;
    private Enemy _currentEnemy;

    private bool _isBossIntroPending;


    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        _ingameController.OnPlayerDead += HandlePlayerDead;
        _ingameController.OnEnemyAppear += HandleEnemyAppear;
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
            Debug.LogWarning("[StageFlowController] Enemy Prefab이 할당되지 않았습니다. 기존 씬 Enemy를 재사용합니다.");
        }

        _battleController.SetEnemy(_currentEnemy);
        _ingameController.SetEnemy(_currentEnemy);
       _player.SetPopupTarget(_currentEnemy.HeadPoint);
        ApplyEnemyStatsForCurrentStage();

        _uiController.HideGameOver();
        _uiController.SetStageText(_stageCodes[_currentStageIndex]);

        _doraController.ApplyStage(_currentStageIndex);
        ApplyStageSpecialRule();
        _isRunning = true;
        StartCoroutine(StageTurnLoop());
    }

    private void OnDestroy()
    {
        if (_ingameController != null)
        {
            _ingameController.OnPlayerDead -= HandlePlayerDead;
            _ingameController.OnEnemyAppear -= HandleEnemyAppear;
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
        if (_ingameController == null
            || _deckController == null
            || _battleController == null
            || _uiController == null
            || _player == null
            || _doraController == null
            || _tempStorageController == null
            || _yakuEpicMomentController == null)
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
                    List<ComboSynergyJudge.SquadJudgement> judgements = _comboSynergyJudge.Evaluate(fireExecutionData.ScoreResult);
                    _player.PrepareComboOverrides(judgements);
                }
            }

            bool hasCardAttack = fireExecutionData.IsFirePressed
                && fireExecutionData.ScoreResult != null
                && fireExecutionData.ScoreResult.SquadResults.Exists(s => s.IsValid);

            if (hasCardAttack)
            {
                _player.ClearCombo();
                foreach (SquadScoreResult squad in fireExecutionData.ScoreResult.SquadResults)
                {
                    if (squad.IsValid)
                        _player.AddAttack(CardTypeToAttackType(squad.CardType));
                }
            }

            int enemyHpBeforeBattle = _currentEnemy.CurrentHp;
            int playerHpBeforeBattle = _player.CurrentHp;

            if (fireExecutionData.IsFirePressed)
            {
                yield return _yakuEpicMomentController.PlayMatched(fireExecutionData.ScoreResult);

                if (!_isRunning || !_ingameController.IsRunning) yield break;

                if (_synergyTierEffect != null && fireExecutionData.ScoreResult != null)
                    yield return _synergyTierEffect.Play(fireExecutionData.ScoreResult.FinalMultiplier);

                if (!_isRunning || !_ingameController.IsRunning) yield break;
            }


            yield return _ingameController.RunFireProcessPhase(fireExecutionData);

            if (!_isRunning || !_ingameController.IsRunning)
            {
                yield break;
            }

            int enemyRemainHp = _currentEnemy.CurrentHp;
            int playerRemainHp = _player.CurrentHp;
            int enemyDamageToPlayer = Mathf.Max(0, playerHpBeforeBattle - playerRemainHp);


            if (!_isRunning || !_ingameController.IsRunning)
            {
                yield break;
            }

            _deckController.CompleteTurnAfterFire();

            if (fireExecutionData.IsFirePressed)
            {
                _doraController.RollAfterFire();
            }


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

        _doraController.ApplyStage(_currentStageIndex);
        ApplyStageSpecialRule();

        return true;
    }

    private void HandlePlayerDead()
    {
        StopFlow();

        _checkpointStageIndex = GetCheckpointIndex(_currentStageIndex);
        _uiController.ShowGameOver(_stageCodes[_checkpointStageIndex]);
    }
    private void HandleEnemyAppear()
    {
        if (_currentStageIndex != _stage1BossIndex) return;
        if (!_isBossIntroPending) return;

        _currentEnemy.PlayAttackAnimation();
        _isBossIntroPending = false;
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
        _uiController.SetStageText(_stageCodes[_currentStageIndex]);

        _doraController.ApplyStage(_currentStageIndex);
        ApplyStageSpecialRule();

        _isRunning = true;
        StartCoroutine(StageTurnLoop());
    }

    private void SpawnNewEnemy()
    {
        if (_enemyPrefab == null)
        {
            _currentEnemy.ResetEnemy();
            ApplyEnemyStatsForCurrentStage();
            _battleController.SetEnemy(_currentEnemy);
            _ingameController.SetEnemy(_currentEnemy);
            _player.SetPopupTarget(_currentEnemy.HeadPoint);
            return;
        }

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
    private void ApplyStageSpecialRule()
    {
        if (_currentStageIndex == _stage1BossIndex)
        {
            _isBossIntroPending = true;
            _tempStorageController.SetUsableSlotCount(_bossTempUsableSlotCount, true);
            return;
        }

        if (_currentStageIndex == _stage2BossIndex)
        {
            _isBossIntroPending = false;
            _tempStorageController.SetUsableSlotCount(_bossTempUsableSlotCount, true);
            return;
        }

        _isBossIntroPending = false;
        _tempStorageController.RestoreDefaultSlotCount();
    }


}
