using System;
using UnityEngine;

namespace BackPackLike
{
    [CreateAssetMenu(fileName = "BaseData", menuName = "BackPackLike/BaseData")]
    [Serializable]
    public class BaseData 
    {
        [Header("ID")]
        public int Id;

        [Header("名字")]
        public string itemName;

        [Header("稀有度")]
        [Min(1)]
        public int rarity = 1;

        [Header("使用次数")]
        [Min(0)]
        public int usedAsMaterialCount;

        [Header("尺寸")]
        public Vector2 size = Vector2.one;

        [Header("类型")]
        public CardMaterialType itemType = CardMaterialType.None;

        [Header("费用")]
        [Min(0)]
        public int cost;

        [Header("预计战力")]
        [Min(0)]
        public int estimatedCombatPower;

        public string DisplayName => string.IsNullOrWhiteSpace(itemName) ? "未命名" : itemName;
    }
}
