using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster);
    }
}
