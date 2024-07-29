using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataController : MonoBehaviour
{

    [SerializeField]
    GameObject testPanel;
    [SerializeField]
    Button showBtn;
    [SerializeField]
    Button setDataBtn;
    [SerializeField]
    TMP_InputField[] tems;
    void Start()
    {
        testPanel.SetActive(false);
        showBtn.onClick.AddListener(() => ShowTestPanel(true));
        setDataBtn.onClick.AddListener(() =>
        {
            SetUtilData();
            ShowTestPanel(false);
        });
    }

    void ShowTestPanel(bool isActive)
    {
        testPanel.SetActive(isActive);
    }

    void SetUtilData()
    {
        if (!tems[0].text.Equals(String.Empty))
            GameData.GameTime = float.Parse(tems[0].text);
        if (!tems[1].text.Equals(String.Empty))
            GameData.Combo = int.Parse(tems[1].text);
        if (!tems[2].text.Equals(String.Empty))
            GameData.FeverTime = float.Parse(tems[2].text);
        if (!tems[3].text.Equals(String.Empty))
            GameData.GameSpeed = float.Parse(tems[3].text);
    }
}
