using BackPackLike;
using HuaHaiLiKanHua.Gods;
using Sirenix.OdinInspector;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 namespace HuaHaiLiKanHua
{
    [DefaultExecutionOrder(-100)]
    public partial class GameLogic : Singleton<GameLogic>
    {

        public static bool Initialized = false;

        #region 生命周期（原：初始化）

        private void Awake()
        {
            GameManager.GameLogic = this;
            Intit();
        }

        public virtual void Intit()
        {
            SingletonInit();
            GameEntry.Event.Subscribe(StartGameEvent.EventId, PlayerEntityIntilize);
        }
        protected override void Clear()
        {
            base.Clear();
            GameEntry.Event.Unsubscribe(StartGameEvent.EventId, PlayerEntityIntilize);
        }
        private  void OnDestroy()
        {
            Clear();
        }
        #endregion

        #region 游戏状态机
        #endregion

        #region 回合系统

        [LabelText("流程控制")]
        public PhaseManager phaseManager = new ();
        #endregion

        #region 衍生攻击

        [Tooltip("触发攻击的攻击队列")]
        public Queue<BuffData> battleQueue = new();

        public void QueueAdd(BuffData _buffData)
        {
            //if(!battleQueue.TryGetValue(attack,out var lit))
            //{
            //    battleQueue.Add(attack, new Queue<TargetSkillData>());
            //}
            BuffData buffData = _buffData.Clone();
            BuffDataMethod.CheckBuffData(buffData, _buffData.skillData.GetPersonObject.gameObject);
            battleQueue.Enqueue(buffData);
        }

        public void BattleProcess()
        {
            StartCoroutine(BattleQueueProcess());
        }
        /// <summary>
        /// 战斗队列处理
        /// </summary>
        public IEnumerator BattleQueueProcess()
        {
            // 安全的检查：在尝试取出元素前，先确认队列不为空
            while (battleQueue.Count > 0)
            {
                // 使用 Dequeue 取出并移除队列中的第一个元素

                BuffData _buffData = battleQueue.Dequeue();

                //// 在这里编写处理 nextSkill 的逻辑
                //Debug.Log($"开始执行技能：{nextSkill.Name}");


                yield return StartCoroutine(Process(_buffData));
            }

        }
        /// <summary>
        /// 战斗队列处理
        /// </summary>
        /// <param Name="buffData"></param>
        /// <returns></returns>
        public IEnumerator Process(BuffData buffData)
        {
            SkillData skillData = buffData.skillData;

            BuffDataMethod.CheckBuffData(buffData, skillData.GetPersonObject.gameObject);

            if (skillData == null) Debug.Log($"出错了");

            PersonObject personObject = skillData.GetPersonObject;

            personObject.PersonSkillManager.currentSkillData = skillData;

            PersonSkillManager personSkillManager = personObject.PersonSkillManager;

            PersonManager personManager = personObject.PersonManager;

            if (personObject.IsDead) { Debug.Log("已死亡"); yield break; }
            if (personSkillManager.currentSkillData == null) { Debug.Log("技能为空"); yield break; }
            personObject.ChangeBool(false);

            if (personManager.BaseAttackManager(buffData))
            {

            }
            else
            {

            }
            yield return new WaitUntil(() => personObject.skillComplete);
            yield return new WaitUntil(() => Instance.deadHeroes.Count == 0);
            yield return null;
        }
        #endregion

        #region 战斗系统

        [HideInInspector]
        public HashSet<Entity> deadHeroes = new();
        [HideInInspector]
        public List<Entity> deadEntityList = new List<Entity>();


        #endregion

        #region 地图与寻路
        #endregion

        #region 事件系统

        private PhaseBasedSkillSystem _phaseBasedSkillSystem = new();

        public PhaseBasedSkillSystem PhaseBasedSkillSystem
        {
            get { return _phaseBasedSkillSystem; }
        }

        public void OnTrigger(TriggerTime triggerTime, BuffData buffData = null)
        {
            PhaseBasedSkillSystem.TriggerPhase(triggerTime, null);
        }

        #endregion

        #region 数据管理
        #endregion

        #region 规则校验
        #endregion

        #region AI 系统（可选）
        #endregion

        #region UI 通信（可选）

        public void OnButton()
        {

        }

        #endregion
    }
}

