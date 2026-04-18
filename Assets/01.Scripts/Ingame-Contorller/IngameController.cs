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
    [SerializeField] private float _detectOffset = 0.2f;
    [SerializeField] private InfiniteBackGround _backGround;

    [Header("References")]
    [SerializeField] private Slider _timerGauge;
    [SerializeField] private BattleController _battleController;  
    [SerializeField] private Player _player;
    [SerializeField] private Enemy _enemy;

    private TurnPhase _currentPhase;
    private float _timer;
    private bool _isFirePressed;
    private bool _isRunning;
    private bool _isSlowMotion;
    private bool _isEnemyReady;
    private Camera _camera;
    private float _enemyHalfWidth;
    public event Action OnEnemyAppear;
    public event Action OnFireExecuted;
    public event Action OnTurnEnd;

    public bool IsRunning => _isRunning;

    private void Start()
    {
        _player.OnPlayerDead += HandlePlayerDead;
        _enemy.OnEnemyReady += HandleEnemyReady;
        _isRunning = true;
        UpdateTimerUI(0f);
    }
    private void OnDestroy()
    {
        _player.OnPlayerDead -= HandlePlayerDead;
        _enemy.OnEnemyReady -= HandleEnemyReady;
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
        _isRunning = false;
        ExitSlowMotion();
        StopAllCoroutines();
        //TODO StageFlowCOntroller에 게임오버 통보
    }

    private void EnterSlowMotion()
    {
        if (_isSlowMotion) return;
        _isSlowMotion = true;
        _backGround.SetSpeedMultiplier(_slowMotionScale);
        _enemy.PauseMovement();
    }

    private void ExitSlowMotion()
    {
        if (!_isSlowMotion) return;
        _isSlowMotion = false;
        _backGround.SetSpeedMultiplier(1f);
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

    public IEnumerator RunFireProcessPhase(int finalDamage, bool isFirePressed)
    {
        _currentPhase = TurnPhase.FireProcess;
        _player.StopMovement();

        if (isFirePressed)
        {
            ExitSlowMotion();
            _player.AttackMove(_enemy.transform.position.x);
            yield return new WaitUntil(() => !_player.IsAttacking || !_isRunning);

            if (!_isRunning) yield break;
            _battleController.ExecuteBattle(finalDamage);
        }
        else
        {
            ExitSlowMotion();
            _battleController.ExecuteBattle(0);
        }

        OnFireExecuted?.Invoke();
    }

    public IEnumerator RunTurnResultPhase()
    {
        _currentPhase = TurnPhase.TurnResult;
        yield return new WaitUntil(() => !_player.IsKnockBack || !_isRunning);

        if (!_isRunning) yield break;

        OnTurnEnd?.Invoke();
        yield return new WaitForSecondsRealtime(1f);
    }

    

    private IEnumerator IdlePhase()
    {
        _currentPhase = TurnPhase.Idle;
        Debug.Log("[TurnPhase] Idle : 플레이어 이동 시작");

        _isEnemyReady = false;
        yield return new WaitUntil(() => _isEnemyReady);

        //감지 즉시 슬로우 모션 ON + 이동 시작
        _currentPhase = TurnPhase.EnemyAppear;
        EnterSlowMotion();
        OnEnemyAppear?.Invoke();
        Debug.Log("[TurnPhase] EnemyAppear : 적 감지 - 슬로우모션 + 이동 시작");

        _player.MoveToEnemy(_enemy.transform.position.x, _deckBuildTimeLimit, _slowMotionScale);
    }

    private IEnumerator DeckBuildPhase()
    {
        _currentPhase = TurnPhase.DeckBuild;
        Debug.Log($"[TurnPhase] DeckBuild : {_deckBuildTimeLimit}초 타이머 시작");
        
        _isFirePressed = false;
        _timer = _deckBuildTimeLimit;

        while (_timer > 0f && !_isFirePressed)
        {
            _timer -= Time.unscaledDeltaTime;
            UpdateTimerUI(_timer / _deckBuildTimeLimit);
            yield return null;
        }

        UpdateTimerUI(0f);
    }

    private IEnumerator FireProcessPhase()
    {
        _currentPhase = TurnPhase.FireProcess;
        _player.StopMovement();

        if (_isFirePressed)
        {
            Debug.Log($"[TurnPhase] FireProcess : Fire 입력, {_player.AttackDamage} 데미지");
            //공격 이동 연출 후 전투 처리
            _player.AttackMove(_enemy.transform.position.x);
            yield return new WaitUntil(() => !_player.IsAttacking);
            
            
            _battleController.ExecuteBattle(_player.AttackDamage);  //!! 뎀지 계산 이후에 교체 예정

        }
        else
        {
            Debug.Log("[TurnPhase] FireProcess : 타임오버, 0 데미지 전투 처리");
            ExitSlowMotion();
            _battleController.ExecuteBattle(0); 
        }

        OnFireExecuted?.Invoke();
        yield return null;
    }

    private IEnumerator TurnResultPhase()
    {
        _currentPhase = TurnPhase.TurnResult;
        Debug.Log("[TurnPhase] TurnResult : 넉백 종료 대기 중");
        yield return new WaitUntil(() => !_player.IsKnockBack);


        Debug.Log("[TurnPhase] TurnResult : 턴 종료, 다음 턴 준비");
        OnTurnEnd?.Invoke();
        yield return new WaitForSecondsRealtime(1f); 
    }

    private void UpdateTimerUI(float normalizedValue)
    {
        if (_timerGauge != null)
            _timerGauge.value = normalizedValue;
    }

    [ContextMenu("Debug_FireInput")]
    private void Debug_FireInput() => HandleFireInput();
}


