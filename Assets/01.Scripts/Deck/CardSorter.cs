using System.Collections.Generic;

public class CardSorter
{
    public void Sort(List<CardData> cards)
    {
        if (cards == null || cards.Count <= 1)
        {
            return;
        }

        cards.Sort(Compare);
    }

    private int Compare(CardData left, CardData right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left == null) return 1;
        if (right == null) return -1;

        int typeCompare = GetTypeOrder(left.Type).CompareTo(GetTypeOrder(right.Type));
        if (typeCompare != 0)
        {
            return typeCompare;
        }

        return left.Number.CompareTo(right.Number);
    }

    private int GetTypeOrder(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Sword: return 0;     
            case CardType.Kunai: return 1;   
            case CardType.FoxSpirit: return 2; 
            default: return 99;
        }
    }
}
