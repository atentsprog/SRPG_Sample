using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContextMenuUI : BaseUI<ContextMenuUI>
{
    public GameObject baseItem;
    protected override void OnInit()
    {
        baseItem = transform.Find("BG/Button").gameObject;

        Dictionary<string, UnityAction> menus = new Dictionary<string, UnityAction>();
        menus.Add("턴 종료(F10)", EndTurnPlayer);
        menus.Add("테스트 메뉴", TestMenu);
        menus.Add("테스트 메뉴2",()=> { print("무명함수"); });

        foreach( var item in menus)
        {
            GameObject go = Instantiate(baseItem, baseItem.transform.parent);
            go.GetComponentInChildren<Text>().text = item.Key;
            go.GetComponent<Button>().AddListener(this, item.Value);
        }

        baseItem.SetActive(false);
    }

    private void TestMenu()
    {
        print("TestMenu");
    }

    private void EndTurnPlayer()
    {
        print("EndTurnPlayer");
    }

    internal void Show(Vector3 uiPosition)
    {
        base.Show();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>()
            , uiPosition, null, out Vector2 localPoint);

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = localPoint;
    }
}
