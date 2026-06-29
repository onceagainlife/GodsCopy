using GameFramework;
using HuaHaiLiKanHua.Gods;
using System.Collections.Generic;

namespace HuaHaiLiKanHua
{
    // 基础指令接口
    public interface IActionCommand : IReference
    {
        float ExecuteTime { get; set; }
        void Execute();
    }

    // 1. 发动攻击指令
    public class AttackCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int CasterID;
        public List<int> TargetIDs = new();
        public int SkillID;

        public static AttackCommand Create()
        {
            return ReferencePool.Acquire<AttackCommand>();
        }

        public void Execute()
        {
            // 表现层：让 CasterID 播放 SkillID 对应的攻击动画
        }

        public void Clear()
        {
            CasterID = 0;
            TargetIDs.Clear();
            SkillID = 0;
        }
    }

    // 2. 受到伤害指令
    public class DamageCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public int DamageValue;

        public static DamageCommand Create()
        {
            return ReferencePool.Acquire<DamageCommand>();
        }

        public void Execute()
        {
            // 表现层：播放受击动画、生成伤害飘字
            // 数据层：扣除血量
        }

        public void Clear()
        {
            TargetID = 0;
            DamageValue = 0;
        }
    }

    // 3. 护盾破碎指令
    public class ShieldBreakCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public int BrokenShieldValue;

        public static ShieldBreakCommand Create()
        {
            return ReferencePool.Acquire<ShieldBreakCommand>();
        }

        public void Execute()
        {
            // 表现层：播放护盾碎裂特效、特殊的UI提示
        }

        public void Clear()
        {
            TargetID = 0;
            BrokenShieldValue = 0;
        }
    }

    // 4. 死亡指令
    public class DeathCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;

        public static DeathCommand Create()
        {
            return ReferencePool.Acquire<DeathCommand>();
        }

        public void Execute()
        {
            // 表现层：播放死亡/倒地动画，隐藏血条
            // 数据层：将 TargetID 从存活列表中移除
        }

        public void Clear()
        {
            TargetID = 0;
        }
    }

    // 5. 生成实体指令（英雄/野怪/召唤物）
    public class SpawnCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int EntityID;
        public int ConfigID;
        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public static SpawnCommand Create()
        {
            return ReferencePool.Acquire<SpawnCommand>();
        }

        public void Execute()
        {
            // 数据层：在实体管理器中注册该 EntityID，初始化血量、属性等
            // 表现层：在 (X,Y,Z) 位置实例化对应的 Prefab，播放出场特效
        }

        public void Clear()
        {
            EntityID = 0;
            ConfigID = 0;
            PositionX = 0f;
            PositionY = 0f;
            PositionZ = 0f;
        }
    }

    // 6. 位移指令（冲刺/击退/拉拽）
    public class MoveCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public float DirX;
        public float DirY;
        public float DirZ;
        public float Distance;
        public float Speed;

        public static MoveCommand Create()
        {
            return ReferencePool.Acquire<MoveCommand>();
        }

        public void Execute()
        {
            // 数据层：根据方向和距离计算新坐标，更新目标的位置数据，进行碰撞检测
            // 表现层：播放滑步/击退动画，触发位移拖尾特效
        }

        public void Clear()
        {
            TargetID = 0;
            DirX = 0f;
            DirY = 0f;
            DirZ = 0f;
            Distance = 0f;
            Speed = 0f;
        }
    }

    // 7. 添加Buff指令
    public class AddBuffCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public int BuffID;
        //public float Duration;
        public int StackCount;//层数
        public List<PositiveStruct> positiveStructs = new();

        public static AddBuffCommand Create(float executeTime,int targetID,int buffID,int stackCount,List<PositiveStruct> positiveStructs = null)
        {
            var cmd = ReferencePool.Acquire<AddBuffCommand>();
            cmd.ExecuteTime = executeTime;
            cmd.TargetID = targetID;
            cmd.BuffID = buffID;
            cmd.StackCount = stackCount;
            if (positiveStructs != null)
                cmd.positiveStructs.AddRange(positiveStructs);
            return cmd;
        }

        public void Execute()
        {
            // 数据层：将 Buff 加入目标的 Buff 列表，开始计时，触发 Buff 挂载时的逻辑
            // 表现层：在目标头顶/脚下添加对应的 Buff 图标，播放上Buff特效
        }

        public void Clear()
        {
            TargetID = 0;
            BuffID = 0;
            //Duration = 0f;
            StackCount = 0;
            positiveStructs.Clear();
        }
    }

    // 8. 移除Buff指令
    public class RemoveBuffCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public int BuffID;

        public static RemoveBuffCommand Create()
        {
            return ReferencePool.Acquire<RemoveBuffCommand>();
        }

        public void Execute()
        {
            // 数据层：从目标的 Buff 列表中移除该 Buff，触发 Buff 移除时的逻辑
            // 表现层：移除对应的 Buff 图标，播放Buff消失特效
        }

        public void Clear()
        {
            TargetID = 0;
            BuffID = 0;
        }
    }

    // 9. 资源变动指令（蓝量/怒气/能量）
    public class ResourceChangeCommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public int ResourceType;
        public int ChangeValue;

        public static ResourceChangeCommand Create()
        {
            return ReferencePool.Acquire<ResourceChangeCommand>();
        }

        public void Execute()
        {
            // 数据层：修改目标的对应资源数值，检查是否达到上限/下限
            // 表现层：更新 UI 上的蓝条/怒气条，播放资源增加/扣除的飘字或特效
        }

        public void Clear()
        {
            TargetID = 0;
            ResourceType = 0;
            ChangeValue = 0;
        }
    }

    // 10. 更新技能冷却UI指令
    public class UpdateSkillCooldownUICommand : IActionCommand
    {
        public float ExecuteTime { get; set; }
        public int TargetID;
        public int SkillID;
        public float ChangeValue;

        public static UpdateSkillCooldownUICommand Create()
        {
            return ReferencePool.Acquire<UpdateSkillCooldownUICommand>();
        }

        public void Execute()
        {
            // 表现层/UI层：根据 IsReady 状态，更新对应技能的 UI 图标。
        }

        public void Clear()
        {
            TargetID = 0;
            SkillID = 0;
            ChangeValue = 0f;
        }
    }
}