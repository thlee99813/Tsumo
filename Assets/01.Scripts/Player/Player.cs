using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerStateSO _stats;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _knockBackHeight = 1.5f;
    [SerializeField] private float _knockBackDuration = 0.4f;
    [SerializeField] private float _laserRange = 10f;
    [SerializeField] private float _stopOffset = 1.5f;
    [SerializeField] private LayerMask _enemyLayer;

    private int _currentHp;
    private Vector3 _startPosition;
    private bool _isKnockBack;
    private bool _isReached;        //접근 중인가

    public int CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;
    public bool IsKnockBack => _isKnockBack;
    public bool IsReached => _isReached;

    public event Action OnPlayerDead;
    public event Action<int> OnHpChanged;

    private void Awake()
    {
        _startPosition = transform.position;
        _currentHp = _stats.MaxHp;
    }

    public bool DetectEnemy()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, _laserRange, _enemyLayer);
        return hit.collider != null;
    }


    //30초 언스케일 타임 안에 도착하도록 duration 계산
    public void MoveToEnemy(float targetX, float unscaledTimeBudget, float currentTimeScale)
    {
        _isReached = false;
        transform.DOKill();     // 복귀 트윈 등 기존 트윈 제거

        float stoppedX = targetX - _stopOffset;     //적 앞 일정 거리에서 정지
        float distance = Mathf.Abs(transform.position.x - targetX);
        float normalduration = distance / _moveSpeed;

        //닷트윈은 스케일 시간 기준, 언스케일 예산을 스케일 시간으로 변환
        float maxScaledDuration = unscaledTimeBudget * currentTimeScale;
        float duration = Mathf.Min(normalduration, maxScaledDuration); 
    

        transform.DOMoveX(stoppedX, duration)
            .SetEase(Ease.Linear)
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

    private System.Collections.IEnumerator KnockBackCoroutine()
    {
        float arcHeight = _startPosition.y + _knockBackHeight;  // 피격 시점 기준 아크 높이

        // 1단계: 피격 반응 Y축 위로 튕김
        transform.DOMoveY(arcHeight, _knockBackDuration * 0.4f)
            .SetEase(Ease.OutQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_knockBackDuration * 0.4f);

        // 2단계: Y축 아래로 낙하 
        transform.DOMoveY(_startPosition.y, _knockBackDuration * 0.4f)
            .SetEase(Ease.InQuad).SetUpdate(true);
        yield return new WaitForSecondsRealtime(_knockBackDuration * 0.4f);

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

#if UNITY_EDITOR    
    //레이저 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.right *  _laserRange);
    }
#endif
}
