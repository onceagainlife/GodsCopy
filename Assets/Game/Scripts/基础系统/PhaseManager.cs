

namespace HuaHaiLiKanHua
{
    using BackPackLike;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    public class PhaseManager
    {
        [SerializeReference]
        public List<IGamePhase> phases = new();
        private int currentIndex = -1;

        public IGamePhase CurrentPhase { get; private set; }

        public Coroutine InitializeAsync()
        {
            return CoroutineRunner.Instance.StartCoroutine(InitializeRoutine());
        }
        /// <summary>
        /// 按配置顺序初始化阶段
        /// </summary>
        private IEnumerator InitializeRoutine()
        {

            if (phases == null || phases.Count == 0)
                yield break;

            //排序
            phases = phases
                .OrderBy(p => p.PhaseId)
                .ToList();

            // 等待 GameLogic 初始化
            yield return new WaitUntil(() => GameLogic.Initialized);

            StartFirst();
        }

     

        /// <summary>
        /// 开始第一个阶段
        /// </summary>
        public void StartFirst()
        {
            currentIndex = 0;
            CurrentPhase = phases[currentIndex];

            CurrentPhase.Enter();
        }

        /// <summary>
        /// 切换到下一个阶段
        /// </summary>
        public void Next()
        {
            CurrentPhase?.Exit();

            currentIndex++;

            if (currentIndex >= phases.Count)
            {
                // 回合结束，重新开始
                currentIndex = 0;
            }

            CurrentPhase = phases[currentIndex];

            CurrentPhase.Enter();
        }

        /// <summary>
        /// 外界调用
        /// </summary>
        /// <returns></returns>
        public Coroutine TickAsync()
        {
            return CoroutineRunner.Instance.StartCoroutine(Tick());
        }
        /// <summary>
        /// 每帧驱动当前阶段
        /// </summary>
        public IEnumerator Tick()
        {
            if (CurrentPhase == null || CurrentPhase.MyTimerWait == null) yield break;
            float timer = CurrentPhase.MyTimerWait.deadline;

            while (timer > 0)
            {
                // 暂停时直接跳过本帧，不要死循环空转
                if (GlobalStatic.paused)
                {
                    yield return null;
                }
                timer = CurrentPhase?.Tick() ?? 0;
                // 必须等待下一帧，否则会在同一帧内把时间耗尽导致卡顿
                yield return null;
            }


            if (timer <= 0)
            {
                Next();
            }
        }




        /// <summary>
        /// 跳转到指定阶段（例如：提前开始战斗）
        /// </summary>
        public void GoTo(int phaseId)
        {
            var target = GetPhase(phaseId) ?? throw new ArgumentException($"未找到阶段: {phaseId}");

            CurrentPhase?.Exit();

            CurrentPhase = target;

            CurrentPhase.Enter();

            currentIndex = phases.IndexOf(target);
        }
        /// <summary>
        /// 按 PhaseId 查找阶段
        /// </summary>
        public IGamePhase GetPhase(int phaseId)
        {
            return phases.Find(p => p.PhaseId == phaseId);
        }
       

        //bool paused = false;
        //public void Tick()
        //{
        //    if (GlobalStatic.paused|| paused) return;
        //        float timer = CurrentPhase?.Tick() ?? 0;

        //    if (timer <= 0)
        //    {
        //        paused = true;
        //        Next();
        //    }
        //}
    }
}
