using System;
using Core;
using UnityEngine;

namespace DefaultNamespace
{
    public class StatusBox : MonoBehaviour
    {
        private EnergyBar _energyBar;
        private HealthBar _healthBar;
        public EnergyBar EnergyBar => _energyBar;
        public HealthBar HealthBar => _healthBar;

        private void Awake()
        {
            _energyBar = GetComponentInChildren<EnergyBar>();
            _healthBar = GetComponentInChildren<HealthBar>();
        }

        public void Initialize(Character owner)
        {
            _healthBar.Initialize(owner.Health);
            _energyBar.Initialize(owner.Energy);
        }

        public void ShowBox(bool enable)
        {
        }
    }
}