using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class ViewPanelsManagement : MonoBehaviour
    {
        private BaseViewComponent[] PauseComponents;

        private void Awake()
        {
            PauseComponents = GetComponents<BaseViewComponent>();
        }

        private void Start()
        {
            foreach (var pauseComponent in PauseComponents)
            {
                pauseComponent.Init();
            }
        }

        public void CloseOpened()
        {
            foreach (var view in PauseComponents)
            {
                if (view.IsOpened)
                {
                    view.EnableView(false);
                }
            }
        }
    }
}