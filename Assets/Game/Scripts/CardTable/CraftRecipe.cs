using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BackPackLike
{
    [CreateAssetMenu(fileName = "CraftRecipe", menuName = "BackPackLike/CraftRecipe")]
    /// <summary>
    /// 一条合成配方：若干材料合成一个最终产物。
    /// </summary>
    [Serializable]
    public class CraftRecipe
    {
        [Header("配方ID")]
        public int Id;

        [Header("合成表名字")]
        public string recipeName;

        [Header("合成结果")]
        public BaseData result;

        [Header("合成结果数量")]
        [Min(1)]
        public int resultCount = 1;

        [Header("所需材料")]
        public List<MaterialEntry> materials = new();

        public string DisplayName => string.IsNullOrWhiteSpace(recipeName)
            ? (result != null ? result.DisplayName : "未命名配方")
            : recipeName;

        public int Rarity => result != null ? result.rarity : 0;

        public void RefreshEstimatedCombatPower()
        {
            if (result == null) return;
            result.estimatedCombatPower = CalculateEstimatedCombatPower();
        }

        public int CalculateEstimatedCombatPower()
        {
            int totalPower = 0;
            if (materials == null)
            {
                return totalPower;
            }

            foreach (MaterialEntry entry in materials)
            {
                if (entry?.material == null)
                {
                    continue;
                }

                totalPower += entry.material.estimatedCombatPower * Mathf.Max(1, entry.count);
            }

            return totalPower;
        }

        public string GetFormulaText()
        {
            StringBuilder builder = new();
            builder.Append(result != null ? result.DisplayName : "未设置产物");

            if (resultCount > 1)
            {
                builder.Append(" x").Append(resultCount);
            }

            builder.Append(" = ");

            if (materials == null || materials.Count == 0)
            {
                builder.Append("无材料");
                return builder.ToString();
            }

            for (int i = 0; i < materials.Count; i++)
            {
                MaterialEntry entry = materials[i];
                if (i > 0)
                {
                    builder.Append(" + ");
                }

                builder.Append(entry != null ? entry.GetDisplayText() : "空材料");
            }

            return builder.ToString();
        }

        private void OnValidate()
        {
            resultCount = Mathf.Max(1, resultCount);

            if (materials != null)
            {
                foreach (MaterialEntry entry in materials)
                {
                    if (entry != null)
                    {
                        entry.count = Mathf.Max(1, entry.count);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(recipeName) && result != null)
            {
                recipeName = result.DisplayName;
            }

            RefreshEstimatedCombatPower();
        }
    }
}
