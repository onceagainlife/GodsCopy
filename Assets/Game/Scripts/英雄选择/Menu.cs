
namespace BackPackLike
{
    using GameFramework.DataTable;
    using GameFramework.Event;
    using HuaHaiLiKanHua;
    using StarForce;
    using UGFExtensions;
    using UnityEngine;
    using UnityEngine.UI;

    public class Menu : UGuiForm
    {
        public Image bg;

        private void Awake()
        {
            GameEntry.Event.Subscribe(PlayerSelectionEvent.EventId,ChangeBg);
         
        }
        public void OnStartButtonClick()
        {
            if (GlobalStatic.HeroIndex <= 0)
            {
                //提示没有选择英雄
                GameEntry.UI.OpenDialog(new DialogParams()
                {
                    Mode = 2,
                    Title = "没有选择英雄",
                    Message = "请选择一个英雄开始游戏",
                    OnClickConfirm = delegate (object userData) { },
                });

                Debug.Log("没有选择英雄");
                return;
            }

            GameEntry.Event.Fire(this, StartGameEvent.Create());

            Debug.Log("开始游戏");


        }

        public void ChangeBg(object sender, GameEventArgs e)
        {
            IDataTable<DRPlayer> dt = GameEntry.DataTable.GetDataTable<DRPlayer>();
            DRPlayer dr = dt.GetDataRow(GlobalStatic.HeroIndex);
            //根据玩家编号更换

            //bg.sprite=
        }
    }
}
