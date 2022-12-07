using System;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public enum Intent
    {
        None,
        Attack,
        Defence,
        UnKnown
    }

    public class IntentComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI AttackText;
        [SerializeField] private Image IntentAttack;
        [SerializeField] private Image IntentDefence;
        [SerializeField] private Image IntentUnKnown;
        private Dictionary<Intent, Image> _intents;
        private Intent _currentIntent;
        private Character _owner;

        private void Awake()
        {
            _intents = new Dictionary<Intent, Image>();
            _intents.Add(Intent.Attack, IntentAttack);
            _intents.Add(Intent.Defence, IntentDefence);
            _intents.Add(Intent.UnKnown, IntentUnKnown);
        }

        public void Initialize(Character owner)
        {
            _owner = owner;
            SetIntent(Intent.None);
        }

        public void SetIntent(Intent intent)
        {
            _currentIntent = intent;
            foreach (var intentPair in _intents)
            {
                intentPair.Value.enabled = false;
            }

            if (_intents.ContainsKey(intent))
            {
                _intents[intent].enabled = true;
            }
        }

        private void Update()
        {
            // Refresh Attack Num
            if (_owner && !_owner.IsDead)
            {
                if (_currentIntent == Intent.Attack)
                {
                    AttackText.text = _owner.BehaviourController.CurrentSkill.GetDamage().ToString();
                }
                else
                {
                    AttackText.text = string.Empty;
                }
            }
        }
    }
}