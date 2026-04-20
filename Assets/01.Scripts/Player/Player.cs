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
    private Dictionary<AttackType, Sprite[]> _attackOverrides = new();
    private HashSet<CardType> _synergyCardTypes = new();

    [Header("Test HP")]
    public int _currentHp;
    private Vector3 _startPosition;
    private bool _isKnockBack;
    private bool _isReached;        //접근 중인가
    private bool _isAttacking;
    private bool _attackAnimDone;
    private bool _isRunStopping;
    private float _currentAttackX = float.MinValue;
    private bool _isTeleporting;
    private PlayerEffect _effectAnimator;
    
    
    private List<AttackType> _comboList = new List<AttackType>();

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

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
        _playerAnimator = GetComponent<PlayerAnimator>();
        _effectAnimator = GetComponentInChildren<PlayerEffect>();
        _comboSynergyEffect = GetComponent<ComboSynergyEffect>();
        _synergyOverlayEffect = GetComponentInChildren<SynergyOverlayEffect>();
        //공격 완료 이벤트 구독
        _playerAnimator.OnAnimationComplete += () => _attackAnimDone = true;
    }

    // StageFlowController에서 콤보 판정 후 호출
    public void PrepareComboOverrides(List<ComboSynergyJudge.SquadJudgement> judgements)
    {
        _attackOverrides.Clear();
        _synergyCardTypes.Clear();

        foreach (var j in judgements)
        {
            if (j.ResultType == SquadResultType.None) continue;

            // 강화 콤보 → 공격 애니메이션 스프라이트 교체
            if (_comboSynergyEffect != null)
            {
                Sprite[] sprites = _comboSynergyEffect.GetAttackSprites(j.ResultType, j.CardType);
                if (sprites != null && sprites.Length > 0)
                {
                    _attackOverrides[CardTypeToAttackType(j.CardType)] = sprites;
                    Debug.Log($"[Player] 강화 콤보 등록: {j.CardType} ({sprites.Length}프레임)");
                }
            }
            else
            {
                Debug.LogWarning("[Player] ComboSynergyEffect 없음 — Player 오브젝트에 붙여야 합니다.");
            }

            // 시너지 → 오버레이 이펙트 대상 카드 타입 기억
            if (j.HasSynergy)
            {
                _synergyCardTypes.Add(j.CardType);
                Debug.Log($"[Player] 시너지 등록: {j.CardType}");
                if (_synergyOverlayEffect == null)
                    Debug.LogWarning("[Player] SynergyOverlayEffect 없음 — Player 자식 오브젝트에 붙여야 합니다.");
            }
        }

        Debug.Log($"[Player] 준비 완료 — 강화콤보 {_attackOverrides.Count}개 / 시너지 {_synergyCardTypes.Count}개");
    }

    private static AttackType CardTypeToAttackType(CardType cardType) => cardType switch
    {
        CardType.Sword     => AttackType.Sword,
        CardType.Kunai     => AttackType.Shuriken,
        CardType.FoxSpirit => AttackType.Spell,
        _                  => AttackType.Sword
    };

