using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using Lean.Pool;
using MoreMountains.Tools;
using ParadoxNotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Core
{
    public class BattleManager : MMSingleton<BattleManager>
    {
        [SerializeField] private StatusBox PlayerStatus;
        [SerializeField] private StatusBox EnemyStatus;
        [SerializeField] public EnemyData EncounterEnemyData;
        [SerializeField] private string[] InitSkills;
        [SerializeField] private SkillReward SkillRewardPref;
        [SerializeField] private GameObject HeroPrefab;
        [SerializeField] private Transform SkillTransform;
        [SerializeField] private Transform[] SkillSockets;
        [SerializeField] private Transform RewardSkillSocket;
        [SerializeField] private Transform SpawnSocket;
        [SerializeField] private GameObject ContinueButton;
        [SerializeField] private Image ChoosePanel;
        [SerializeField] private Transform SkillViewSocket;
        [SerializeField] private Transform SkillView;
        [SerializeField] public GameObject WinningPrefab;

        private bool _isRefreshOpen;
        private Character _hero;
        private Character _encounterEnemy;
        private LoopMoveGrid _loopMoveGrid;
        private List<string> _skillNames;
        private List<SkillReward> _allRewards;
        private List<SkillReward> _currentRewards;
        private List<LoopSocket> _loopSockets;
        private List<SkillComponent> _currentSkills;
        private Dictionary<string, SkillComponent> _skillDict;
        public int CurrentRewardNum => SkillViewSocket.childCount;
        public Character Hero => _hero;
        public Character EncounterEnemy => _encounterEnemy;

        protected override void Awake()
        {
            base.Awake();
            _isRefreshOpen = true;
            _allRewards = new List<SkillReward>();
            _currentRewards = new List<SkillReward>();
            _currentSkills = new List<SkillComponent>();
            _skillDict = new Dictionary<string, SkillComponent>();
            _skillNames = new List<string>();
            _loopSockets = new List<LoopSocket>();
            _loopMoveGrid = GetComponentInChildren<LoopMoveGrid>();
        }

        public void OnGameStart()
        {
            // TODO:检查是否完成了初始化任务再出发
            StartCoroutine(GameStart_Cro());
        }

        IEnumerator GameStart_Cro()
        {
            foreach (var _reward in _allRewards)
            {
                if (_reward)
                {
                    LeanPool.Despawn(_reward);
                }
            }

            _allRewards.Clear();

            foreach (var skill in _currentSkills)
            {
                if (skill)
                {
                    LeanPool.Despawn(skill);
                }
            }

            _currentSkills.Clear();

            var hero = LeanPool.Spawn(HeroPrefab, SpawnSocket.position, Quaternion.identity);
            SetHero(hero.GetComponent<Character>());
            PlayerStatus.Initialize(Hero);
            Hero.TriggerIdle();
            ContinueButton.SetActive(false);
            SkillView.gameObject.SetActive(false);
            //
            var color = ChoosePanel.color;
            color.a = 0;
            ChoosePanel.color = color;
            //
            _loopSockets.Clear();
            foreach (var trans in SkillSockets)
            {
                _loopSockets.Add(new LoopSocket(trans));
            }

            InitSkillData();
            ResetBattlePanel();
            foreach (var skillName in InitSkills)
            {
                var skillReward = SpawnReward(skillName);
                skillReward.OnAddSkill();
            }

            _loopMoveGrid.OnReset();
            yield break;
        }

        private void InitSkillData()
        {
            _skillDict.Clear();
            var skills = Resources.LoadAll<SkillComponent>("Skills");
            foreach (var skill in skills)
            {
                if (!_skillDict.ContainsKey(skill.SkillName))
                {
                    _skillDict.Add(skill.SkillName, skill);
                }
            }

            _skillNames = new List<string>(_skillDict.Keys);
        }

        private void OnAddSkill(SkillReward skillReward)
        {
            if (skillReward.SkillTarget.IsExhausted)
            {
                skillReward.SkillTarget.SetOwner(Hero);
                skillReward.SkillTarget.OnUse();
                LeanPool.Despawn(skillReward);
            }
            else
            {
                skillReward.transform.SetParent(SkillViewSocket);
                _allRewards.Add(skillReward);
                AddSkillTarget(skillReward.SkillTarget);
                // TODO: Run Continue Delay
            }

            GameEventManager.Instance.OnRunContinue.Invoke();
        }

        private void AddSkillTarget(SkillComponent skillTarget)
        {
            int index = _currentSkills.Count;
            var skill = LeanPool.Spawn(skillTarget, SkillTransform);
            skill.transform.position = 10 * Vector3.down;
            _hero.BehaviourController.AddSkill(skill);
            skill.SetOwner(Hero);
            if (index == 3)
            {
                skill.SetInvisible();
                return;
            }

            skill.SetFollow(_loopSockets[index]);
            _currentSkills.Add(skill);
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.IsPaused)
            {
                return;
            }

            switch (GameManager.Instance.CurrentState)
            {
                case GameStatus.Encounter:
                    Hero.BehaviourController.FixedTick(Time.fixedDeltaTime);
                    _encounterEnemy.BehaviourController.FixedTick(Time.fixedDeltaTime);
                    break;
                case GameStatus.Run:
                    _loopMoveGrid.FixedTick(Time.fixedDeltaTime);
                    Hero.BehaviourController.FixedTick(Time.fixedDeltaTime);
                    break;
            }
        }

        private void SetHero(Character character)
        {
            _hero = character;
        }

        /// <summary>
        /// OnDisable, we start listening to events.
        /// </summary>
        protected virtual void OnEnable()
        {
            GameEventManager.Instance.OnRunEncounter += OnRunEncounter;
            GameEventManager.Instance.OnRunReward += OnRunReward;
            GameEventManager.Instance.OnRunContinue += OnRunContinue;
            GameEventManager.Instance.OnEnemyDead += OnReward;
            GameEventManager.Instance.OnGameOver += OnGameOver;
            GameEventManager.Instance.OnAddSkill += OnAddSkill;
        }

        /// <summary>
        /// OnDisable, we stop listening to events.
        /// </summary>
        protected virtual void OnDisable()
        {
            GameEventManager.Instance.OnRunEncounter -= OnRunEncounter;
            GameEventManager.Instance.OnRunReward -= OnRunReward;
            GameEventManager.Instance.OnRunContinue -= OnRunContinue;
            GameEventManager.Instance.OnEnemyDead -= OnReward;
            GameEventManager.Instance.OnGameOver -= OnGameOver;
            GameEventManager.Instance.OnAddSkill -= OnAddSkill;
        }

        private void OnGameOver()
        {
            _hero = null;
            _encounterEnemy = null;
        }

        private void OnRunEncounter(Character target)
        {
            _encounterEnemy = target;
            EnemyStatus.Initialize(target);
            EnemyStatus.ShowBox(true);
            Hero.BehaviourController.SetTarget(EncounterEnemy);
            Hero.BehaviourController.InitializeOnCombat();
            _encounterEnemy.BehaviourController.SetTarget(Hero);
            _encounterEnemy.BehaviourController.InitializeOnCombat();
        }

        private void OnRunContinue()
        {
            ResetBattlePanel();
        }

        private void OnRunReward()
        {
            ChoosePanel.DOFade(0.6f, 0.3f);
            // EnemyStatus.Initialize(null);
            EnemyStatus.ShowBox(false);
        }

        private void ResetBattlePanel()
        {
            ChoosePanel.DOFade(0, 0.3f);
        }

        private void OnReward()
        {
            _encounterEnemy = null;
            Hero.BehaviourController.SetTarget(EncounterEnemy);
            GameEventManager.Instance.OnRunReward.Invoke();
            AddRandomRewards();
        }

        private void AddRandomRewards()
        {
            _currentRewards.Clear();
            List<int> record = new List<int>();
            int count = 0;
            while (count < 3)
            {
                int rand = Random.Range(0, _skillNames.Count);
                bool ok = false;
                while (!ok)
                {
                    if (record.Contains(rand))
                    {
                        rand = Random.Range(0, _skillNames.Count);
                    }
                    else
                    {
                        ok = true;
                    }
                }

                record.Add(rand);
                count += 1;
            }

            foreach (var index in record)
            {
                var skillName = _skillNames[index];
                var skillReward = SpawnReward(skillName, RewardSkillSocket);
                _currentRewards.Add(skillReward);
            }
        }

        private SkillReward SpawnReward(string skillName)
        {
            var reward = LeanPool.Spawn(SkillRewardPref);
            reward.SetSkillObject(_skillDict[skillName]);
            return reward;
        }

        private SkillReward SpawnReward(string skillName, Transform parent)
        {
            var reward = LeanPool.Spawn(SkillRewardPref, parent);
            reward.SetSkillObject(_skillDict[skillName]);
            return reward;
        }

        public void ChooseRewardInput_1(InputAction.CallbackContext context)
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameStatus.Reward:
                    switch (context.phase)
                    {
                        case InputActionPhase.Performed:
                            _currentRewards[0].OnAddSkill();
                            break;
                    }

                    break;
            }
        }

        public void ChooseRewardInput_2(InputAction.CallbackContext context)
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameStatus.Reward:
                    switch (context.phase)
                    {
                        case InputActionPhase.Performed:
                            _currentRewards[1].OnAddSkill();
                            break;
                    }

                    break;
            }
        }

        public void ChooseRewardInput_3(InputAction.CallbackContext context)
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameStatus.Reward:
                    switch (context.phase)
                    {
                        case InputActionPhase.Performed:
                            _currentRewards[2].OnAddSkill();
                            break;
                    }

                    break;
            }
        }

        public void OnSkipReward(InputAction.CallbackContext context)
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameStatus.Reward:
                    switch (context.phase)
                    {
                        case InputActionPhase.Performed:
                            GameEventManager.Instance.OnRunContinue.Invoke();
                            break;
                    }

                    break;
            }
        }

        public void ChangeSkillView()
        {
            bool active = SkillView.gameObject.activeSelf;
            SkillView.gameObject.SetActive(!active);
            if (active)
            {
                GameEventManager.Instance.OnGameContinue.Invoke();
            }
            else
            {
                GameEventManager.Instance.OnGamePause.Invoke();
            }
        }

        public void OnRefreshSkills(InputAction.CallbackContext context)
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameStatus.Encounter:
                    switch (context.phase)
                    {
                        case InputActionPhase.Started:
                            RefreshSkills();
                            break;
                    }

                    break;
            }
        }

        private void RefreshSkills()
        {
            if (!_isRefreshOpen || GameManager.Instance.IsPaused)
            {
                return;
            }

            OnRefreshUseEnergy();
            _isRefreshOpen = false;
            foreach (var skill in _currentSkills)
            {
                skill.OnRefresh();
            }

            _currentSkills.Clear();
            StartCoroutine(RefreshRandomSkills(0.2f));
        }

        private void OnRefreshUseEnergy()
        {
            var cost = Mathf.FloorToInt(_hero.CurrentEnergy / 2);
            _hero.Energy.CostEnergy(cost);
        }

        IEnumerator RefreshRandomSkills(float delay)
        {
            yield return new WaitForSeconds(delay);
            List<SkillComponent> skills = new List<SkillComponent>();

            foreach (var skill in _hero.BehaviourController.CurrentSkills)
            {
                skills.Add(skill);
            }

            skills.Shuffle();
            int num = Mathf.Min(3, skills.Count);
            for (int i = 0; i < num; i++)
            {
                skills[i].Initialize();
                skills[i].SetFollow(_loopSockets[i]);
                _currentSkills.Add(skills[i]);
            }

            _isRefreshOpen = true;
        }

        public void CheckSkillRefresh()
        {
            int count = 0;
            foreach (var skill in _currentSkills)
            {
                if (skill.IsActive)
                {
                    count += 1;
                }
            }

            if (count == 0)
            {
                RefreshSkills();
            }
        }
    }
}