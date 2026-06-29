
using Sirenix.OdinInspector;
using UnityEngine;


/// <summary>
/// 技能效果的目标属性,凡是需要直接削减的，都需要
/// </summary>
public enum Attribute
{
    None,
    [LabelText("等级")]
    Grade,//这里的等级其实是稀有度，两者混用了。
    [LabelText("生命值")]
    HP,
    [LabelText("怒气值")]
    MP,
    [LabelText("生命上限")]
    MaxHp,
    [LabelText("怒气上限")]
    MaxMP,
    [LabelText("物攻")]
    ATK,
    [LabelText("物防")]
    DEF,
    [LabelText("法强")]
    MATK,
    [LabelText("法防")]
    RES,
    [LabelText("移动距离")]
    SPD,
    [LabelText("攻击方向")]
    AttDir,
    [LabelText("命中")]
    HIT,
    [LabelText("闪避")]
    DOD,
    [LabelText("暴击")]
    CRI,
    [LabelText("暴击倍数")]
    CHM,
    [LabelText("防爆")]
    ANTI_RIOT,
    [LabelText("攻击距离")]
    AttackRange,
    [LabelText("连击")]
    ATTACK_COUNT,
    [LabelText("攻击目标")]
    Target_COUNT,
    [LabelText("护盾")]
    Shield,
    [LabelText("治疗")]
    Healing,
    [LabelText("怒气获取")]
    AngerRate,
    [LabelText("冷却时间")]
    CoolTime,
    [LabelText("伤害减免")]
    Derate,
    [LabelText("伤害")]
    damage,
    [LabelText("攻击加成")]
    damageAddition,
    [LabelText("最大护盾数量")]
    MaxShield,
    [LabelText("售价")]
    SellingPrice,
}
