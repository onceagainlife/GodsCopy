using UnityEngine;
using HuaHaiLiKanHua;

namespace BackPackLike 
{
    public class PoolManager : Singleton<PoolManager>
    {
        public DiamondShaderPool DiamondShaderPool;
        protected void Awake()
        {
            SingletonInit();
            if (DiamondShaderPool != null)
                DiamondShaderPool.Init();
            else { Debug.LogError("没有正确创建DiamondShaderPool对象池"); }
        }
    }
}


