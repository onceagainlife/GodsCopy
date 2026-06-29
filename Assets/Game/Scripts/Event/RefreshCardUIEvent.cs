using GameFramework;
using GameFramework.Event;
using UnityEngine;

namespace BackPackLike
{
    public class RefreshCardUIEvent : GameEventArgs
    {
        public static readonly int EventId = typeof(RefreshCardUIEvent).GetHashCode();

        public override int Id => EventId;

        public override void Clear()
        {

        }

        /// <summary>
        /// 创建加载全局配置更新事件。
        /// </summary>
        /// <param Name="e">内部事件。</param>
        /// <returns>传递事件数据参数</returns>
        public static RefreshCardUIEvent Create()
        {
            RefreshCardUIEvent startGameEvent = ReferencePool.Acquire<RefreshCardUIEvent>();

            return startGameEvent;
        }
    }
}

