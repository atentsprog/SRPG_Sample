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
    public string nickname;
    public float hp;
    public float mp;
    public float maxHp;
    public float maxMp;
    public StatusType status;
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
