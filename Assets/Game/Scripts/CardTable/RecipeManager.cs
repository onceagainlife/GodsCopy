using System;
using System.Collections.Generic;
using HuaHaiLiKanHua;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    /// 配方管理类：用 List 保存所有配方，维护 Id→配方 字典，
    /// 并维护 材料 Id→使用该材料的配方列表 的反向索引。
    /// </summary>
    [Serializable]
    public class RecipeManager
    {
        [Header("所有配方")]
        [SerializeField]
        private List<CraftRecipe> recipes = new();

        [Header("下一个可用配方 Id")]
        [SerializeField]
        private int nextRecipeId = 1;

        /// <summary>
        /// 配方 Id→配方 字典（运行时索引，不参与序列化）
        /// </summary>
        [NonSerialized]
        private Dictionary<int, CraftRecipe> recipeDic;

        /// <summary>
        /// 材料 Id→使用该材料的配方列表（运行时反向索引，不参与序列化）
        /// </summary>
        [NonSerialized]
        private Dictionary<int, List<CraftRecipe>> materialToRecipes;

        /// <summary>
        /// 所有配方（只读访问）
        /// </summary>
        public IReadOnlyList<CraftRecipe> Recipes => recipes;

        /// <summary>
        /// 配方数量
        /// </summary>
        public int Count => recipes != null ? recipes.Count : 0;

        /// <summary>
        /// 初始化：根据 List 生成 Id→配方 字典，以及 材料→配方 反向索引。
        /// 不校正 nextRecipeId（自增计数只增不减）。
        /// 应在数据加载完成后调用。
        /// </summary>
        public void Init()
        {
            // 确保字典已创建
            DictionaryExtensions.CreateDic(ref recipeDic);
            recipeDic.Clear();

            DictionaryExtensions.CreateDic(ref materialToRecipes);
            materialToRecipes.Clear();

            // 用扩展方法把 list 批量加入字典，键为 recipe.Id
            recipeDic.AddRangeAsLookup(recipes, r => r.Id);

            // 构建 材料 Id→配方列表 反向索引
            ListExtensions.TraversalList(recipes, recipe =>
            {
                if (recipe?.materials == null) return;
                ListExtensions.TraversalList(recipe.materials, entry =>
                {
                    if (entry?.material == null) return;
                    int materialId = entry.material.Id;
                    if (!materialToRecipes.TryGetValue(materialId, out List<CraftRecipe> list))
                    {
                        list = new List<CraftRecipe>();
                        materialToRecipes[materialId] = list;
                    }
                    if (!list.Contains(recipe))
                    {
                        list.Add(recipe);
                    }
                });
            });
        }

        /// <summary>
        /// 添加新配方：自动分配 Id，加入 List、字典和反向索引，
        /// 并增加各材料的 usedAsMaterialCount，返回该配方。
        /// </summary>
        public CraftRecipe AddRecipe(CraftRecipe recipe)
        {
            if (recipe == null) return null;

            recipe.Id = nextRecipeId++;
            recipes ??= new List<CraftRecipe>();
            recipes.Add(recipe);

            recipeDic ??= new Dictionary<int, CraftRecipe>();
            recipeDic[recipe.Id] = recipe;

            // 注册反向索引并累加材料使用次数
            if (recipe.materials != null)
            {
                materialToRecipes ??= new Dictionary<int, List<CraftRecipe>>();
                foreach (MaterialEntry entry in recipe.materials)
                {
                    if (entry?.material == null) continue;
                    int materialId = entry.material.Id;

                    if (!materialToRecipes.TryGetValue(materialId, out List<CraftRecipe> list))
                    {
                        list = new List<CraftRecipe>();
                        materialToRecipes[materialId] = list;
                    }
                    if (!list.Contains(recipe))
                    {
                        list.Add(recipe);
                    }

                    // 增加材料的使用次数
                    entry.material.usedAsMaterialCount += Mathf.Max(1, entry.count);
                }
            }

            // 刷新产物预计战力
            recipe.RefreshEstimatedCombatPower();

            return recipe;
        }

        /// <summary>
        /// 根据 Id 获取配方。
        /// </summary>
        public CraftRecipe GetRecipe(int id)
        {
            if (recipeDic != null && recipeDic.TryGetValue(id, out CraftRecipe r))
            {
                return r;
            }
            return null;
        }

        /// <summary>
        /// 获取使用指定材料 Id 的所有配方（点击卡牌后展示相关配方用）。
        /// 返回只读列表；若无则返回空列表。
        /// </summary>
        public IReadOnlyList<CraftRecipe> GetRecipesByMaterial(int materialId)
        {
            if (materialToRecipes != null && materialToRecipes.TryGetValue(materialId, out List<CraftRecipe> list))
            {
                return list;
            }
            return Array.Empty<CraftRecipe>();
        }

        /// <summary>
        /// 根据 Id 移除配方（Id 不会回收，nextRecipeId 不回退）。
        /// 同步从反向索引中移除，并回退材料的 usedAsMaterialCount。
        /// </summary>
        public bool RemoveRecipe(int id)
        {
            if (recipeDic == null) return false;
            if (!recipeDic.TryGetValue(id, out CraftRecipe r)) return false;

            recipeDic.Remove(id);
            recipes.Remove(r);

            // 从反向索引中移除，并回退材料的 usedAsMaterialCount
            if (materialToRecipes != null && r.materials != null)
            {
                foreach (MaterialEntry entry in r.materials)
                {
                    if (entry?.material == null) continue;
                    int materialId = entry.material.Id;

                    // 回退材料使用次数（不低于 0）
                    int decrement = Mathf.Max(1, entry.count);
                    entry.material.usedAsMaterialCount = Mathf.Max(0, entry.material.usedAsMaterialCount - decrement);

                    if (materialToRecipes.TryGetValue(materialId, out List<CraftRecipe> list))
                    {
                        list.Remove(r);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 判断指定 Id 的配方是否存在。
        /// </summary>
        public bool Contains(int id)
        {
            return recipeDic != null && recipeDic.ContainsKey(id);
        }
    }
}
