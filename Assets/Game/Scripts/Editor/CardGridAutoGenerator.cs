using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BackPackLike.Editor
{
    /// <summary>
    /// 自动扫描选中 2D 物体的包围盒，计算覆盖网格，并自动添加 CurrentCard 脚本、设置 offsets。
    /// </summary>
    public class CardGridAutoGenerator : EditorWindow
    {
        [Tooltip("网格单元尺寸。x/y 可以不同，用于支持非正方形网格。")]
        public Vector2 cellSize = Vector2.one;

        [Tooltip("是否把计算出的最小偏移归零，让所有偏移从非负数开始。\n关闭此选项可保留以中心为 (0,0) 的正负偏移。")]
        public bool offsetZeroBased = false;

        [Tooltip("自动从场景中的 Backpack 读取 cellSize。")]
        public bool autoReadBackpackCellSize = true;

        [Tooltip("生成时覆盖已存在的 CurrentCard 脚本的 offsets（否则跳过已有 CurrentCard 的物体）。")]
        public bool overwriteExistingCard = true;

        [Tooltip("是否在 bounds 边缘多扩展一圈采样，防止边缘像素刚好在边界上漏判。")]
        public bool extraEdgeSample = true;

        private Vector2 scrollPos;
        private string resultLog = "";

        [MenuItem("BackPackLike/CurrentCard 网格自动生成器")]
        public static void ShowWindow()
        {
            var window = GetWindow<CardGridAutoGenerator>("CurrentCard 网格生成");
            window.minSize = new Vector2(400, 350);
        }

        private void OnEnable()
        {
            TryReadBackpackCellSize();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("CurrentCard 网格自动生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 自动读取 Backpack cellSize
            EditorGUILayout.BeginHorizontal();
            autoReadBackpackCellSize = EditorGUILayout.ToggleLeft("自动读取 Backpack cellSize", autoReadBackpackCellSize, GUILayout.Width(200));
            if (GUILayout.Button("立即读取", GUILayout.Width(80)))
            {
                TryReadBackpackCellSize();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(autoReadBackpackCellSize);
            cellSize = EditorGUILayout.Vector2Field("网格单元尺寸 (Cell Size)", cellSize);
            EditorGUI.EndDisabledGroup();

            offsetZeroBased = EditorGUILayout.ToggleLeft("偏移归零（最小偏移移到 (0,0)，会丢失中心原点）", offsetZeroBased);
            overwriteExistingCard = EditorGUILayout.ToggleLeft("覆盖已存在的 CurrentCard offsets", overwriteExistingCard);
            extraEdgeSample = EditorGUILayout.ToggleLeft("边缘额外采样（防止边界漏判）", extraEdgeSample);

            if (!offsetZeroBased)
            {
                EditorGUILayout.HelpBox(
                    "当前以物体中心为 (0,0) 原点，offsets 会包含正负值。\n" +
                    "例如 3 格竖直物体会生成 (0,-1), (0,0), (0,1)。",
                    MessageType.Info);
            }

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.4f, 0.75f, 1f);
            if (GUILayout.Button("为选中物体生成 CurrentCard 网格", GUILayout.Height(40)))
            {
                GenerateForSelection();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("清除选中物体的 CurrentCard 脚本", GUILayout.Height(25)))
            {
                RemoveCardFromSelection();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("生成结果", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "box", GUILayout.ExpandHeight(true));
            EditorGUILayout.SelectableLabel(resultLog, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void TryReadBackpackCellSize()
        {
            if (!autoReadBackpackCellSize) return;

            Backpack backpack = FindFirstObjectByType<Backpack>();
            if (backpack != null && backpack.cellSize != Vector2.zero)
            {
                cellSize = backpack.cellSize;
            }
        }

        private void GenerateForSelection()
        {
            if (Selection.gameObjects.Length == 0)
            {
                resultLog = "[错误] 未选中任何物体，请在 Hierarchy 中选中需要生成网格的 GameObject。";
                return;
            }

            if (cellSize.x <= 0 || cellSize.y <= 0)
            {
                resultLog = "[错误] cellSize 必须大于 0。";
                return;
            }

            System.Text.StringBuilder sb = new();
            sb.AppendLine($"===== 生成开始 | CellSize={cellSize} | 选中数量={Selection.gameObjects.Length} =====\n");

            foreach (GameObject go in Selection.gameObjects)
            {
                GenerateForGameObject(go, sb);
            }

            sb.AppendLine("===== 生成结束 =====");
            resultLog = sb.ToString();
        }

        private void GenerateForGameObject(GameObject go, System.Text.StringBuilder sb)
        {
            // 获取物体的 2D 包围盒
            Bounds? boundsOpt = Get2DBounds(go);
            if (!boundsOpt.HasValue)
            {
                sb.AppendLine($"[跳过] {go.name}：未找到 SpriteRenderer / Collider2D / MeshRenderer，无法获取包围盒。");
                return;
            }

            Bounds bounds = boundsOpt.Value;

            // 检查是否已有 CurrentCard 脚本
            Card existingCard = go.GetComponent<Card>();
            if (existingCard != null && !overwriteExistingCard)
            {
                sb.AppendLine($"[跳过] {go.name}：已存在 CurrentCard 脚本，且未勾选【覆盖已存在的 CurrentCard offsets】。");
                return;
            }

            // 计算覆盖的网格偏移
            List<Vector2> offsets = CalculateOffsets(bounds, go.transform.position);

            if (offsets.Count == 0)
            {
                sb.AppendLine($"[警告] {go.name}：未计算出任何覆盖网格，可能物体尺寸过小或 cellSize 过大。bounds={bounds}, cellSize={cellSize}");
                return;
            }

            // 添加或获取 CurrentCard 脚本
            Card card = existingCard ?? go.AddComponent<Card>();
            card.offsets = offsets;

            // 标记场景和 prefab 为 dirty
            EditorUtility.SetDirty(go);
            if (PrefabUtility.IsPartOfAnyPrefab(go))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(card);
            }

            sb.AppendLine($"[成功] {go.name}");
            sb.AppendLine($"         Bounds: {bounds}");
            sb.AppendLine($"         覆盖网格数: {offsets.Count}");
            sb.AppendLine($"         Offsets: {string.Join(", ", offsets.Select(o => $"({o.x},{o.y})"))}");
            sb.AppendLine();
        }

        /// <summary>
        /// 获取物体的 2D 包围盒。优先 SpriteRenderer，其次 Collider2D，最后 MeshRenderer。
        /// </summary>
        private Bounds? Get2DBounds(GameObject go)
        {
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                return sr.bounds;
            }

            Collider2D col2d = go.GetComponent<Collider2D>();
            if (col2d != null)
            {
                return col2d.bounds;
            }

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                return mr.bounds;
            }

            return null;
        }


        private T GetComponent<T>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据 bounds 和物体世界坐标，计算覆盖的所有网格偏移（相对锚点中心）。
        /// </summary>
        private List<Vector2> CalculateOffsets(Bounds bounds, Vector3 worldPos)
        {
            List<Vector2> offsets = new();
            HashSet<Vector2> offsetSet = new(); // 保留你的去重逻辑

            // 【步骤1】保留你的原代码：确定锚点
            Vector2 anchorWorld = bounds.center;

            // 【步骤2】保留你的原代码：获取极值
            float minX = bounds.min.x;
            float maxX = bounds.max.x;
            float minY = bounds.min.y;
            float maxY = bounds.max.y;

            // ✅ 容差：超过 10% 才算下一格
            const float tolerance = 0.1f;
            // X
            float widthInCells = (maxX - minX) / cellSize.x;
            int cols = Mathf.FloorToInt(widthInCells);
            if (widthInCells - cols > tolerance)
                cols++;

            // Y
            float heightInCells = (maxY - minY) / cellSize.y;
            int rows = Mathf.FloorToInt(heightInCells);
            if (heightInCells - rows > tolerance)
                rows++;

            //// 【核心修正：直接对接你的“步骤A”】
            //// 1. 计算实际占据的网格数量（向上取整：1.9格算2格，0.7格算1格）
            //int cols = Mathf.CeilToInt((maxX - minX) / cellSize.x);
            //int rows = Mathf.CeilToInt((maxY - minY) / cellSize.y);
            if (cols <= 0) cols = 1;
            if (rows <= 0) rows = 1;

            // 2. 生成以物体中心为 (0,0) 的整数偏移
            // 你期望的 0,-0.5 和 0,0.5 在整数网格中会自动映射为 (0,-1) 和 (0,0)
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {

                    float offsetX = x - (cols - 1) * 0.5f;
                    float offsetY = y - (rows - 1) * 0.5f;

                    Vector2 offset = new Vector2(offsetX, offsetY);
                    if (offsetSet.Add(offset))
                    {
                        offsets.Add(offset);
                    }
                }
            }


            return offsets;
        }

        private void RemoveCardFromSelection()
        {
            if (Selection.gameObjects.Length == 0)
            {
                resultLog = "[错误] 未选中任何物体。";
                return;
            }

            System.Text.StringBuilder sb = new();
            foreach (GameObject go in Selection.gameObjects)
            {
                Card card = go.GetComponent<Card>();
                if (card != null)
                {
                    Undo.DestroyObjectImmediate(card);
                    sb.AppendLine($"[移除] {go.name} 的 CurrentCard 脚本。");
                }
                else
                {
                    sb.AppendLine($"[跳过] {go.name} 没有 CurrentCard 脚本。");
                }
            }
            resultLog = sb.ToString();
        }
    }
}
