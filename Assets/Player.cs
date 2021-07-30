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
    public SaveInt exp, level;

    new protected void Awake()
    {
        base.Awake();
        Players.Add(this);

        InitLevelData();
    }

    private void InitLevelData()
    {
        exp = new SaveInt("exp" + ID);
        level = new SaveInt("level" + ID, 1);
        SetLevelData();
    }

    private void SetLevelData()
    {
        if (GlobalData.Instance.playerDataMap.ContainsKey(level.Value)== false)
            Debug.LogError($"{level}레벨 정보가 없습니다");

        var data = GlobalData.Instance.playerDataMap[level.Value];
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
        exp.Value += rewardExp;

        // 경험치가 최대 경험치 보다 클경우 레벨 증가.
        if(exp.Value >= maxExp)
        {
            exp.Value = exp.Value - maxExp;

            //레벨업, 
            level.Value++;
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

    [System.Serializable]
    public class InventoryItemInfo
    {
        public int itemID;
        public int count;
    }

    public List<InventoryItemInfo> haveItems = new List<InventoryItemInfo>();
    protected override void OnCompleteMove()
    {
        //아이템 있다면 먹자. 지금 위치에 아이템 있다면 획득하자.
        BlockInfo blockInfo = GroundManager.Instance.GetBlockInfo(transform.position);
        if(blockInfo.dropItemID != 0)
        {
            int addItemID = blockInfo.dropItemID;
            int addCount = 1;

            //인벤토리에서 증가.
            var existItem = haveItems.Find(x => x.itemID == addItemID);
            if (existItem != null)
            {
                existItem.count += addCount;
            }
            else
            {
                InventoryItemInfo addItem = new InventoryItemInfo() { itemID = addItemID };
                haveItems.Add(addItem);
            }

            // 획득소식 UI에 표시.
            string iconName = GlobalData.Instance.itemDataMap[addItemID].iconName;
            NotifyUI.Instance.Show($"{iconName}을 획득했습니다");

            // 블럭에서 아이템 정보 삭제
            GroundManager.Instance.RemoveItemInfo(transform.position);
        }

        bool existAttackTarget = ShowAttackableArea();
        if (existAttackTarget)
            StageManager.GameState = GameStateType.SelectToAttackTarget;
        else
            StageManager.GameState = GameStateType.SelectPlayer;
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
