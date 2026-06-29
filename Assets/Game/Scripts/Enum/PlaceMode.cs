using UnityEngine;

public enum PlaceMode
{
    None,
    /// <summary>
    /// 完全可放置（整体校验通过）
    /// </summary>
    Valid,

    /// <summary>
    /// 格子为空，但当前不可放置
    /// </summary>
    Empty,

    /// <summary>
    /// 格子已被占用（冲突）
    /// </summary>
    Occupied
}
