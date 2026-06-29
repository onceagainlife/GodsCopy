
using System;

namespace HuaHaiLiKanHua
{
    /// <summary>
    /// 格子位置信息
    /// </summary>
    [Serializable]
    public class CellPosition : IEquatable<CellPosition>
    {
        /// <summary>
        /// 字典坐标
        /// </summary>
        public UnityEngine.Vector2Int posInDic;

        /// <summary>
        /// 真实坐标
        /// </summary>
        public UnityEngine.Vector2Int posTrue;

        public bool Equals(CellPosition other)
        {
            return posInDic == other.posInDic &&
                   posTrue == other.posTrue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(posInDic, posTrue);
        }
    }
}
