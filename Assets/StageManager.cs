﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateType
{
    NotInit,                    // 아직 초기화 되지 않음.
    SelectPlayer,               // 조정할 아군 선택, 선택된 플레이어가 갈 수 있는 영역과 공격 가능한 영역 표시.
    SelectBlockToMoveOrAttackTarget, // 이동혹은 공격 타겟을 선택.
    IngPlayerMove,              // 플레이어 이동중
    SelectToAttackTarget,       // 이동후에 공격할 타겟을 선택.
                                // 공격할 타겟이 없다면 SelectPlayer로 변경
    //AttackToTarget, // 의미 없어서 삭제
    // 모든 플레이어 선택했다면 MonsterTurn을 진행 시킨다. -> 파랜드 택틱스는 자동으로 몬스터 턴으로 넘어가지 않고 턴 넘기기를 해줘야한다.
    MonsterTurn,
}

public class StageManager : SingletonMonoBehavior<StageManager>
{
    [SerializeField] GameStateType gameState;
    static public GameStateType GameState
    {
        get => Instance.gameState;
        set
        {
            Debug.Log($"{Instance.gameState} => {value}");
            NotifyUI.Instance.Show($"{value}");
            Instance.gameState = value;
        }
    }

    public int turn = 0;
    private void Start()
    {
        GameState = GameStateType.SelectPlayer;

        ShowNextTurn();
    }
    public void ProcessEndOfPlayerTurn()
    {
        CenterNotifyUI.Instance.Show($"플레이어의 {turn}턴을 종료합니다.", 1.5f);

        GameState = GameStateType.MonsterTurn;
        StartCoroutine(ProcessMonsterTurnCo());
    }

    private IEnumerator ProcessMonsterTurnCo()
    {
        /// 씬에 있는 모든 움직일 수 있는 몬스터를 대상으로 한다.
        /// 1. 즉시 공격 가능한 대상있는지 확인
        ///  1-1. 있으면 즉시 공격
        ///  1-2. 없으면가장 가까운 공격대상으로 이동.
        ///     공격 가능하면 공격
        foreach(var m in Monster.Monsters)
        {
            var attackTargets = m.SetAttackableEnemyPoint();
            if(attackTargets.Count > 0) // 공격할 대상이 있다.
            {
                //첫번째 공격 대상을 선택해서 공격하자.
                var target = attackTargets[0];
                yield return StartCoroutine(m.AttackTarget(target));
            }
            else
            {
                //없다. 가장 가까운 공격대상으로 이동하자.
                var target = m.FindNearestAttackTarget();
                yield return StartCoroutine(m.MoveToTarget(target));
            }
        }

        ProcessNextTurn();
    }

    public void ProcessNextTurn()
    {
        turn++;

        ClearActorFlag();

        ShowNextTurn();

        GameState = GameStateType.SelectPlayer;
    }

    private void ClearActorFlag()
    {
        Actor.Actors.ForEach(x => { x.completeAct = false; x.completeMove = false; });
    }

    public void ShowNextTurn()
    {
        CenterNotifyUI.Instance.Show($"{turn}턴 시작", 1.5f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ContextMenuUI.Instance.ShowStageMenu();

        if ( Input.GetKeyDown(KeyCode.Alpha1))
            ProcessEndOfPlayerTurn();

        if( Input.GetKeyDown(KeyCode.Alpha2))
            ProcessNextTurn();
    }

}
