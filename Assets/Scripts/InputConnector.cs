using System;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DefaultNamespace
{
    [RequireComponent(typeof(ButtonPresentation))]
    public class InputConnector : MonoBehaviour
    {
        private ButtonPresentation _buttonPresentation;
        private SkillComponent _current;
        private bool _interactable;

        private void Awake()
        {
            _interactable = false;
            _buttonPresentation = GetComponent<ButtonPresentation>();
        }

        public void InputCallBack(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (GameManager.Instance.IsPaused)
                    {
                        return;
                    }

                    switch (GameManager.Instance.CurrentState)
                    {
                        case GameStatus.Encounter:
                            // Debug.Log(context.action.name + " " + "Started");
                            _buttonPresentation.SetPressStatus(true);
                            if (!_interactable)
                            {
                                return;
                            }

                            _current.OnUse();
                            break;
                    }

                    break;
                case InputActionPhase.Performed:
                    // Debug.Log(context.action.name + " " + "Performed");
                    break;
                case InputActionPhase.Canceled:
                    // Debug.Log(context.action.name + " " + "Canceled");
                    _buttonPresentation.SetPressStatus(false);
                    if (!_interactable)
                    {
                        return;
                    }

                    _current.OnCancel();
                    break;
                case InputActionPhase.Waiting:
                    // Debug.Log(context.action.name + " " + "Waiting");
                    break;
                case InputActionPhase.Disabled:
                    // Debug.Log(context.action.name + " " + "Disabled");
                    break;
            }
        }

        public void SetSkill(SkillComponent skill)
        {
            _current = skill;
            SetInteractable(true);
        }

        public void SetInteractable(bool enable)
        {
            _interactable = enable;
        }
    }
}