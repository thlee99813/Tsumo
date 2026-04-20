using System.Collections.Generic;
using System.Text;

public class FireScoreResult
{
    public int BaseScoreTotal;
    public float YakuBonusSum;
    public float FinalMultiplier = 1f;
    public int FinalScore;
    public bool YakuApplied;

    public List<SquadScoreResult> SquadResults = new List<SquadScoreResult>();
    public List<YakuScoreResult> YakuResults = new List<YakuScoreResult>();

    public string BuildDebugText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[Fire Score Result]");

        for (int i = 0; i < SquadResults.Count; i++)
        {
            SquadScoreResult squad = SquadResults[i];
            string numbers = squad.SortedNumbers != null && squad.SortedNumbers.Length == 3
                ? $"{squad.SortedNumbers[0]} {squad.SortedNumbers[1]} {squad.SortedNumbers[2]}"
                : "-";

            if (!squad.IsValid)
            {
                sb.AppendLine($"- Squad {squad.SquadIndex + 1}: {numbers} -> 무효 (0)");
                continue;
            }

            if (squad.DoraBonusScore > 0)
            {
                sb.AppendLine($"- Squad {squad.SquadIndex + 1}: {squad.CardType} {numbers} -> {squad.ComboDisplayName} (기본 {squad.ComboScore} + 도라 {squad.DoraBonusScore} = {squad.BaseScore})");
            }
            else
            {
                sb.AppendLine($"- Squad {squad.SquadIndex + 1}: {squad.CardType} {numbers} -> {squad.ComboDisplayName} ({squad.BaseScore})");
            }
        }

        if (!YakuApplied)
        {
            sb.AppendLine("- 역: 미발동");
        }
        else
        {
            for (int i = 0; i < YakuResults.Count; i++)
            {
                YakuScoreResult yaku = YakuResults[i];
                sb.AppendLine($"- 역: {yaku.Name} (+{yaku.BonusMultiplier:0.##})");
            }
        }

        sb.AppendLine($"- 기본점수 합: {BaseScoreTotal}");
        sb.AppendLine($"- 역 보너스 합: +{YakuBonusSum:0.##}");
        sb.AppendLine($"- 최종 배율: x{FinalMultiplier:0.##}");
        sb.AppendLine($"- 최종 공격력: {FinalScore}");

        return sb.ToString();
    }
}
