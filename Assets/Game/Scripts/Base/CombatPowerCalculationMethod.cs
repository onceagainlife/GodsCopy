

namespace HuaHaiLiKanHua
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 技能战力评估结果
    /// </summary>
    public struct SkillPowerResult
    {
        public float dpsPower;          // 该技能提供的输出战力
        public float defensePower;      // 该技能提供的防御战力
        public float controlPower;      // 该技能提供的控制战力
    }
    /// <summary>
    /// 战斗力计算
    /// </summary>
    public static class CombatPowerCalculationMethod
    {
        public static int roundCount = 10;

        //所谓的增伤，是要其他算完之后再计算。
        //攻击力提升则是算之前计算。
        //技能伤害增加10%则是，基础伤害*（技能倍率+10%）

        /// <summary>
        /// 【基础】输出战力计算方法
        /// </summary>
        /// <param name="baseAtk">基础伤害</param>
        /// <param name="critRate">暴击概率</param>
        /// <param name="critDamage">暴击倍数</param>
        /// <param name="skillMultiplier">技能倍率</param>
        /// <param name="damageBonus">最终增伤</param>
        /// <returns></returns>
        public static float CalculateBaseAttackPower(float baseAtk, float critRate, float critDamage, float skillMultiplier=1, float damageBonus=0, float cooldown=0)
        {
            // 暴击期望乘区，基础概率1+概率*额外伤害的倍率
            float critExpectation = 1f + Mathf.Max(0, critRate) * Mathf.Max(0, critDamage - 1f);
            // 基础DPS = 基础攻击 × 暴击期望 × 增伤乘区
            float finalAttack = baseAtk * critExpectation * Mathf.Max(0, skillMultiplier) * (1f + Mathf.Max(0, damageBonus));

            return finalAttack > 0 ? cooldown > 0 ? finalAttack / cooldown : finalAttack:0;

            //return cooldown > 0 ? finalAttack / cooldown : finalAttack;
        }

        /// <summary>
        /// 【基础】防御战力计算方法
        /// 将角色的基础血量、防御力和百分比减伤，统一转化为“等效生命值(EHP)”
        /// EHP代表角色在承受敌方伤害时，实际需要被扣除的真实伤害总量。
        /// </summary>
        /// <param name="baseHp">角色的基础生命值面板</param>
        /// <param name="defConstant">等级常数，由被攻击者的等级决定</param>
        /// <param name="defense">角色的防御力/护甲值面板</param>
        /// <param name="dmgReduction">角色的固定百分比减伤 (0~1)，例如 0.2 代表 20% 减伤</param>
        /// <returns>最终计算得出的等效生命值战力分数</returns>
        public static float CalculateBaseDefensePower(float baseHp,float defConstant=100, float defense = 0, float dmgReduction=0)
        {
            // 将防御和减伤转化为等效生命值(EHP)
            float totalMitigation = 1f - Mathf.Clamp01(dmgReduction + defense / (defense + defConstant));

            float finalHp= totalMitigation > 0 ? baseHp / totalMitigation : baseHp;

            return finalHp;
        }
        /// <summary>
        /// 减伤,闪避EHP计算方法
        /// </summary>
        /// <param name="baseHp"></param>
        /// <param name="dmgReduction"></param>
        /// <returns></returns>
        public static float CalculateEHP(float baseHp, float dmgReduction = 0)
        {
            // 将防御和减伤转化为等效生命值(EHP)
            float totalMitigation = 1f - Mathf.Clamp01(dmgReduction);

            float finalHp = totalMitigation > 0 ? baseHp / totalMitigation : baseHp;

            return finalHp;
        }

        /// <summary>
        /// 综合战力评估入口，汇总基础面板与所有技能的战力得分
        /// </summary>
        /// <param name="baseAtk">角色基础攻击力面板</param>
        /// <param name="baseHp">角色基础生命值面板</param>
        /// <param name="def">角色防御力/护甲值面板</param>
        /// <param name="dmgReduction">角色基础减伤比例 (0~1)</param>
        /// <param name="critRate">角色基础暴击率 (0~1)</param>
        /// <param name="critDmg">角色基础暴击伤害倍率 (如 1.5 代表 150%)</param>
        /// <param name="dmgBonus">角色基础最终增伤乘区总和 (如 0.2 代表 +20%)</param>
        /// <param name="allSkillPowers">角色身上所有已计算完毕的技能战力结果集合</param>
        /// <param name="enemyDps">敌方预期单位时间输出(DPS)，用于动态折算控制类技能的等效战力</param>
        /// <returns>最终的综合战斗力评分（浮点数）</returns>
        public static class CombatPowerEvaluator
        {
            public static float GetTotalCombatPower(
                float baseAtk, float baseHp, float def, float dmgReduction, float critRate, float critDmg, float dmgBonus,
                List<SkillPowerResult> allSkillPowers, // 传入角色身上所有技能的计算结果
                float enemyDps)                        // 用于给控制技能打分
            {
                // 1. 基础面板战力
                float baseAtkPower = CombatPowerCalculationMethod.CalculateBaseAttackPower(baseAtk, critRate, critDmg, 1,dmgBonus);
                float baseDefPower = CombatPowerCalculationMethod.CalculateBaseDefensePower(baseHp, def, dmgReduction);

                // 2. 累加所有技能的额外战力
                float skillAtkPower = 0f, skillDefPower = 0f, controlPower = 0f;
                foreach (var skill in allSkillPowers)
                {
                    skillAtkPower += skill.dpsPower;
                    skillDefPower += skill.defensePower;
                    controlPower += skill.controlPower; // 控制战力在技能内部已经用 enemyDps 折算过了
                }

                // 3. 加权求和 (权重根据你的游戏类型调)
                float totalOutputScore = (baseAtkPower + skillAtkPower) * 5f;
                float totalSurvivalScore = (baseDefPower + skillDefPower) * 3f;
                float totalControlScore = controlPower * 2f;

                return totalOutputScore + totalSurvivalScore + totalControlScore; 
            }
        }
    }
}


