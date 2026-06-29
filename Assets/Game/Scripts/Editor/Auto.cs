
using BackPackLike;
using BackPackLike.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public static class Auto
{
    [MenuItem("BackPackLike/CurrentCard 网格自动生成器1")]

    static void ApplyPlacement()
    {
        float cellSize = 1f; // 可改成配置

        foreach (GameObject go in Selection.gameObjects)
        {
            if (go == null)
                continue;

            // ✅ 自动添加 PolygonCollider2D
            PolygonCollider2D collider = go.GetComponent<PolygonCollider2D>();
            if (collider == null)
            {
                collider = Undo.AddComponent<PolygonCollider2D>(go);
                Debug.Log($"[GridOccupy] 自动添加 PolygonCollider2D: {go.name}");
            }

            // ✅ 执行生成
            List<GridOffsetData> result =
                GridOccupyGenerator.Generate(collider, cellSize);
            ApplyToCard(go, result);
            Debug.Log($"{go.name} 生成了 {result.Count} 个 GridOffsetData");
        }

        AssetDatabase.Refresh();
    }
    static void ApplyToCard(GameObject go, List<GridOffsetData> datas)
    {
        Card card = go.GetComponent<Card>();
        if (card == null)
            return;

        Undo.RecordObject(card, "Apply Grid Offsets");

        card.offsets.Clear(); // 或保留旧数据

        foreach (var data in datas)
        {
            card.offsets.Add(data.localOffset);
        }

        EditorUtility.SetDirty(card);
    }
}

