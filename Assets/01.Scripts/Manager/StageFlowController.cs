using System.Collections;
using UnityEngine;

public class StageFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngameController _ingameController;
    [SerializeField] private DeckController _deckController;
    [SerializeField] private BattleController _battleController;

    [Header("Settings")]
    [SerializeField] private float _turnResultDelay = 1f;

    private Coroutine _flowRoutine;
    private bool _isRunning;

    private void OnEnable()
    {
        StartFlow();
    }

    private void OnDisable()
    {
        StopFlow();
    }

    public void StartFlow()
    {
        if (_flowRoutine != null || _ingameController == null)
        {
            return;
        }

        _isRunning = true;
        _flowRoutine = StartCoroutine(RunFlow());
    }

    public void StopFlow()
    {
        _isRunning = false;

        if (_flowRoutine != null)
        {
            StopCoroutine(_flowRoutine);
            _flowRoutine = null;
        }

        if (_ingameController != null)
        {
            _ingameController.ForceExitSlowMotion();
        }
    }

    private IEnumerator RunFlow()
    {
        while (_isRunning && !_ingameController.IsPlayerDead)
        {
            _ingameController.EnterIdlePhase();
            yield return new WaitUntil(() => !_isRunning || _ingameController.IsPlayerDead || _ingameController.IsEnemyReady);
            if (!CanContinue()) yield break;

            _ingameController.EnterEnemyAppearPhase();

            _ingameController.EnterDeckBuildPhase();
            while (_isRunning && !_ingameController.IsPlayerDead && !_ingameController.IsDeckBuildFinished)
            {
                _ingameController.TickDeckBuildPhase(Time.unscaledDeltaTime);
                yield return null;
            }
            if (!CanContinue()) yield break;

            _ingameController.PrepareFirePhase();

            FireExecutionData fireData = BuildFireData();
            if (_battleController != null)
            {
                _battleController.ExecuteBattle(fireData.FinalDamage);
            }

            if (_deckController != null)
            {
                _deckController.CompleteTurnAfterFire();
            }

            _ingameController.CompleteFirePhase(fireData);

            _ingameController.EnterTurnResultPhase();
            yield return new WaitUntil(() => !_isRunning || _ingameController.IsPlayerDead || _ingameController.IsTurnResultFinished);
            if (!CanContinue()) yield break;

            _ingameController.NotifyTurnEnd();

            if (_turnResultDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(_turnResultDelay);
            }
        }

        _flowRoutine = null;
        if (_ingameController != null)
        {
            _ingameController.ForceExitSlowMotion();
        }
    }

    private FireExecutionData BuildFireData()
    {
        if (_deckController == null)
        {
            return new FireExecutionData
            {
                IsFirePressed = _ingameController != null && _ingameController.IsFirePressed,
                FinalDamage = 0,
                ScoreResult = null
            };
        }

        return _deckController.BuildFireExecutionData(_ingameController != null && _ingameController.IsFirePressed);
    }

    private bool CanContinue()
    {
        return _isRunning && _ingameController != null && !_ingameController.IsPlayerDead;
    }
}
