using Core;
using DefaultNamespace;
using UnityEngine;

namespace HeroPerform
{
    public class Rope : SkillComponent
    {
        public float Duration = 3;

        public override void OnUse()
        {
            if (!Target || Target.IsDead)
            {
                return;
            }

            if (!TryGetPermission())
            {
                return;
            }

            Target.AddBuff<WeakBuff>(BuffType.Weak, Duration);
            base.OnUse();
        }

        public override void OnCancel()
        {
            base.OnCancel();
        }
    }
}