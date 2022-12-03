using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ButtonPresentation : MonoBehaviour
    {
        [SerializeField] private Image KeyUp;
        [SerializeField] private Image keyDown;

        private void Start()
        {
            if (KeyUp)
            {
                KeyUp.gameObject.SetActive(true);
            }

            if (keyDown)
            {
                keyDown.gameObject.SetActive(false);
            }
        }

        public void SetPressStatus(bool enable)
        {
            KeyUp.gameObject.SetActive(!enable);
            keyDown.gameObject.SetActive(enable);
        }
    }
}