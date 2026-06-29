using BackPackLike;
using GameFramework.DataTable;
using UGFExtensions;
using UnityEngine;
using UnityGameFramework.Runtime;


namespace HuaHaiLiKanHua
{
    public class HeroDataSelect : SelectNew
    {
        private void Awake()
        {
            Init();
        }
        public override void Init()
        {
            base.Init();

            AddDataRowBaseToDic<DRPlayer>();
        }

        public void AddDataRowBaseToDic<T>() where T : DataRowBase
        {
            if (StarForce.GameEntry.DataTable == null){Debug.LogWarning("无法获取数据表,请检查"); return; }
            IDataTable<T> dt = StarForce.GameEntry.DataTable.GetDataTable<T>();
            for(int i=0;i< dt.GetAllDataRows().Length;i++)
            {
                if(i >= Childs.Count)
                {
                    Log.Warning("英雄数据行数超过了选择界面预设的选择项数量，请检查");
                    break;
                }
                var choose = Childs[i] as ChooseHero;
                choose.heroIndex = dt.GetAllDataRows()[i].Id;
            }
            if (Childs.Count > 0)
                Childs[0].Select();
        }
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="_index"></param>
        public override void TriggerEvent(int _index)
        {
            base.TriggerEvent(_index);
            StarForce.GameEntry.Event.Fire(this, PlayerSelectionEvent.Create());
        }
    }
}

