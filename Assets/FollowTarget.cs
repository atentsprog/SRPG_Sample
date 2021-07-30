using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : SingletonMonoBehavior<FollowTarget>
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -7);
    public void SetTarget(Transform target)
    {
        this.target = target;
        if (target)
        {
            var pos = target.position;
            // 기존 카메라 높이를 유지 해야지
            // 카메라가 땅으로 가서 렌더링 안되는 버그를막는다
            //pos.y = transform.position.y; 
            transform.position = pos + offset;
        }
    }

    //다른 컴포넌트에 Pan기능때문에 사용안함. -> 타겟이 있을때는 

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
