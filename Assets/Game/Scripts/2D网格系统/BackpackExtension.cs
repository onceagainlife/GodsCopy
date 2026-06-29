using UnityEngine;
using UnityEngine.EventSystems;

namespace BackPackLike
{
    /// <summary>
    /// 背包扩展物。放到固定网格后，会把自身占据的格子开放为可放置区域。
    /// </summary>
    public class BackpackExtension : Card
    {
        ///// <summary>
        ///// 已经放置在该背包扩展区域上的物体列表。
        ///// </summary>
        //public List<Item> placedItems = new();
        ///// <summary>
        ///// 记录一个放置到该扩展物区域上的物体。
        ///// </summary>
        //public void AddPlacedItem(Item item)
        //{
        //    if (item != null && !placedItems.Contains(item))
        //    {
        //        placedItems.Add(item);
        //    }
        //}

        ///// <summary>
        ///// 从该扩展物的已放置列表中移除物体。
        ///// </summary>
        //public void RemovePlacedItem(Item item)
        //{
        //    if (item != null)
        //    {
        //        placedItems.Remove(item);
        //    }
        //}

        /// <summary>
        /// 初始化背包扩展物的默认放置规则。
        /// </summary>
        protected override void Awake()
        {
            placeOnFixedGrid = true;
            placePriority = 0;
            selfSortingOrder = -180;
            childSortingOrder = selfSortingOrder - 10;
            showSelfPreviewColor = true;
            base.Awake();
        }


        public override void ApplyPlacement(CellClass cellClass)
        {
            if (cellClass.backpackDl == null)
                cellClass.backpackDl = this;
            else Debug.LogError($"错误,尝试堆叠");
        }
 

        //public override void OnPointerUp(PointerEventData eventData)
        //{
        //    if (eventData.button != PointerEventData.InputButton.Left)
        //    {
        //        return;
        //    }

        //    Backpack backpack = GetCurrentBackpack();
        //    if (backpack != null && backpack.TryPlaceCard(this, transform.position, out Vector3 anchorWorldPosition))
        //    {
        //        transform.position = anchorWorldPosition;
        //    }
        //    else
        //    {
        //        backpack?.ClearCurrentPlacementPreview();
        //    }

        //    GlobalStatic.card = null;
        //}

        //背包乱斗原版,背包扩展物是可以被移动的。
        //需要添加一个方法，当玩具移动BackpackExtension时，同步移动放置在上方的item
        //放松开手后，再遍历这些item，重新放置，如果某一部分由于周围没有别的BackpackExtension时，那就
        //把他们弹回到未放置区域。
        //不过我的设计是不能移动背包扩展物，因此不需要这个。
    }
}