//PlayerInputHandler에서 호출
    public void AddAttack(AttackType type)
    {
        _comboList.Add(type);
        //Debug.Log($"[Player] 공격 추가 : {type} (현재콤보 : {_comboList.Count})");
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

    //IngameController에서 호출 내부 콤보 리스트 사용
    public void ExecuteCombo(float targetX, List<int> squadScores, int finalScore, Action onTeleport = null)
    {
        if(_comboList.Count == 0)
        {
            _isAttacking = true;
            StartCoroutine(EmptyComboCoroutine());
            return;
        }
        _isAttacking = true;
        StartCoroutine(ComboCoroutine(targetX, squadScores, finalScore, onTeleport));
    }

    private IEnumerator EmptyComboCoroutine()
    {
        TeleportToStart();
        yield return new WaitUntil(() => !_isTeleporting);
        _isAttacking = false;
    }

    private IEnumerator ComboCoroutine(float targetX, List<int> squadScores, int finalScore, Action onTeleport = null)
    {
        for(int i = 0; i < _comboList.Count; i++)
        {
            AttackType type = _comboList[i];
            bool isLast = (i == _comboList.Count - 1);

            yield return ExecuteSingleAttack(targetX, type);

            if(squadScores != null && i < squadScores.Count)
            {
                Debug.Log($"[Player] ShowBaseScore 호출 : {squadScores[i]}");
                _popupController?.ShowBaseScore(squadScores[i]);
            }
                

            if(!isLast)
                yield return new WaitForSecondsRealtime(0.1f);
        }

        _currentAttackX = float.MinValue;
        _attackOverrides.Clear();
        _synergyCardTypes.Clear();
        ClearCombo();
        if (finalScore > 0)
            _popupController?.ShowFinalScore(finalScore);
        onTeleport?.Invoke();
        TeleportToStart();
        yield return new WaitUntil(() => !_isTeleporting);
        _isAttacking = false;
    }
    
    private IEnumerator ExecuteSingleAttack(float targetX, AttackType type)
    {
        switch(type)
        {
            case AttackType.Sword:
                yield return MeleeAttack(targetX, type);
                break;
            case AttackType.Shuriken:
            case AttackType.Spell:
                yield return RangedAttack(targetX, type);
                break;
        }

    }
    private IEnumerator MeleeAttack(float targetX, AttackType type)     //근접 공격할 경우의 거리
    {
        float stoppedX = targetX - _stopOffset;

        if(!Mathf.Approximately(_currentAttackX, stoppedX))
        {
            transform.DOMoveX(stoppedX, _attackMoveDuration)
            .SetEase(Ease.OutCubic).SetUpdate(true);
            yield return new WaitForSecondsRealtime(_attackMoveDuration);

            // 공격 위치에서 정지 애니메이션
            _isRunStopping = true;
            _playerAnimator.PlayRunStop(() => _isRunStopping = false);
            yield return new WaitUntil(() => !_isRunStopping);

            _currentAttackX = stoppedX;
        }

        // 공격 애니메이션
        _attackAnimDone = false;
        if(type == AttackType.Sword)
        {
            if(_attackOverrides.TryGetValue(AttackType.Sword, out Sprite[] swordOverride))
            {
                //Debug.Log($"[Player] Sword 강화 콤보 애니메이션 재생 ({swordOverride.Length}프레임)");
                _playerAnimator.PlayCustomAttack(swordOverride, _comboSynergyEffect.Fps);
            }
            else
                _playerAnimator.PlaySword();
            if(_synergyCardTypes.Contains(CardType.Sword) && _synergyOverlayEffect != null)
            {
                //Debug.Log("[Player] Sword 시너지 오버레이 재생");
                _synergyOverlayEffect.PlaySynergy(CardType.Sword);
            }
            if(_effectAnimator != null)
                StartCoroutine(PlayEffectDelayed(_effectAnimator.PlaySwordEffect, _playerAnimator.HitFrameDelay));
        }

        yield return new WaitUntil(() => _attackAnimDone); 
        
    }

    private IEnumerator RangedAttack(float targetX, AttackType type)              //원거리 공격할 경우 수리검만
    {
        //적 앞에서 _rangedOffset 만큼 뒤에서 공격
        float rangedX = targetX - _stopOffset - _shurikenOffset;

        //현재 위치와 다를 때만 이동 + 정지 애니메이션
        if(!Mathf.Approximately(_currentAttackX, rangedX))
        {
            transform.DOMoveX(rangedX, _attackMoveDuration)
            .SetEase(Ease.OutCubic).SetUpdate(true);
            yield return new WaitForSecondsRealtime(_attackMoveDuration);

            //공격 위치에서 정지 애니메이션
            _isRunStopping = true;
            _playerAnimator.PlayRunStop(() => _isRunStopping = false);
            yield return new WaitUntil(() => !_isRunStopping);

            _currentAttackX = rangedX;    
        }
        
        _attackAnimDone = false;
        if(type == AttackType.Shuriken)
        {
            if(_attackOverrides.TryGetValue(AttackType.Shuriken, out Sprite[] shurikenOverride))
            {
                Debug.Log($"[Player] Shuriken 강화 콤보 애니메이션 재생 ({shurikenOverride.Length}프레임)");
                _playerAnimator.PlayCustomAttack(shurikenOverride, _comboSynergyEffect.Fps);
            }
            else
                _playerAnimator.PlayShuriken();
            if(_synergyCardTypes.Contains(CardType.Kunai) && _synergyOverlayEffect != null)
            {
                Debug.Log("[Player] Kunai 시너지 오버레이 재생");
                _synergyOverlayEffect.PlaySynergy(CardType.Kunai);
            }
            if(_effectAnimator != null)
                StartCoroutine(PlayEffectDelayed(_effectAnimator.PlayShurikenEffect, _playerAnimator.HitFrameDelay));
        }
        else if(type == AttackType.Spell)
        {
            if(_attackOverrides.TryGetValue(AttackType.Spell, out Sprite[] spellOverride))
            {
                Debug.Log($"[Player] Spell 강화 콤보 애니메이션 재생 ({spellOverride.Length}프레임)");
                _playerAnimator.PlayCustomAttack(spellOverride, _comboSynergyEffect.Fps);
            }
            else
                _playerAnimator.PlaySpell();
            if(_synergyCardTypes.Contains(CardType.FoxSpirit) && _synergyOverlayEffect != null)
            {
                Debug.Log("[Player] FoxSpirit 시너지 오버레이 재생");
                _synergyOverlayEffect.PlaySynergy(CardType.FoxSpirit);
            }
            if(_effectAnimator != null)
                StartCoroutine(PlayEffectDelayed(_effectAnimator.PlaySpellEffect, _playerAnimator.HitFrameDelay));
        }
            
        
        yield return new WaitUntil(() => _attackAnimDone);
        
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
        _isTeleporting = true;
        
        _playerAnimator.HideSprite();
        transform.position = _startPosition;
        _playerAnimator.ShowSprite();

        _playerAnimator.PlayTeleport(() =>
        {
            _isTeleporting = false;
            _playerAnimator.PlayRun();
        });


    }

    public void PlayStopThenBattle()
    {
        if(_isRunStopping || _isTeleporting) return;

        _isRunStopping = true;
        _playerAnimator.PlayRunStop(() =>
        {
            _isRunStopping = false;
            TeleportToStart();
        });
    }

#endregion 


#region 이동 관련
    // slowFactor 배율로 느리게 이동, unscaledTimeBudget 이내에 도착 (Time.timeScale 비의존)
    public void MoveToEnemy(float targetX, float unscaledTimeBudget, float slowFactor)
    {
        _isReached = false;
        transform.DOKill();     // 복귀 트윈 등 기존 트윈 제거
        _playerAnimator.PlayRun(_runFps * slowFactor);

        float stoppedX = targetX - _stopOffset;     //적 앞 일정 거리에서 정지
        float distance = Mathf.Abs(transform.position.x - stoppedX);
        float slowSpeed = _moveSpeed * slowFactor;
        float duration = Mathf.Min(distance / slowSpeed, unscaledTimeBudget);

        transform.DOMoveX(stoppedX, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)    // 언스케일 타임 기준으로 이동
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
        //앞으로 돌진
        seq.Append(transform.DOMoveX(stoppedX, 0.2f)
        .SetEase(Ease.OutCubic));
        // 뒤로 복귀
        seq.Append(transform.DOMoveX(_startPosition.x, 0.4f)
            .SetEase(Ease.OutCubic));
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
        if(_isKnockBack) return;
        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage);
        OnHpChanged?.Invoke(_currentHp);

        PlayerKnockBack();      //때린뒤, 혹은 맞은 뒤에 원래 자리로 돌아가기
    }

    private void PlayerKnockBack()
    {
        if(_isKnockBack) return;
        _isKnockBack = true;

        transform.DOKill();
        StartCoroutine(KnockBackCoroutine());
    }

    private IEnumerator KnockBackCoroutine()
    {
        float returnDuration = 0.6f;
        float halfReturn = returnDuration * 0.5f;

        // 텔레포트 직후처럼 이미 시작 위치에 있으면 애니메이션 중단 없이 유지
        bool alreadyAtStart = (transform.position - _startPosition).sqrMagnitude < 0.001f;

        if(!alreadyAtStart)
        {
            transform.DOMoveX(_startPosition.x, returnDuration)
                .SetEase(Ease.OutCubic).SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(halfReturn);

        transform.DOKill();
        _isKnockBack = false;

        if(!alreadyAtStart)
            _playerAnimator.PlayRun();

        if(IsDead)
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

}
#endregion