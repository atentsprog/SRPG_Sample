using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapMover : MonoBehaviour
{
    private void Start()
    {
        if (Application.isPlaying)
        {
            Destroy(this);
        }
    }
    void Update()
    {
        //var pos = transform.position;
        //pos.x = Mathf.Round(pos.x);
        //pos.z = Mathf.Round(pos.z);
        //transform.position = pos;

        transform.position = transform.position.ToVector3Snap();
    }
}
