using System;
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
    private void ProcessEndTurnPlayer()
    {
        CenterNotifyUI.Instance.Show($"플레이어의 {turn}턴을 종료합니다.", 1.5f);

        GameState = GameStateType.MonsterTurn;
    }

    public void ProcessNextTurn()
    {
        turn++;

        ClearActorFlag();

        ShowNextTurn();
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
        if( Input.GetKeyDown(KeyCode.Alpha1))
            ProcessEndTurnPlayer();

        if( Input.GetKeyDown(KeyCode.Alpha2))
            ProcessNextTurn();
    }

}
