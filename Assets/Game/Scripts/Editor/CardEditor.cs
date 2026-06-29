using BackPackLike;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Card))]
public class CardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 先画默认 Inspector
        DrawDefaultInspector();

        // 拿到目标脚本
        Card card = (Card)target;


        // 插入按钮
        if (GUILayout.Button("测试网格"))
        {
            Backpack backpack = card.GetCurrentBackpack();
            if (backpack == null) { Debug.Log($"错误backpack为空"); return; }
            backpack.CheckPlaceUI(card, card.GetUIPosition(), out bool canPlace);
            //backpack.CheckPlace(card, card.gameObject.transform.position, out bool canPlace);
        }
    }
}
