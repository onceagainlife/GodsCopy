using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using HuaHaiLiKanHua;
using StarForce;







#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BackPackLike
{
    [ExecuteAlways]
    public class GridUISystem : MonoBehaviour
    {

        #region 网格
        [Header("Grid Settings")]
        public Vector2 cellSize = new Vector2(100, 100);
        public Vector2Int gridSize = new Vector2Int(5, 5);
        public RectTransform parentRect;

        private void Awake()
        {
            parentRect = GetComponentInParent<RectTransform>();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                FitSelfToGrid();
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += DelayFit;
#endif
            }
        }

#if UNITY_EDITOR
        private void DelayFit()
        {
            if (this == null) return;
            FitSelfToGrid();
        }
#endif
        /// <summary>
        /// 只设置 RectTransform 的尺寸，不改 pivot / anchor
        /// </summary>
        void FitSelfToGrid()
        {
            if (!(transform is RectTransform rt))
                return;

            int columns = Mathf.FloorToInt(gridSize.x);
            int rows = Mathf.FloorToInt(gridSize.y);

            rt.sizeDelta = new Vector2(
                columns * cellSize.x,
                rows * cellSize.y
            );
        }

        #endregion

    }
}