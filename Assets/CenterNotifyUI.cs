using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenterNotifyUI : SingletonMonoBehavior<CenterNotifyUI>
{
    Text contentsText;
    protected override void OnInit()
    {
        contentsText = transform.Find("ContentsText").GetComponent<Text>();
    }

    internal void Show(string text, float displayTime = 3)
    {
        base.Show();

        contentsText.text = text;

        StopAllCoroutines();
        StartCoroutine(HideUiCo(displayTime));
    }

    private IEnumerator HideUiCo(float displayTime)
    {
        yield return new WaitForSeconds(displayTime);
        Close();
    }
}
