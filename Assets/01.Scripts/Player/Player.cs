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
    [SerializeField] private float _knockBackHeight = 1.5f;
    [SerializeField] private float _knockBackDuration = 0.4f;
    [SerializeField] private float _stopOffset = 1.5f;
    [SerializeField] private float _shurikenOffset = 3f;

    [Header("Combat")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackMoveDuration = 0.2f;

    //! Animation Test=======================
    private SpriteRenderer _spriteRenderer;
    //!=======================================

    private int _currentHp;
    private Vector3 _startPosition;
    private bool _isKnockBack;
    private bool _isReached;        //접근 중인가
    private bool _isAttacking;
    private bool _attackAnimDone;
    private Animator _animator;
    private List<AttackType> _comboList = new List<AttackType>();

    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;
    public bool IsKnockBack => _isKnockBack;
    public bool IsReached => _isReached;
    public int AttackDamage => _attackDamage;
    public bool IsAttacking => _isAttacking;
    public IReadOnlyList<AttackType> ComboList => _comboList;

    public event Action OnPlayerDead;
    public event Action<int> OnHpChanged;
    public event Action<AttackType> OnAttackAdded;  // UI 연동용 포트

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
        _animator = GetComponent<Animator>();

        //! Animation Test
        _spriteRenderer = GetComponent<SpriteRenderer>();
        //! Animation Test
    }

    //PlayerInputHandler에서 호출
    public void AddAttack(AttackType type)
    {
        _comboList.Add(type);
        Debug.Log($"[Player] 공격 추가 : {type} (현재콤보 : {_comboList.Count})");
        OnAttackAdded?.Invoke(type);
    }
    public void ClearCombo()
    {
        _comboList.Clear();
    }
#region 공격 콤보 처리
    //IngameController에서 호출 내부 콤보 리스트 사용
    public void ExecuteCombo(float targetX)
    {
        if(_comboList.Count == 0)
        {
            _isAttacking = false;
            return;
        }
        _isAttacking = true;
        StartCoroutine(ComboCoroutine(targetX));
    }

    private IEnumerator ComboCoroutine(float targetX)
    {
        for(int i = 0; i < _comboList.Count; i++)
        {
            AttackType type = _comboList[i];
            bool isLast = (i == _comboList.Count - 1);

            yield return ExecuteSingleAttack(targetX, type);

            if(!isLast)
                yield return new WaitForSecondsRealtime(0.1f);
        }

        ClearCombo();
        TeleportToStart();
        _isAttacking = false;
    }
    
    private IEnumerator ExecuteSingleAttack(float targetX, AttackType type)
    {
        switch(type)
        {
            case AttackType.Sword:
            case AttackType.Spell:
                yield return MeleeAttack(targetX, type);
                break;
            case AttackType.Shuriken:
                yield return RangedAttack();
                break;
        }

    }
    private IEnumerator MeleeAttack(float targetX, AttackType type)     //근접 공격할 경우의 거리
    {
        float stoppedX = targetX - _stopOffset;
        transform.DOMoveX(stoppedX, _attackMoveDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_attackMoveDuration);

        //_attackAnimDone = false;      //! Animation 연결
        //_animator.SetTrigger(type == AttackType.Sword ? "Sword" : "Spell");   //! Animation 연결
        //yield return new WaitUntil(() => _attackAnimDone);        //! Animation 연결
        _spriteRenderer.color = type == AttackType.Sword ? Color.red : Color.magenta;
        yield return new WaitForSecondsRealtime(0.3f); 
        _spriteRenderer.color = Color.white;
    }

    private IEnumerator RangedAttack()              //원거리 공격할 경우 수리검만
    {
        float rangedX = _startPosition.x + _shurikenOffset;
        transform.DOMoveX(rangedX, _attackMoveDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_attackMoveDuration);

        //_attackAnimDone = false;      //! Animation 연결
        //_animator.SetTrigger("Shuriken"); //! Animation 연결
        //yield return new WaitUntil(() => _attackAnimDone); //! Animation 연결
        _spriteRenderer.color = Color.yellow;
        yield return new WaitForSecondsRealtime(0.3f);
        _spriteRenderer.color = Color.white;
    }

    public void OnAttackAniComplete()
    {
        _attackAnimDone = true;
    }

    private void TeleportToStart()
    {
        gameObject.SetActive(false);
        transform.position = _startPosition;
        gameObject.SetActive(true);

        //TODO 복귀 이펙트 포트
    }
#endregion 


#region 이동 관련
    // slowFactor 배율로 느리게 이동, unscaledTimeBudget 이내에 도착 (Time.timeScale 비의존)
    public void MoveToEnemy(float targetX, float unscaledTimeBudget, float slowFactor)
    {
        _isReached = false;
        transform.DOKill();     // 복귀 트윈 등 기존 트윈 제거

        float stoppedX = targetX - _stopOffset;     //적 앞 일정 거리에서 정지
        float distance = Mathf.Abs(transform.position.x - stoppedX);
        float slowSpeed = _moveSpeed * slowFactor;
        float duration = Mathf.Min(distance / slowSpeed, unscaledTimeBudget);

        transform.DOMoveX(stoppedX, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(true)    // 언스케일 타임 기준으로 이동
            .OnComplete(() => _isReached = true);
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
        .SetEase(Ease.OutQuad));
        // 뒤로 복귀
        seq.Append(transform.DOMoveX(_startPosition.x, 0.4f)
            .SetEase(Ease.InQuad));
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
        float arcHeight = _startPosition.y + _knockBackHeight;  // 피격 시점 기준 아크 높이

        /*
        // 1단계: 피격 반응 Y축 위로 튕김
        transform.DOMoveY(arcHeight, _knockBackDuration * 0.4f)
            .SetEase(Ease.OutQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_knockBackDuration * 0.4f);

        // 2단계: Y축 아래로 낙하 
        transform.DOMoveY(_startPosition.y, _knockBackDuration * 0.4f)
            .SetEase(Ease.InQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_knockBackDuration * 0.4f);
        */

        float returnDuration = 0.6f;
        float halfReturn = returnDuration * 0.5f;

        // 3단계: X축 복귀 + Y 아크 동시 시작 (점프하며 뒤로)
        transform.DOMoveX(_startPosition.x, returnDuration)
            .SetEase(Ease.InOutQuad).SetUpdate(true);
        transform.DOMoveY(arcHeight, halfReturn)
            .SetEase(Ease.OutQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(halfReturn);

        // 4단계: 착지 (X는 계속 이동 중, Y만 낙하, 실제 시간)
        transform.DOMoveY(_startPosition.y, halfReturn)
            .SetEase(Ease.InQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(halfReturn);

        transform.DOKill();     //스냅 전 잔여 트윈 전부 제거
        transform.position = _startPosition;  // 정확한 위치 스냅
        _isKnockBack = false;

        if(IsDead)
            OnPlayerDead?.Invoke();
    }

    public void ResetHp()
    {
        _currentHp = _stats.MaxHp;
        OnHpChanged?.Invoke(_currentHp);
    }

}
#endregion