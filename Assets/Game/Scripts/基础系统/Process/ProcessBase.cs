

namespace HuaHaiLiKanHua
{

    using System;

    [Serializable]
    public class ProcessBase : IGamePhase
    {
        public int Id;
        public string Name;
        public int PhaseId => Id;

        public string PhaseName => Name;

        public MyTimerWait MyTimerWait
        {
            get { return myTimerWait; }
        }

        public MyTimerWait myTimerWait = new();

        public virtual void Enter()
        {
            myTimerWait.Init();
        }

        public virtual void Exit()
        {
            myTimerWait.OnTimerOver();
        }

        public virtual float Tick()
        {
            return myTimerWait?.TimerOver() ?? 0;
        }
    }
}

