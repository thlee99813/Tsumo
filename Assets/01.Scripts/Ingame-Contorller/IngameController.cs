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

    private void Start()
    {
        _camera = Camera.main;
        _enemyHalfWidth = _enemy.GetComponent<SpriteRenderer>().bounds.extents.x;
        _player.OnPlayerDead += HandlePlayerDead;
        _enemy.OnEnemyReady += HandleEnemyReady;
        _isRunning = true;
        StartCoroutine(TurnLoop());
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

    // 적 스프라이트 오른쪽 끝이 카메라 안으로 완전히 들어왔는지 체크
    private bool IsEnemyInView()
    {
        float enemyRightEdge = _enemy.transform.position.x + _enemyHalfWidth;
        Vector3 viewportPos = _camera.WorldToViewportPoint(new Vector3(enemyRightEdge, _enemy.transform.position.y, 0f));
        return viewportPos.x <= 1f - _detectOffset;
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

        _isEnemyReady = false;
        yield return new WaitUntil(() => _isEnemyReady);

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


