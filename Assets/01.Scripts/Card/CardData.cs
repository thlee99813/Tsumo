using UnityEngine;

public enum CardType { Sword, Kunai, FoxSpirit }
public enum CardSkill
{
    DownSlash, VerticalSlash, UpperSlash,
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

    public string TypeDisplayName => _type 
    switch
    {
        CardType.Sword => "검",
        CardType.Kunai => "쿠나이",
        CardType.FoxSpirit => "여우신령",
        _ => _type.ToString()
    };

    public string SkillDisplayName => _skill 
    switch
    {
        CardSkill.DownSlash => "내려베기",
        CardSkill.UpperSlash => "올려베기",
        CardSkill.VerticalSlash => "가로베기",
        CardSkill.ThrowOne => "1개 던지기",
        CardSkill.ThrowThree => "3개 던지기",
        CardSkill.ScatterThree => "3개 뿌리기",
        CardSkill.Heal => "힐",
        CardSkill.Buff => "버프",
        CardSkill.Debuff => "디버프",
        _ => _skill.ToString()
    };

}
