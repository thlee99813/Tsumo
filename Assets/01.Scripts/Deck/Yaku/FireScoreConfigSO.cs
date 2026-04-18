using UnityEngine;

public enum YakuId
{
    PartialFlushTwoSets, // 편일문
    DoubleTriple,        // 이중타
    DoubleSequence,      // 이연격
    AllTriples,          // 전면집결
    StraightOneToNine,   // 구단일섬
    FullFlush,           // 일문오의
    SanshokuDojun,       // 삼문연격
    TripleSequence,      // 삼연극의
    SanshokuDoukou       // 삼문집결
}

[CreateAssetMenu(fileName = "FireScoreConfig", menuName = "Tsumo/Score/FireScoreConfig")]
public class FireScoreConfigSO : ScriptableObject
{
    [Header("Base Score")]
    [SerializeField, Min(0)] private int _sequenceScore = 1000;
    [SerializeField, Min(0)] private int _tripleScore = 1500;

    [Header("편일문")]
    [SerializeField, Min(0f)] private float _partialFlushTwoSetsBonus = 0.5f;
    [SerializeField, Min(0)] private int _partialFlushTwoSetsStandalone = 4500;

    [Header("이중타")]
    [SerializeField, Min(0f)] private float _doubleTripleBonus = 0.8f;
    [SerializeField, Min(0)] private int _doubleTripleStandalone = 5400;

    [Header("이연격")]
    [SerializeField, Min(0f)] private float _doubleSequenceBonus = 1.0f;
    [SerializeField, Min(0)] private int _doubleSequenceStandalone = 6000;

    [Header("전면집결")]
    [SerializeField, Min(0f)] private float _allTriplesBonus = 1.5f;
    [SerializeField, Min(0)] private int _allTriplesStandalone = 7500;

    [Header("구단일섬")]
    [SerializeField, Min(0f)] private float _straightOneToNineBonus = 1.7f;
    [SerializeField, Min(0)] private int _straightOneToNineStandalone = 8100;

    [Header("일문오의")]
    [SerializeField, Min(0f)] private float _fullFlushBonus = 1.8f;
    [SerializeField, Min(0)] private int _fullFlushStandalone = 8400;

    [Header("삼문연격")]
    [SerializeField, Min(0f)] private float _sanshokuDojunBonus = 2.3f;
    [SerializeField, Min(0)] private int _sanshokuDojunStandalone = 9900;

    [Header("삼연극의")]
    [SerializeField, Min(0f)] private float _tripleSequenceBonus = 3.5f;
    [SerializeField, Min(0)] private int _tripleSequenceStandalone = 13500;

    [Header("삼문집결")]
    [SerializeField, Min(0f)] private float _sanshokuDoukouBonus = 4.0f;
    [SerializeField, Min(0)] private int _sanshokuDoukouStandalone = 15000;

    public int SequenceScore => _sequenceScore;
    public int TripleScore => _tripleScore;

    public float GetBonus(YakuId yakuId)
    {
        switch (yakuId)
        {
            case YakuId.PartialFlushTwoSets: return _partialFlushTwoSetsBonus;
            case YakuId.DoubleTriple: return _doubleTripleBonus;
            case YakuId.DoubleSequence: return _doubleSequenceBonus;
            case YakuId.AllTriples: return _allTriplesBonus;
            case YakuId.StraightOneToNine: return _straightOneToNineBonus;
            case YakuId.FullFlush: return _fullFlushBonus;
            case YakuId.SanshokuDojun: return _sanshokuDojunBonus;
            case YakuId.TripleSequence: return _tripleSequenceBonus;
            case YakuId.SanshokuDoukou: return _sanshokuDoukouBonus;
            default: return 0f;
        }
    }

    public int GetStandaloneScore(YakuId yakuId)
    {
        switch (yakuId)
        {
            case YakuId.PartialFlushTwoSets: return _partialFlushTwoSetsStandalone;
            case YakuId.DoubleTriple: return _doubleTripleStandalone;
            case YakuId.DoubleSequence: return _doubleSequenceStandalone;
            case YakuId.AllTriples: return _allTriplesStandalone;
            case YakuId.StraightOneToNine: return _straightOneToNineStandalone;
            case YakuId.FullFlush: return _fullFlushStandalone;
            case YakuId.SanshokuDojun: return _sanshokuDojunStandalone;
            case YakuId.TripleSequence: return _tripleSequenceStandalone;
            case YakuId.SanshokuDoukou: return _sanshokuDoukouStandalone;
            default: return 0;
        }
    }

    public string GetDisplayName(YakuId yakuId)
    {
        switch (yakuId)
        {
            case YakuId.PartialFlushTwoSets: return "편일문";
            case YakuId.DoubleTriple: return "이중타";
            case YakuId.DoubleSequence: return "이연격";
            case YakuId.AllTriples: return "전면집결";
            case YakuId.StraightOneToNine: return "구단일섬";
            case YakuId.FullFlush: return "일문오의";
            case YakuId.SanshokuDojun: return "삼문연격";
            case YakuId.TripleSequence: return "삼연극의";
            case YakuId.SanshokuDoukou: return "삼문집결";
            default: return yakuId.ToString();
        }
    }
}
