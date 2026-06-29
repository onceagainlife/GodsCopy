using System;
using UnityEngine;
using static UnityEditor.ShaderData;

namespace HuaHaiLiKanHua
{
    /// <summary>
    /// 游戏回合阶段接口（Turn / Phase）。
    /// </summary>
    public interface IGamePhase
    {
        /// <summary>
        /// 阶段唯一标识（不可重复）。
        /// 用途：
        /// - 阶段注册 / 查找
        /// - 存档 / 热更配置
        /// - 日志与调试
        /// 示例："Preparation", "Combat", "Result"
        /// </summary>
        int PhaseId { get; }

        /// <summary>
        /// 阶段名称（可读）。
        /// 用途：
        /// - UI 显示
        /// - 日志与调试
        /// 示例："准备阶段", "战斗阶段", "结算阶段"
        /// </summary>
        string PhaseName { get; }

        /// <summary>
        /// 进入该阶段时调用一次。
        /// 常见操作：
        /// - 解锁/锁定背包
        /// - 初始化战斗单位
        /// - 重置计时器
        /// - 触发事件（OnPhaseEnter）
        /// </summary>
        void Enter();

        /// <summary>
        /// 该阶段的每帧更新逻辑。
        /// 不要在 Tick 中写复杂逻辑，建议只做：
        /// - 计时
        /// - 状态检测
        /// - 驱动子系统 Tick
        /// </summary>
        /// <param name="deltaTime">Unity Time.deltaTime</param>
        float Tick();

        /// <summary>
        /// 退出该阶段时调用一次。
        /// 常见操作：
        /// - 清理临时状态
        /// - 保存数据
        /// - 停止战斗 / 清空战场
        /// - 触发事件（OnPhaseExit）
        /// </summary>
        void Exit();

        public MyTimerWait MyTimerWait
        {
            get;
        }
    }
}

