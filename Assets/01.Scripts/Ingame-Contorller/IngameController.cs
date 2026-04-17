using System;
using UnityEngine;
using UnityEngine.UI;

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

    private TurnPhase _currentPhase;
    private float _timer;
    private bool _isFirePressed;
    private bool _isRunning;
    private bool _isSlowMotion;
    public event Action OnEnemyAppear;
    public event Action OnFireExecuted;
    public event Action OnTurnEnd;

    private void Start()
    {
        _player.OnPlayerDead += HandlePlayerDead;
        _isRunning = true;
        StartCoroutine(TurnLoop());
    }

    private void OnDestroy()
    {
        _player.OnPlayerDead -= HandlePlayerDead;
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

    private System.Collections.IEnumerator TurnLoop()
    {
        while (_isRunning)
        {
            yield return StartCoroutine(IdlePhase());
            yield return StartCoroutine(DeckBuildPhase());
            yield return StartCoroutine(FireProcessPhase());
            yield return StartCoroutine(TurnResultPhase());
        }
    }
    

    private System.Collections.IEnumerator IdlePhase()
    {
        _currentPhase = TurnPhase.Idle;
        Debug.Log("[TurnPhase] Idle : 플레이어 이동 시작");

        yield return new WaitUntil(() => _player.DetectEnemy());

        //감지 즉시 슬로우 모션 ON + 이동 시작
        _currentPhase = TurnPhase.EnemyAppear;
        EnterSlowMotion();
        OnEnemyAppear?.Invoke();
        Debug.Log("[TurnPhase] EnemyAppear : 적 감지 - 슬로우모션 + 이동 시작");

        _player.MoveToEnemy(_enemy.transform.position.x, _deckBuildTimeLimit, _slowMotionScale);
    }

    private System.Collections.IEnumerator DeckBuildPhase()
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

    private System.Collections.IEnumerator FireProcessPhase()
    {
        _currentPhase = TurnPhase.FireProcess;
        _player.StopMovement();

        if (!_isFirePressed)
        {
            Debug.Log("[TurnPhase] FireProcess : 타임오버, 0 데미지 전투 처리");
            ExitSlowMotion();
            _battleController.ExecuteBattle(0);
        }
        else
        {
            Debug.Log("[TurnPhase] FireProcess : Fire 입력, 전투 시작");
            //! 무조건 ComboResolver 연결 후 실제 계산값으로 교체 해야함
            //! 임시값임
            int finalDamage = 10;
            _battleController.ExecuteBattle(finalDamage);  
        }

        OnFireExecuted?.Invoke();
        yield return null;
    }

    private System.Collections.IEnumerator TurnResultPhase()
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


