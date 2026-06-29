using GameFramework;
using GameFramework.Event;

namespace HuaHaiLiKanHua
{
    public class StartGameEvent : GameEventArgs
    {
        public static readonly int EventId = typeof(StartGameEvent).GetHashCode();

        public override int Id => EventId;

        /// <summary>
        /// 创建加载全局配置更新事件。
        /// </summary>
        /// <param Name="e">内部事件。</param>
        /// <returns>传递事件数据参数</returns>
        public static StartGameEvent Create()
        {
            StartGameEvent startGameEvent = ReferencePool.Acquire<StartGameEvent>();

            return startGameEvent;
        }
        public override void Clear()
        {

        }
    }
}

