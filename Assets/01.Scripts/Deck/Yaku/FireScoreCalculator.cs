using System.Collections.Generic;
using UnityEngine;

public class FireScoreCalculator : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private FireScoreConfigSO _config;

    [Header("Option")]
    [SerializeField] private bool _applyYakuOnlyWhenAllThreeValid = true;

    public FireScoreResult Calculate(List<SquadDropZone> squadZones)
    {
        FireScoreContext context = new FireScoreContext();
        FireScoreResult result = new FireScoreResult();

        if (squadZones != null)
        {
            for (int i = 0; i < squadZones.Count; i++)
            {
                SquadScoreResult squadResult = EvaluateSquad(i, squadZones[i]);
                context.SquadResults.Add(squadResult);
                result.SquadResults.Add(squadResult);
                result.BaseScoreTotal += squadResult.BaseScore;
            }
        }

        bool canApplyYaku = !_applyYakuOnlyWhenAllThreeValid || context.AreAllSquadsValid(3);
        float yakuBonusSum = 0f;

        if (canApplyYaku)
        {
            EvaluateYakus(context, result, ref yakuBonusSum);
        }

        result.YakuBonusSum = yakuBonusSum;
        result.FinalMultiplier = 1f + yakuBonusSum;
        result.YakuApplied = result.YakuResults.Count > 0;
        result.FinalScore = Mathf.RoundToInt(result.BaseScoreTotal * result.FinalMultiplier);

        return result;
    }

    private void EvaluateYakus(FireScoreContext context, FireScoreResult result, ref float yakuBonusSum)
    {
        if (HasSameTypeAtLeast(context, 2))
        {
            AddYaku(result, YakuId.PartialFlushTwoSets, ref yakuBonusSum); // 편일문
        }

        if (HasTripleAtLeast(context, 2))
        {
            AddYaku(result, YakuId.DoubleTriple, ref yakuBonusSum); // 이중타
        }

        if (HasSameSequenceAtLeast(context, 2))
        {
            AddYaku(result, YakuId.DoubleSequence, ref yakuBonusSum); // 이연격
        }

        if (IsAllTriple(context))
        {
            AddYaku(result, YakuId.AllTriples, ref yakuBonusSum); // 전면집결
        }

        if (IsStraightOneToNine(context))
        {
            AddYaku(result, YakuId.StraightOneToNine, ref yakuBonusSum); // 구단일섬
        }

        if (IsAllSameType(context))
        {
            AddYaku(result, YakuId.FullFlush, ref yakuBonusSum); // 일문오의
        }

        if (IsSanshokuDojun(context))
        {
            AddYaku(result, YakuId.SanshokuDojun, ref yakuBonusSum); // 삼문연격
        }

        if (HasSameSequenceAtLeast(context, 3))
        {
            AddYaku(result, YakuId.TripleSequence, ref yakuBonusSum); // 삼연극의
        }

        if (IsSanshokuDoukou(context))
        {
            AddYaku(result, YakuId.SanshokuDoukou, ref yakuBonusSum); // 삼문집결
        }
    }

    private void AddYaku(FireScoreResult result, YakuId yakuId, ref float yakuBonusSum)
    {
        float bonus = GetYakuBonus(yakuId);
        if (bonus <= 0f)
        {
            return;
        }

        result.YakuResults.Add(new YakuScoreResult
        {
            Name = GetYakuName(yakuId),
            BonusMultiplier = bonus,
        });

        yakuBonusSum += bonus;
    }

    private bool HasSameTypeAtLeast(FireScoreContext context, int requiredCount)
    {
        return context.GetMaxValidTypeComboCount() >= requiredCount;
    }

    private bool HasTripleAtLeast(FireScoreContext context, int requiredCount)
    {
        int count = 0;

        for (int i = 0; i < context.SquadResults.Count; i++)
        {
            SquadScoreResult squad = context.SquadResults[i];
            if (squad.IsValid && squad.ComboType == SquadComboType.Triple)
            {
                count++;
            }
        }

        return count >= requiredCount;
    }

    private bool HasSameSequenceAtLeast(FireScoreContext context, int requiredCount)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();

        for (int i = 0; i < context.SquadResults.Count; i++)
        {
            SquadScoreResult squad = context.SquadResults[i];
            if (!squad.IsValid || squad.ComboType != SquadComboType.Sequence)
            {
                continue;
            }

            if (squad.SortedNumbers == null || squad.SortedNumbers.Length != 3)
            {
                continue;
            }

            int start = squad.SortedNumbers[0];
            string key = $"{squad.CardType}_{start}";

            if (!counts.ContainsKey(key))
            {
                counts[key] = 0;
            }

            counts[key]++;

            if (counts[key] >= requiredCount)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAllTriple(FireScoreContext context)
    {
        if (!context.AreAllSquadsValid(3))
        {
            return false;
        }

        for (int i = 0; i < context.SquadResults.Count; i++)
        {
            if (context.SquadResults[i].ComboType != SquadComboType.Triple)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsStraightOneToNine(FireScoreContext context)
    {
        if (!context.AreAllSquadsValid(3))
        {
            return false;
        }

        List<int> starts = new List<int>(3);

        for (int i = 0; i < context.SquadResults.Count; i++)
        {
            SquadScoreResult squad = context.SquadResults[i];
            if (squad.ComboType != SquadComboType.Sequence)
            {
                return false;
            }

            if (squad.SortedNumbers == null || squad.SortedNumbers.Length != 3)
            {
                return false;
            }

            starts.Add(squad.SortedNumbers[0]);
        }

        starts.Sort();
        return starts[0] == 1 && starts[1] == 4 && starts[2] == 7;
    }

    private bool IsAllSameType(FireScoreContext context)
    {
        if (!context.AreAllSquadsValid(3))
        {
            return false;
        }

        CardType firstType = context.SquadResults[0].CardType;

        for (int i = 1; i < context.SquadResults.Count; i++)
        {
            if (context.SquadResults[i].CardType != firstType)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsSanshokuDojun(FireScoreContext context)
    {
        if (!context.AreAllSquadsValid(3))
        {
            return false;
        }

        bool[,] hasSequence = new bool[3, 10];

        for (int i = 0; i < context.SquadResults.Count; i++)
        {
            SquadScoreResult squad = context.SquadResults[i];
            if (squad.ComboType != SquadComboType.Sequence)
            {
                continue;
            }

            if (squad.SortedNumbers == null || squad.SortedNumbers.Length != 3)
            {
                continue;
            }

            int typeIndex = TypeToIndex(squad.CardType);
            int start = squad.SortedNumbers[0];

            if (typeIndex < 0 || start < 1 || start > 7)
            {
                continue;
            }

            hasSequence[typeIndex, start] = true;
        }

        for (int start = 1; start <= 7; start++)
        {
            if (hasSequence[0, start] && hasSequence[1, start] && hasSequence[2, start])
            {
                return true;
            }
        }

        return false;
    }

    private bool IsSanshokuDoukou(FireScoreContext context)
    {
        if (!context.AreAllSquadsValid(3))
        {
            return false;
        }

        bool[,] hasTriple = new bool[3, 10];

        for (int i = 0; i < context.SquadResults.Count; i++)
        {
            SquadScoreResult squad = context.SquadResults[i];
            if (squad.ComboType != SquadComboType.Triple)
            {
                continue;
            }

            if (squad.SortedNumbers == null || squad.SortedNumbers.Length != 3)
            {
                continue;
            }

            int typeIndex = TypeToIndex(squad.CardType);
            int number = squad.SortedNumbers[0];

            if (typeIndex < 0 || number < 1 || number > 9)
            {
                continue;
            }

            hasTriple[typeIndex, number] = true;
        }

        for (int number = 1; number <= 9; number++)
        {
            if (hasTriple[0, number] && hasTriple[1, number] && hasTriple[2, number])
            {
                return true;
            }
        }

        return false;
    }

    private int TypeToIndex(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Sword: return 0;
            case CardType.Kunai: return 1;
            case CardType.FoxSpirit: return 2;
            default: return -1;
        }
    }

    private SquadScoreResult EvaluateSquad(int squadIndex, SquadDropZone squadZone)
    {
        SquadScoreResult result = new SquadScoreResult
        {
            SquadIndex = squadIndex,
            IsValid = false,
            ComboType = SquadComboType.None,
            BaseScore = 0
        };

        if (squadZone == null)
        {
            return result;
        }

        List<CardData> cards = squadZone.GetRegisteredCardsSnapshot();
        if (cards == null || cards.Count != 3)
        {
            return result;
        }

        if (cards[0] == null || cards[1] == null || cards[2] == null)
        {
            return result;
        }

        CardType type = cards[0].Type;
        if (cards[1].Type != type || cards[2].Type != type)
        {
            return result;
        }

        int[] numbers = new[] { cards[0].Number, cards[1].Number, cards[2].Number };
        System.Array.Sort(numbers);

        result.CardType = type;
        result.SortedNumbers = numbers;

        if (numbers[0] == numbers[1] && numbers[1] == numbers[2])
        {
            result.IsValid = true;
            result.ComboType = SquadComboType.Triple;
            result.BaseScore = GetTripleScore();
            return result;
        }

        if (numbers[0] + 1 == numbers[1] && numbers[1] + 1 == numbers[2])
        {
            result.IsValid = true;
            result.ComboType = SquadComboType.Sequence;
            result.BaseScore = GetSequenceScore();
            return result;
        }

        return result;
    }

    private int GetSequenceScore()
    {
        return _config != null ? _config.SequenceScore : 1000;
    }

    private int GetTripleScore()
    {
        return _config != null ? _config.TripleScore : 1500;
    }

    private float GetYakuBonus(YakuId yakuId)
    {
        return _config != null ? _config.GetBonus(yakuId) : 0f;
    }

    private string GetYakuName(YakuId yakuId)
    {
        return _config != null ? _config.GetDisplayName(yakuId) : yakuId.ToString();
    }
}
