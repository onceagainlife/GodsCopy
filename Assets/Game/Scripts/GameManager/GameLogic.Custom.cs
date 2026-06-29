using BackPackLike;
using GameFramework.Event;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityExtension = HuaHaiLiKanHua.Gods.EntityExtension;

namespace HuaHaiLiKanHua
{
    public partial class GameLogic
    {
        /// <summary>
        /// 座位编号到玩家逻辑的映射。
        /// </summary>
        public Dictionary<int, PlayerLogic> playerLogicDic = new();

        public GameObject playerLogicPrefab;
        public int maxIndex = 6;

        [HideInInspector]
        [Tooltip("当前玩家")]
        public PlayerLogic playerLogic;
        public Player player;//ui数据
        //[Tooltip("敌人列表")]
        //public List<PlayerLogic> enemyList = new();
        [HideInInspector]
        public PlayerLogic enemy;

        [Tooltip("战斗顺序")]
        public List<PlayerLogic> playerLogicList = new();

        [Tooltip("玩家阵营字典，记录阵营之间的关系。")]
        public Dictionary<CampType, Dictionary<CampType, RelationType>> PlayerLogicDic = new();

        /// <summary>
        /// 获取指定座位上的玩家逻辑。该方法要求字典里必须已经存在玩家。
        /// </summary>
        public PlayerLogic PlayerLogic(int index)
        {
            return playerLogicDic[index];
        }

        /// <summary>
        /// 安全获取指定座位上的玩家逻辑，避免字典里不存在玩家时直接抛异常。
        /// </summary>
        /// <param name="index">座位编号。</param>
        /// <param name="playerLogic">找到时返回玩家逻辑。</param>
        /// <returns>是否成功找到玩家逻辑。</returns>
        public bool TryGetPlayerLogic(int index, out PlayerLogic playerLogic)
        {
            return playerLogicDic.TryGetValue(index, out playerLogic);
        }

        #region 生命周期

        private void Update()
        {

            if (Input.GetMouseButtonDown(1))
            {
                if (GlobalStatic.card != null && GlobalStatic.card.Move())
                {
                    GlobalStatic.card.Rotation();
                }
            }
        }

        protected override void SingletonInit()
        {
            base.SingletonInit();
        }
        #endregion

        #region 战斗流程

        public GameResult gameResult = GameResult.Battle;

        /// <summary>
        /// 进入战斗阶段,载入敌人
        /// </summary>
        public void GetEnemy()
        {
            
        }

        /// <summary>
        /// 战斗前
        /// </summary>
        public void OnBeforeBattle()
        {
            playerLogicList.Clear();
            playerLogicList.Add(enemy);
            playerLogicList.Add(playerLogic);
           
            foreach (var playerLogic in playerLogicList)
            {
                playerLogic.backpack.SortByBattlePriority();
            }

            gameResult = GetGameResult();
            //如果游戏结果是战斗中，则进行战斗顺序排序
            if (gameResult == GameResult.Battle)
                Sort();
        }

        /// <summary>
        /// 战斗顺序排序,只适合双人对战
        /// </summary>
        public void Sort()
        {
            System.Random random = new();

            playerLogicList.Sort((a, b) =>
            {
                // 1. 首先按照 numberOfTurns 降序排列（大的在前）
                int compareResult = a.numberOfTurns.CompareTo(b.numberOfTurns);

                if (compareResult != 0)
                {
                    return compareResult;
                }

                // 3. 如果回合数完全相等，则通过随机数决定先后顺序
                // 返回 -1 表示 a 排在前面，返回 1 表示 b 排在前面
                return random.Next(0, 2) == 0 ? -1 : 1;
            });
            playerLogicList[0].numberOfTurns++;
        }

        // 方法名改为获取结果
        public GameResult GetGameResult()
        {
            if (enemy.Cards.Count <= 0 && playerLogic.Cards.Count <= 0)
                return GameResult.Draw;
            else if (enemy.Cards.Count <= 0 && playerLogic.Cards.Count > 0)
                return GameResult.PlayerWin;
            else if (enemy.Cards.Count >0 && playerLogic.Cards.Count <= 0)
                return GameResult.EnemyWin;
            else return GameResult.Battle;
        }
        /// <summary>
        /// 主要战斗流程
        /// </summary>
        /// <returns></returns>
        public IEnumerator BattleStage()
        {
            int currentTurnIndex = 0; // 记录当前轮到哪个玩家(0=玩家, 1=敌人...)

            //战斗开始前触发 OnTurnStart事件,这是先手技能的触发时机
            OnTrigger(TriggerTime.OnTurnStart);

            while (gameResult == GameResult.Battle)
            {
                var currentPlayer = playerLogicList[currentTurnIndex];

                // 【核心改动 1】：不再用 yield break，而是检查是否还能继续战斗
                if (currentPlayer.Cards.Count <= 0)
                {
                    gameResult = GetGameResult(); // 重新判定游戏结果
                    yield break; // 直接结束当前的 BattleStage 协程
                }

                // 【核心改动 2】：只负责推进到下一个【存活且有效】的卡牌，绝不在此处 Remove
                currentPlayer.backpack.AdvanceToNextValidAliveCard();

                // 如果推进后依然没有有效卡牌（说明全死了或全是Null）
                if (currentPlayer.CurrentCard == null)
                {
                    gameResult = GetGameResult();
                    // 切换回合，不 yield break
                }
                else
                {
                    // 正常执行攻击
                    yield return StartCoroutine(currentPlayer.CurrentCard.obj.Attack());

                    // 每次行动后都检查一次胜负
                    gameResult = GetGameResult();
                }

                // 【核心改动 3】：平滑切换回合
                if (playerLogicList.Count > 0)
                {
                    currentTurnIndex = (currentTurnIndex + 1) % playerLogicList.Count;
                }
            }
        }
        #endregion



        /// <summary>
        /// 玩家英雄实体化
        /// </summary>
        /// <param Name="Id"></param>
        public void PlayerEntityIntilize(object sender, GameEventArgs e)
        {
            EntityExtension.GetPlayerEntity(player, CampType.Faction1, GlobalStatic.HeroIndex);
        }
    }
}
