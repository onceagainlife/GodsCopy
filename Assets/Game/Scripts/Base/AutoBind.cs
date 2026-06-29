namespace HuaHaiLiKanHua
{
    using UnityEngine;
    using UnityEditor;
    using System.Reflection;

#if UNITY_EDITOR
    public static class AutoBindUtil
    {

        /// <summary>
        /// 自动绑定组件：通过字段名在子物体中查找对应名称的节点并获取组件
        /// </summary>
        public static void Bind(MonoBehaviour target)
        {
            if (target == null) return;

            // 获取所有私有和公有字段（包含 [SerializeField]）
            var type = target.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var field in fields)
            {
                // 仅处理标记了 SerializeField 的字段
                if (!field.IsDefined(typeof(SerializeField), false)) continue;

                string nodeName = field.Name;

                // 【核心修改】使用递归查找，穿透所有层级寻找同名节点
                Transform node = FindChildRecursive(target.transform, nodeName);

                if (node != null)
                {
                    // 尝试将找到的节点转换为字段所需的组件类型 (Text, Image, Button 等)
                    object component = node.GetComponent(field.FieldType);

                    if (component != null)
                    {
                        field.SetValue(target, component);
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoBind] 节点 [{nodeName}] 上未找到组件: {field.FieldType.Name}", target);
                    }
                }
                else
                {
                    Debug.LogWarning($"[AutoBind] 在所有子物体中找不到名为 [{nodeName}] 的节点", target);
                }
            }
        }

        /// <summary>
        /// 递归查找子物体（包括未激活的物体）
        /// </summary>
        private static Transform FindChildRecursive(Transform parent, string name)
        {
            // 遍历当前物体的所有直接子物体
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                // 1. 如果名字匹配，直接返回
                if (child.name == name)
                {
                    return child;
                }

                // 2. 如果不匹配，继续深入查找该子物体的后代
                Transform found = FindChildRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    

}
#endif
}
