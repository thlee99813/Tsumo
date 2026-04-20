using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum AttackType
{
    Sword = 1,
    Shuriken = 2,
    Spell = 3
}

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerState _stats;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _stopOffset = 1.5f;
    [SerializeField] private float _runFps = 12f;
    [SerializeField] private float _shurikenOffset = 3f;

    [Header("Combat")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackMoveDuration = 0.2f;

    [Header("DamagePopup")]
    [SerializeField] private DamagePopupController _popupController;

    private PlayerAnimator _playerAnimator;
    private ComboSynergyEffect _comboSynergyEffect;
    private SynergyOverlayEffect _synergyOverlayEffect;
    private PlayerEffect _effectAnimator;

    private readonly List<Sprite[]> _attackOverridesList = new();
    private readonly List<Sprite[]> _effectOverridesList = new();
    private readonly HashSet<CardType> _synergyCardTypes = new();
    private readonly List<AttackType> _comboList = new();

    [Header("Test HP")]
    public int _currentHp;

    private Vector3 _startPosition;
    private bool _isKnockBack;
    private bool _isReached;
    private bool _isAttacking;
    private bool _attackAnimDone;
    private bool _isRunStopping;
    private float _currentAttackX = float.MinValue;
    private bool _isTeleporting;

    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;
    public bool IsKnockBack => _isKnockBack;
    public bool IsReached => _isReached;
    public int AttackDamage => _attackDamage;
    public bool IsAttacking => _isAttacking;
    public bool IsRunStopping => _isRunStopping;
    public IReadOnlyList<AttackType> ComboList => _comboList;
    public bool IsTeleporting => _isTeleporting;

    public event Action OnPlayerDead;
    public event Action<int> OnHpChanged;
    public event Action<AttackType> OnAttackAdded;
    public event Action OnHitFrame;
    public event Action OnEnemyAttack;

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
        _playerAnimator = GetComponent<PlayerAnimator>();
        _effectAnimator = GetComponentInChildren<PlayerEffect>();
        _comboSynergyEffect = GetComponentInChildren<ComboSynergyEffect>();
        _synergyOverlayEffect = GetComponentInChildren<SynergyOverlayEffect>();

        _playerAnimator.OnAnimationComplete += HandleAnimationComplete;
        _playerAnimator.OnHitFrame += HandleHitFrame;
    }

    private void OnDestroy()
    {
        if (_playerAnimator == null)
            return;

        _playerAnimator.OnAnimationComplete -= HandleAnimationComplete;
        _playerAnimator.OnHitFrame -= HandleHitFrame;
    }

    private void HandleAnimationComplete()
    {
        _attackAnimDone = true;
    }

    private void HandleHitFrame()
    {
        OnHitFrame?.Invoke();
    }

    public void PrepareComboOverrides(List<ComboSynergyJudge.SquadJudgement> judgements)
    {
        _attackOverridesList.Clear();
        _effectOverridesList.Clear();
        _synergyCardTypes.Clear();

        if (judgements == null)
            return;

        foreach (var judgement in judgements)
        {
            if (judgement.ResultType == SquadResultType.None)
                continue;

            Sprite[] attackOverride = null;
            Sprite[] effectOverride = null;

            if (_comboSynergyEffect != null)
            {
                Sprite[] attackSprites = _comboSynergyEffect.GetAttackSprites(judgement.ResultType, judgement.CardType);
                attackOverride = attackSprites != null && attackSprites.Length > 0 ? attackSprites : null;

                Sprite[] effectSprites = _comboSynergyEffect.GetEffectSprites(judgement.ResultType, judgement.CardType);
                effectOverride = effectSprites != null && effectSprites.Length > 0 ? effectSprites : null;
            }

            _attackOverridesList.Add(attackOverride);
            _effectOverridesList.Add(effectOverride);

            if (judgement.HasSynergy)
                _synergyCardTypes.Add(judgement.CardType);
        }
    }

    public void AddAttack(AttackType type)
    {
        _comboList.Add(type);
        OnAttackAdded?.Invoke(type);
    }

    public void ClearCombo()
    {
        _comboList.Clear();
    }

    #region 공격 콤보 처리, 애니메이션
    public void SetPopupTarget(Transform enemyTransform)
    {
        _popupController?.SetEnemyHeadPoint(enemyTransform);
    }

    public void ExecuteCombo(float targetX, List<int> squadScores, int finalScore, FireScoreResult scoreResult = null, Action onTeleport = null)
    {
        if (_comboList.Count == 0)
        {
            _isAttacking = true;
            StartCoroutine(EmptyComboCoroutine());
            return;
        }

        _isAttacking = true;
        StartCoroutine(ComboCoroutine(targetX, squadScores, finalScore, scoreResult, onTeleport));
    }

    private IEnumerator EmptyComboCoroutine()
    {
        TeleportToStart();
        yield return new WaitUntil(() => !_isTeleporting);
        _isAttacking = false;
    }

    private IEnumerator ComboCoroutine(float targetX, List<int> squadScores, int finalScore, FireScoreResult scoreResult = null, Action onTeleport = null)
    {
        for (int i = 0; i < _comboList.Count; i++)
        {
            AttackType type = _comboList[i];
            bool isLast = i == _comboList.Count - 1;

            Sprite[] attackOverride = i < _attackOverridesList.Count ? _attackOverridesList[i] : null;
            Sprite[] effectOverride = i < _effectOverridesList.Count ? _effectOverridesList[i] : null;

            yield return ExecuteSingleAttack(targetX, type, attackOverride, effectOverride);

            if (squadScores != null && i < squadScores.Count)
            {
                Debug.Log($"[Player] ShowBaseScore 호출 : {squadScores[i]}");
                _popupController?.ShowBaseScore(squadScores[i]);
            }

            if (!isLast)
                yield return new WaitForSecondsRealtime(0.1f);
        }

        _currentAttackX = float.MinValue;
        _attackOverridesList.Clear();
        _effectOverridesList.Clear();
        _synergyCardTypes.Clear();
        ClearCombo();

        onTeleport?.Invoke();
        TeleportToStart();

        yield return new WaitUntil(() => !_isTeleporting);

        if (finalScore > 0 && _popupController != null)
        {
            bool popupDone = false;
            _popupController.ShowFinalScore(finalScore, () => popupDone = true);
            yield return new WaitUntil(() => popupDone);
        }

        _playerAnimator.PlayRun(_runFps);
        _isAttacking = false;
    }

    private IEnumerator ExecuteSingleAttack(float targetX, AttackType type, Sprite[] attackOverride, Sprite[] effectOverride)
    {
        switch (type)
        {
            case AttackType.Sword:
                yield return MeleeAttack(targetX, type, attackOverride, effectOverride);
                break;

            case AttackType.Shuriken:
            case AttackType.Spell:
                yield return RangedAttack(targetX, type, attackOverride, effectOverride);
                break;
        }
    }

    private IEnumerator MeleeAttack(float targetX, AttackType type, Sprite[] attackOverride, Sprite[] effectOverride)
    {
        float stoppedX = targetX - _stopOffset;

        if (!Mathf.Approximately(_currentAttackX, stoppedX))
        {
            transform.DOMoveX(stoppedX, _attackMoveDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            yield return new WaitForSecondsRealtime(_attackMoveDuration);

            _isRunStopping = true;
            _playerAnimator.PlayRunStop(() => _isRunStopping = false);
            yield return new WaitUntil(() => !_isRunStopping);

            _currentAttackX = stoppedX;
        }

        _attackAnimDone = false;

        if (attackOverride != null)
            _playerAnimator.PlayCustomAttack(type, attackOverride, _comboSynergyEffect != null ? _comboSynergyEffect.Fps : 12f);
        else
            _playerAnimator.PlaySword();

        if (_synergyCardTypes.Contains(CardType.Sword) && _synergyOverlayEffect != null)
            _synergyOverlayEffect.PlaySynergy(CardType.Sword);

        PlayAttackEffect(type, effectOverride);
        _effectAnimator.PlayLeafParticle();

        
        yield return new WaitUntil(() => _attackAnimDone);
    }

    private IEnumerator RangedAttack(float targetX, AttackType type, Sprite[] attackOverride, Sprite[] effectOverride)
    {
        float rangedX = targetX - _stopOffset - _shurikenOffset;

        if (!Mathf.Approximately(_currentAttackX, rangedX))
        {
            transform.DOMoveX(rangedX, _attackMoveDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            yield return new WaitForSecondsRealtime(_attackMoveDuration);

            _isRunStopping = true;
            _playerAnimator.PlayRunStop(() => _isRunStopping = false);
            yield return new WaitUntil(() => !_isRunStopping);

            _currentAttackX = rangedX;
        }

        _attackAnimDone = false;

        if (attackOverride != null)
            _playerAnimator.PlayCustomAttack(type, attackOverride, _comboSynergyEffect != null ? _comboSynergyEffect.Fps : 12f);
        else if (type == AttackType.Shuriken)
            _playerAnimator.PlayShuriken();
        else
            _playerAnimator.PlaySpell();

        if (type == AttackType.Shuriken)
        {
            if (_synergyCardTypes.Contains(CardType.Kunai) && _synergyOverlayEffect != null)
                _synergyOverlayEffect.PlaySynergy(CardType.Kunai);
        }
        else if (type == AttackType.Spell)
        {
            if (_synergyCardTypes.Contains(CardType.FoxSpirit) && _synergyOverlayEffect != null)
                _synergyOverlayEffect.PlaySynergy(CardType.FoxSpirit);
        }

        PlayAttackEffect(type, effectOverride);
        _effectAnimator.PlayLeafParticle();

        yield return new WaitUntil(() => _attackAnimDone);
    }

    private void PlayAttackEffect(AttackType type, Sprite[] effectOverride)
    {
        if (_effectAnimator == null)
            return;

        float delay = _playerAnimator.GetEffectDelay(type);

        if (effectOverride != null)
        {
            StartCoroutine(PlayEffectDelayed(
                () => _effectAnimator.PlayCustomEffect(
                    effectOverride,
                    _comboSynergyEffect != null ? _comboSynergyEffect.EffectFps : 12f),
                delay));
            return;
        }

        switch (type)
        {
            case AttackType.Sword:
                StartCoroutine(PlayEffectDelayed(_effectAnimator.PlaySwordEffect, delay));
                break;

            case AttackType.Shuriken:
                StartCoroutine(PlayEffectDelayed(_effectAnimator.PlayShurikenEffect, delay));
                break;

            case AttackType.Spell:
                StartCoroutine(PlayEffectDelayed(_effectAnimator.PlaySpellEffect, delay));
                break;
        }
    }

    public void OnAttackAniComplete()
    {
        _attackAnimDone = true;
    }

    private IEnumerator PlayEffectDelayed(Action effectAction, float delay)
    {
        yield return new WaitForSeconds(delay);
        effectAction?.Invoke();
    }

    private void TeleportToStart()
    {
        OnEnemyAttack?.Invoke();
        _isTeleporting = true;

        _playerAnimator.HideSprite();
        transform.position = _startPosition;
        _playerAnimator.ShowSprite();

        _playerAnimator.PlayTeleport(() =>
        {
            _isTeleporting = false;
            _playerAnimator.PlayIdle();
        });
    }

    public void PlayStopThenBattle()
    {
        if (_isRunStopping || _isTeleporting)
            return;

        _isRunStopping = true;
        _playerAnimator.PlayRunStop(() =>
        {
            _isRunStopping = false;
            TeleportToStart();
        });
    }
    #endregion

    #region 이동 관련
    public void MoveToEnemy(float targetX, float unscaledTimeBudget, float slowFactor)
    {
        _isReached = false;
        transform.DOKill();
        _playerAnimator.PlayRun(_runFps * slowFactor);

        float stoppedX = targetX - _stopOffset;
        float distance = Mathf.Abs(transform.position.x - stoppedX);
        float slowSpeed = _moveSpeed * slowFactor;

        transform.DOMoveX(stoppedX, unscaledTimeBudget)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _isReached = true;
            });
    }

    public void StopMovement()
    {
        transform.DOKill();
        _isReached = false;
    }

    public void ReturnToStart()
    {
        _isReached = false;
        transform.DOMove(_startPosition, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    public void AttackMove(float targetX)
    {
        _isAttacking = true;
        transform.DOKill();

        float stoppedX = targetX - _stopOffset;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(transform.DOMoveX(stoppedX, 0.2f).SetEase(Ease.OutCubic));
        seq.Append(transform.DOMoveX(_startPosition.x, 0.4f).SetEase(Ease.OutCubic));
        seq.OnComplete(() =>
        {
            transform.position = new Vector3(_startPosition.x, transform.position.y, transform.position.z);
            _isAttacking = false;
        });
    }
    #endregion

    #region 전투 처리
    public void TakeDamage(int damage)
    {
        if (_isKnockBack)
            return;

        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage);
        OnHpChanged?.Invoke(_currentHp);

        PlayerKnockBack();
    }

    private void PlayerKnockBack()
    {
        if (_isKnockBack)
            return;

        _isKnockBack = true;
        transform.DOKill();
        StartCoroutine(KnockBackCoroutine());
    }

    private IEnumerator KnockBackCoroutine()
    {
        float returnDuration = 0.6f;
        float halfReturn = returnDuration * 0.5f;
        bool alreadyAtStart = (transform.position - _startPosition).sqrMagnitude < 0.001f;

        if (!alreadyAtStart)
        {
            transform.DOMoveX(_startPosition.x, returnDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(halfReturn);

        transform.DOKill();
        _isKnockBack = false;

        if (!alreadyAtStart)
            _playerAnimator.PlayRun();

        if (IsDead)
        {
            yield return null;
            OnPlayerDead?.Invoke();
        }
    }

    public void ResetHp()
    {
        _currentHp = _stats.MaxHp;
        OnHpChanged?.Invoke(_currentHp);
    }
    #endregion
}