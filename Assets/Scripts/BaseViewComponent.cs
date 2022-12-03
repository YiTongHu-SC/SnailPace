using System;
using System.Collections;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DefaultNamespace
{
    [RequireComponent(typeof(ViewPanelsManagement))]
    public class BaseViewComponent : MonoBehaviour
    {
        public GameObject ShowCanvas;
        public Image MaskImage;
        private ViewPanelsManagement _viewPanelsManagement;
        public bool IsOpened { get; set; }

        private void Awake()
        {
            _viewPanelsManagement = GetComponent<ViewPanelsManagement>();
        }

        public virtual void Init()
        {
            IsOpened = false;
            SetPanelAlpha(0);
            ShowCanvas.SetActive(false);
        }

        public virtual void OnTrigger(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    if (IsOpened)
                    {
                        EnableView(false);
                        StartCoroutine(ContinueDelay(0.2f));
                    }
                    else
                    {
                        _viewPanelsManagement.CloseOpened();
                        EnableView(true);
                    }

                    break;
            }
        }

        public virtual void EnableView(bool enable)
        {
            IsOpened = enable;
            if (enable)
            {
                GameEventManager.Instance.OnGamePause.Invoke();
                ShowCanvas.SetActive(true);
                SetPanelAlpha(0);
                MaskImage.DOFade(0.7f, 0.2f);
            }
            else
            {
                SetPanelAlpha(0.7f);
                MaskImage.DOFade(0, 0.2f);
                StartCoroutine(DeActiveDelay(0.2f));
            }
        }

        IEnumerator DeActiveDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            MaskImage.DOKill();
            ShowCanvas.SetActive(false);
        }

        IEnumerator ContinueDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GameEventManager.Instance.OnGameContinue.Invoke();
        }

        private void SetPanelAlpha(float a)
        {
            var color = MaskImage.color;
            color.a = a;
            MaskImage.color = color;
        }
    }
}