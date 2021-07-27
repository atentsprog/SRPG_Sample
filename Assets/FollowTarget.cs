using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : SingletonMonoBehavior<FollowTarget>
{
    Transform target;
    public Vector3 offset = new Vector3(0, 0, -7);
    public void SetTarget(Transform target)
    {
        this.target = target;
        if (target)
        {
            var pos = target.position;
            pos.y = transform.position.y;
            transform.position = pos;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        var newPos = target.position + offset;

        newPos.x = transform.position.x;
        newPos.y = transform.position.y;

        transform.position = newPos;
    }
}
