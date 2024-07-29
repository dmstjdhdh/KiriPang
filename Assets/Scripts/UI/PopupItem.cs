using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupItem : MonoBehaviour
{
    public TMP_Text Msg;
    public TMP_Text Msg2;
    public Action Button1;
    public Action Button2;
    public Action Button3;
    public Action Close;
    public Action End;

    public void OnButton1()
    {
        PopupManager.instance.GetDim().gameObject.SetActive(false);
        PopupManager.instance.SetActivePopup(false);

        if (Button1 != null)
        {
            Button1.Invoke();
        }

        End.Invoke();
    }

    public void OnButton2()
    {
        PopupManager.instance.GetDim().gameObject.SetActive(false);
        PopupManager.instance.SetActivePopup(false);
        
        if (Button2 != null)
        {
            Button2.Invoke();
        }

        End.Invoke();
    }

    public void OnButton3()
    {
        PopupManager.instance.GetDim().gameObject.SetActive(false);
        PopupManager.instance.SetActivePopup(false);

        if (Button3 != null)
        {
            Button3.Invoke();
        }

        End.Invoke();
    }

    public void OnClose()
    {
        PopupManager.instance.GetDim().gameObject.SetActive(false);
        PopupManager.instance.SetActivePopup(false);

        if (Close != null)
        {
            Close.Invoke();
        }

        End.Invoke();
    }
}
