using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateType
{
    NotInit,                    // 아직 초기화 되지 않음.
    SelectPlayer,               // 조정할 아군 선택, 선택된 플레이어가 갈 수 있는 영역과 공격 가능한 영역 표시.
    SelectBlockToMoveOrAttackTarget, // 이동혹은 공격 타겟을 선택.
    IngPlayerMove,              // 플레이어 이동중
    SelectToAttackTarget,       // 이동후에 공격할 타겟을 선택. 공격할 타겟이 없다면 SelectPlayer로 변경
    AttackToTarget,
    // 모든 플레이어 선택했다면 MonsterTurn을 진행 시킨다.
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
            NotifyUI.Instance.ShowText($"{value}");
            Instance.gameState = value;
        }
    }
    private void Start()
    {
        GameState = GameStateType.SelectPlayer;
    }
}
