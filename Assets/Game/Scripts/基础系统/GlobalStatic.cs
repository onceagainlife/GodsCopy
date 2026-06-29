using UnityEngine;

namespace BackPackLike
{
    public class GlobalStatic : MonoBehaviour
    {
        [Tooltip("暂停计时")]
        public static bool paused = false;

        [Tooltip("座位号")]
        public static int seetNember;

        public static int HeroIndex;
        /// <summary>
        /// 当前正在被鼠标操作的卡牌。
        /// </summary>
        public static Card card;

        /// <summary>
        /// 四个旋转方向的 X 轴方向向量。
        /// 0: 右, 1: 上, 2: 左, 3: 下。
        /// CurrentCard 会用它在开始时缓存 0/90/180/270 四个角度的占用格。
        /// </summary>
        public static readonly Vector2[] directionVector =
        {
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, 0),
            new Vector2(0, -1)
        };

        [Tooltip("可放置")]
        public static Color blueColor = Color.blue; 

        [Tooltip("已被占据")]
        public static Color redColor = new Color(1f, 34f / 255f, 0f, 0.35f);

        [Tooltip("不可放置")]
        public static Color yellowColor = new Color(1f, 144f / 255f, 0f, 1f);

        public static float cardSize=90;


        #region 动画
        [Tooltip("回合数")]
        public static int roundIndex = 0;
        [Tooltip("波次")]
        public static int waveIndex = 0;
        #endregion
    }
}
