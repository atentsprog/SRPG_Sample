﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateType
{
    NotInit,                    // 아직 초기화 되지 않음.
    SelectPlayer,               // 조정할 아군 선택, 선택된 플레이어가 갈 수 있는 영역과 공격 가능한 영역 표시.
    SelectedPlayerMoveOrAct,     // 선택된 플레이어가 이동하거나 행동을 할 차례
    IngPlayerMove,              // 플레이어 이동중
    SelectToAttackTarget,       // 이동후에 공격할 타겟을 선택. 공격할 타겟이 없다면 SelectPlayer로 변경
    //AttackToTarget, <- 사용안학 있음.
    // 모든 플레이어 선택했다면 MonsterTurn을 진행 시킨다.
    MonsterTurn,
}

public class StageManager : SingletonMonoBehavior<StageManager>
{
    [SerializeField] GameStateType m_gameState;
    static public GameStateType GameState
    {
        get => Instance.m_gameState;
        set {
            Debug.Log($"{Instance.m_gameState} => {value}");

            NotifyUI.Instance.Show(value.ToString(), 10);
            Instance.m_gameState = value;
        }
    }
    private void Start()
    {
        OnStartTurn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (GameState == GameStateType.SelectedPlayerMoveOrAct)
            {
                Player.SelectedPlayer = null;
                BlockInfo.ClearMoveableArea();
                GameState = GameStateType.SelectPlayer;
            }
            else
            {
                ContextMenuUI.Instance.Show(Input.mousePosition);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QueryUI.Instance.Show("버튼 1개 내용", (string result) =>
            {
                print(result + "를 눌렀다");
            }, "확인");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QueryUI.Instance.Show("버튼 2개 내용", (string result) =>
            {
                print(result + "를 눌렀다");
            }, "확인", "취소");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QueryUI.Instance.Show("버튼 3개 내용", (string result) =>
            {
                print(result + "를 눌렀다");
            }, "확인", "취소", "3번째 버튼");
        }
    }

    internal void EndTurnPlayer()
    {
        GameState = GameStateType.MonsterTurn;

        StartCoroutine(MonserTurnCo());
    }

    private IEnumerator MonserTurnCo()
    {
        foreach(var monster in Monster.Monsters)
        {
            FollowTarget.Instance.SetTarget(monster.transform);
            yield return monster.AutoAttackCo();
        }

        ProcessNextTurn();
    }
    int turn = 1;
    private void ProcessNextTurn()
    { 
        //// 턴 정보 초기화 해주자.
        ClearTrunInfo();
      
        turn++;

        OnStartTurn();
    }

    private void ClearTrunInfo()
    {
        Player.Players.ForEach(x =>
        {
            x.completeMove = false;
            x.completeAct = false;
        });

        Monster.Monsters.ForEach(x =>
        {
            x.completeMove = false;
            x.completeAct = false;
        });
    }

    private void OnStartTurn()
    {
        FollowTarget.Instance.SetTarget(Player.Players[0].transform);

        // 몇번째 턴인지 보여주자.
        ShowCurrentTurn();

        // 게임상태를 SelectPlayer
        GameState = GameStateType.SelectPlayer;
    }

    private void ShowCurrentTurn()
    {
        CenterNotifyUI.Instance.Show($"{turn}턴이 시작되었습니다");
    }
}
