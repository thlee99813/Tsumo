using UnityEngine;

public enum CardType
{
    Sword,
    Kunai,
    FoxSpirit
}


[CreateAssetMenu(fileName = "CardData", menuName = "Tsumo/Card Data")]
public class CardData : ScriptableObject
{
    [SerializeField] private CardType _type;
    [SerializeField] private int _number = 1;

    public CardType Type => _type;
    public int Number => _number;

    public string TypeDisplayName => _type 
    switch
    {
        CardType.Sword => "검",
        CardType.Kunai => "쿠나이",
        CardType.FoxSpirit => "여우신령",
        _ => _type.ToString()
    };

    public string NumberDisplayName => _number.ToString();


}
