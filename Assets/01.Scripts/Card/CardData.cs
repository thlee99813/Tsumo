using UnityEngine;

public enum CardType { Sword, Kunai, FoxSpirit }
public enum CardSkill
{
    Batto, VerticalSlash, HorizontalSlash,
    ThrowOne, ThrowThree, ScatterThree,
    Heal, Buff, Debuff
}

[CreateAssetMenu(fileName = "CardData", menuName = "Tsumo/Card Data")]
public class CardData : ScriptableObject
{
    [SerializeField] private CardType _type;
    [SerializeField] private CardSkill _skill;

    public CardType Type => _type;
    public CardSkill Skill => _skill;
}
