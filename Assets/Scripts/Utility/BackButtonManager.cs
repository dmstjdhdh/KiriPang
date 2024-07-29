using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonManager : MonoBehaviour
{
    public static Stack History = new Stack();

    public enum E_BACKBUTTON_CASE
    {
        none = 0,
        Nothing,
        DirectQuit,
        AppQuit,
        ClosePopup,
        MoveToMain
    }

    public static E_BACKBUTTON_CASE backButtonType = E_BACKBUTTON_CASE.none;

    void Awake()
    {
        History.Push(E_BACKBUTTON_CASE.AppQuit);
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetKeyUp(KeyCode.Backspace))
            {
                Debugger.PrintLog("Editor Get Key Down BackSpace");
                //OnClickBackButton();
                OnClickEscape();
            }
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debugger.PrintLog("Android Get Key Down Escape");
                //OnClickBackButton();
                OnClickEscape();
            }
        }

    }
   
    public static void LastStackCheck()
    {
        E_BACKBUTTON_CASE his = (E_BACKBUTTON_CASE)History.Pop();
        if (his != E_BACKBUTTON_CASE.Nothing)
        {
            History.Push(his);
        }
    }

    private void OnClickEscape()
    {
        var history = (E_BACKBUTTON_CASE)History.Pop();
        if (history == E_BACKBUTTON_CASE.Nothing)
        {
            History.Push(E_BACKBUTTON_CASE.Nothing);
            return;
        }

        if(PopupManager.instance.GetActivePopup())
        {
            ResetStackMain();
            PopupManager.instance.CloseAllPopups();
            History.Push((E_BACKBUTTON_CASE)history);
            return;
        }

        Debugger.PrintLog("== On Click Escape Button , Cur History : " + history);

        switch(history)
        {
            case E_BACKBUTTON_CASE.none:

                break;

            case E_BACKBUTTON_CASE.Nothing:
                History.Push(E_BACKBUTTON_CASE.Nothing);
                break;

            case E_BACKBUTTON_CASE.DirectQuit:

                break;
            case E_BACKBUTTON_CASE.AppQuit:    
                History.Push(E_BACKBUTTON_CASE.AppQuit);
                PopupManager.instance.CloseAllPopups();
                AppQuit();
                break;
            case E_BACKBUTTON_CASE.ClosePopup: 
                ClosePopup();
                break;
            case E_BACKBUTTON_CASE.MoveToMain:
                ClosePopup();
                UIManager.instance.MoveToMain();
                break;
        }

    }

    public static void ResetStackMain()
    {
        History.Clear();
        History.Push(E_BACKBUTTON_CASE.AppQuit);
    }

    public static void ClosePopup()
    {
        PopupManager.instance.CloseAllPopups();
    }


    #region Application Quit
    private bool isBackButton = false;
    private Coroutine C_Back = null;

    public void AppQuit()
    {
        if (isBackButton)
        {
            StopCoroutine(C_Back);
            Application.Quit();
        }
        else
        {
            C_Back = StartCoroutine(C_AppQuit());
        }
    }
    private IEnumerator C_AppQuit()
    {
        isBackButton = true;
        //Debugger.PrintLog("한번 더 누르면 앱이 종료됩니다.");
        yield return new WaitForSeconds(1.0f);
        isBackButton = false;
    }

    #endregion



}
