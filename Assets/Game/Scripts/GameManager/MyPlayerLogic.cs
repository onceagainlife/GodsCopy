using UnityEngine;

namespace BackPackLike
{
    public class MyPlayerLogic : PlayerLogic
    {

        private void Awake()
        {
            OnInit();
        }

        /// <summary>
        /// 非系统玩家启动时注册为当前座位玩家。
        /// </summary>
        private void Start()
        {
           OnShow();
        }
        public override void OnShow()
        {

            GameManager.GameLogic.playerLogic = this;
        }
    }
}

