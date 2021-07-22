using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
    Normal,
    Sleep,
    Die,
}

public class Actor : MonoBehaviour
{
    public string nickname = "이름 이력해주세요";
    public string iconName;
    public float hp = 20;
    public float mp = 0;
    public float maxHp = 20;
    public float maxMp = 0;
    public StatusType status;

    public int moveDistance = 5;
}
public class Monster : Actor
{
    private void Awake()
    {
        
    }
    Animator animator;
    void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
    }
}
