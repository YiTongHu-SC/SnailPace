using System;
using DefaultNamespace;
using Lean.Pool;
using UnityEngine;

namespace Core
{
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField] private float MaxHp;
        [SerializeField] private bool ShowHealthBar;
        [SerializeField] private Color TipsColor;

        private float _maxHp;
        private int _armor;
        private bool _isDead;
        private Character _owner;
        private HealthBar _healthBar;
        public float CurrentHp { get; private set; }
        public int RoundHp => Mathf.RoundToInt(CurrentHp);
        public int RoundMaxHp => Mathf.RoundToInt(_maxHp);
        public int Armors => _armor;
        public bool IsWithArmor => Armors != 0;
        public float ArmorCountDown => _armorTimer / RemoveArmorAfter;
        public bool IsDead => _isDead;
        private float _armorTimer;
        private const float RemoveArmorAfter = 3;

        public delegate void CallBack();

        private void Awake()
        {
            _owner = GetComponent<Character>();
            _maxHp = MaxHp;
        }

        public void SetBar(HealthBar bar)
        {
            _healthBar = bar;
            if (_healthBar)
            {
                _healthBar.gameObject.SetActive(ShowHealthBar);
                // _healthBar.Initialize(this);
            }
            else
            {
                ShowHealthBar = false;
            }
        }

        public void Init()
        {
            _isDead = false;
            ResetArmorTimer();
            ResetHp();
        }

        public void RemoveAllArmors()
        {
            RemoveArmor(Int32.MaxValue);
        }

        private void ResetArmorTimer()
        {
            _armorTimer = RemoveArmorAfter;
        }

        public float GetHpRatio()
        {
            if (CurrentHp + Armors > _maxHp)
            {
                return CurrentHp / (CurrentHp + Armors);
            }
            else
            {
                return CurrentHp / _maxHp;
            }
        }

        public float GetArmorRatio()
        {
            if (CurrentHp + Armors > _maxHp)
            {
                return 1;
            }
            else
            {
                return (float)(CurrentHp + Armors) / _maxHp;
            }
        }

        private void ResetHp()
        {
            _armor = 0;
            CurrentHp = _maxHp;
        }

        public void Cure(float cure)
        {
            ChangeHp(cure);
            _healthBar.UpdateDamageBar();
        }

        public void TakeDamage(float damage, CallBack onDeadCallBack = null)
        {
            var roundToInt = Mathf.RoundToInt(_owner.GetBuffDamageMultiplier() * damage);
            // show tip
            var tip = LeanPool.Spawn(GameManager.Instance.ShowTipComponent);
            tip.transform.position = _owner.TipSocket.position;
            tip.SetTips(roundToInt.ToString(), TipsColor);
            // 
            _armor -= roundToInt;
            var RoundDamage = _armor;
            _armor = Mathf.Clamp(_armor, 0, Int32.MaxValue);
            RoundDamage = Mathf.Clamp(RoundDamage, Int32.MinValue, 0);
            ChangeHp(RoundDamage, onDeadCallBack);
            if (RoundDamage < 0)
            {
                _owner.TriggerHurt();
            }

            _healthBar.UpdateDamageBar();
        }

        private void ChangeHp(float hp, CallBack changeHpCallBack = null)
        {
            CurrentHp += hp;
            CurrentHp = Mathf.Clamp(CurrentHp, 0, _maxHp);
            if (RoundHp == 0)
            {
                Dead(changeHpCallBack);
            }
        }

        private void Dead(CallBack callBack)
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            switch (_owner.CharacterType)
            {
                case CharacterType.Hero:
                    GameEventManager.Instance.OnGameOver.Invoke();
                    break;
                case CharacterType.Enemy:
                    Debug.Log("OnEnemyDead");
                    GameEventManager.Instance.OnEnemyDead.Invoke();
                    break;
            }

            callBack?.Invoke();
            LeanPool.Despawn(_owner.gameObject);
            // StartCoroutine(DelayDead_Cro(0.6f));
        }

        // IEnumerator DelayDead_Cro(float delay)
        // {
        //     yield return new WaitForSeconds(delay);
        //     LeanPool.Despawn(_owner.gameObject);
        // }

        public void AddArmor(int armor)
        {
            _armor += Mathf.CeilToInt(_owner.GetBuffArmorMultiplier() * armor);
            _armor = Mathf.Clamp(_armor, 0, Int32.MaxValue);
            ResetArmorTimer();
            if (ShowHealthBar)
            {
                _healthBar.UpdateDamageBar();
            }
        }

        private void RemoveArmor(int armor)
        {
            _armor -= armor;
            _armor = Mathf.Clamp(_armor, 0, Int32.MaxValue);
            if (ShowHealthBar)
            {
                _healthBar.UpdateDamageBar();
            }
        }

        public void SetLevel(int level)
        {
            _maxHp = NumFunc.GetLevelUpHP(MaxHp, level);
            Init();
        }
    }
}