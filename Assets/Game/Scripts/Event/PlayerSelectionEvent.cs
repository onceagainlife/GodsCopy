using GameFramework;
using GameFramework.Event;
using UnityEngine;

namespace BackPackLike
{
    public class PlayerSelectionEvent : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayerSelectionEvent).GetHashCode();

        public override int Id => EventId;

        /// <summary>
        /// 创建加载全局配置更新事件。
        /// </summary>
        /// <param Name="e">内部事件。</param>
        /// <returns>传递事件数据参数</returns>
        public static PlayerSelectionEvent Create()
        {
            PlayerSelectionEvent Event = ReferencePool.Acquire<PlayerSelectionEvent>();

            return Event;
        }
        public override void Clear()
        {

        }
    }
}
