using System.Collections.Generic;
using UnityEngine;

namespace BackPackLike
{
    public class CardTableManager : HuaHaiLiKanHua.Singleton<HandleGrid>
    {

        public struct LayoutInfo
        {
            public Vector2 position; // 卡片左上角的局部坐标
            public float rowHeight;  // 该卡片所在行的总高度
        }

        public static List<LayoutInfo> Calculate(List<BaseData> cards, float containerWidth, float cellUnit, float spacing)
        {
            List<LayoutInfo> layouts = new List<LayoutInfo>();
            if (cards == null || cards.Count == 0) return layouts;

            float currentX = 0f;
            float currentY = 0f;
            float currentRowMaxHeight = 0f;

            foreach (var card in cards)
            {
                float cardW = card.size.x * cellUnit;
                float cardH = card.size.y * cellUnit;

                // 1. 判断当前行是否放不下这张卡
                // (如果当前行有元素，且加上这张卡超出了容器宽度，则换行)
                if (currentX > 0 && currentX + cardW > containerWidth)
                {
                    currentY += currentRowMaxHeight + spacing; // 换行：Y轴下移
                    currentX = 0f;                             // 换行：X轴归零
                    currentRowMaxHeight = 0f;                  // 换行：重置行高
                }

                // 2. 记录当前位置
                layouts.Add(new LayoutInfo
                {
                    position = new Vector2(currentX, -currentY), // UI 坐标系 Y 轴向下为负
                    rowHeight = 0f // 稍后统一赋值
                });

                // 3. 累加当前行的 X 和 最大高度
                currentX += cardW + spacing;
                if (cardH > currentRowMaxHeight)
                {
                    currentRowMaxHeight = cardH;
                }
            }

            // 4. 将行高回写到属于该行的卡片上（为了滚动时对齐）
            // 这里为了简单演示，我们可以重新遍历一次来赋值行高，或者在计算时记录行索引
            // 实际项目中建议用一个 List<float> rowHeights 来存储
            return layouts;
        }

    }
}

