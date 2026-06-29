using System.Collections.Generic;
using UnityEngine;

namespace HuaHaiLiKanHua
{
    public class SkillAnimation : Singleton<SkillAnimation>
    {
        private void Awake()
        {
            SingletonInit();
        }
        // 使用优先队列，按 ExecuteTime 升序排列
        private SimplePriorityQueue<IActionCommand, float> timeline = new();
        private float currentTime = 0f;

        /// <summary>
        /// 加入
        /// </summary>
        /// <param name="skillAction"></param>
        public  void Enqueue(IActionCommand skillAction)
        {
            timeline.Enqueue(skillAction, skillAction.ExecuteTime);
        }
        /// <summary>
        /// 取出
        /// </summary>
        public void UpdateTimeline()
        {
            while(timeline.Count > 0)
            {
                IActionCommand actionCommand= timeline.Peek();

                if (actionCommand == null){ timeline.Dequeue(); continue;  }

                if (currentTime > actionCommand.ExecuteTime)
                {
                    timeline.Dequeue();
                    //进行演出
                    actionCommand.Execute();
                }
                else
                {
                    // 3. 关键优化：因为是最小堆，队首是时间最早的。
                    // 如果队首的时间都还没到，后面的肯定也没到。
                    // 直接 break 退出循环，等待下一帧，避免无效遍历！
                    break;
                }
            }
        }

        //1.几秒的时候,某某发动某个攻击,目标是谁谁谁。
        //2.几秒的时候,某某受到某个伤害,伤害是多少。
        //3.几秒的时候,某某护盾破碎，破碎值是多少。
        //4.几秒的时候,某某死亡。
        //5.几秒的时候，在某个位置生成某个英雄
        //6.几秒时，某某向某方向冲刺/击退/拉拽，速度是多少。距离是多少。
        //7.几秒时，给某某添加某个buff图标。
        //8.几秒时，给某某移除某个Buff图标。
        //9.几秒时，某某增加/扣除蓝量/怒气/能量。
        //10.几秒时,给某某更新技能冷却UI。
    }
}

