using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : Actor
{
    public static List<Player> Players = new List<Player>();
    public int ID;
    public int exp
    {
        set { data.exp = value; }
        get { return data.exp; }
    }

    public int level
    {
        set { data.level = value; }
        get { return data.level; }
    }

    private void InitLevelData()
    {
        //exp = new SaveInt("exp" + ID);
        //level = new SaveInt("level" + ID, 1);
        var log = PlayerPrefs.GetString(PlayerDataKey);
        print(log);
        data = JsonUtility.FromJson<PlayerData>(log);

        SetLevelData();
    }

    new protected void Awake()
    {
        base.Awake();
        Players.Add(this);

        InitLevelData();
    }
    private void SetLevelData()
    {
        if (GlobalData.Instance.playerDataMap.ContainsKey(level)== false)
            Debug.LogError($"{level}레벨 정보가 없습니다");

        var data = GlobalData.Instance.playerDataMap[level];
        maxExp = data.maxExp;
        hp = maxHp = data.maxHp;
        mp = maxMp = data.maxMp;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddExp(5);
    }
    new protected void OnDestroy()
    {
        base.OnDestroy();
        Players.Remove(this);

        SaveData();
    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(data);

        try
        {
            PlayerPrefs.SetString(PlayerDataKey, json);
            Debug.Log("json:" + json);
        }
        catch (System.Exception err)
        {
            Debug.Log("Got: " + err);
        }
    }
    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Player; }

    static public Player SelectedPlayer;

    void Start()
    {
        //SelectedPlayer = this;
        //animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);

    }

    internal void MoveToPosition(Vector3 position)
    {
        Vector2Int findPos = position.ToVector2Int();//
        FindPath(findPos);
    }
    //public float moveDistanceMultiply = 1.2
    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }

    internal bool CanAttackTarget(Actor enemy)
    {
        //같은팀을 공격대상으로 하지 않기
        if (enemy.ActorType != ActorTypeEnum.Monster)
            return false;

        // 공격 가능한 범위 안에 있는지 확인.
        if (IsInAttackableArea(enemy.transform.position) == false)
            return false;

        if (completeAct)
            return false;

        return true;
    }

    internal void AttackToTarget(Monster actor)
    {
        ClearEnemyExistPoint();
        StartCoroutine(AttackToTargetCo_(actor));
    }

    private IEnumerator AttackToTargetCo_(Monster monster)
    {
        yield return AttackToTargetCo(monster);
        if (monster.status == StatusType.Die)
        {
            AddExp(monster.rewardExp);

            if (monster.dropItemGroup.ratio > Random.Range(0, 1f))
                DropItem(monster.dropItemGroup.dropItemID, monster.transform.position);
        }
        StageManager.GameState = GameStateType.SelectPlayer;
    }

    [ContextMenu("DropTestTemp")]
    void DropTestTemp()
    {
        DropItem(1);
    }
    private void DropItem(int dropGroupID, Vector3? position = null)
    {
        var dropGroup = GlobalData.Instance.dropItemGroupDataMap[dropGroupID];
        
        var dropItemRaioInfo = dropGroup.dropItmes
            .OrderByDescending(x => x.ratio * Random.Range(0, 1f)).First();
        print(dropItemRaioInfo.ToString());

        var dropItem = GlobalData.Instance.itemDataMap[dropItemRaioInfo.dropItemID];
        print(dropItem.ToString());
        GroundManager.Instance.AddBlockInfo(position.Value, BlockType.Item, dropItem);
    }

    public int maxExp;
    private void AddExp(int rewardExp)
    {
        // 경험치 추가.
        exp += rewardExp;

        // 경험치가 최대 경험치 보다 클경우 레벨 증가.
        if(exp >= maxExp)
        {
            exp = exp - maxExp;

            //레벨업, 
            level++;
            SetLevelData();

            CenterNotifyUI.Instance
                .Show($"Lv.{level}으로 증가했습니다");
        }
    }

    internal bool OnMoveable(Vector3 position, int maxDistance)
    {
        Vector2Int goalPos = position.ToVector2Int();
        Vector2Int playerPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;
        var path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);

        if (path.Count == 0 || path.Count > maxDistance + 1)
            return false;

        //if (path.Count == 0)
        //    Debug.Log("길 업따 !");
        //else if (path.Count > maxDistance + 1)
        //    Debug.Log("이동모태 !");
        //else
        //    return true;

        return true;
    }

    public void ClearEnemyExistPoint()
    {
        enemyExistPoint.ForEach( x => x.ToChangeOriginalColor());
        enemyExistPoint.Clear();
    }
    protected override void OnCompleteMove()
    {
        bool existAttackTarget = ShowAttackableArea();
        if (existAttackTarget)
            StageManager.GameState = GameStateType.SelectToAttackTarget;
        else
            StageManager.GameState = GameStateType.SelectPlayer;

        // 도착한 지역에 아이템이 있다면 획득하자.
        var intPos = transform.position.ToVector2Int();
        // 어떤 아이템이 있는가?
        int itemID = GroundManager.Instance.blockInfoMap[intPos].dropItemID;
        if (itemID > 0)
        {
            // 아이템 획득하기.
            AddItem(itemID);

            // 땅에서는 아이템 삭제하기.
            GroundManager.Instance.RemoveItem(transform.position);
        }
    }
    [System.Serializable]
    public class PlayerData
    {
        public List<int> haveItem = new List<int>();
        public int exp;
        public int level;
    }
    public PlayerData data;
    string PlayerDataKey => "PlayerData" + ID;
    private void AddItem(int itemID)
    {
        data.haveItem.Add(itemID);
    }

    public List<BlockInfo> enemyExistPoint = new List<BlockInfo>();
    internal bool ShowAttackableArea()
    {
        //현재 위치에서 공격 가능한 지역을 체크하자.
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        //공격가능한 지역에 적이 있는지 확인하자.
        foreach (var item in attackableLocalPositions)
        {
            Vector2Int pos = item + currentPos; //item의 월드 지역 위치;

            if (map.ContainsKey(pos))
            {
                if (IsEnemyExist(map[pos])) //map[pos]에 적이 있는가? -> 적인지 판단은 actorType으로 하자.
                {
                    enemyExistPoint.Add(map[pos]);
                }
            }
        }

        enemyExistPoint.ForEach(x => x.ToChangeColor(Color.red));

        return enemyExistPoint.Count > 0;
    }

    private bool IsEnemyExist(BlockInfo blockInfo)
    {
        //if (blockInfo.actor == null)
        //    return false;

        if (blockInfo.blockType.HasFlag(BlockType.Monster) == false)
            return false;

        Debug.Assert(blockInfo.actor != null, "액터는 꼭 있어야 해!");

        return true;
    }

    public override BlockType GetBlockType()
    {
        return BlockType.Player;
    }
    protected override void OnDie()
    {
        // 모든 플레이어가 죽었는지 파악해서 다 죽었다면 GameOver표시.
        if (Players.Where(x => x.status != StatusType.Die).Count() == 0)
        {
            CenterNotifyUI.Instance.Show(@"유다이
Game Over");
        }
    }
}
