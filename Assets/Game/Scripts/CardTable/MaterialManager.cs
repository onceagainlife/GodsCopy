using System;
using System.Collections.Generic;
using System.Reflection;
using HuaHaiLiKanHua;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    /// 材料管理类：用 List 保存所有材料，维护按类型自增的全局唯一 Id，
    /// 并提供 Id→材料 的字典索引。
    /// 材料 Id 只增不减（即使删除材料，Id 也不回收）。
    ///
    /// Id 编码规则：Id = 类型索引 * IdTypeOffset + 类型内自增计数
    /// 不同类型的 Id 互不冲突，从而保证全局唯一。
    /// </summary>
    [Serializable]
    public class MaterialManager
    {
        /// <summary>
        /// 类型 Id 偏移量：每类型预留的 Id 区间大小。
        /// </summary>
        private const int IdTypeOffset = 10000;

        [Header("所有材料")]
        [SerializeField]
        private List<BaseData> materials = new();

        [Header("各类型下一个可用计数（索引=CardMaterialType值）")]
        [SerializeField]
        private int[] nextIdsByType = new int[6] { 1, 1, 1, 1, 1, 1 };

        /// <summary>
        /// Id→材料 字典（运行时索引，不参与序列化）
        /// </summary>
        [NonSerialized]
        private Dictionary<int, BaseData> materialDic;

        /// <summary>
        /// 所有材料（只读访问）
        /// </summary>
        public IReadOnlyList<BaseData> Materials => materials;

        /// <summary>
        /// 材料数量
        /// </summary>
        public int Count => materials != null ? materials.Count : 0;

        /// <summary>
        /// 初始化：根据 List 生成 Id→材料 字典。
        /// 不校正 nextIdsByType（自增计数只增不减）。
        /// 应在数据加载完成后调用。
        /// </summary>
        public void Init()
        {
            // 确保字典已创建（CreateDic 是静态方法，非扩展方法，需用类名调用）
            DictionaryExtensions.CreateDic(ref materialDic);
            materialDic.Clear();

            // 用扩展方法把 list 批量加入字典，键为 material.Id
            materialDic.AddRangeAsLookup(materials, m => m.Id);
        }

        /// <summary>
        /// 添加新材料：按类型分配全局唯一 Id，加入 List 和字典，并返回该材料。
        /// </summary>
        public BaseData AddMaterial(BaseData material)
        {
            if (material == null) return null;

            int typeIndex = (int)material.itemType;
            EnsureNextIdsArraySize(typeIndex);

            int perTypeCount = nextIdsByType[typeIndex]++;
            material.Id = typeIndex * IdTypeOffset + perTypeCount;

            materials ??= new List<BaseData>();
            materials.Add(material);

            materialDic ??= new Dictionary<int, BaseData>();
            materialDic[material.Id] = material;

            return material;
        }

        /// <summary>
        /// 根据 Id 获取材料。
        /// </summary>
        public BaseData GetMaterial(int id)
        {
            if (materialDic != null && materialDic.TryGetValue(id, out BaseData m))
            {
                return m;
            }
            return null;
        }

        /// <summary>
        /// 根据 Id 移除材料（Id 不会回收，nextIdsByType 不回退）。
        /// </summary>
        public bool RemoveMaterial(int id)
        {
            if (materialDic == null) return false;
            if (!materialDic.TryGetValue(id, out BaseData m)) return false;

            materialDic.Remove(id);
            materials.Remove(m);
            return true;
        }

        /// <summary>
        /// 判断指定 Id 的材料是否存在。
        /// </summary>
        public bool Contains(int id)
        {
            return materialDic != null && materialDic.ContainsKey(id);
        }

        /// <summary>
        /// 获取指定类型下一个将分配的 Id（不消耗）。
        /// </summary>
        public int PeekNextId(CardMaterialType type)
        {
            int typeIndex = (int)type;
            EnsureNextIdsArraySize(typeIndex);
            return typeIndex * IdTypeOffset + nextIdsByType[typeIndex];
        }

        /// <summary>
        /// 确保计数数组容量覆盖指定类型索引。
        /// 若枚举后续扩展了新类型，自动扩容数组。
        /// </summary>
        private void EnsureNextIdsArraySize(int typeIndex)
        {
            if (nextIdsByType == null)
            {
                nextIdsByType = new int[Math.Max(typeIndex + 1, 6)];
                for (int i = 0; i < nextIdsByType.Length; i++) nextIdsByType[i] = 1;
                return;
            }

            if (typeIndex < nextIdsByType.Length) return;

            int newLen = typeIndex + 1;
            int[] newArr = new int[newLen];
            for (int i = 0; i < newLen; i++)
            {
                newArr[i] = i < nextIdsByType.Length ? nextIdsByType[i] : 1;
            }
            nextIdsByType = newArr;
        }



        #region  筛选排序

        /// <summary>
        /// 按稀有度筛选并按类型分组，组内按指定字段排序。
        /// </summary>
        public Dictionary<CardMaterialType, List<BaseData>> GetGroupedByType<TField>(
            int rarity, Func<BaseData, TField> keySelector, bool ascending = true)
        {
            var result = GetGroupedByType(rarity);
            SortGroupedLists(result, keySelector, ascending);
            return result;
        }

        /// <summary>
        /// 按稀有度筛选，并按类型分组（"选择稀有度→按类型显示"模式）。
        /// 组内按 Id 升序。返回字典的每个分组 Count 即为该类型下卡牌数量。
        /// </summary>
        /// <param name="rarity">目标稀有度</param>
        private Dictionary<CardMaterialType, List<BaseData>> GetGroupedByType(int rarity)
        {
            var result = new Dictionary<CardMaterialType, List<BaseData>>();
            ListExtensions.TraversalList(materials, m =>
            {
                if (m != null && m.rarity == rarity)
                {
                    result.AddToCollection(m.itemType, m);
                }
            });
            return result;
        }

        /// <summary>
        /// 按类型筛选并按稀有度分组，组内按指定字段排序。
        /// </summary>
        public Dictionary<int, List<BaseData>> GetGroupedByRarity<TField>(
            CardMaterialType type, Func<BaseData, TField> keySelector, bool ascending = true)
        {
            var result = GetGroupedByRarity(type);
            SortGroupedLists(result, keySelector, ascending);
            return result;
        }

        /// <summary>
        /// 按类型筛选，并按稀有度分组（"选择类型→按稀有度显示"模式）。
        /// 组内按 Id 升序。返回字典的每个分组 Count 即为该稀有度下卡牌数量。
        /// </summary>
        /// <param name="type">目标类型</param>
        private Dictionary<int, List<BaseData>> GetGroupedByRarity(CardMaterialType type)
        {
            var result = new Dictionary<int, List<BaseData>>();
            ListExtensions.TraversalList(materials, m =>
            {
                if (m != null && m.itemType == type)
                {
                    result.AddToCollection(m.rarity, m);
                }
            });
            return result;
        }


        /// <summary>
        /// 对分组字典中每个列表按指定键排序。
        /// </summary>
        private static void SortGroupedLists<TGroupKey, TField>(
            Dictionary<TGroupKey, List<BaseData>> grouped,
            Func<BaseData, TField> keySelector, bool ascending)
        {
            if (grouped == null || keySelector == null) return;
            IComparer<TField> comparer = Comparer<TField>.Default;
            foreach (var pair in grouped)
            {
                pair.Value.Sort((a, b) =>
                {
                    TField va = a != null ? keySelector(a) : default;
                    TField vb = b != null ? keySelector(b) : default;
                    int r = comparer.Compare(va, vb);
                    return ascending ? r : -r;
                });
            }
        }
        /// <summary>
        /// 根据任意字段名对材料排序（反射方式）。
        /// 字段不存在时输出警告并跳过。
        /// </summary>
        /// <param name="fieldName">BaseData 上的公共字段名</param>
        /// <param name="ascending">是否升序，默认 true</param>
        public void SortBy(string fieldName, bool ascending = true)
        {
            if (materials == null || materials.Count == 0) return;

            FieldInfo field = typeof(BaseData).GetField(fieldName);
            if (field == null)
            {
                Debug.LogWarning($"MaterialManager.SortBy: BaseData 上找不到字段 '{fieldName}'");
                return;
            }

            materials.Sort((a, b) =>
            {
                object va = a != null ? field.GetValue(a) : null;
                object vb = b != null ? field.GetValue(b) : null;
                int result = CompareValues(va, vb);
                return ascending ? result : -result;
            });
        }

        /// <summary>
        /// 根据选择器对材料排序（类型安全方式，推荐）。
        /// </summary>
        /// <typeparam name="TField">排序键类型（需实现 IComparable）</typeparam>
        /// <param name="keySelector">从材料提取排序键的函数</param>
        /// <param name="ascending">是否升序，默认 true</param>
        public void SortBy<TField>(Func<BaseData, TField> keySelector, bool ascending = true)
        {
            if (materials == null || materials.Count == 0 || keySelector == null) return;

            IComparer<TField> comparer = Comparer<TField>.Default;
            materials.Sort((a, b) =>
            {
                TField va = a != null ? keySelector(a) : default;
                TField vb = b != null ? keySelector(b) : default;
                int result = comparer.Compare(va, vb);
                return ascending ? result : -result;
            });
        }

        /// <summary>
        /// 比较两个任意类型的值（支持 null 和 IComparable）。
        /// </summary>
        private static int CompareValues(object a, object b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            if (a is IComparable c) return c.CompareTo(b);
            return string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal);
        }

        #endregion
    }
}
