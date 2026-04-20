using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum TurnPhase
{
    Idle,
    EnemyAppear,
    DeckBuild,
    FireProcess,
    TurnResult
}

public class IngameController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _deckBuildTimeLimit = 30f;
    [SerializeField] private float _slowMotionScale = 0.1f;
    [SerializeField] private InfiniteBackGround _backGround;

    [Header("References")]
    [SerializeField] private Slider _timerGauge;
    [SerializeField] private BattleController _battleController;
    [SerializeField] private Player _player;
    [SerializeField] private Enemy _enemy;

    [SerializeField] private TurnPhase _currentPhase;
    private float _timer;
    private bool _isFirePressed;
    private bool _isRunning;
    private bool _isSlowMotion;
    private bool _isEnemyReady;

    public event Action OnEnemyAppear;
    public event Action<Enemy> OnEnemyChanged;
    public event Action OnFireExecuted;
    public event Action OnTurnEnd;
    public event Action OnPlayerDead;

    public bool IsRunning => _isRunning;
    public TurnPhase CurrentPhase => _currentPhase;
    public bool IsDeckBuildPhase => _currentPhase == TurnPhase.DeckBuild;
    public Enemy CurrentEnemy => _enemy;

    private void Awake()
    {
        _currentPhase = TurnPhase.Idle;
        _isRunning = true;
        _isFirePressed = false;
        _isEnemyReady = false;
    }

    private void Start()
    {
        _player.OnPlayerDead += HandlePlayerDead;
        SetEnemy(_enemy);
        UpdateTimerUI(0f);
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.OnPlayerDead -= HandlePlayerDead;
        }

        if (_enemy != null)
        {
            _enemy.OnEnemyReady -= HandleEnemyReady;
        }
    }

    private void HandleEnemyReady()
    {
        _isEnemyReady = true;
    }

    public void HandleFireInput()
    {
        if (_currentPhase != TurnPhase.DeckBuild) return;

        _isFirePressed = true;
        ExitSlowMotion();
    }

    private void HandlePlayerDead()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        ExitSlowMotion();
        StopAllCoroutines();
        OnPlayerDead?.Invoke();
    }

    private void EnterSlowMotion()
    {
        if (_isSlowMotion) return;
        _isSlowMotion = true;
        _backGround.SetSpeedMultiplier(0f);
        _enemy.PauseMovement();
        _enemy.SetIdleFps(_slowMotionScale);
    }

    private void ExitSlowMotion()
    {
        if (!_isSlowMotion) return;
        _isSlowMotion = false;
        _enemy?.SetIdleFps(1f);
    }

    public IEnumerator RunIdlePhase()
    {
        _currentPhase = TurnPhase.Idle;
        _isEnemyReady = false;

        yield return new WaitUntil(() => _isEnemyReady || !_isRunning);
        if (!_isRunning) yield break;

        _currentPhase = TurnPhase.EnemyAppear;
        EnterSlowMotion();
        OnEnemyAppear?.Invoke();

        _player.MoveToEnemy(_enemy.transform.position.x, _deckBuildTimeLimit, _slowMotionScale);
    }

    public void BeginDeckBuildPhase()
    {
        _currentPhase = TurnPhase.DeckBuild;
        _isFirePressed = false;
        _timer = _deckBuildTimeLimit;
        UpdateTimerUI(1f);
    }

    public bool TickDeckBuildPhase()
    {
        if (_currentPhase != TurnPhase.DeckBuild)
        {
            return true;
        }

        if (_isFirePressed)
        {
            UpdateTimerUI(0f);
            return true;
        }

        _timer -= Time.unscaledDeltaTime;
        float normalized = Mathf.Clamp01(_timer / _deckBuildTimeLimit);
        UpdateTimerUI(normalized);

        return _timer <= 0f;
    }

    public bool ConsumeFireRequest()
    {
        bool requested = _isFirePressed;
        _isFirePressed = false;
        return requested;
    }

    public IEnumerator RunFireProcessPhase(FireExecutionData fireData)
    {
        _currentPhase = TurnPhase.FireProcess;
        _player.StopMovement();

        if (fireData.IsFirePressed)
        {
            ExitSlowMotion();

            var squadScores = new System.Collections.Generic.List<int>();
            if (fireData.ScoreResult != null)
            {
                foreach (var squad in fireData.ScoreResult.SquadResults)
                    if (squad.IsValid) squadScores.Add(squad.BaseScore);
            }

            _player.ExecuteCombo(_enemy.transform.position.x, squadScores, fireData.FinalDamage, fireData.ScoreResult);
            yield return new WaitUntil(() => !_player.IsAttacking || !_isRunning);
            if (!_isRunning) yield break;

            _battleController.ExecuteBattle(fireData.FinalDamage);
        }
        else
        {
            ExitSlowMotion();
            _player.PlayStopThenBattle();

            yield return new WaitUntil(() =>
                !_player.IsRunStopping && !_player.IsTeleporting || !_isRunning);

            if (!_isRunning) yield break;
            _battleController.ExecuteBattle(0);
        }

        OnFireExecuted?.Invoke();
    }

    public IEnumerator RunTurnResultPhase()
    {
        _backGround.SetSpeedMultiplier(1f);
        _currentPhase = TurnPhase.TurnResult;
        yield return new WaitUntil(() => !_player.IsKnockBack || !_isRunning);

        if (!_isRunning) yield break;

        OnTurnEnd?.Invoke();
    }

    public void SetEnemy(Enemy enemy)
    {
        if (_enemy != null)
        {
            _enemy.OnEnemyReady -= HandleEnemyReady;
        }

        _enemy = enemy;
        _isEnemyReady = false;

        if (_enemy != null)
        {
            _enemy.OnEnemyReady += HandleEnemyReady;
        }

        OnEnemyChanged?.Invoke(_enemy);
    }

    public void ResetForRespawn()
    {
        _currentPhase = TurnPhase.Idle;
        _isFirePressed = false;
        _isEnemyReady = false;
        _isRunning = true;
        ExitSlowMotion();
        UpdateTimerUI(0f);
        _backGround.SetSpeedMultiplier(1f);
    }

    private void UpdateTimerUI(float normalizedValue)
    {
        if (_timerGauge != null)
            _timerGauge.value = normalizedValue;
    }
}