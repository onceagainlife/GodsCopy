using HuaHaiLiKanHua;
using UnityEngine;
using UnityEngine.UI;


namespace BackPackLike
{
    public class CardDetailedUI:MonoBehaviour
    {
        // 在 Inspector 里把对应的 Text 拖进来
        [SerializeField] Text nameDes;
        [SerializeField] Text describe;
        [SerializeField] Text typeDes;

        // 2. 图片部分
        [Header("图片内容")]
        [SerializeField] Image iconBg;
        [SerializeField] Image icon;
        [SerializeField] Image typeBg;

        [ContextMenu("Auto Bind Components")]
        private void Reset()
        {
            AutoBindUtil.Bind(this);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="atkValue"></param>
        /// <param name="hpValue"></param>
        /// <param name="_size"></param>
        public void RefreshData()
        {
            
        }
    }
}

