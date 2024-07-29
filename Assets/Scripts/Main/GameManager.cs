using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    int combo;
    int score;

    float gameTime;

    bool comboEnable;
    bool isFever;
    bool isGameStart;

    bool makeItem;
    bool isLastPangMode;

    Coroutine comboCountCoroutine;

    bool timeWarning;

    bool isInit;

    private void Awake() 
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 240;
    }

    private void Start()
    {
        GameData.Initialize();
    }

    public void StartGame()
    {
        Init();
    }

    public void StopGame()
    {
        if (!FieldManager.instance.GetFieldActive()) return;
        
        FieldManager.instance.SetFieldActive(false);
        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.touch);
        Time.timeScale = 0f;
        PopupManager.instance.Open(E_POPUP_TYPE.GameStop,
            () => {
                // 게임 계속 하기
                FieldManager.instance.SetFieldActive(true);
                Time.timeScale = 1f;
                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.touch);
            },
            () => {
                // 게임 종료
                isInit = false;
                gameTime = 0f;
                Release();
                UIManager.instance.MoveToMain();
                Time.timeScale = 1f;
                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.touch);
            });
    }

    void CloseGame()
    {
        PopupManager.instance.OpenGameResult(
           () => {
               // 게임 다시 하기
               gameTime = 0f;
               isInit = false;
               Release();
               StartGame();
               SoundManager.instance.PlayEffect(E_AUDIO_TYPE.click);
           },
           () => {
               // 게임 종료
               isInit = false;
               gameTime = 0f;
               Release();
               UIManager.instance.MoveToMain();
               SoundManager.instance.PlayEffect(E_AUDIO_TYPE.touch);
           });
    }


    private void Init() 
    {
        if (isInit) return;

        score = 0;
        combo = 0;

        timeWarning = false;

        gameTime = GameData.GameTime;

        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.click);

        UIManager.instance.UpdateScoreText();
        UIManager.instance.UpdateTimeBar();
        UIManager.instance.SetTimeBarMaxValue(gameTime);

        isFever = false;
        isLastPangMode = false;
        makeItem = true;
        UIManager.instance.SetFeverTimeUI();

        FieldManager.instance.SetFieldActive(false);

        UIManager.instance.StartGameIntro(true,() => SetField() ,() => StartCoroutine(CoCountGameTime()));
        UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Default_Pink);

        isInit = true;
        //UIManager.instance.SetEnableGameCanvas(true);
        //SetField();
        //StartCoroutine(CoCountGameTime());
        //FieldManager.instance.InitJelly();
    }

    public void Release()
    {
        FieldManager.instance.ReleaseField();
        StopAllCoroutines();
        isGameStart = false;
    }

    void SetField()
    { 
        FieldManager.instance.InitField();
        isGameStart = true;
    }

    IEnumerator CoCountComboEnableTime()
    {
        yield return new WaitForSeconds(20f);

        combo = 0;
        comboEnable = false;
        Debugger.PrintLog("5초가 경과해 콤보 끊김!");

        bool isFind = FieldManager.instance.FindPotentialMatches();

        if (isFind)
        {
            Debugger.PrintLog("Match 가능한 젤리가 있습니다!");
            if (!isFever)
            {
                UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Default_Pink);
            }
        }
        else
        {
            if (!isLastPangMode)
            {
                if (FieldManager.instance.IsItemExist())
                {
                    Debugger.PrintLog("Match 가능한 젤리가 없지만, 아이템이 있습니다.");
                }
                else
                {
                    Debugger.PrintLog("Match 가능한 젤리가 없습니다.");
                    FieldManager.instance.ReleaseField();
                    FieldManager.instance.InitJelly();
                    UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Reset);

                    yield return new WaitForSeconds(2f);

                    UIManager.instance.DoStartAnimation();

                    yield return new WaitForSeconds(1.5f);

                    FieldManager.instance.SetFieldActive(true);
                }
            }
        }
    }

    IEnumerator CoCountGameTime()
    {
        float timeCountScale = 0.1f;
        ActiveComboEnable();

        while (gameTime > 0f)
        {
            if (!isFever)
            {
                gameTime -= timeCountScale;
                UIManager.instance.UpdateTimeBar();

                if (gameTime <= 10f)
                {
                    if (!timeWarning)
                    {
                        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.warning);
                        timeWarning = true;
                    }
                }
            }

            yield return new WaitForSeconds(timeCountScale);
        }

        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.time_up);

        isLastPangMode = true;
        FieldManager.instance.PlayLastPang();
    }

    IEnumerator CoCountFeverTime()
    {
        float timeCountScale = 1f;

        while (true)
        {
            if (isFever)
            {
                GameData.FeverTime -= timeCountScale;
                Debugger.PrintLog("남은 피버 타임 : " + GameData.FeverTime);
            }

            yield return new WaitForSeconds(timeCountScale);

            if (GameData.FeverTime <= 0f)
            {
                Debugger.PrintLog("피버 종료 처리");
                isFever = false;
                UIManager.instance.SetFeverTimeUI();
                UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Default_Pink);
                break;
            }
        }
    }

    void ActiveComboEnable()
    {
        comboEnable = true;
        comboCountCoroutine = StartCoroutine(CoCountComboEnableTime());
    }

    public void AddScore(int addNum)
    {
        int itemPivotScroe = 2500; 

        if (isFever)
        {
            itemPivotScroe = 1500;  
            addNum = Mathf.RoundToInt(addNum * 1.5f);
        }

        score += addNum;
        UIManager.instance.UpdateScoreText();

        if (score % itemPivotScroe == 0 && makeItem)
        {
            Debugger.PrintLog("아이템 생성");
            FieldManager.instance.SetItemFlag(true);
        }
    }

    public void AddCombo(int addNum)
    {
        if (isFever || isLastPangMode)
        {
            if (comboCountCoroutine != null)
            {
                StopCoroutine(comboCountCoroutine);
            }

            return;
        }

        if (comboEnable)
        {
            combo += addNum;
            if (comboCountCoroutine != null)
            {
                StopCoroutine(comboCountCoroutine);
            }
            Debugger.PrintLog("현재 콤보 : " + combo);
            comboCountCoroutine = StartCoroutine(CoCountComboEnableTime());
        }
        else
        {
            combo += addNum;
            ActiveComboEnable();
        }

        if (!isFever)
        {
            UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Combo, combo);
            if (combo >= GameData.Combo) // 조정해서 테스트 가능.
            {
                UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Fever);
                Debugger.PrintLog("피버타임!!");
                GameData.FeverTime = 15f;
                isFever = true;
                combo = 0;
                UIManager.instance.SetFeverTimeUI();
                FieldManager.instance.SetFieldActive(false);
                FieldManager.instance.StartFeverTime();

                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.eya);
            }
        }
    }

    
    public int GetScore()
    {
        return score;
    }

    public int GetCombo()
    {
        return combo;
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    public void AddGameTime(int sec)
    {
        gameTime += sec;
    }

    public bool GetFeverTime()
    {
        return isFever;
    }

    public bool GetGameStatus()
    {
        return isGameStart;
    }

    public void StartFeverTimeCount()
    {
        StartCoroutine(CoCountFeverTime());
    }

    public void FinishGame()
    {
        GameData.SaveScore(score);
        isGameStart = false;
        CloseGame();
        FieldManager.instance.ReleaseField();
        isGameStart = false;
    }

    public void SetMakeItem(bool make)
    {
        makeItem = make;
    }

    public bool IsLastPangMode()
    {
        return isLastPangMode;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
