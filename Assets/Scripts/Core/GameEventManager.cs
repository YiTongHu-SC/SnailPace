using DefaultNamespace;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace Core
{
    public class GameEventManager : MMPersistentSingleton<GameEventManager>
    {
        // public UnityAction OnGameStart;
        public UnityAction OnGamePause;
        public UnityAction OnGameContinue;
        public UnityAction<Character> OnRunEncounter;
        public UnityAction OnRunReward;
        public UnityAction OnRunContinue;
        public UnityAction OnEnemyDead;
        public UnityAction OnGameOver;
        public UnityAction OnGameRestart;
        public UnityAction<SkillReward> OnAddSkill;
        public UnityAction OnGameWinning;
        public UnityAction OnFetchScores;
        public UnityAction<int> OnStartCountDown;
    }
}