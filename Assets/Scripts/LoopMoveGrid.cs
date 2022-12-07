using System;
using System.Collections.Generic;
using Core;
using DefaultNamespace;
using Lean.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

class MoveSocketStruct
{
    public LoopBlock Block;
    public MoveSocketStruct Next;
    public MoveSocketStruct Prev;
}

public class LoopMoveGrid : MonoBehaviour
{
    public float MoveWidth = 6;
    public List<LoopBlock> MoveSockets;
    private List<Vector3> _initPositions;
    private List<MoveSocketStruct> MoveSocketStructs;
    private float _endPos;
    private float _passedBlocks;
    private bool _isCompleteGame;
    private int _currentLevel;
    private bool _doUpdateLoop;
    private GameObject _lastEnemy;
    private MoveSocketStruct _lastSocket;

    private void Awake()
    {
        _initPositions = new List<Vector3>();
    }


    void Start()
    {
        MoveSocketStructs = new List<MoveSocketStruct>();
        foreach (var socket in MoveSockets)
        {
            _initPositions.Add(socket.transform.position);
            MoveSocketStructs.Add(new MoveSocketStruct()
            {
                Block = socket
            });
        }

        int num = MoveSocketStructs.Count;
        for (int i = 0; i < num; i++)
        {
            MoveSocketStructs[i].Next = MoveSocketStructs[(i + num + 1) % num];
            MoveSocketStructs[i].Prev = MoveSocketStructs[(i + num - 1) % num];
        }

        // _endPos = MoveSockets[0].transform.localPosition.x - 0.5f * MoveWidth;
        _lastSocket = MoveSocketStructs[0];
    }

    public void OnReset()
    {
        _doUpdateLoop = false;
        _lastEnemy = null;
        _isCompleteGame = false;
        _passedBlocks = 0;
        for (int i = 0; i < MoveSockets.Count; i++)
        {
            MoveSockets[i].transform.position = _initPositions[i];
        }

        InitStartEncounters();
    }

    private void InitStartEncounters()
    {
        _currentLevel = 0;
        for (int i = 3; i < MoveSocketStructs.Count; i++)
        {
            var encounter = LeanPool.Spawn(GetRandomEnemy(), MoveSocketStructs[i].Block.IncidentSocket);
            encounter.transform.localPosition = Vector3.zero;
            encounter.GetComponent<Character>().SetLevel(_currentLevel);
            _currentLevel += 1;
        }
    }

    public void FixedTick(float deltaTime)
    {
        float deltaDistance = BattleManager.Instance.Hero.SpeedComponent.MoveSpeed * deltaTime;
        int num = MoveSocketStructs.Count;
        var pos = _lastSocket.Block.transform.localPosition;
        pos.x -= deltaDistance;
        _lastSocket.Block.transform.localPosition = pos;
        var current = _lastSocket;
        for (int i = 0; i < num - 1; i++)
        {
            current = current.Next;
            pos.x += MoveWidth;
            current.Block.transform.localPosition = pos;
        }

        if (_doUpdateLoop)
        {
            _doUpdateLoop = false;
            _passedBlocks += 1;
            var movePos = _lastSocket.Prev.Block.transform.localPosition;
            movePos.x += MoveWidth;
            _lastSocket.Block.transform.localPosition = movePos;
            if (_passedBlocks + 3 < GameManager.Instance.MaxEncounters)
            {
                var encounter = LeanPool.Spawn(GetRandomEnemy(), _lastSocket.Block.IncidentSocket);
                encounter.transform.localPosition = Vector3.zero;
                encounter.GetComponent<Character>().SetLevel(_currentLevel);
                _currentLevel += 1;
            }
            else
            {
                if (!_isCompleteGame)
                {
                    _isCompleteGame = true;
                    var winning = LeanPool.Spawn(BattleManager.Instance.WinningPrefab, _lastSocket.Block.IncidentSocket);
                    winning.transform.localPosition = Vector3.zero;
                }
            }

            _lastSocket = _lastSocket.Next;
        }
    }

    private GameObject GetRandomEnemy()
    {
        int num = BattleManager.Instance.EncounterEnemyData.Enemys.Length;
        Debug.Assert(num > 1, "enemy num should not less than 2");
        bool ok = false;
        GameObject enemy = null;
        while (!ok)
        {
            enemy = BattleManager.Instance.EncounterEnemyData.Enemys[Random.Range(0, num)];
            ok = _lastEnemy == null || _lastEnemy != enemy;
            if (ok)
            {
                _lastEnemy = enemy;
            }
        }

        return _lastEnemy;
    }

    private void OnEnable()
    {
        GameEventManager.Instance.OnRunEncounter += OnEncounter;
    }

    private void OnEncounter(Character arg0)
    {
        _doUpdateLoop = true;
    }

    private void OnDisable()
    {
        GameEventManager.Instance.OnRunEncounter -= OnEncounter;
    }
}