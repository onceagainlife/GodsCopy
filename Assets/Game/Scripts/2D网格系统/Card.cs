using HuaHaiLiKanHua;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace BackPackLike
{
    /// <summary>
    /// UI card that keeps the card placement data and drag/rotate/place behavior.
    /// </summary>
    public class Card : Item, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Tooltip("当前卡牌占据的相对网格坐标。(0,0) 表示锚点格。")]
        public List<Vector2> offsets = new();

        [Tooltip("拖拽指针到卡牌 UI 锚点之间的偏移。")]
        public Vector2 offset;

        [Tooltip("当前旋转次数，每次代表顺时针 90 度。")]
        public int indexInAngle;

        [Tooltip("是否被选中。")]
        public bool choose;

        public TargetableObject obj;

        private readonly List<Vector2>[] rotatedOffsetCaches = new List<Vector2>[4];
        private bool rotatedOffsetsCached;
        private RectTransform rectTransformCache;
        
        private BoxCollider2D m_Collider2D;
        private Canvas m_Canvas;
        public HandleGrid m_HandleGrid;
        public bool Move()
        {
            return occupiedCells.Count == 0;
        }

        #region 生命周期
        protected override void Awake()
        {
            if (this is not BackpackExtension)
            {
                placePriority = 10;
                placeOnFixedGrid = false;
                selfSortingOrder = -80;
                childSortingOrder = selfSortingOrder - 10;
            }

            base.Awake();
        }

        private void Start()
        {
            CacheAllRotatedOffsets();
        }

        #endregion

        #region 变化
        /// <summary>
        /// 修改尺寸
        /// </summary>
        /// <param name="wide"></param>
        /// <param name="height"></param>
        public void ModifySize(int wide, int height)
        {
            GetCardRectTransform().sizeDelta = new Vector2(wide * GlobalStatic.cardSize, height * GlobalStatic.cardSize);
            GetCardCollider2D().size = GetCardRectTransform().sizeDelta;

            ComputerOffset(wide, height);


        }
        /// <summary>
        /// 计算偏移
        /// </summary>
        /// <param name="wide"></param>
        /// <param name="height"></param>
        public void ComputerOffset(int wide, int height)
        {
            offsets.Clear();
            GetCardRectTransform().localRotation = Quaternion.identity;
            for (int x = 0; x < wide; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float offsetX = x - (wide - 1) / 2f;
                    float offsetY = y - (height - 1) / 2f;

                    offsets.Add(new Vector2(offsetX, offsetY));
                }
            }
        }
        
        #endregion

        #region 点击
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            GlobalStatic.card = this;
            //调整父物体
            transform.SetParent(GetCardCanvas().transform, true);
            //transform.SetParent(GetCardCanvas().transform, false);
            //重置尺寸,因为父物体上会有缩放。
            GetCardRectTransform().localScale= Vector3.one;


            CacheUIDragOffset(eventData);

            if (Move())
            {
                transform.SetAsLastSibling();
                SetSortingOrder(0);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!Move())
            {
                return;
            }

            UpdateUIPosition(eventData);

            Backpack backpack = GetCurrentBackpack();
            backpack?.CheckPlaceUI(this, GetUIPosition(), out bool canPlace);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!Move())
            {
                GlobalStatic.card = null;
                return;
            }

            Backpack backpack = GetCurrentBackpack();
            if (backpack == null)
            {
                GlobalStatic.card = null;
                return;
            }

            if (backpack.TryPlaceCardUI(this, GetUIPosition(), out Vector2 anchorPosition))
            {
                SetUIPosition(anchorPosition);
            }
            else
            {
                backpack.ClearCurrentPlacementPreview();
                //放回手牌区域
                PutBackToHandle();
            }

            GlobalStatic.card = null;
        }
       
        /// <summary>
        /// Rotates the UI card and refreshes the UI grid placement preview.
        /// </summary>
        public void Rotation()
        {
            if (!Move())
            {
                return;
            }

            indexInAngle = (indexInAngle + 1) & 3;
            ApplyUIRotation();

            Backpack backpack = GetCurrentBackpack();
            backpack?.UpdatePlacementPreviewUI(this, GetUIPosition());
        }
        #endregion

        #region 2dUI共用
        public Backpack GetCurrentBackpack()
        {
            if (GameManager.GameLogic == null)
            {
                return null;
            }

            return GameManager.GameLogic.TryGetPlayerLogic(GlobalStatic.seetNember, out PlayerLogic playerLogic)
                ? playerLogic.backpack
                : null;
        }
        #endregion

        #region ui相关
        public void UpdateUIPosition(PointerEventData eventData)
        {
            RectTransform rectTransform = GetCardRectTransform();
            RectTransform parentRect = GetUIParentRectTransform();
            if (rectTransform == null || parentRect == null)
            {
                return;
            }

            Camera eventCamera = eventData.pressEventCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, out Vector2 localPoint))
            {
                rectTransform.anchoredPosition = localPoint - offset;
            }
        }

        public Vector2 GetUIPosition()
        {
            return transform.position;
            //RectTransform rectTransform = GetCardRectTransform();
            //return rectTransform != null ? rectTransform.anchoredPosition : (Vector2)transform.localPosition;
        }

        /// <summary>
        /// 放置
        /// </summary>
        /// <param name="anchoredPosition"></param>
        public void SetUIPosition(Vector2 anchoredPosition)
        {
            RectTransform rectTransform = GetCardRectTransform();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = anchoredPosition;
                return;
            }

            transform.localPosition = anchoredPosition;
        }

        public RectTransform GetUIParentRectTransform()
        {
            RectTransform rectTransform = GetCardRectTransform();
            return rectTransform != null ? rectTransform.parent as RectTransform : null;
        }

        /// <summary>
        /// Applies the placement result to one cell.
        /// </summary>
        public virtual void ApplyPlacement(CellClass cellClass)
        {
            cellClass.AddOccupant(this);
        }

        public List<Vector2> GetRotatedOffsets()
        {
            if (!rotatedOffsetsCached)
            {
                CacheAllRotatedOffsets();
            }

            return rotatedOffsetCaches[indexInAngle & 3];
        }

        public void SetPlacedCells(List<CellClass> cells)
        {
            SetOccupiedCells(cells);
            UpdateSortingOrder();
        }

        public void ClearPlacement()
        {
            foreach (CellClass cell in occupiedCells)
            {
                cell.RemoveOccupant(this);

                if (this is BackpackExtension extension && cell.backpackDl == extension)
                {
                    cell.backpackDl = null;
                }
            }

            ClearOccupiedCells();
            ApplyUnplacedSortingOrder();
        }
       
        private RectTransform GetCardRectTransform()
        {
            if (rectTransformCache == null)
            {
                rectTransformCache = transform as RectTransform;
            }

            return rectTransformCache;
        }
        private BoxCollider2D GetCardCollider2D()
        {
            if (m_Collider2D == null)
            {
                m_Collider2D=GetComponent<BoxCollider2D>();
            }
            return m_Collider2D;
        }

        /// <summary>
        /// 放回手牌
        /// </summary>
        private void PutBackToHandle()
        {
            if (m_HandleGrid == null) return;

            transform.SetParent(m_HandleGrid.transform,false);
        }

        private Canvas GetCardCanvas()
        {
            if (m_Canvas == null)
            {
                m_Canvas = GetComponentInParent<Canvas>();
            }
            return m_Canvas;
        }
        private void CacheUIDragOffset(PointerEventData eventData)
        {
            RectTransform rectTransform = GetCardRectTransform();
            RectTransform parentRect = GetUIParentRectTransform();
            if (rectTransform == null || parentRect == null)
            {
                offset = Vector2.zero;
                return;
            }

            Camera eventCamera = eventData.pressEventCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, out Vector2 localPoint))
            {
                offset = localPoint - rectTransform.anchoredPosition;
            }
        }

        private void ApplyUIRotation()
        {
            RectTransform rectTransform = GetCardRectTransform();
            if (rectTransform != null)
            {
                rectTransform.localEulerAngles = new Vector3(0, 0, indexInAngle * 90);
                return;
            }

            transform.eulerAngles = new Vector3(0, 0, indexInAngle * 90);
        }

        private void UpdateSortingOrder()
        {
            if (this is BackpackExtension)
            {
                SetSortingOrder(-180);
                transform.SetAsFirstSibling();
                return;
            }

            int maxCoveredOrder = int.MinValue;

            foreach (CellClass cell in occupiedCells)
            {
                foreach (Item item in cell.itemList)
                {
                    if (item == null || item == this)
                    {
                        continue;
                    }

                    maxCoveredOrder = Mathf.Max(maxCoveredOrder, item.selfSortingOrder);
                }
            }

            int placedOrder = maxCoveredOrder == int.MinValue ? -80 : maxCoveredOrder + 1;
            SetSortingOrder(placedOrder);
            transform.SetAsLastSibling();
        }

        private void ApplyUnplacedSortingOrder()
        {
            if (this is BackpackExtension)
            {
                SetSortingOrder(-180);
                transform.SetAsFirstSibling();
                return;
            }

            SetSortingOrder(-80);
        }

        private void CacheAllRotatedOffsets()
        {
            for (int angle = 0; angle < rotatedOffsetCaches.Length; angle++)
            {
                rotatedOffsetCaches[angle] = new List<Vector2>();

                if (offsets.Count == 0)
                {
                    rotatedOffsetCaches[angle].Add(Vector2.zero);
                    continue;
                }

                Vector2 xDirection = GlobalStatic.directionVector[angle];
                Vector2 yDirection = GlobalStatic.directionVector[(angle + 1) & 3];

                foreach (Vector2 itemOffset in offsets)
                {
                    Vector2 rotatedOffset = new Vector2(
                        itemOffset.x * xDirection.x + itemOffset.y * yDirection.x,
                        itemOffset.x * xDirection.y + itemOffset.y * yDirection.y
                    );

                    rotatedOffset.x = Mathf.Round(rotatedOffset.x * 1000f) / 1000f;
                    rotatedOffset.y = Mathf.Round(rotatedOffset.y * 1000f) / 1000f;

                    rotatedOffsetCaches[angle].Add(rotatedOffset);
                }
            }

            rotatedOffsetsCached = true;
        }
        #endregion

      
    }
}
