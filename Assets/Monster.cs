using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddTileInfo(transform.position, BlockType.Enemy);
    }
}
