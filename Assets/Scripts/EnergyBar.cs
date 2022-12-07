using System;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class EnergyBar : MonoBehaviour
    {
        [SerializeField] private Image EnergyBarImage;
        [SerializeField] private TextMeshProUGUI ShowEnergyAmount;
        private EnergyComponent _owner;

        public void Initialize(EnergyComponent owner)
        {
            _owner = owner;
        }

        private void Update()
        {
            UpdateBar();
        }

        private void UpdateBar()
        {
            if (!_owner) return;
            EnergyBarImage.fillAmount = _owner.EnergyRatio;
            ShowEnergyAmount.text = _owner.CurrentEnergyAmount
                                    + "/"
                                    + _owner.MaxEnergyAmount;
        }
    }
}