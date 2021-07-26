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
        menus.Add("테스트 메뉴2",()=> { print("무명함수"); OnClick(); });

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
        OnClick();
    }

    private void EndTurnPlayer()
    {
        print("EndTurnPlayer");

        StageManager.Instance.EndTurnPlayer();

        OnClick();
    }

    private void OnClick()
    {
        Close();
    }

    internal void Show(Vector3 uiPosition)
    {
        base.Show();

        //https://youtu.be/zKjVdTQbV9w?t=512  참고
        // 스크린 포지션을 캔바스 포지션으로 수정 ( 마우스 클릭 지점을 UI 위치로 옮길때 사용)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>()
            , uiPosition, null, out Vector2 localPoint);

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = localPoint;
    }
}
