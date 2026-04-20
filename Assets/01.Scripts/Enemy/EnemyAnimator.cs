using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private float _idleFps = 8f;
    [SerializeField] private float _hitDuration = 0.15f;    // 피격 스프라이트 유지 시간
    
    private SpriteRenderer _spriteRenderer;
    private Coroutine _idleCoroutine;

    private Sprite[] _idleSprites;
    private Sprite _hitSprite;
    private Sprite _attackSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprites(Sprite[] idleSprites, Sprite hitSprite, Sprite attackSprite)
    {
        if (idleSprites == null || idleSprites.Length == 0)
        {
            Debug.LogWarning($"[EnemyAnimator] SetSprites: idleSprites가 비어 있습니다. Inspector에서 스프라이트를 확인하세요.", this);
        }
        else
        {
            // null 요소 필터링 (Inspector에서 배열 크기만 설정하고 스프라이트를 할당하지 않은 경우)
            int nullCount = 0;
            for (int i = 0; i < idleSprites.Length; i++)
                if (idleSprites[i] == null) nullCount++;

            if (nullCount > 0)
                Debug.LogWarning($"[EnemyAnimator] SetSprites: idleSprites 중 {nullCount}개가 null입니다. Inspector에서 스프라이트를 확인하세요.", this);
        }

        _idleSprites = idleSprites;
        _hitSprite = hitSprite;
        _attackSprite = attackSprite;

        PlayIdle();
    }

    public void PlayIdle()
    {
        StopIdle();
        if(_idleSprites == null || _idleSprites.Length == 0) return;
        _idleCoroutine = StartCoroutine(IdleLoop());
    }

    public void PlayHit()
    {
        if (_hitSprite == null) return;
        StopIdle();
        StartCoroutine(HitCoroutine());
    }

    // 공격 — 1프레임 표시 후 idle 복귀
    public void PlayAttack()
    {
        if (_attackSprite == null) return;
        StopIdle();
        StartCoroutine(AttackCoroutine());
    }

    private void StopIdle()
    {
        if (_idleCoroutine != null)
        {
            StopCoroutine(_idleCoroutine);
            _idleCoroutine = null;
        }
    }

    private IEnumerator IdleLoop()
    {
        if (_idleSprites == null || _idleSprites.Length == 0) yield break;

        float interval = 1f / _idleFps;
        int i = 0;
        while (true)
        {
            // null 스프라이트는 건너뜀 (Inspector에서 슬롯만 만들고 미할당 시)
            if (_idleSprites[i] != null)
                _spriteRenderer.sprite = _idleSprites[i];
            i = (i + 1) % _idleSprites.Length;
            yield return new WaitForSecondsRealtime(interval);
        }
    }

    private IEnumerator HitCoroutine()
    {
        _spriteRenderer.sprite = _hitSprite;
        yield return new WaitForSecondsRealtime(_hitDuration);
        PlayIdle();
    }

    private IEnumerator AttackCoroutine()
    {
        _spriteRenderer.sprite = _attackSprite;
        yield return new WaitForSecondsRealtime(0.1f); // 1프레임 정도 유지
        PlayIdle();
    }
    
}
