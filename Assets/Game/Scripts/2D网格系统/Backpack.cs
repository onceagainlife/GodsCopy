using HuaHaiLiKanHua;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BackPackLike
{
    /// <summary>
    /// Maintains backpack grid data, placement validation, preview, and battle order.
    /// </summary>
    public class Backpack : MonoBehaviour,IPointerDownHandler
    {
        [Tooltip("所有固定网格的数据数组。")]
        public CellClass[,] allCells;

        [Tooltip("固定网格尺寸。")]
        public Vector2Int side = new Vector2Int(3, 3);

        [Tooltip("每个逻辑格子的宽高。")]
        public Vector2 cellSize = Vector2.one;

        [Tooltip("网格原点在背包 RectTransform 本地坐标中的位置。")]
        public Vector2 gridOriginLocalPosition;

        [Tooltip("UI 格子 Y 轴是否向下增长。")]
        public bool uiYAxisDown = false;

        [Header("调试显示")]
        [Tooltip("是否在 Scene 视图中显示旧 2D 逻辑网格。")]
        public bool showGridGizmos = true;

        [Tooltip("逻辑网格线颜色。")]
        public Color gridGizmoColor = new Color(0.1f, 0.8f, 1f, 0.65f);

        [Tooltip("可放置区域格子的调试颜色。")]
        public Color placeableGizmoColor = new Color(0.1f, 1f, 0.2f, 0.35f);

        [Tooltip("旧 2D 调试显示在世界坐标 Z 轴上的偏移。")]
        public float gizmoZOffset;

        public List<CellStruct> cellStructList = new();

        [Tooltip("可放置地显示视图列表")]//当判断一个格子可放置时，会在这个列表里添加一个预设的显示对象来显示放置预览。
        private readonly List<CellClass> pendingPlacementCells = new(20);
        [Tooltip("UI网格系统")]
        public GridUISystem gridUISystem;
        [Tooltip("缓存的RectTransform组件")]
        private RectTransform rectTransformCache;
        [Tooltip("当前移动的卡牌")]
        private Card previewCard;
        [Tooltip("测试用物体")]
        public GameObject obj;
        protected virtual void Awake()
        {
            InitGrid();
        }

        #region 初始化

        /// <summary>
        /// Initializes the UI grid from GridUISystem when it is present.
        /// </summary>
        public void InitGrid()
        {
            gridUISystem = GetComponent<GridUISystem>();

            if (gridUISystem != null)
            {
                side = gridUISystem.gridSize;
                cellSize = gridUISystem.cellSize;
            }

            if (side.x <= 0 || side.y <= 0)
            {
                allCells = new CellClass[0, 0];
                return;
            }

            allCells = new CellClass[side.x, side.y];

            for (int x = 0; x < allCells.GetLength(0); x++)
            {
                for (int y = 0; y < allCells.GetLength(1); y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    CellClass cell = new CellClass(pos)
                    {
                        truePos = GridToUIWorldPosition(pos)
                    };

                    allCells[x, y] = cell;

                    Debug.Log($"Cell Position: {cell.posiInDic}, True Position: {cell.truePos}");
                    //GameObject go = Instantiate(obj, transform);
                    //RectTransform rt = go.GetComponent<RectTransform>();
                    //对齐到网格中心点,当pivot不为0.5,0.5时，网格中心点相对于cell.truePos会有偏移，所以需要根据pivot进行调整。
                    // rt.localPosition = new Vector2(
                    //    cell.truePos.x + rt.sizeDelta.x * (GetGridRectTransform().pivot.x - 0.5f),
                    //    cell.truePos.y + rt.sizeDelta.y * (GetGridRectTransform().pivot.y - 0.5f)
                    //);
                    //go.transform.SetParent(gameObject.transform, true);
                }
            }
        }

        #endregion

        #region UI相关
       
        public bool TryPlaceCardUI(Card card, Vector2 anchorPosition, out Vector2 placedAnchorPosition)
        {
            placedAnchorPosition = Vector2.zero;
            pendingPlacementCells.Clear();

            if (card == null)
            {
                ClearCurrentPlacementPreview();
                return false;
            }

            if (!CheckPlaceUI(card, anchorPosition, out bool canPlace))
            {
                ClearCurrentPlacementPreview();
                return false;
            }

            if (!canPlace)
            {
                Debug.Log("<color=red>UI 放置失败</color>");
                return false;
            }

            foreach (CellStruct cellStruct in cellStructList)
            {
                if (!cellStruct.show)
                {
                    continue;
                }

                CellClass cellClass = allCells[cellStruct.posInDic.x, cellStruct.posInDic.y];
                pendingPlacementCells.Add(cellClass);
                card.ApplyPlacement(cellClass);
            }

            card.SetPlacedCells(pendingPlacementCells);

            placedAnchorPosition = TryGetPlacedAnchorPosition(card, out Vector2 anchor)
                ? anchor
                : anchorPosition;

            ClearCurrentPlacementPreview();
            Debug.Log($"<color=red>UI 放置成功</color>:{pendingPlacementCells.Count}");
            return true;
        }

        public void UpdatePlacementPreviewUI(Card card, Vector2 anchorPosition)
        {
            ClearCurrentPlacementPreview();

            if (card == null || !CheckPlaceUI(card, anchorPosition, out bool canPlace))
            {
                return;
            }

            previewCard = card;

            if (card.showSelfPreviewColor)
            {
                card.SetSelfPreview(canPlace ? Color.green : Color.red);
            }
        }

        public bool CheckPlaceUI(Card card, Vector2 anchorPosition, out bool canPlace)
        {
            cellStructList.Clear();
            canPlace = true;

            if (card == null)
            {
                Debug.LogWarning("[Backpack.CheckPlaceUI] 计算失败：card 为 null。");
                canPlace = false;
                return false;
            }

            if (!EnsureGridInitialized())
            {
                canPlace = false;
                return false;
            }

            Vector2 anchorLocalPosition = CardParentToBackpackLocalPosition(card, anchorPosition);
            List<Vector2> rotatedOffsets = card.GetRotatedOffsets();

            foreach (Vector2 offset in rotatedOffsets)
            {
                Vector2 cellLocalPosition = anchorLocalPosition + GridOffsetToUILocalOffset(offset);
                Vector2Int pos = TryGetAnchorPositionUI(cellLocalPosition);

                if (!IsInside(pos))
                {
                    canPlace = false;
                    cellStructList.Add(new CellStruct
                    {
                        placeMode = PlaceMode.None,
                        show = false,
                    });
                    continue;
                }

                CellClass cell = allCells[pos.x, pos.y];
                PlaceMode placeMode = cell.CanPlace(card);
                if (placeMode != PlaceMode.Valid)
                {
                    canPlace = false;
                }

                cellStructList.Add(new CellStruct
                {
                    placeMode = placeMode,
                    offset = offset,
                    posInDic = cell.posiInDic,
                    truePos = cell.truePos,
                    show = true,
                });
            }
            UpdateDiamondPreview();
            return true;
        }

        private bool TryGetPlacedAnchorPosition(Card card, out Vector2 anchorPosition)
        {
            foreach (CellStruct cellStruct in cellStructList)
            {
                if (!cellStruct.show)
                {
                    continue;
                }

                Vector2 anchorLocalPosition = cellStruct.truePos - GridOffsetToUILocalOffset(cellStruct.offset);
                anchorPosition = BackpackLocalToCardParentPosition(card, anchorLocalPosition);
                return true;
            }

            anchorPosition = Vector2.zero;
            return false;
        }
        /// <summary>
        /// 将卡牌的世界坐标转换屏幕坐标，再转换为为相对于背包网格的局部坐标。
        /// </summary>
        private Vector2 CardParentToBackpackLocalPosition(Card card, Vector2 cardParentPosition)
        {
           return CoordinateConverter.WorldToUI_LocalPosition(gridUISystem.parentRect, cardParentPosition);
//            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main,cardParentPosition);

//            Vector2 localPoint;
//            RectTransformUtility.ScreenPointToLocalPointInRectangle(
//    m_HandleGrid.parentRect, // 目标UI所在的父级面板
//    screenPos,
//    Camera.main,
//    out localPoint
//);
            //Debug.Log($"世界坐标是: {cardParentPosition}, ScreenPos: {screenPos}, 相对坐标是: {localPoint}"); 
            //return localPoint;
        }
        /// <summary>
        /// 【坐标系转换】将相对于“背包网格”的局部坐标，转换为卡牌所在父容器的局部坐标。
        /// （这是 CardParentToBackpackLocalPosition 的反向操作）
        /// </summary>
        private Vector2 BackpackLocalToCardParentPosition(Card card, Vector2 backpackLocalPosition)
        {
            RectTransform gridRect = GetGridRectTransform();
            RectTransform cardParent = card.GetUIParentRectTransform();

            if (gridRect == null || cardParent == null)
            {
                return backpackLocalPosition;
            }

            Vector3 worldPosition = gridRect.TransformPoint(backpackLocalPosition);
            return cardParent.InverseTransformPoint(worldPosition);
        }
        /// <summary>
        /// 【网格偏移转UI偏移】将二维数组中的网格索引偏移量（如 x:1, y:-1），转换为实际的 UI 像素偏移量。
        /// 内部自动处理了 UGUI Y轴方向的问题（向下或向上）。
        /// </summary>
        private Vector2 GridOffsetToUILocalOffset(Vector2 offset)
        {
            float ySign = uiYAxisDown ? -1f : 1f;
            return new Vector2(offset.x * cellSize.x, offset.y * cellSize.y * ySign);
        }
        /// <summary>
        /// 【网格坐标转UI坐标】将二维数组中的绝对网格坐标（如第3行第4列），转换为对应的 UI 局部锚点坐标。
        /// 基于网格原点位置、单元格大小以及 Y轴方向进行计算。
        /// </summary>
        private Vector2 GridToUILocalPosition(Vector2Int gridPosition)
        {
            float ySign = uiYAxisDown ? -1f : 1f;
            return gridOriginLocalPosition + new Vector2(
                gridPosition.x * cellSize.x,
                gridPosition.y * cellSize.y * ySign
            );
        }
        private Vector3 GridToUIWorldPosition(Vector2Int gridPosition)
        {
            //父物体局部坐标
            Vector2 location = GetComponent<RectTransform>().localPosition;
            // 1. 计算网格相对于原点(Anchor/Pivot)的局部二维偏移量
            float ySign = uiYAxisDown ? -1f : 1f;
            Vector2 localOffset = new Vector2(
                gridPosition.x * cellSize.x,
                gridPosition.y * cellSize.y * ySign
            );

            // 2. 将“局部偏移 + 局部原点”组合成完整的局部坐标 (Vector3)
            Vector3 localPos = new Vector3(location.x+localOffset.x,
                                           location.y + localOffset.y,
                                           0f); // Z轴通常设为0或你需要的深度

            // 3. 【核心转换】使用 TransformPoint 将局部坐标映射到世界坐标
            // transform.TransformPoint 会自动处理父级的平移、旋转和缩放
            return transform.TransformPoint(localPos);
        }

        /// <summary>
        /// 【UI坐标反查网格坐标】根据传入的 UI 局部坐标（通常是鼠标点击或拖拽位置），反向推算出其所在的网格格子坐标。
        /// </summary>
        private Vector2Int TryGetAnchorPositionUI(Vector2 localPosition)
        {

            //使用 FloorToInt 向下取整（Mathf.Floor 返回的是 float，需要转 int）
            //1.这是因为我以整个ui为网格，并且因为pivot为0,0。所以需要向下取整。
            //导致第一个网格的实际中心点相对坐标为0,5,0.5，而不是0,0,
            //如果pivot设置为0.5,0.5,那么相对坐标就会有负数。更加不可取。
            //如果在以ui的坐标为网格原点的情况下，不管pivot怎么设置，都需要四舍五入取整。
            // 1. 计算相对坐标并除以格子大小（注意加 f 转换为浮点数除法）
            float relativeX = (localPosition.x - gridOriginLocalPosition.x) / cellSize.x;
            float relativeY = (localPosition.y - gridOriginLocalPosition.y) / cellSize.y;

            int gridX = Mathf.FloorToInt(relativeX);
            int gridY = Mathf.FloorToInt(relativeY);
            Vector2Int _pos = new Vector2Int(gridX, gridY);

            Debug.Log($"得到网格坐标: {_pos}");
            return _pos;
        }
        /// <summary>
        /// 【性能优化/获取缓存】获取当前物体的 RectTransform 组件。
        /// 使用了懒加载和缓存机制（rectTransformCache），避免频繁调用 GetComponent 带来的性能开销。
        /// </summary>
        public RectTransform GetGridRectTransform()
        {
            if (rectTransformCache == null)
            {
                rectTransformCache = transform as RectTransform;
            }

            return rectTransformCache;
        }

        /// <summary>
        /// 检查网格是否存在
        /// </summary>
        /// <returns></returns>
        private bool EnsureGridInitialized()
        {
            if (cellSize.x == 0 || cellSize.y == 0)
            {
                Debug.LogWarning("[Backpack] cellSize 不能为 0。");
                return false;
            }

            if (allCells == null
                || allCells.GetLength(0) != side.x
                || allCells.GetLength(1) != side.y)
            {
                InitGrid();
            }

            return allCells != null && allCells.Length > 0;
        }

        #endregion

        #region 放置公共逻辑

        /// <summary>
        /// 清除当前的放置预览状态
        /// </summary>
        public void ClearCurrentPlacementPreview()
        {
            if (previewCard != null && previewCard.showSelfPreviewColor)
            {
                previewCard.ClearSelfPreview();
            }

            previewCard = null;
        }

        public void Clear()
        {
            pendingPlacementCells.Clear();
            ClearCurrentPlacementPreview();
        }
        private void UpdateDiamondPreview()
        {
            PoolManager poolManager = PoolManager.Instance;
            if (poolManager == null || poolManager.DiamondShaderPool == null)
            {
                return;
            }

            for (int i = 0; i < cellStructList.Count; i++)
            {
                poolManager.DiamondShaderPool.UpdateDiamond(i, cellStructList[i], this);
            }
        }

        private bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < side.x && pos.y >= 0 && pos.y < side.y;
        }

        #endregion

        #region editor图形化

        private void OnDrawGizmos()
        {

            DrawGridGizmos();

        }
        private void DrawGridGizmos()
        {
            for (int x = 0; x < side.x; x++)
            {
                for (int y = 0; y < side.y; y++)
                {
                    Vector2Int gridPosition = new Vector2Int(x, y);
                    Vector3 center = GridToWorldPosition(gridPosition);
                    center.z += gizmoZOffset;

                    Vector3 size = new Vector3(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y), 0.01f);

                    if (allCells != null
                        && x < allCells.GetLength(0)
                        && y < allCells.GetLength(1)
                        && allCells[x, y] != null
                        && allCells[x, y].backpackDl != null)
                    {
                        Gizmos.color = placeableGizmoColor;
                        Gizmos.DrawCube(center, size);
                    }

                    Gizmos.color = gridGizmoColor;
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }

        #endregion

        #region 2D相关

        public void AllCellInitialize()
        {
            allCells = new CellClass[side.x, side.y];

            for (int x = 0; x < allCells.GetLength(0); x++)
            {
                for (int y = 0; y < allCells.GetLength(1); y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    CellClass cell = new CellClass(pos)
                    {
                        truePos = GridToWorldPosition(pos)
                    };

                    allCells[x, y] = cell;
                }
            }
        }
        /// <summary>
        /// 2. 尝试放置卡牌 (核心交互逻辑)
        /// 当用户点击确认放置时调用。
        /// </summary>
        /// <param name="card">要放置的卡牌对象</param>
        /// <param name="worldPosition">鼠标当前的世界坐标（作为锚点）</param>
        /// <param name="anchorWorldPosition">输出参数：最终确定的锚点位置</param>
        /// <returns>是否放置成功</returns>
        public bool TryPlaceCard(Card card, Vector3 worldPosition, out Vector3 anchorWorldPosition)
        {
            anchorWorldPosition = Vector3.zero;
            pendingPlacementCells.Clear();

            if (card == null)
            {
                ClearCurrentPlacementPreview();
                return false;
            }
            // 检查在该位置是否允许放置（包含边界检查和重叠检查）
            if (!CheckPlace(card, worldPosition, out bool canPlace))
            {
                ClearCurrentPlacementPreview();
                return false;
            }
            // 如果 CheckPlace 返回了 false (canPlace为假)，说明有阻挡或越界
            if (!canPlace)
            {
                Debug.Log("<color=red>放置失败</color>");
                return false;
            }

            foreach (CellStruct cell in cellStructList)
            {
                CellClass cellClass = allCells[cell.posInDic.x, cell.posInDic.y];
                pendingPlacementCells.Add(cellClass);
                card.ApplyPlacement(cellClass);
            }

            card.SetPlacedCells(pendingPlacementCells);
            ClearCurrentPlacementPreview();
            Debug.Log($"<color=red>放置成功</color>:{pendingPlacementCells.Count}");
            return true;
        }

        /// <summary>
        /// 3. 更新放置预览 (UI反馈)
        /// 通常在 Update 中调用，跟随鼠标移动显示红/绿高亮。
        /// </summary>
        /// <param name="card">当前拖拽的卡牌</param>
        /// <param name="anchor">当前的锚点坐标（通常是鼠标位置）</param>
        public void UpdatePlacementPreview(Card card, Vector2 anchor)
        {
            ClearCurrentPlacementPreview();

            if (card == null || !CheckPlace(card, anchor, out bool canPlace))
            {
                return;
            }

            previewCard = card;

            // 根据检查结果设置颜色：绿色代表可放置，红色代表冲突
            if (card.showSelfPreviewColor)
            {
                card.SetSelfPreview(canPlace ? Color.green : Color.red);
            }
        }
        /// <summary>
        /// 4. 检查放置合法性 (核心算法)
        /// 计算卡牌覆盖的所有格子，判断是否越界或被占用。
        /// </summary>
        public bool CheckPlace(Card card, Vector2 anchor, out bool canPlace)
        {
            cellStructList.Clear();
            canPlace = true;

            if (card == null)
            {
                Debug.LogWarning("[Backpack.CheckPlace] 计算失败：card 为 null。");
                canPlace = false;
                return false;
            }
            // 获取卡牌旋转后的相对偏移量（例如卡牌是 2x2 大小，这里会有4个偏移向量）
            List<Vector2> rotatedOffsets = card.GetRotatedOffsets();

            // 遍历卡牌占据的每一个“单元格”
            foreach (Vector2 offset in rotatedOffsets)
            {  
                // 计算该单元格在世界空间中的绝对位置
                Vector2 nowTruePos = anchor + offset;

                // 将世界坐标转换为网格索引 (x, y)
                Vector2Int pos = TryGetAnchorPosition(nowTruePos);

                if (!IsInside(pos))
                {
                    canPlace = false;
                    cellStructList.Add(new CellStruct
                    {
                        placeMode = PlaceMode.None,
                        show = false,
                    });
                    continue;
                }

                CellClass cell = allCells[pos.x, pos.y];
                PlaceMode placeMode = cell.CanPlace(card);
                if (placeMode != PlaceMode.Valid)
                {
                    canPlace = false;
                }

                cellStructList.Add(new CellStruct
                {
                    placeMode = placeMode,
                    offset = offset,
                    posInDic = cell.posiInDic,
                    truePos = cell.truePos,
                    show = true,
                });
            }
            UpdateDiamondPreview();
            return true;
        }

        /// <summary>
        /// 尝试获取指定世界坐标对应的网格锚点位置（整数坐标）
        /// </summary>
        /// <param name="worldPosition">目标点在世界空间中的三维坐标</param>
        /// <returns>该点在本地网格系统中对应的二维整数坐标 (Vector2Int)</returns>
        private Vector2Int TryGetAnchorPosition(Vector3 worldPosition)
        {
            // 1. 将世界坐标转换为当前对象（网格系统）的本地相对坐标
            Vector2 localPosition = transform.InverseTransformPoint(worldPosition);

            // 2. 根据网格原点和单元格大小，计算出精确的浮点数网格坐标
            //    公式：(当前本地坐标 - 网格原点坐标) / 单元格尺寸
            Vector2 gridPosition = new Vector2(
                (localPosition.x - gridOriginLocalPosition.x) / cellSize.x,
                (localPosition.y - gridOriginLocalPosition.y) / cellSize.y
            );

            return new Vector2Int(
                Mathf.RoundToInt(gridPosition.x),
                Mathf.RoundToInt(gridPosition.y)
            );
        }

        private Vector2 GridToWorldPosition(Vector2Int gridPosition)
        {
            Vector2 localPosition = gridOriginLocalPosition + new Vector2(
                gridPosition.x * cellSize.x,
                gridPosition.y * cellSize.y
            );

            return transform.TransformPoint(localPosition);
        }

       

       

        #endregion

        #region 战斗排序

        public List<Card> cards = new(30);
        public int index = 0;

        public Card CurrentCard
        {
            get
            {
                if (cards.Count == 0 || index < 0 || index >= cards.Count)
                {
                    return null;
                }

                return cards[index];
            }
        }

        public void AdvanceToNextValidAliveCard()
        {
            if (cards.Count == 0)
            {
                return;
            }

            if (index < 0 || index >= cards.Count)
            {
                index = 0;
            }

            int startIndex = index;
            do
            {
                if (cards[index] != null && cards[index].obj != null && !cards[index].obj.IsDead)
                {
                    return;
                }

                index++;
                if (index >= cards.Count)
                {
                    index = 0;
                }
            }
            while (index != startIndex);

            index = -1;
        }

        public void SortByBattlePriority()
        {
            cards.Clear();
            index = 0;

            if (allCells == null)
            {
                return;
            }

            for (int y = allCells.GetLength(1) - 1; y >= 0; y--)
            {
                for (int x = 0; x < allCells.GetLength(0); x++)
                {
                    CellClass cell = allCells[x, y];
                    if (cell != null && cell.itemList.Count > 0)
                    {
                        Card topCard = cell.itemList[cell.itemList.Count - 1];
                        if (!cards.Contains(topCard))
                        {
                            cards.Add(topCard);
                        }
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransform rectTransform = transform as RectTransform;
            Vector2 localPoint;

            if (rectTransform != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // 1. 计算相对坐标并除以格子大小（注意加 f 转换为浮点数除法）
                float relativeX = (localPoint.x - gridOriginLocalPosition.x) / cellSize.x;
                float relativeY = (localPoint.y - gridOriginLocalPosition.y) / cellSize.y;

                // 2. 使用 FloorToInt 向下取整（Mathf.Floor 返回的是 float，需要转 int）
                //1.这是因为我以整个ui为网格，并且因为pivot为0,0。所以需要向下取整。
                //导致第一个网格的实际中心点相对坐标为0,5,0.5，而不是0,0,
                //如果pivot设置为0.5,0.5,那么相对坐标就会有负数。更加不可取。
                //如果在以ui的坐标为网格原点的情况下，不管pivot怎么设置，都需要四舍五入取整。
                int gridX = Mathf.FloorToInt(relativeX);
                int gridY = Mathf.FloorToInt(relativeY);

                Vector2Int _pos = new Vector2Int(gridX, gridY);
                Debug.Log($"原点世界坐标是: {transform.position},屏幕坐标是{eventData.position},相对坐标是{localPoint}, 网格坐标是{_pos}");
            }
            else
            {
                Debug.Log($"原点世界坐标是: {transform.position},屏幕坐标是{eventData.position},相对坐标无法计算");
            }
        }

        #endregion


    }
}
