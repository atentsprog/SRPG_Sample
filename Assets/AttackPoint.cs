using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPoint : MonoBehaviour
{
    public List<AttackArea> attackAreas;
    void Start()
    {
        attackAreas.Clear();
        attackAreas.AddRange(GetComponentsInChildren<AttackArea>());
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    internal List<AttackArea> GetAttackableAreas()
    {
        return attackAreas;
    }
}
