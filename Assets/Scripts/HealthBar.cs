using System;
using System.Collections;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image HealthBarImage;
        [SerializeField] private Image DamageBarImage;
        [SerializeField] private Image WhiteBarImage;
        [SerializeField] private TextMeshProUGUI HealthAmount;
        [SerializeField] private TextMeshProUGUI ArmorAmount;
        [Range(0, 1)] public float CurrentRatio;

        private const float Duration = 0.4f;
        private const float Delay = 0.1f;
        private HealthComponent _owner;

        public void Initialize(HealthComponent owner)
        {
            _owner = owner;
            owner.SetBar(this);
            WhiteBarImage.fillAmount = _owner.GetArmorRatio();
            CurrentRatio = _owner.GetHpRatio();
            HealthBarImage.fillAmount = CurrentRatio;
            DamageBarImage.fillAmount = CurrentRatio;
            HealthAmount.text = _owner.RoundHp
                                + "/"
                                + _owner.RoundMaxHp;
            int armors = _owner.Armors;
            ArmorAmount.text = armors == 0 ? string.Empty : armors.ToString();
        }

        public void UpdateDamageBar()
        {
            if (!_owner || _owner.IsDead)
            {
                return;
            }

            CurrentRatio = _owner.GetHpRatio();
            HealthBarImage.fillAmount = CurrentRatio;
            WhiteBarImage.fillAmount = _owner.GetArmorRatio();
            HealthAmount.text = _owner.RoundHp
                                + "/"
                                + _owner.RoundMaxHp;
            int armors = _owner.Armors;
            ArmorAmount.text = armors == 0 ? string.Empty : armors.ToString();
            StartCoroutine(UpdateDamageDelay_Cro(Delay));
        }

        IEnumerator UpdateDamageDelay_Cro(float delay)
        {
            yield return new WaitForSeconds(delay);
            DOTween.To(() => DamageBarImage.fillAmount,
                x => DamageBarImage.fillAmount = x,
                CurrentRatio,
                Duration);
        }
    }
}