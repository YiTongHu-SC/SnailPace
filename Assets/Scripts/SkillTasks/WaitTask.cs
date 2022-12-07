using Core;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace DefaultNamespace.SkillTasks
{
    [Category("Snail")]
    public class WaitTask : ActionTask
    {
        private Character Owner;
        public Intent ShowIntent;
        public BBParameter<float> waitTime = 1f;
        public CompactStatus finishStatus = CompactStatus.Success;

        protected override string info
        {
            get { return string.Format("Wait {0} sec.", waitTime); }
        }

        protected override void OnUpdate()
        {
            if (elapsedTime >= waitTime.value)
            {
                EndAction(finishStatus == CompactStatus.Success ? true : false);
            }
            else
            {
                Owner.Energy.SetEnergy(elapsedTime / waitTime.value);
            }
        }

        protected override string OnInit()
        {
            Owner = this.agent.gameObject.GetComponent<Character>();
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            base.OnExecute();
            Owner.Energy.ResetEnergy(waitTime.value * 25.0f);
            BattleManager.Instance.IntentComponent.SetIntent(ShowIntent);
        }
    }
}