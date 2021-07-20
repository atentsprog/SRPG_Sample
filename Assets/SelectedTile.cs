using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedTile : SingletonMonoBehavior<SelectedTile>
{
    Vector3 originalScale;

    protected override void OnInit()
    {
        originalScale = transform.localScale;
    }

    internal void Hide()
    {
        transform.DOKill();
        transform.localScale = originalScale;
        gameObject.SetActive(false);
    }

    internal void SetPosition(Vector2Int pos)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = new Vector3(pos.x, 0, pos.y);


        transform.DOPunchScale(Vector3.one * funchValue, funchTime)
            .SetEase(funchEase)
            .SetLoops(-1, LoopType.Restart);
    }
    public float funchValue = 1.1f;
    public float funchTime = 0.1f;
    public Ease funchEase = Ease.InElastic;
}
