using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public enum E_POPUP_TYPE
{
    GameStop,
    GameResult,
    AppQuit
}
public class PopupManager : Singleton<PopupManager>
{
    [SerializeField]
    Image Dim;

    GameObject createdPopup;
    E_POPUP_TYPE currentPopupType;

    bool isPopupOn = false;

    void Start()
    {
        Dim.gameObject.SetActive(false);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.P))
        {
            Open(E_POPUP_TYPE.GameStop);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            Open(E_POPUP_TYPE.GameResult);
        }
#endif
    }

    public void Open(E_POPUP_TYPE type, Action onButton1 = null, Action onButton2 = null, Action onButton3 = null, Action onClose = null)
    {
        Debugger.PrintLog("IsPopup On = " + isPopupOn);
        if (isPopupOn)
        {
            return;
        }

        isPopupOn = true;

        // load popup
        var popup = Resources.Load<GameObject>("Popup/" + type.ToString());

        if (popup == null)
        {
            Debugger.PrintLog(type.ToString() + " popup is missing!");
            return;
        }

        createdPopup = Instantiate(popup, transform);
        var popupItem = createdPopup.GetComponent<PopupItem>();

        popupItem.Button1 = onButton1;
        popupItem.Button2 = onButton2;
        popupItem.Button3 = onButton3;
        popupItem.Close = onClose;
        popupItem.End = () =>
        {
            isPopupOn = false;
            Debugger.PrintLog("Close Popup");
            if (createdPopup == null) 
                Debugger.PrintLog("==== Popup is null");

            if (transform.childCount == 2)
            {
                Destroy(createdPopup);
                Debugger.PrintLog("transform.childCount == 2");
            }
            else
            {
                for (int i = 1; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject != createdPopup)
                        Destroy(transform.GetChild(i).gameObject);
                }
                Debugger.PrintLog("else");
            }

        };

        createdPopup.transform.localScale = Vector3.zero;
        createdPopup.SetActive(true);


        DOTween.Kill(Dim);
        Dim.gameObject.SetActive(true);
        createdPopup.transform.DOScale(1, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);

        Resources.UnloadUnusedAssets();
    }


    public void OpenGameResult(Action onButton1 = null, Action onButton2 = null, Action onButton3 = null, Action onClose = null)
    {
        Debugger.PrintLog("IsPopup On = " + isPopupOn);
        if (isPopupOn)
        {
            return;
        }

        isPopupOn = true;
        // load popup
        var popup = Resources.Load<GameObject>("Popup/GameResult");

        createdPopup = Instantiate(popup, transform);
        var popupItem = createdPopup.GetComponent<PopupItem>();
        int score = GameData.LoadScores()[0] == 0 ? PlayerPrefs.GetInt("Current Score") : GameData.LoadScores()[0];
        Debugger.PrintLog("score = " + score);
        popupItem.Msg.text = PlayerPrefs.GetInt("Current Score").ToString("#,##0");
        popupItem.Msg2.text = score.ToString("#,##0");
        popupItem.Button1 = onButton1;
        popupItem.Button2 = onButton2;
        popupItem.Button3 = onButton3;
        popupItem.Close = onClose;
        popupItem.End = () =>
        {
            isPopupOn = false;
            Debugger.PrintLog("Close Popup");
            if (createdPopup == null)
                Debugger.PrintLog("==== Popup is null");

            if (transform.childCount == 2)
            {
                Destroy(createdPopup);
                Debugger.PrintLog("transform.childCount == 2");
            }
            else
            {
                for (int i = 1; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject != createdPopup)
                        Destroy(transform.GetChild(i).gameObject);
                }
                Debugger.PrintLog("else");
            }

        };

        createdPopup.transform.localScale = Vector3.zero;
        createdPopup.SetActive(true);


        DOTween.Kill(Dim);
        Dim.gameObject.SetActive(true);
        createdPopup.transform.DOScale(1, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);

        Resources.UnloadUnusedAssets();
    }

    public void CloseAllPopups()
    {
        if (createdPopup != null)
        {
            Destroy(createdPopup);
        }

        Dim.gameObject.SetActive(false);

        isPopupOn = false;
    }

    public bool isActivePopup()
    {
        return createdPopup != null;
    }

    public bool GetActivePopup()
    {
        return isPopupOn;
    }

    public void SetActivePopup(bool isPopupOn)
    {
        this.isPopupOn = isPopupOn;
    }
    public Image GetDim()
    {
        return Dim;
    }
}
