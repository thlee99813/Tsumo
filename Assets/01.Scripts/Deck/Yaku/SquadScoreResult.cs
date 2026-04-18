public class SquadScoreResult
{
    public int SquadIndex;
    public bool IsValid;
    public SquadComboType ComboType;
    public CardType CardType;
    public int[] SortedNumbers = new int[0];
    public int BaseScore;

    public string ComboDisplayName => ComboType switch
    {
        SquadComboType.Sequence => "일반 콤보",
        SquadComboType.Triple => "강화 콤보",
        _ => "무효"
    };
}
