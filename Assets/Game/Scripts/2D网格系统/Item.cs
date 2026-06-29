using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BackPackLike
{
    /// <summary>
    /// 物体基础类，保存放置优先级、占用格子和预览显示相关数据。
    /// </summary>
    public class Item : MonoBehaviour
    {
        [Tooltip("放置优先级。数值越高，越可以叠放在低优先级物体上方。")]
        public int placePriority = 10;

        [Tooltip("是否允许直接放置在固定网格上。背包扩展物或特殊道具可以开启。")]
        public bool placeOnFixedGrid;

        [Tooltip("拖拽预览时，是否让物体本身也跟随显示预览颜色。")]
        public bool showSelfPreviewColor;

        [Header("显示层级")]
        [Tooltip("当前物体自身显示层级。普通 CurrentCard 默认 -5，BackpackExtension 会在 SingletonInit 中改为 -10。")]
        public int selfSortingOrder = -5;

        [Tooltip("当前物体子物体显示层级。自动保持为自身层级 - 10。")]
        public int childSortingOrder = -15;

        [Tooltip("SingletonInit 时是否自动应用自身和子物体显示层级。")]
        public bool applySortingOrderOnAwake = true;

        [Tooltip("shader管理")]
        public ItemShader itemShader;

        /// <summary>
        /// 当前物体已经占据的所有网格。
        /// </summary>
        public List<CellClass> occupiedCells = new();

        // 缓存可染色组件和初始颜色，用于拖拽预览结束后还原。
        private readonly List<SpriteRenderer> spriteRenderers = new();
        private readonly List<Image> images = new();
        private readonly List<Color> spriteRendererColors = new();
        private readonly List<Color> imageColors = new();
        private readonly List<Canvas> canvases = new();
        private bool visualsCached;

        [Tooltip("是否被压住的判断")]
        public bool Oppressed=false;
        protected virtual void Awake()
        {
            CacheVisuals();

            if (applySortingOrderOnAwake)
            {
                ApplySortingOrder();
            }
        }

        /// <summary>
        /// 记录物体成功放置后占据的网格。
        /// </summary>
        public void SetOccupiedCells(List<CellClass> cells)
        {
            occupiedCells.Clear();
            occupiedCells.AddRange(cells);
        }

        /// <summary>
        /// 清空物体保存的占用网格记录。
        /// </summary>
        public void ClearOccupiedCells()
        {
            occupiedCells.Clear();
        }

        /// <summary>
        /// 设置物体本身的预览颜色。
        /// 当前采用的是对子物体 SpriteRenderer / Image 直接改颜色的方式。
        /// </summary>
        public void SetSelfPreview(Color color)
        {
            CacheVisuals();

            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = color;
                }
            }

            foreach (Image image in images)
            {
                if (image != null)
                {
                    image.color = color;
                }
            }
        }

        /// <summary>
        /// 清除物体本身的预览颜色，并还原为初始颜色。
        /// </summary>
        public void ClearSelfPreview()
        {
            CacheVisuals();

            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = spriteRendererColors[i];
                }
            }

            for (int i = 0; i < images.Count; i++)
            {
                if (images[i] != null)
                {
                    images[i].color = imageColors[i];
                }
            }
        }

        /// <summary>
        /// 应用当前物体和子物体的显示层级。
        /// SpriteRenderer 使用 sortingOrder；带 Canvas 的 UI 物体使用 Canvas.sortingOrder。
        /// 普通 Image 本身没有 sortingOrder，如果没有独立 Canvas，就仍由父 Canvas 和 Hierarchy 顺序控制。
        /// </summary>
        public void ApplySortingOrder()
        {
            CacheVisuals();

            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer == null)
                {
                    continue;
                }

                spriteRenderer.sortingOrder = spriteRenderer.transform == transform
                    ? selfSortingOrder
                    : childSortingOrder;
            }

            foreach (Canvas canvas in canvases)
            {
                if (canvas == null)
                {
                    continue;
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = canvas.transform == transform
                    ? selfSortingOrder
                    : childSortingOrder;
            }
        }

        #region 2D相关
        /// <summary>
        /// 设置并立即应用自身和子物体显示层级。
        /// 子物体层级固定为父物体层级 - 10。
        /// </summary>
        public void SetSortingOrder(int selfOrder)
        {
            selfSortingOrder = selfOrder;
            childSortingOrder = selfOrder - 10;
            ApplySortingOrder();
        }
        #endregion

        /// <summary>
        /// 缓存子物体上的 SpriteRenderer、Image 和 Canvas，避免拖拽时重复查找。
        /// </summary>
        private void CacheVisuals()
        {
            if (itemShader == null) itemShader = GetComponent<ItemShader>();

            if (visualsCached)
            {
                return;
            }

            visualsCached = true;
            GetComponentsInChildren(true, spriteRenderers);
            GetComponentsInChildren(true, images);
            GetComponentsInChildren(true, canvases);

            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRendererColors.Add(spriteRenderer != null ? spriteRenderer.color : Color.white);
            }

            foreach (Image image in images)
            {
                imageColors.Add(image != null ? image.color : Color.white);
            }
        }
    }
}
