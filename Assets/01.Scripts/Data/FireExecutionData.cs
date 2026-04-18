public class FireExecutionData
{
    public bool IsFirePressed;
    public int FinalDamage;
    public FireScoreResult ScoreResult;

    public bool IsTimeout => !IsFirePressed;
}
