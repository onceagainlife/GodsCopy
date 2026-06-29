using System.Collections.Generic;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    /// 网格数据，保存坐标、实际位置、占用物体和背包扩展物关系。
    /// </summary>
    public class CellClass
    {
        [Tooltip("网格相对坐标。")]
        public Vector2Int posiInDic;

        [Tooltip("网格在世界坐标中的位置。")]
        public Vector2 truePos;

        [Tooltip("让该格子变为可放置区域的背包扩展物。")]
        public BackpackExtension backpackDl;
        /// <summary>
        /// 当前占据该格子的物体列表，后加入的物体代表叠放优先级更高。
        /// </summary>
        public List<Card> itemList = new();

        /// <summary>
        /// 使用相对坐标创建网格数据。
        /// </summary>
        public CellClass(Vector2Int pos)
        {
            posiInDic = pos;
        }

        /// <summary>
        /// 无参构造，保留给序列化或临时创建使用。
        /// </summary>
        public CellClass()
        {
        }

        /// <summary>
        /// 兼容旧逻辑：检查该格子是否完全没有占用。
        /// </summary>
        public bool CheckOccupation()
        {
            return itemList.Count == 0;
        }

        /// <summary>
        /// 判断该格子的区域类型是否允许放入指定物体。
        /// </summary>
        public bool CanAcceptItemType(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning($"[CellClass.CanAcceptItemType] 格子 {posiInDic} 判断失败：item 为 null。");
                return false;
            }

            bool allowed = item.placeOnFixedGrid || backpackDl != null;
            if (!allowed)
            {
                Debug.LogWarning($"[CellClass.CanAcceptItemType] 格子 {posiInDic} 不允许放入 \"{item.name}\"：placeOnFixedGrid={item.placeOnFixedGrid}, backpackDl={(backpackDl != null ? backpackDl.name : "null")}。");
            }
            return allowed;
        }

        /// <summary>
        /// 判断指定物体是否能按照优先级叠放到当前格子的最上层。
        /// </summary>
        public bool CanStack(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning($"[CellClass.CanStack] 格子 {posiInDic} 判断失败：item 为 null。");
                return false;
            }

            if (itemList.Count == 0)
            {
                return true;
            }

            Item topItem = itemList[itemList.Count - 1];
            bool canStack = topItem == null || item.placePriority > topItem.placePriority;
            if (!canStack)
            {
                string topName = topItem != null ? $"\"{topItem.name}\"(优先级{topItem.placePriority})" : "null";
                Debug.LogWarning($"[CellClass.CanStack] 格子 {posiInDic} 不允许叠放：尝试放入 \"{item.name}\"(优先级{item.placePriority})，但当前最上层是 {topName}。");
            }
            return canStack;
        }

        /// <summary>
        /// 判断指定物体是否能放到当前格子。
        /// </summary>
        public PlaceMode CanPlace(Item item)
        {
            //如果是背包扩展物
            if (item.placeOnFixedGrid)
            {
                //已被占据
                if (backpackDl != null)
                    return PlaceMode.Occupied;
                else
                   //可以放置
                    return PlaceMode.Valid;
            }

            //非被背包扩展物,背包扩展物为空
            if (backpackDl == null) return PlaceMode.Empty;

            //并且非空
            bool canStack = CanStack(item);
            //没有放置或者可以压住
            if (canStack)
                return PlaceMode.Valid;
            else
                //已被占据
                return PlaceMode.Occupied;
        }

        /// <summary>
        /// 把物体加入该格子的占用列表。
        /// </summary>
        public void AddOccupant(Card item)
        {
            if (item == null || itemList.Contains(item))
            {
                return;
            }
            if (itemList.Count > 0)
            {
                itemList[itemList.Count - 1].itemShader.Oppressed();
                itemList[itemList.Count - 1].Oppressed = true;
            }
                

            itemList.Add(item);
        }

        /// <summary>
        /// 从该格子的占用列表移除物体。
        /// </summary>
        public void RemoveOccupant(Card item)
        {
            itemList.Remove(item);
        }
    }
}
