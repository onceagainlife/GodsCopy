using GameFramework.Event;
using UnityGameFramework.Runtime;


namespace HuaHaiLiKanHua
{
    public class PlayerAttritubeChangeEvent : GameEventArgs
    {
        public override int Id => typeof(PlayerAttritubeChangeEvent).GetHashCode();

        public override void Clear()
        {

        }
    }
}

