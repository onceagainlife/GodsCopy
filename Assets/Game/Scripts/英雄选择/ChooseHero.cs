
using BackPackLike;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HuaHaiLiKanHua
{
    public class ChooseHero:SelectChild
    {
        public int heroIndex;
        public override void Init(SelectNew selectNew, int _index)
        {
            base.Init(selectNew, _index);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (heroIndex <= 0) return;
            select.TriggerEvent(index);
        }
        /// <summary>
        /// 取消选中
        /// </summary>
        public override void UnSelect()
        {
            base.UnSelect();
            if (heroIndex == GlobalStatic.HeroIndex)
                GlobalStatic.HeroIndex = -1;
        }
        /// <summary>
        /// 选中
        /// </summary>
        public override void Select()
        {
            if (heroIndex <= 0) { Debug.Log("Invalid hero index"); return; }
            base.Select();
            GlobalStatic.HeroIndex = heroIndex;
        }
    }
}
