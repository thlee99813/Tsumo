using System.Collections.Generic;
using UnityEngine;

public class FireScoreContext
{
    public List<SquadScoreResult> SquadResults { get; } = new List<SquadScoreResult>();

    public bool AreAllSquadsValid(int requiredSquadCount)
    {
        if (SquadResults.Count != requiredSquadCount)
        {
            return false;
        }

        for (int i = 0; i < SquadResults.Count; i++)
        {
            if (!SquadResults[i].IsValid)
            {
                return false;
            }
        }

        return true;
    }

    public int GetMaxValidTypeComboCount()
    {
        int sword = 0;
        int kunai = 0;
        int foxSpirit = 0;

        for (int i = 0; i < SquadResults.Count; i++)
        {
            SquadScoreResult result = SquadResults[i];
            if (!result.IsValid)
            {
                continue;
            }

            switch (result.CardType)
            {
                case CardType.Sword:
                    sword++;
                    break;
                case CardType.Kunai:
                    kunai++;
                    break;
                case CardType.FoxSpirit:
                    foxSpirit++;
                    break;
            }
        }

        return Mathf.Max(sword, Mathf.Max(kunai, foxSpirit));
    }
}
