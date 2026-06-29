using HuaHaiLiKanHua;
using System.Collections.Generic;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    ///手牌网格
    /// </summary>
    public class HandleGrid : HuaHaiLiKanHua.Singleton<HandleGrid>
    {
        [Tooltip("所有固定网格的数据数组。")]
        public CellClass[,] allCells;

        [Tooltip("UI网格视图系统")]
        public GridUISystem gridUISystem;//调整网格的大小,以适配shader

        [Tooltip("固定网格尺寸。")]
        public Vector2Int side = new Vector2Int(3, 3);

        [Tooltip("每个逻辑格子的宽高。")]
        public Vector2 cellSize = Vector2.one;

        [Tooltip("UI 格子 Y 轴是否向下增长。")]
        public bool uiYAxisDown = false;

        #region 初始化

        private void Awake()
        {
            base.SingletonInit();
            InitGrid();
        }
        /// <summary>
        /// Initializes the UI grid from GridUISystem when it is present.
        /// </summary>
        public void InitGrid()
        {
            gridUISystem = GetComponent<GridUISystem>();
            contentRt=GetComponent<RectTransform>();

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
                }
            }
        }

        #endregion

        #region 自动放入
        private Vector2Int GetMinOffset(Card card)
        {
            var offsets = card.GetRotatedOffsets();

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            foreach (var o in offsets)
            {
                if (o.x < minX) minX = o.x;
                if (o.y < minY) minY = o.y;
            }

            return new Vector2Int(Mathf.RoundToInt(minX), Mathf.RoundToInt(minY));
        }
        public bool TryPlaceCard(Card card)
        {
            if (card == null || allCells.Length == 0)
                return false;

            var offsets = card.GetRotatedOffsets();
            Vector2Int minOffset = GetMinOffset(card);

            for (int y = 0; y < side.y; y++)
            {
                for (int x = 0; x < side.x; x++)
                {
                    Vector2Int anchorCell = new Vector2Int(x, y);

                    // 反推 card 中心点应落在哪
                    Vector2Int cardCenterCell = anchorCell - minOffset;

                    if (CanPlace(cardCenterCell, offsets))
                    {
                        OccupyCells(cardCenterCell, offsets, card);
                        card.offset = cardCenterCell;
                        return true;
                    }
                }
            }

            return false;
        }
        private bool CanPlace(Vector2Int center, List<Vector2> offsets)
        {
            foreach (var o in offsets)
            {
                Vector2Int pos = center + Vector2Int.RoundToInt(o);

                if (pos.x < 0 || pos.x >= side.x ||
                    pos.y < 0 || pos.y >= side.y)
                    return false;

                if (allCells[pos.x, pos.y].itemList.Count>0)
                    return false;
            }

            return true;
        }
        private void OccupyCells(Vector2Int center, List<Vector2> offsets, Card card)
        {
            foreach (var o in offsets)
            {
                Vector2Int pos = center + Vector2Int.RoundToInt(o);
                allCells[pos.x, pos.y].itemList.Add(card);
            }
        }

        #endregion

        #region 手牌管理
        [Header("Object Config")]

        public GameObject objectPrefab;
        public float unitGridSize = 90f;
        private float totalUsedLength = 0f;
        private RectTransform gridRT;
        private List<RectTransform> placedObjects = new();

        public float targetWidth = 360f;
        public RectTransform contentRt;

        /// <summary>
        /// 调整父物体的大小,使得子物体视觉上宽度不超过targetWidth
        /// </summary>
        /// <param name="obj"></param>
        void Adjust(GameObject obj)
        {
            RectTransform childRt = obj.GetComponent<RectTransform>();
            if (childRt == null || childRt.rect.width <= 0)
                return;

            float realWidth = childRt.rect.width;

            if (realWidth > targetWidth)
            {
                float scale = targetWidth / realWidth;

                if (contentRt.localScale.y < scale) return;

                contentRt.localScale = new Vector3(scale, 1f, 1f);
            }
            else
            {
                contentRt.localScale = Vector3.one;
            }
        }

        float totalUsedHeight;
        /// <summary>
        /// 竖向排列
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public enum Alignment
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        public Alignment alignment = Alignment.TopLeft;

        public RectTransform PlaceObjectVertical(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("Object 为空");
                return null;
            }

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) return null;

            RectTransform parentRT = transform as RectTransform;

            // 当前物体 pivot（不改）
            Vector2 pivot = rt.pivot;

            // 高度计算
            float height = rt.rect.height;
            int occupiedCells = Mathf.CeilToInt(height / unitGridSize);
            float objectHeight = occupiedCells * cellSize.y;

            float x = 0f;
            float y = 0f;

            switch (alignment)
            {
                case Alignment.TopLeft:
                    // pivot.x 决定横向偏移
                    x = objectHeight * pivot.x;
                    y = -(totalUsedHeight + objectHeight * (1f - pivot.y));
                    break;

                case Alignment.TopRight:
                    float parentWidth = parentRT.rect.width;
                    x = parentWidth - objectHeight * (1f - pivot.x);
                    y = -(totalUsedHeight + objectHeight * (1f - pivot.y));
                    break;
            }

            rt.anchoredPosition = new Vector2(x, y);

            placedObjects.Add(rt);
            totalUsedHeight += objectHeight;

            return rt;
        }

        [ContextMenu("PlaceObjectVertical")]
        public void PlaceVer()
        {
            totalUsedHeight = 0; placedObjects.Clear();
            foreach (RectTransform child in transform)
            {
                //PlaceObjectVertical(child.gameObject);
                Adjust(child.gameObject);
            }
        }
        [ContextMenu("PlaceObjectHorizontal")]
        public void Place()
        {
            foreach (RectTransform child in transform)
            {
                PlaceObject(child.gameObject);
            }
        }
        /// <summary>
        /// 放置一个物体，横向排列
        /// </summary>
        [ContextMenu("PlaceObjectHorizontal")]
        public RectTransform PlaceObject(GameObject obj)
        {
            if (objectPrefab == null)
            {
                Debug.LogError("Object Prefab 未设置");
                return null;
            }

            RectTransform rt = obj.GetComponent<RectTransform>();

            //强制 pivot
            rt.pivot = new Vector2(0.5f, 0.5f);

            //直接用 RectTransform 的宽度
            float wide = rt.rect.width;

            //计算占用格子数（向上取整）
            int occupiedCells = Mathf.CeilToInt(wide / unitGridSize);

            //物体宽度
            float objectWidth = occupiedCells * cellSize.x;

            //X 轴位置（你给的公式）
            float x = objectWidth / 2f + totalUsedLength;

            //Y 固定为 0
            rt.anchoredPosition = new Vector2(x, 0);

            //设置位置
            rt.anchoredPosition = new Vector2(x, 0);

            placedObjects.Add(rt);

            //累加已使用长度
            totalUsedLength += objectWidth;

            return rt;
        }

        #endregion


        public virtual void OnAfterBirth(Card card)
        {
            //大小
            RectTransform rect = card.gameObject.GetComponent<RectTransform>();
            rect.SetParent(transform.GetChild(0), false);
            Adjust(card.gameObject);
            //获得卡牌,放入手牌
            GameLogic.Instance.playerLogic.GetCard(card);
        }
        #region 生成卡牌(已弃用)

        ///// <summary>
        ///// 生成卡牌
        ///// </summary>
        //public void InstantiateCard(CampType campType, ItemData itemData)
        //{
        //    int Id = itemData.m_Id;
        //    Card card = InstantiateCard(campType, Id);

        //    if (itemData.m_DataRowBase is DRHero drhero)//人物动物
        //    {
        //        InstantiateHero(card, campType, Id);
        //    }
        //    else if (itemData.m_DataRowBase is DRBlessing drblessing)//法术,诅咒
        //    {

        //        InstantiateBlessing(card, campType, Id);

        //    }
        //    else if (itemData.m_DataRowBase is DREquip dREquip)//装备，道具，环境
        //    {
        //        InstantiateEquip(card, campType, Id);
        //    }
        //    //大小

        //    Adjust(card.gameObject);

        //    //获得卡牌,放入手牌
        //    GameLogic.Instance.playerLogic.GetCard(card);
        //}
        //public Card InstantiateCard(CampType campType, int Id)
        //{
        //    GameObject obj = Instantiate(objectPrefab);

        //    Card card = obj.GetComponent<Card>();

        //    return card;
        //}
        //public void InstantiateHero(Card card, CampType campType, int Id)
        //{
        //    Hero hero = card.AddComponent<Hero>();
        //    HeroData heroData = new HeroData(GameEntry.Entity.GenerateSerialId(), Id, campType);
        //    hero.MyOnInit(heroData);
        //    hero.MyOnShow(heroData);
        //    card.ModifySize(heroData.drHero.SizeX, heroData.drHero.SizeY);
        //    CardUI cardUI= card.GetComponent<CardUI>();
        //    cardUI.RefreshData(null, heroData.Attack,(int)heroData.HP,heroData.drHero.SizeX,heroData.drHero.SizeY);
        //}
        //public void InstantiateBlessing(Card card, CampType campType, int Id)
        //{
        //    Blessing hero = card.AddComponent<Blessing>();
        //    BlessingData heroData = new BlessingData(GameEntry.Entity.GenerateSerialId(), Id, campType);
        //    hero.MyOnInit(heroData);
        //    hero.MyOnShow(heroData);
        //    card.ModifySize(heroData.drBlessing.SizeX, heroData.drBlessing.SizeY);
        //    CardUI cardUI = card.GetComponent<CardUI>();
        //    cardUI.RefreshData(null, 0, (int)0, heroData.drBlessing.SizeX, heroData.drBlessing.SizeY);
        //}
        //public void InstantiateEquip(Card card, CampType campType, int Id)
        //{
        //    Equip entity = card.AddComponent<Equip>();
        //    EquipData heroData = new EquipData(GameEntry.Entity.GenerateSerialId(), Id, campType);
        //    entity.MyOnInit(heroData);
        //    entity.MyOnShow(heroData);
        //    card.ModifySize(heroData.drEquip.SizeX, heroData.drEquip.SizeY);
        //    CardUI cardUI = card.GetComponent<CardUI>();
        //    cardUI.RefreshData(null, 0, (int)0, heroData.drEquip.SizeX, heroData.drEquip.SizeY);
        //}
        #endregion

        #region 辅助
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
            Vector3 localPos = new Vector3(location.x + localOffset.x,
                                           location.y + localOffset.y,
                                           0f); // Z轴通常设为0或你需要的深度

            // 3. 【核心转换】使用 TransformPoint 将局部坐标映射到世界坐标
            // transform.TransformPoint 会自动处理父级的平移、旋转和缩放
            return transform.TransformPoint(localPos);
        }
        #endregion
    }
}

