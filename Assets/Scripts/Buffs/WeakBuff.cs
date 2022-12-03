using Core;
using UnityEngine;

namespace DefaultNamespace
{
    public class WeakBuff : Buff
    {
        private const float WeekMultiplier = 0.5f;

        public override void OnAddBuff(Character owner, float duration = -1)
        {
            base.OnAddBuff(owner, duration);
            Owner.AddAtkMultiplier(BuffType.Weak, WeekMultiplier);
        }

        public override void OnRemoveBuff()
        {
            Owner.RemoveBuffAtkMultiplier(BuffType.Weak);
            base.OnRemoveBuff();
        }

        protected override void OnBuffTick(float deltaTime)
        {
            Owner.AddAtkMultiplier(BuffType.Weak, WeekMultiplier);
        }

        public override void OnOverride(float duration)
        {
            ResetCoolDown(duration);
        }
    }
}