using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class SnapMover : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
    void Update()
    {
        var intPos = transform.position.ToVector2Int();
        transform.position = intPos.ToVector3(transform.position.y);
    }
}
