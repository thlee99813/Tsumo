using System;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopupController : MonoBehaviour
{
    [SerializeField] private DamagePopup _popupPrefab;
    [SerializeField] private Transform _enemyHeadPoint;
    [SerializeField] private int _initialPoolSize = 5;

    private List<DamagePopup> _pool = new List<DamagePopup>();

    private void Awake()
    {
        for (int i = 0; i < _initialPoolSize; i++)
            AddToPool();
    }

    public void ShowBaseScore(int score)
    {
        GetNext().ShowBaseScore(score, _enemyHeadPoint.position);
    }

    public void ShowFinalScore(int score)
    {
        GetNext().ShowFinalScore(score, _enemyHeadPoint.position);
    }

    public void ShowYakuBonus(string yakuName, float bonusMultiplier, Action onComplete)
    {
        GetNext().ShowYakuBonus(yakuName, bonusMultiplier, _enemyHeadPoint.position, onComplete);
    }

    public void SetEnemyHeadPoint(Transform headPoint)
    {
        _enemyHeadPoint = headPoint;
    }

    private DamagePopup GetNext()
    {
        foreach (DamagePopup p in _pool)
        {
            if (!p.gameObject.activeSelf)
                return p;
        }
        AddToPool();
        return _pool[_pool.Count - 1];
    }

    private void AddToPool()
    {
        DamagePopup popup = Instantiate(_popupPrefab, transform);
        popup.gameObject.SetActive(false);
        _pool.Add(popup);
    }
}
