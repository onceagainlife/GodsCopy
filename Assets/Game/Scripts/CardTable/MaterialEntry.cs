using System;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    /// 配方中的一条材料项：材料 + 数量。
    /// </summary>
    [Serializable]
    public class MaterialEntry
    {
        [Header("材料")]
        public BaseData material;

        [Header("数量")]
        [Min(1)]
        public int count = 1;

        public int MaterialId => material != null ? material.Id : 0;

        public string MaterialName => material != null ? material.DisplayName : "未设置材料";

        public int CombatPower => material != null ? material.estimatedCombatPower * Mathf.Max(1, count) : 0;

        public string GetDisplayText()
        {
            return count > 1 ? $"{MaterialName} x{count}" : MaterialName;
        }
    }
}
