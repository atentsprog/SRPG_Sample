using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateType
{
    NotInit,                    // ���� �ʱ�ȭ ���� ����.
    SelectPlayer,               // ������ �Ʊ� ����, ���õ� �÷��̾ �� �� �ִ� ������ ���� ������ ���� ǥ��.
    SelectBlockToMoveOrAttackTarget, // �̵�Ȥ�� ���� Ÿ���� ����.
    IngPlayerMove,              // �÷��̾� �̵���
    SelectToAttackTarget,       // �̵��Ŀ� ������ Ÿ���� ����. ������ Ÿ���� ���ٸ� SelectPlayer�� ����
    AttackToTarget,
    // ��� �÷��̾� �����ߴٸ� MonsterTurn�� ���� ��Ų��.
    MonsterTurn,
}

public class StageManager : SingletonMonoBehavior<StageManager>
{
    [SerializeField] GameStateType gameState;
    static public GameStateType GameState
    {
        get => Instance.gameState;
        set => Instance.gameState = value;
    }
    private void Start()
    {
        GameState = GameStateType.SelectPlayer;
    }
}
