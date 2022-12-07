using System;
using Core;
using UnityEngine;

namespace DefaultNamespace
{
    public class EnergyComponent : MonoBehaviour
    {
        [SerializeField] private float MaxEnergy = 100;
        public bool IsAutoRecovery;
        private Character _owner;
        public int MaxEnergyAmount => (int)MaxEnergy;
        public int CurrentEnergyAmount => (int)_current;

        private float _recovery;

        // private EnergyBar _energyBar;
        private float _current;
        public float Current => _current;
        public float EnergyRatio => (float)_current / MaxEnergy;

        private void Awake()
        {
            _owner = GetComponent<Character>();
        }

        public void Init()
        {
            _current = 0;
        }

        public void ResetEnergy(float maxEnergy)
        {
            _current = 0;
            MaxEnergy = maxEnergy;
        }

        public void SetEnergy(float ratio)
        {
            _current = ratio * MaxEnergy;
            _current = Mathf.Clamp(_current, 0, MaxEnergy);
        }

        public void FixedTick(float deltaTime)
        {
            if (!IsAutoRecovery)
            {
                return;
            }

            _current += deltaTime * NumFunc.GetEnergyRecovery(_owner.SpeedComponent.Speed);
            _current = Mathf.Clamp(_current, 0, MaxEnergy);
        }

        public void CostEnergy(float value)
        {
            _current -= value;
            _current = Mathf.Clamp(_current, 0, MaxEnergy);
        }
    }
}