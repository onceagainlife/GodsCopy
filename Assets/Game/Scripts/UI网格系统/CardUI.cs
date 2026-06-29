
namespace BackPackLike
{
    using UnityEngine;
    using UnityEngine.UI;

    // 1. 让脚本在编辑器模式下也能运行，这样拖拽上去就会自动执行 Awake
    [ExecuteInEditMode]
    public class CardUI : MonoBehaviour
    {
        [Header("=== 基础信息 ===")]
        public Image iconImage;        // Icon 下的图片
        public RectTransform sizeRect; // size 节点 (通常用于控制卡牌大小)

        [Header("=== 攻击力模块 ===")]
        public Image attackBg;         // attackBg (背景)
        public Text attackText;        // attack (数值文本)

        [Header("=== 生命值模块 ===")]
        public Image hpBg;             // hpBg (背景)
        public Text hpText;            // hp (数值文本)

        public Text size;           // size 节点下的 Text (显示卡牌尺寸)
        // --- 数据绑定方法 ---

        /// <summary>
        /// 外部调用此方法来刷新卡牌显示的数据
        /// </summary>
        public void RefreshData(Sprite sprite, int atkValue, int hpValue,int _sizeX,int _sizeY)
        {
            if (iconImage != null) iconImage.sprite = sprite;
            if (attackText != null) attackText.text = atkValue.ToString();
            if (hpText != null) hpText.text = hpValue.ToString();
            if(size != null) size.text = (_sizeX*_sizeY).ToString();


            // 1. 获取当前的 sizeDelta (防止覆盖掉锚点带来的偏移量)
            Vector2 newSize = sizeRect.sizeDelta;

            // 2. 修改数值
            newSize.x = _sizeX * GlobalStatic.cardSize;
            newSize.y = _sizeY*GlobalStatic.cardSize;

            // 3. 赋值回去
            sizeRect.sizeDelta = newSize;
        }

        #region 自动绑定
        // --- 自动绑定逻辑 ---
        private void Reset()
        {
            AutoBind();
        }

        // 2. 添加右键菜单功能
        [ContextMenu("Auto Bind Components")]
        public void AutoBind()
        {
            // 使用 transform.Find 根据层级路径查找子物体
            // 注意：路径是相对于当前 card 节点的
            sizeRect = GetComponent<RectTransform>();

            // 1. 绑定 Icon
            Transform iconTrans = transform.Find("icon");
            if (iconTrans != null)
                iconImage = iconTrans.GetComponent<Image>();

            // 2. 绑定 Size
            Transform sizeTrans = transform.Find("size");
            size = sizeTrans?.GetComponent<Text>();

            // 3. 绑定 Attack 组
            Transform attackBgTrans = transform.Find("attackBg");
            if (attackBgTrans != null)
            {
                attackBg = attackBgTrans.GetComponent<Image>();
                // 继续向下查找具体的 Text
                Transform attackTextTrans = attackBgTrans.Find("attack");
                if (attackTextTrans != null)
                    attackText = attackTextTrans.GetComponent<Text>();
            }

            // 4. 绑定 HP 组
            Transform hpBgTrans = transform.Find("hpBg");
            if (hpBgTrans != null)
            {
                hpBg = hpBgTrans.GetComponent<Image>();
                // 继续向下查找具体的 Text
                Transform hpTextTrans = hpBgTrans.Find("hp");
                if (hpTextTrans != null)
                    hpText = hpTextTrans.GetComponent<Text>();
            }

            Debug.Log($"[CardUI] Auto Binding Completed for {gameObject.name}");
        }
        #endregion
    }
}