using System;
using System.Collections.Generic;
using DefaultNamespace;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core
{
    public class BehaviourController : MonoBehaviour
    {
        [SerializeField] private BehaviourTreeOwner _behaviourTreeOwner;
        private Character _owner;
        private EnergyComponent _energy;
        private HashSet<SkillComponent> _skills;
        private BehaviourTree _behaviourTree;
        public HashSet<SkillComponent> CurrentSkills => _skills;
        public SkillComponent _currentSkill;
        public SkillComponent CurrentSkill => _currentSkill;

        private void Awake()
        {
            TryGetComponent(out _owner);
            TryGetComponent(out _energy);
            if (_behaviourTreeOwner)
            {
                _behaviourTreeOwner.updateMode = Graph.UpdateMode.Manual;
                _behaviourTreeOwner.updateInterval = Time.fixedDeltaTime;
            }

            _skills = new HashSet<SkillComponent>();
        }

        public void Init()
        {
            _skills.Clear();
            foreach (var skill in _owner.SkillSocket.GetComponentsInChildren<SkillComponent>())
            {
                skill.SetOwner(_owner);
                _skills.Add(skill);
            }
        }

        public virtual void InitializeOnCombat()
        {
            if (_behaviourTreeOwner)
            {
                _behaviourTreeOwner.StartBehaviour();
            }
        }

        public void SetTarget(Character target)
        {
            // _target = target;
            foreach (var skill in _skills)
            {
                if (skill)
                {
                    skill.SetTarget(target);
                }
            }
        }

        public void AddSkill(SkillComponent skill)
        {
            _skills.Add(skill);
        }

        public void RemoveSkill(SkillComponent skill)
        {
            _skills.Remove(skill);
        }

        public virtual void FixedTick(float deltaTime)
        {
            if (_energy)
            {
                _energy.FixedTick(deltaTime);
            }

            foreach (var buff in _owner.Buffs)
            {
                buff.FixedTick(deltaTime);
            }

            if (_behaviourTreeOwner)
            {
                _behaviourTreeOwner.UpdateBehaviour();
            }
        }

        public void SetCurrent(SkillComponent current)
        {
            _currentSkill = current;
        }
    }
}