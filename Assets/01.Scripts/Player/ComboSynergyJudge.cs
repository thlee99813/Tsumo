using System.Collections.Generic;
using UnityEngine;

public enum SquadResultType
{
    None,
    NormalCombo,    // 연속 숫자 조합 (1-2-3, 2-3-4 등)
    EnhancedCombo   // 동일 숫자 조합 (1-1-1, 7-7-7 등)
}

public class ComboSynergyJudge : MonoBehaviour
{
    public struct SquadJudgement
    {
        public int SquadIndex;
        public CardType CardType;
        public SquadResultType ResultType;
        public bool HasSynergy;    // 같은 카드 종류 스쿼드가 2개 이상일 때 true
    }

    public List<SquadJudgement> Evaluate(FireScoreResult scoreResult)
    {
        var judgements = new List<SquadJudgement>();

        if (scoreResult == null)
            return judgements;

        // 1패스: 스쿼드별 콤보 종류 판단
        for (int i = 0; i < scoreResult.SquadResults.Count; i++)
        {
            SquadScoreResult squad = scoreResult.SquadResults[i];
            SquadResultType resultType = SquadResultType.None;

            if (squad.IsValid)
            {
                resultType = squad.ComboType == SquadComboType.Triple
                    ? SquadResultType.EnhancedCombo
                    : SquadResultType.NormalCombo;
            }

            judgements.Add(new SquadJudgement
            {
                SquadIndex = squad.SquadIndex,
                CardType = squad.CardType,
                ResultType = resultType,
                HasSynergy = false
            });
        }

        // 2패스: 유효한 스쿼드 중 같은 카드 종류가 2개 이상이면 시너지
        var validCountByType = new Dictionary<CardType, int>();
        for (int i = 0; i < judgements.Count; i++)
        {
            if (judgements[i].ResultType == SquadResultType.None) continue;
            CardType type = judgements[i].CardType;
            if (!validCountByType.ContainsKey(type))
                validCountByType[type] = 0;
            validCountByType[type]++;
        }

        for (int i = 0; i < judgements.Count; i++)
        {
            SquadJudgement j = judgements[i];
            if (j.ResultType != SquadResultType.None &&
                validCountByType.TryGetValue(j.CardType, out int count) && count >= 2)
            {
                j.HasSynergy = true;
                judgements[i] = j;
            }
        }

        // 디버그 로그 + 이펙트 재생
        for (int i = 0; i < judgements.Count; i++)
        {
            SquadJudgement j = judgements[i];
            string comboLabel = j.ResultType switch
            {
                SquadResultType.NormalCombo   => "일반 콤보",
                SquadResultType.EnhancedCombo => "강화 콤보",
                _                             => "무효"
            };
            string synergyLabel = j.HasSynergy ? " + 시너지" : "";
            Debug.Log($"[ComboSynergy] 스쿼드 {j.SquadIndex + 1} ({j.CardType}): {comboLabel}{synergyLabel}");

        }

        // 디버그 로그 - 시너지 발동 카드 종류
        bool anySynergy = false;
        foreach (var pair in validCountByType)
        {
            if (pair.Value >= 2)
            {
                Debug.Log($"[ComboSynergy] 시너지 발동: {pair.Key} ({pair.Value}스쿼드 동일 종류)");
                anySynergy = true;
            }
        }
        if (!anySynergy)
            Debug.Log("[ComboSynergy] 시너지: 없음");

        return judgements;
    }
}
