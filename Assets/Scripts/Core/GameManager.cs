using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Tools.IncrementScoreCharacters;
using Lean.Pool;
using LootLocker;
using LootLocker.Requests;
using MoreMountains.Tools;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class PlayerScoreData
    {
        public int Score;
        public int Rank;
        public string PlayerName;
        public string PlayerIDSubmit;
        public string PlayerID;
    }

    public enum GameStatus
    {
        Idle,
        Run,
        Encounter,
        Reward,
        Splash,
        GameOver,
        GameWining
    }

    public enum GameTransition
    {
        StartRun,
        Encounter,
        ContinueRun,
        Reward,
        StartGame,
        OnGameOver,
        Restart,
        WinGame
    }

    public class GameManager : MMPersistentSingleton<GameManager>
    {
        [SerializeField] public int MaxEncounters;
        [SerializeField] public float InitTime;
        [SerializeField] public BuffShowData BuffShowData;
        [SerializeField] public ShowTipComponent ShowTipComponent;
        [SerializeField] private float MinLoadDuration;
        [SerializeField] private int ScreenWidth;
        [SerializeField] private int ScreenHeight;
        public int InitSpeed = 0;
        public int MaxNameLength = 16;
        public int LeaderBoardKey;
        private float _runClock;
        private bool _isPaused;
        private float _loadTimer;
        public int CountDown;
        public bool LoggedIn;
        public List<PlayerScoreData> PlayerScores;
        public PlayerScoreData CurrentScore;
        private StateMachine<GameManager, GameStatus, GameTransition> _stateMachine;
        public GameStatus CurrentState => _stateMachine.CurrentStateID;
        public string RunClock => GetStringScore(GetScore());
        public bool IsPaused => _isPaused;
        public float ProgressValue { get; set; }
        private int PlayerScore { get; set; }
        public bool IsSuccessRegistered { get; set; }


        protected override void Awake()
        {
            base.Awake();
            LoggedIn = false;
            IsSuccessRegistered = false;
            CurrentScore = new PlayerScoreData();
            PlayerScores = new List<PlayerScoreData>();
            _stateMachine = new StateMachine<GameManager, GameStatus, GameTransition>(this);
            var splashState = new Splash(GameStatus.Splash);
            var idleState = new Idle(GameStatus.Idle);
            var runState = new Run(GameStatus.Run);
            var encounter = new Encounter(GameStatus.Encounter);
            var reward = new Reward(GameStatus.Reward);
            var gameOverState = new GameOverState(GameStatus.GameOver);
            var gameWining = new GameWiningState(GameStatus.GameWining);

            gameWining.AddTransition(GameTransition.Restart, GameStatus.Idle);
            splashState.AddTransition(GameTransition.StartGame, GameStatus.Idle);
            idleState.AddTransition(GameTransition.StartRun, GameStatus.Run);
            runState.AddTransition(GameTransition.Encounter, GameStatus.Encounter);
            runState.AddTransition(GameTransition.Reward, GameStatus.Reward);
            runState.AddTransition(GameTransition.WinGame, GameStatus.GameWining);
            runState.AddTransition(GameTransition.OnGameOver, GameStatus.GameOver);
            encounter.AddTransition(GameTransition.ContinueRun, GameStatus.Run);
            encounter.AddTransition(GameTransition.Reward, GameStatus.Reward);
            encounter.AddTransition(GameTransition.OnGameOver, GameStatus.GameOver);
            reward.AddTransition(GameTransition.ContinueRun, GameStatus.Run);
            gameOverState.AddTransition(GameTransition.Restart, GameStatus.Idle);
            _stateMachine.AddState(splashState);
            _stateMachine.AddState(idleState);
            _stateMachine.AddState(runState);
            _stateMachine.AddState(encounter);
            _stateMachine.AddState(reward);
            _stateMachine.AddState(gameOverState);
            _stateMachine.AddState(gameWining);
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.SetResolution(ScreenWidth, ScreenHeight, true);
            _stateMachine.SetCurrent(GameStatus.Splash);
        }

        private int GetScore()
        {
            return (int)(_runClock * 100);
        }

        public string GetStringScore(int score)
        {
            return (score / 100).ToString() + '.' + (score % 100).ToString("00");
        }

        public IEnumerator LoginRoutine()
        {
            bool done = false;
            LootLockerSDKManager.StartGuestSession((response) =>
            {
                if (response.success)
                {
                    Debug.Log("Successfully started LootLocker session");
                    LoggedIn = true;
                    done = true;
                    // Save the player ID for use in the leaderboard
                    PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                    Debug.Log("UserLoggedIn:" + PlayerPrefs.GetString("PlayerID"));
                }
                else
                {
                    Debug.Log("Error starting LootLocker session");
                    done = true;
                }
            });
            yield return new WaitWhile(() => done == false);
        }

        private void Update()
        {
            if (_isPaused)
            {
                return;
            }

            _stateMachine.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_isPaused)
            {
                return;
            }

            switch (CurrentState)
            {
                case GameStatus.Run:
                case GameStatus.Encounter:
                    _runClock -= Time.fixedDeltaTime;
                    if (_runClock <= 0)
                    {
                        _runClock = 0;
                        GameEventManager.Instance.OnGameOver?.Invoke();
                    }

                    break;
            }
        }

        /// <summary>
        /// OnDisable, we start listening to events.
        /// </summary>
        protected virtual void OnEnable()
        {
            // GameEventManager.Instance.OnGameStart += OnGameStart;
            GameEventManager.Instance.OnGamePause += OnGamePause;
            GameEventManager.Instance.OnGameContinue += OnGameContinue;
            // GameEventManager.Instance.OnRunStart += OnRunStart;
            GameEventManager.Instance.OnRunEncounter += OnRunEncounter;
            GameEventManager.Instance.OnRunReward += OnRunReward;
            GameEventManager.Instance.OnRunContinue += OnRunContinue;
            GameEventManager.Instance.OnGameOver += OnGameOver;
            GameEventManager.Instance.OnGameWinning += OnGameWinning;
        }

        /// <summary>
        /// OnDisable, we stop listening to events.
        /// </summary>
        protected virtual void OnDisable()
        {
            // GameEventManager.Instance.OnGameStart -= OnGameStart;
            GameEventManager.Instance.OnGamePause -= OnGamePause;
            GameEventManager.Instance.OnGameContinue -= OnGameContinue;
            // GameEventManager.Instance.OnRunStart -= OnRunStart;
            GameEventManager.Instance.OnRunEncounter -= OnRunEncounter;
            GameEventManager.Instance.OnRunReward -= OnRunReward;
            GameEventManager.Instance.OnRunContinue -= OnRunContinue;
            GameEventManager.Instance.OnGameOver -= OnGameOver;
            GameEventManager.Instance.OnGameWinning -= OnGameWinning;
        }

        private void OnGameWinning()
        {
            _stateMachine.PerformTransition(GameTransition.WinGame);
        }

        private void OnGameOver()
        {
            _stateMachine.PerformTransition(GameTransition.OnGameOver);
        }

        private void OnRunContinue()
        {
            _stateMachine.PerformTransition(GameTransition.ContinueRun);
        }

        private void OnRunReward()
        {
            _runClock += 10;
            _stateMachine.PerformTransition(GameTransition.Reward);
        }

        private void OnRunEncounter(Character target)
        {
            _stateMachine.PerformTransition(GameTransition.Encounter);
        }

        // private void OnGameStart()
        // {
        // }

        private void OnGamePause()
        {
            _isPaused = true;
        }

        private void OnGameContinue()
        {
            _isPaused = false;
        }

        private class Splash : FsmState<GameManager, GameStatus, GameTransition>
        {
            public Splash(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override void Reason(float deltaTime = 0)
            {
            }

            public override void Act(float deltaTime = 0)
            {
            }
        }

        private class Idle : FsmState<GameManager, GameStatus, GameTransition>
        {
            private float _timer = 0;
            private const int Duration = 3;
            private int _countDown;
            private int _tempCount;

            public Idle(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
                _timer = Duration;
                _tempCount = 0;
                _countDown = Duration;
                Context._runClock = Context.InitTime;
                Context.BuffShowData.Initialize();
                BattleManager.Instance.OnGameStart();
                // GameEventManager.Instance.OnGameStart.Invoke();
            }

            public override void Exit()
            {
            }

            public override void Reason(float deltaTime = 0)
            {
                if (_timer < 0)
                {
                    Context._stateMachine.PerformTransition(GameTransition.StartRun);
                }
            }

            public override void Act(float deltaTime = 0)
            {
                _timer -= deltaTime;
                _countDown = 1 + (int)_timer;
                if (_countDown != _tempCount)
                {
                    _tempCount = _countDown;
                    GameEventManager.Instance.OnStartCountDown.Invoke(_countDown);
                }
            }
        }

        private class Run : FsmState<GameManager, GameStatus, GameTransition>
        {
            public Run(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
                BattleManager.Instance.Hero.TriggerWalk();
            }

            public override void Exit()
            {
                BattleManager.Instance.Hero.TriggerIdle();
            }

            public override void Reason(float deltaTime = 0)
            {
            }

            public override void Act(float deltaTime = 0)
            {
            }
        }

        private class Encounter : FsmState<GameManager, GameStatus, GameTransition>
        {
            public Encounter(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override void Reason(float deltaTime = 0)
            {
            }

            public override void Act(float deltaTime = 0)
            {
            }
        }

        private class Reward : FsmState<GameManager, GameStatus, GameTransition>
        {
            public Reward(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
                Context._isPaused = true;
            }

            public override void Exit()
            {
                Context._isPaused = false;
            }

            public override void Reason(float deltaTime = 0)
            {
            }

            public override void Act(float deltaTime = 0)
            {
            }
        }

        private class GameOverState : FsmState<GameManager, GameStatus, GameTransition>
        {
            public GameOverState(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
                Context._isPaused = true;
            }

            public override void Exit()
            {
                Context._isPaused = false;
            }

            public override void Reason(float deltaTime = 0)
            {
            }

            public override void Act(float deltaTime = 0)
            {
            }
        }

        private class GameWiningState : FsmState<GameManager, GameStatus, GameTransition>
        {
            public GameWiningState(GameStatus stateId) : base(stateId)
            {
            }

            public override void Enter()
            {
                Context._isPaused = true;
                Context.LoggedIn = false;
                Context.PlayerScore = Context.GetScore();
                Context.OnSubmitScore();
            }

            public override void Exit()
            {
                Context._isPaused = false;
            }

            public override void Reason(float deltaTime = 0)
            {
            }

            public override void Act(float deltaTime = 0)
            {
            }
        }

        public void StartGame()
        {
            StartCoroutine(LoadLeaver());
        }

        IEnumerator LoadLeaver()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync("Scenes/Main");
            operation.allowSceneActivation = false;
            while (!operation.isDone) //当场景没有加载完毕
            {
                _loadTimer += Time.fixedDeltaTime;
                if (_loadTimer > MinLoadDuration
                    && Mathf.Approximately(operation.progress, 0.9f))
                {
                    operation.allowSceneActivation = true;
                }

                ProgressValue = Mathf.Min(operation.progress, _loadTimer / MinLoadDuration);
                yield return new WaitForFixedUpdate();
            }

            yield return StartCoroutine(OnSceneLoaded());
        }

        IEnumerator OnSceneLoaded()
        {
            yield return new WaitForSeconds(0.2f);
            _stateMachine.PerformTransition(GameTransition.StartGame);
        }

        public void Restart()
        {
            GameEventManager.Instance.OnGameRestart.Invoke();
            _stateMachine.PerformTransition(GameTransition.Restart);
        }

        private void OnSubmitScore()
        {
            StartCoroutine(SetupRoutine());
        }

        IEnumerator SetupRoutine()
        {
            // Set the info text to loading
            // InfoText.text = "Logging in...";
            // Wait while the login is happening
            yield return LoginRoutine();
            // If the player couldn't log in, let them know, and then retry
            if (!LoggedIn)
            {
                float loginCountdown = 4;
                float timer = loginCountdown;
                while (timer >= -1f)
                {
                    timer -= Time.deltaTime;
                    // Update the text when we get to a new number
                    if (Mathf.CeilToInt(timer) != Mathf.CeilToInt(loginCountdown))
                    {
                        var info = "Failed to login retrying in " + Mathf.CeilToInt(timer).ToString();
                        Debug.Log(info);
                        loginCountdown -= 1f;
                    }

                    yield return null;
                }

                StartCoroutine(SetupRoutine());
                yield break;
            }

            yield return new WaitWhile(() => !LoggedIn);
            yield return SubmitScore();
            yield return FetchTopLeaderboardScores();
            yield return FetchHighScoresCentered();
        }

        IEnumerator SubmitScore()
        {
            // Get the players saved ID, and add the incremental characters
            string playerID = PlayerPrefs.GetString("PlayerID") + "_" + IncrementCharacters.GetStr();
            string metadata = PlayerPrefs.GetString("PlayerName");

            Debug.Log("Submit Score: PlayerID " + playerID);
            LootLockerSDKManager.SubmitScore(playerID, PlayerScore, LeaderBoardKey, metadata, (response) =>
            {
                if (response.statusCode == 200)
                {
                    Debug.Log("Successful Submit Score " + PlayerScore);
                    // Only let the player upload score once until we reset it
                    CurrentScore.Score = PlayerScore;
                    CurrentScore.PlayerIDSubmit = playerID;
                    CurrentScore.PlayerName = metadata;
                    CurrentScore.PlayerID = playerID.Split('_')[0];
                }
                else
                {
                    Debug.Log("Failed Submit Score: " + response.Error);
                }
            });
            yield break;
        }

        public IEnumerator FetchTopLeaderboardScores()
        {
            PlayerScores.Clear();
            // Let the player know that the scores are loading
            // How many scores?
            int count = 10;
            int after = 0;

            bool done = false;
            LootLockerSDKManager.GetScoreListMain(LeaderBoardKey, count, after, (response) =>
            {
                if (response.statusCode == 200)
                {
                    Debug.Log("Fetch Successful");
                    LootLockerLeaderboardMember[] members = response.items;
                    for (int i = 0; i < members.Length; i++)
                    {
                        PlayerScores.Add(new PlayerScoreData()
                        {
                            Score = members[i].score,
                            Rank = members[i].rank,
                            PlayerName = members[i].metadata,
                            PlayerIDSubmit = members[i].member_id,
                            PlayerID = members[i].member_id.Split('_')[0]
                        });
                        // Show the ranking, players name and score, and create a new line for the next entry
                    }

                    done = true;
                }
                else
                {
                    Debug.Log("failed: " + response.Error);
                    // Give the user information that the leaderboard couldn't be retrieved
                    done = true;
                }
            });
            // Wait until the process has finished
            yield return new WaitWhile(() => !done);
            // Update the TextMeshPro components
        }

        IEnumerator FetchHighScoresCentered()
        {
            bool done = false;
            // Get the player ID from Player prefs with the incremental score string attached
            string latestPlayerID = CurrentScore.PlayerIDSubmit;
            string[] memberIDs = new string[] { latestPlayerID };

            // Get the score that matches this ID
            LootLockerSDKManager.GetByListOfMembers(memberIDs, LeaderBoardKey, (response) =>
            {
                if (response.statusCode == 200)
                {
                    Debug.Log("Get Member Score Successful");
                    // We're only asking for one player, so we just need to check the first entry
                    CurrentScore.Rank = response.members[0].rank;
                    done = true;
                }
                else
                {
                    Debug.Log("failed: " + response.Error);
                    done = true;
                }
            });

            // Wait until request is done
            yield return new WaitWhile(() => !done);
            // Update the TextMeshPro components
            GameEventManager.Instance.OnFetchScores?.Invoke();
        }
    }
}