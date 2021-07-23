using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotifyUI : SingletonMonoBehavior<NotifyUI>
{
    Text contentsText;
    void Start()
    {
        contentsText = transform.Find("ContentsText").GetComponent<Text>();
    }

    internal void ShowText(string text)
    {
        contentsText.text = text;
    }
}
