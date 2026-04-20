using UnityEngine;

public class DamagePopupController : MonoBehaviour
{
    [SerializeField] private DamagePopup _popupPrefab;
    [SerializeField] private Transform _enemyHeadPoint;
    [SerializeField] private int _poolSize = 5;

    private DamagePopup[] _pool;
    private int _poolIndex = 0;

    private void Awake()
    {
        _pool = new DamagePopup[_poolSize];
        for(int i = 0; i < _poolSize; i++)
        {
            _pool[i] = Instantiate(_popupPrefab, transform);
            _pool[i].gameObject.SetActive(false);
        }
    }

    public void ShowBaseScore(int score)
    {
        GetNext().ShowBaseScore(score, _enemyHeadPoint.position);
    }

    public void ShowFinalScore(int score)
    {
        GetNext().ShowFinalScore(score, _enemyHeadPoint.position);
    }

    public void SetEnemyHeadPoint(Transform headPoint)
    {
        _enemyHeadPoint = headPoint;
    }

    private DamagePopup GetNext()
    {
        DamagePopup popup = _pool[_poolIndex];
        _poolIndex = (_poolIndex + 1) % _poolSize;
        return popup;
    }
}
