using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContextMenuUI : BaseUI<ContextMenuUI>
{
    Dictionary<string, UnityAction> menus = new Dictionary<string, UnityAction>();
    public GameObject baseMenuGo;
    public Transform menuParentTr;

    protected override void OnInit()
    {
        menus.Add("턴 종료(F10)", EndTurnPlayer);
        menus.Add("TestMenu1   ", TestMenu1);
        menus.Add("TestMenu2   ", TestMenu2);

        menuParentTr = baseMenuGo.transform.parent;
        baseMenuGo.SetActive(true);
        foreach (var item in menus)
        {
            var newMenu = Instantiate(baseMenuGo, menuParentTr);
            newMenu.GetComponentInChildren<Text>().text = item.Key;

            Button button = newMenu.GetComponent<Button>();
            button.AddListener(this, item.Value);
        }
        baseMenuGo.SetActive(false);
    }

    public void ShowStageMenu()
    {
        base.Show();
        menuParentTr.gameObject.SetActive(true);
    }

    private void TestMenu2()
    {
        OnClickMenu();
        Debug.Log("TestMenu1");
    }

    private void TestMenu1()
    {
        OnClickMenu();
        Debug.Log("TestMenu1");
    }

    private void EndTurnPlayer()
    {
        OnClickMenu();
        StageManager.Instance.ProcessEndOfPlayerTurn();
    }
    
    void OnClickMenu()
    {
        Close();
    }
}
