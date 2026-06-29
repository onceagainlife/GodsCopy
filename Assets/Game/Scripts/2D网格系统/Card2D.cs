using HuaHaiLiKanHua;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace BackPackLike
{
    public class Card2D : Item, IPointerDownHandler, IPointerUpHandler,IDragHandler
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

        #region 点击
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

         
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            UpdatePosition(eventData.position);

        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

           

            GlobalStatic.card = null;
        }

        /// <summary>
        /// Rotates the UI card and refreshes the UI grid placement preview.
        /// </summary>
        public void Rotation()
        {

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

        #region 2D相关

        public void OnDrag2D(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!Move())
            {
                return;
            }

            UpdatePosition(eventData.position);

            Backpack backpack = GetCurrentBackpack();

        }

        public virtual void OnPointerUp2D(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!Move())
            {
                return;
            }

            Backpack backpack = GetCurrentBackpack();
            if (backpack == null)
            {
                return;
            }

            //if (backpack.TryPlaceCard(this, transform.position, out Vector3 anchorWorldPosition))
            //{
            //    if (backpack.cellStructList == null || backpack.cellStructList.Count == 0)
            //    {
            //        return;
            //    }

            //    Vector2 pos = backpack.cellStructList[0].offset;
            //    Vector2 truePos = backpack.cellStructList[0].truePos;
            //    transform.position = truePos - pos;
            //}
            //else
            //{
            //    backpack.ClearCurrentPlacementPreview();
            //}

            //GlobalStatic.card = null;
        }

        public void UpdatePosition(Vector3 mousePosition)
        {
            Vector2 position = CoordinateConverter.ScreenToWorldPointZPlane(mousePosition, transform.position.z);
            transform.position = position - offset;
        }

        public void Rotation2D()
        {
            if (!Move())
            {
                return;
            }

            indexInAngle = (indexInAngle + 1) & 3;
            transform.eulerAngles = new Vector3(0, 0, indexInAngle * 90);

            Backpack backpack = GetCurrentBackpack();
            //backpack?.UpdatePlacementPreview(this, transform.position);
        }

#if UNITY_EDITOR
        [ContextMenu("CreateChild")]
        private void CreateChil()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            string prefabPath = "Assets/Game/Prefab/CardGrid.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError("Prefab 不存在：" + prefabPath);
                return;
            }

            foreach (Vector2 itemOffset in offsets)
            {
                Vector2 pos = (Vector2)transform.position + itemOffset;
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                instance.transform.position = pos;
                instance.transform.SetParent(transform, true);
            }

            Selection.activeGameObject = gameObject;
        }
#endif

        #endregion
    }
}

