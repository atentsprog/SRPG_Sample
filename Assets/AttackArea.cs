using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public float damageRatio = 1;
    public enum Target
    {
        All,
        EnemyOnly,
        AllyOnly
    }
    public Target target = Target.All;
}
