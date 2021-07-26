using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameStateType
{
    NotInit,                    // 아직 초기화 되지 않음.
    SelectPlayer,               // 조정할 아군 선택, 선택된 플레이어가 갈 수 있는 영역과 공격 가능한 영역 표시.
    SelectedPlayerMoveOrAct,     // 선택된 플레이어가 이동하거나 행동을 할 차례
    IngPlayerMove,              // 플레이어 이동중
    SelectToAttackTarget,       // 이동후에 공격할 타겟을 선택. 공격할 타겟이 없다면 SelectPlayer로 변경
    // 모든 플레이어 선택했다면 MonsterTurn을 진행 시킨다.
    MonsterTurn,

    GameOver,   // 모든 플레이어 사망
}

public class StageManager : SingletonMonoBehavior<StageManager>
{
    [SerializeField] GameStateType gameState;
    static public GameStateType GameState
    {
        get => Instance.gameState;
        set {
            Debug.Log($"{Instance.gameState} => {value}");

            NotifyUI.Instance.Show(value.ToString(), 10);
            Instance.gameState = value;
        }
    }
    private void Start()
    {
        GameState = GameStateType.SelectPlayer;
        CenterNotifyUI.Instance.Show("게임이 시작되었습니다.", 1.5f);

        ShowNextTurn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ContextMenuUI.Instance.Show(Input.mousePosition);
    }

    public void StartMonsterTurn()
    {
        StartCoroutine(MonsterTurnCo());
    }

    private IEnumerator MonsterTurnCo()
    {
        foreach (var item in Monster.Monters)
        {
            if (item.status == StatusType.Die)
                continue;

            yield return item.AutoPlay();

            if (Player.Players.Where(x => x.status != StatusType.Die).Count() == 0)
            {
                //플레이어가 모두 죽었다
                CenterNotifyUI.Instance.Show("유다이");
                GameState = GameStateType.GameOver;
                yield break;
            }
        }

        ProcessNextTurn();
    }

    int turn = 1;


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
}
