using HuaHaiLiKanHua;
using System.Collections.Generic;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    /// 玩家逻辑类，背包能力由 backpack 字段持有,特定的游戏逻辑。
    /// </summary>
    public class PlayerLogic : MonoBehaviour
    {
        /// <summary>
        /// 是否为系统用玩家。系统玩家不会注册到玩家字典。
        /// </summary>
        public bool system;

        [Tooltip("先手次数")]
        public int numberOfTurns;

        public int seatId;
        /// <summary>
        /// 当前玩家拥有的背包组件。
        /// </summary>
        public Backpack backpack;

        /// <summary>
        /// 所有可战斗的卡牌
        /// </summary>
        public List<Card> Cards { get { return backpack.cards; } }

        /// <summary>
        /// 当前手牌
        /// </summary>
        public List<Card> handleCards = new();

        /// <summary>
        /// 当前卡牌
        /// </summary>
        public Card CurrentCard
        {
            get
            {
                return backpack.CurrentCard;
            }
        }

        #region 生命周期
        /// <summary>
        /// 缓存同物体上的 Backpack；如果场景里还没挂，会在运行时补一个。
        /// </summary>
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

        public virtual void OnInit()
        {
            if (backpack == null)
            {
                backpack = gameObject.GetOrAddComponent<Backpack>();
            }
        }
        public virtual void OnShow()
        {
            if (GameManager.GameLogic != null)
            {
                if (GameManager.GameLogic.playerLogicDic.ContainsKey(seatId))
                {
                    seatId = 0;
                    Debug.LogError($"出现相同座位号注册!", gameObject);
                    return;
                }
                GameManager.GameLogic.playerLogicDic[seatId] = this;
            }
        }
        #endregion

        #region 手牌管理
        public void GetCard(Card card)
        {
            //添加卡牌
            handleCards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            //添加卡牌
            handleCards.Remove(card);
        }
        #endregion
    }
}
