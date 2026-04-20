using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Serializable]
    public struct StageStat
    {
        [SerializeField] private string _stageCode;
        [SerializeField] private int _maxHp;
        [SerializeField] private int _counterDamage;
        [SerializeField] private Sprite _sprite;

        [Header("Sprites")]
        [SerializeField] private Sprite[] _idleSprites; //idle 루프
        [SerializeField] private Sprite _hitSprite;   // 피격 1 프레임
        [SerializeField] private Sprite _attackSprite;  // 공격 1 프레임

        public string StageCode => _stageCode;
        public int MaxHp => _maxHp;
        public int CounterDamage => _counterDamage;
        public Sprite[] idleSprite => _idleSprites;
        public Sprite HitSprite => _hitSprite;
        public Sprite AttackSprite => _attackSprite;
    }



    [Header("Default")]
    [SerializeField] private int _maxHp = 50;
    [SerializeField] private int _counterDamage = 10;

    [Header("Enemy Stats")]
    [SerializeField] private List<StageStat> _stageStats = new List<StageStat>(6);

    public int MaxHp => _maxHp;
    public int CounterDamage => _counterDamage;

    public void GetStatsByStage(int stageIndex, out int maxHp, out int counterDamage, 
        out Sprite[] idleSprites, out Sprite hitSprite, out Sprite attackSprite)
    {
        if (stageIndex >= 0 && stageIndex < _stageStats.Count)
        {
            StageStat stat = _stageStats[stageIndex];
            maxHp = Mathf.Max(1, stat.MaxHp);
            counterDamage = Mathf.Max(0, stat.CounterDamage);
            idleSprites = stat.idleSprite;
            hitSprite = stat.HitSprite;
            attackSprite = stat.AttackSprite;
            return;
        }

        maxHp = Mathf.Max(1, _maxHp);
        counterDamage = Mathf.Max(0, _counterDamage);
        idleSprites = null;
        hitSprite = null;
        attackSprite = null;
    }

}