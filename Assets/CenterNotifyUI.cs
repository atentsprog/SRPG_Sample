using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenterNotifyUI : SingletonMonoBehavior<CenterNotifyUI>
{
    Text contentsText;
    CanvasGroup canvasGroup;
    protected override void OnInit()
    {
        contentsText = transform.Find("ContentsText").GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    internal void Show(string text, float displayTime = 3)
    {
        base.Show();

        contentsText.text = text;

        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.3f);
        canvasGroup.DOFade(0, 0.3f).SetDelay(displayTime)
            .OnComplete(Close);
    }
}
