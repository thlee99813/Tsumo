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
    [SerializeField] private float _enemyAppearDelay = 3f;

    [Header("References")]
    [SerializeField] private Slider _timerGauge;
    [SerializeField] private BattleController _battleController;  //! 배틀컨트롤로
    [SerializeField] private Player _player;
    [SerializeField] private Enemy _enemy;

    private TurnPhase _currentPhase; 
    private float _timer;
    private bool _isFirePressed;
    private bool _isRunning;
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
        Time.timeScale = 1f;
    }

    private void HandlePlayerDead()
    {
        _isRunning = false;
        Time.timeScale = 1f;
        StopAllCoroutines();
        //TODO StageFlowCOntroller에 게임오버 통보
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
        
        yield return new WaitForSecondsRealtime(_enemyAppearDelay);

        //적 등장 즉시 슬로우 모션 시작
        _currentPhase = TurnPhase.EnemyAppear;
        Time.timeScale = _slowMotionScale;
        OnEnemyAppear?.Invoke();
        
        //플레이어가 적 위치까지 이동
        _player.MoveToEnemy(_enemy.transform.position.x);
    }

    private System.Collections.IEnumerator DeckBuildPhase()
    {
        _currentPhase = TurnPhase.DeckBuild;
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

        if (!_isFirePressed)
        {
            Time.timeScale = 1f;                
            _player.TakePenaltyDamage();        //피격 후 넉백으로 시작 자리 복귀
        }
        else
        {
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
        yield return new WaitUntil(() => !_player.IsKnockBack);
        OnTurnEnd?.Invoke();
        yield return new WaitForSeconds(1f);
    }

    private void UpdateTimerUI(float normalizedValue)
    {
        if (_timerGauge != null)
            _timerGauge.value = normalizedValue;
    }

    [ContextMenu("Debug_FireInput")]
    private void Debug_FireInput() => HandleFireInput();
}


