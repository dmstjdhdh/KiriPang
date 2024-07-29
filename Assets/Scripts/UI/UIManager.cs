using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public Transform boardMother;
    public Transform jellyMother;
    public Transform effectMother;
    public Transform stickerMother;

    [SerializeField]
    GameObject gameCavas;

    [SerializeField]
    Text scoreText;

    [SerializeField]
    Slider timeBar;

    [SerializeField]
    Image timeBarFrame;

    [SerializeField]
    Image timeBarGauge;

    [SerializeField]
    Image gameBoardLight;

    [SerializeField]
    Image scoreBoard;

    [SerializeField]
    Image pauseButton;

    [SerializeField]
    Image bottomDeco;

    [SerializeField]
    Image[] bottomDecoNumbers;

    [SerializeField]
    Image[] backgrounds;

    [SerializeField]
    GameObject introCanvas; 

    [SerializeField]
    Sprite[] timebarFrameSprites;

    [SerializeField]
    Sprite[] timebarSprites;

    [SerializeField]
    Sprite[] gameBoardLightSprites;

    [SerializeField]
    Sprite[] scoreBoardSprites;

    [SerializeField]
    Sprite[] pauseButtonSprites;

    [SerializeField]
    Sprite[] tileSprites;
    
    [SerializeField]
    Sprite[] backgroundSprites;

    [SerializeField]
    Sprite[] bottomDecoSprites;

    [SerializeField]
    Sprite[] bottomDecoNumbersSprites;

    [SerializeField]
    GameObject mainCanvas;

    [SerializeField]
    GameObject rankContent;

    [SerializeField]
    GameObject scorePrefab;

    [SerializeField]
    TMP_Text curScoreTxt;

    [SerializeField]
    Button rankBtn;

    [SerializeField]
    Button settingBtn;

    [SerializeField]
    RectTransform mainBtnsRect;

    [SerializeField]
    Image lastPangDim;

    GameObject openPanel;
    
    List<ScorePanel> scoreList = new List<ScorePanel>();

    int curScore = 0;

    bool doAnimation = false;

    bool isPanelOpen = false;

    void Start()
    {
        curScore = 0;
        rankBtn.onClick.AddListener(OpenRankPanel);
        rankBtn.onClick.AddListener(() => SoundManager.instance.PlayEffect(E_AUDIO_TYPE.click));
        settingBtn.onClick.AddListener(OpenSettingPanel);
        settingBtn.onClick.AddListener(() => SoundManager.instance.PlayEffect(E_AUDIO_TYPE.click));

        SoundManager.instance.PlayBackground(E_AUDIO_TYPE.main_01);
    }

    public void SetFeverTimeUI()
    {
        int fever = 0;

        if (GameManager.instance.GetFeverTime())
        {
            fever = 1;
        }

        gameBoardLight.sprite = gameBoardLightSprites[fever];
        gameBoardLight.SetNativeSize();

        scoreBoard.sprite = scoreBoardSprites[fever];
        timeBarFrame.sprite = timebarFrameSprites[fever];
        timeBarGauge.sprite = timebarSprites[fever];
        pauseButton.sprite = pauseButtonSprites[fever];

        Image[] tiles = boardMother.GetComponentsInChildren<Image>();

        foreach (Image tile in tiles)
        {
            tile.sprite = tileSprites[fever];
        }

        int index = 0;
        foreach (Image bg in backgrounds)
        {
            bg.sprite = backgroundSprites[fever + index];
            index += 2;
        }
    }

    public void UpdateScoreText()
    {
        scoreText.text = GameManager.instance.GetScore().ToString();
    }

    public void UpdateTimeBar()
    {
        timeBar.value = GameManager.instance.GetGameTime();
    }

    public void SetTimeBarMaxValue(float max)
    {
        timeBar.maxValue = max;
    }

    public void SetEnableGameCanvas(bool isEnabled)
    {
        gameCavas.SetActive(isEnabled);
    }

    public void StartGameIntro(bool isActive, Action middleAction, Action endAction)
    {
        if (doAnimation)
            return;

        doAnimation = true;
        introCanvas.SetActive(true);

        RectTransform center = introCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform leftDoor = introCanvas.transform.GetChild(1).GetComponent<RectTransform>();
        RectTransform rightDoor = introCanvas.transform.GetChild(2).GetComponent<RectTransform>();
        
        SoundManager.instance.StopBackground();
        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.door);

        Sequence introSeq = DOTween.Sequence()
            .Append(center.DOAnchorPosY(0f, 1f))
            .Append(leftDoor.DOAnchorPosX(-270f, 0.7f).SetEase(Ease.Unset))
            .Join(rightDoor.DOAnchorPosX(270f, 0.7f).SetEase(Ease.Unset))
            .AppendCallback(() => 
            {
                mainCanvas.SetActive(!isActive);
                gameCavas.SetActive(isActive);
                middleAction.Invoke();
            })
            .AppendInterval(1f)
            .Append(leftDoor.DOAnchorPosX(-820f, 0.7f).SetEase(Ease.Unset))
            .Join(rightDoor.DOAnchorPosX(820f, 0.7f).SetEase(Ease.Unset))
            .Append(center.DOAnchorPosY(1300f, 1f))
            .AppendCallback(() =>
            {
                introCanvas.SetActive(false);

                if (GameManager.instance.GetGameStatus())
                {
                    StartCounting()
                    .OnComplete(() => 
                    {
                        endAction.Invoke();
                        mainCanvas.SetActive(false);
                        doAnimation = false;
                        FieldManager.instance.SetFieldActive(true);
                        SoundManager.instance.PlayBackground(E_AUDIO_TYPE.game);
                    });
                }
            });
    }

    Sequence StartCounting() {

        Sequence countSeq = DOTween.Sequence();

        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.start_total);

        countSeq
            .OnStart(() => stickerMother.gameObject.SetActive(true))
            .Append(stickerMother.Find("Three").GetComponent<RectTransform>().DOAnchorPosX(0f, 0.25f))
            .AppendInterval(0.5f)
            .Append(stickerMother.Find("Three").GetComponent<RectTransform>().DOAnchorPosX(-380f, 0.25f))
            .AppendCallback(() => FieldManager.instance.InitJelly())
            .Append(stickerMother.Find("Two").GetComponent<RectTransform>().DOAnchorPosX(0f, 0.25f))
            .AppendInterval(0.5f)
            .Append(stickerMother.Find("Two").GetComponent<RectTransform>().DOAnchorPosX(380f, 0.25f))
            .Append(stickerMother.Find("One").GetComponent<RectTransform>().DOAnchorPosX(0f, 0.25f))
            .AppendInterval(0.5f)
            .Append(stickerMother.Find("One").GetComponent<RectTransform>().DOAnchorPosX(-380f, 0.25f))
            .Append(stickerMother.Find("Start").GetComponent<RectTransform>().DOAnchorPosY(200f, 0.5f).SetEase(Ease.OutExpo))
            .AppendInterval(0.5f)
            .Append(stickerMother.Find("Start").GetComponent<RectTransform>().DOAnchorPosY(-500f, 0.5f).SetEase(Ease.InExpo))
            .AppendCallback(() => stickerMother.gameObject.SetActive(false));

        //     .AppendCallback(() => jellyMother.GetComponent<RectTransform>().DOAnchorPosY(0f, 3f))
        return countSeq;
    }

    public void DoStartAnimation()
    {
        Sequence countSeq = DOTween.Sequence();

        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.start);

        countSeq
            .OnStart(() => stickerMother.gameObject.SetActive(true))
            .Append(stickerMother.Find("Start").GetComponent<RectTransform>().DOAnchorPosY(200f, 0.5f).SetEase(Ease.OutExpo))
            .AppendInterval(0.5f)
            .Append(stickerMother.Find("Start").GetComponent<RectTransform>().DOAnchorPosY(-500f, 0.5f).SetEase(Ease.InExpo))
            .AppendCallback(() => stickerMother.gameObject.SetActive(false));
    }

    public void CheerUP()
    {
        if (doAnimation)
            return;

        doAnimation = true; 

        RectTransform leftSticker = stickerMother.Find("Cheering_L").GetComponent<RectTransform>();
        RectTransform rightSticker = stickerMother.Find("Cheering_R").GetComponent<RectTransform>();

        Sequence cheerUpSeq = DOTween.Sequence()
            .OnStart(() => stickerMother.gameObject.SetActive(true))
            .Append(leftSticker.DOAnchorPosY(30f, 1f).SetEase(Ease.OutExpo))
            .Join(rightSticker.DOAnchorPosY(30f, 1f).SetEase(Ease.OutExpo))
            .Append(leftSticker.DOAnchorPosY(-500f, 1f).SetEase(Ease.InExpo))
            .Join(rightSticker.DOAnchorPosY(-500f, 1f).SetEase(Ease.InExpo))
            .AppendInterval(0.5f)
            .OnComplete(() =>
            {
                stickerMother.gameObject.SetActive(false);
                doAnimation = false;
            });
    }

    public void DoLastPangAnimation()
    {
        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.bike);

        RectTransform sticker = stickerMother.Find("LastPang").GetComponent<RectTransform>();
        lastPangDim.color = new Color(1f, 1f, 1f, 0f);
        lastPangDim.gameObject.SetActive(true);

        Sequence cheerUpSeq = DOTween.Sequence()
            .OnStart(() => stickerMother.gameObject.SetActive(true))
            .Append(lastPangDim.DOFade(1f, 0.5f))
            .AppendInterval(0.5f)
            .Append(sticker.DOAnchorPosX(-950f, 1f).SetEase(Ease.OutExpo))
            .AppendInterval(1.5f)
            .Append(sticker.DOAnchorPosX(-1900f, 0.5f).SetEase(Ease.OutExpo))
            .AppendInterval(1f)
            .OnComplete(() =>
            {
                stickerMother.gameObject.SetActive(false);
                lastPangDim.gameObject.SetActive(false);

                sticker.transform.localPosition = new Vector3(540f, -342f, 0f);
            });
    }

    void OpenSettingPanel()
    {
        GameObject settingPanel = mainCanvas.transform.Find("SettingPanel").gameObject;
        GameObject mainPanel = mainCanvas.transform.Find("MainPanel").gameObject;

        isPanelOpen = (openPanel != settingPanel);

        settingPanel.SetActive(isPanelOpen);
        if (settingPanel != openPanel)
        {
            mainBtnsRect.DOScale(0.6f, 0.5f);
            if (openPanel)
                openPanel.SetActive(false);
            settingPanel.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            settingPanel.transform.DOScale(1, 0.35f).SetEase(Ease.OutCubic);
            openPanel = settingPanel;
            InitSetting();
        }
        else
        {
            mainBtnsRect.DOScale(1f, 0.5f);
            mainPanel.SetActive(true);
            mainPanel.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            mainPanel.transform.DOScale(1, 0.35f).SetEase(Ease.OutCubic);
            openPanel = mainPanel;
        }
        Debug.Log("openPanel   " + openPanel);
    }

    void InitSetting()
    {
        GameObject settingPanel = mainCanvas.transform.Find("SettingPanel").gameObject;

        float bgVol = SoundManager.instance.GetBGMVolume();
        float fxVol = SoundManager.instance.GetFXVolume();

        Slider bgSlider = settingPanel.transform.Find("BGMSlider").GetComponent<Slider>();
        Slider fxSlider = settingPanel.transform.Find("SFXSlider").GetComponent<Slider>();

        bgSlider.maxValue = 1f;
        fxSlider.maxValue = 1f;

        bgSlider.value = bgVol;
        fxSlider.value = fxVol;

        bgSlider.onValueChanged.RemoveAllListeners();
        fxSlider.onValueChanged.RemoveAllListeners();

        bgSlider.onValueChanged.AddListener(delegate {SoundManager.instance.ApplyBGMVolumeOption(bgSlider.value);});
        fxSlider.onValueChanged.AddListener(delegate {SoundManager.instance.ApplyFXVolumeOption(fxSlider.value);});
    }

    void OpenRankPanel()
    {
        GameObject rankPanel = mainCanvas.transform.Find("RankPanel").gameObject;
        GameObject mainPanel = mainCanvas.transform.Find("MainPanel").gameObject;

        isPanelOpen = (openPanel != rankPanel);

        rankPanel.SetActive(isPanelOpen);
        if (rankPanel != openPanel)
        {
            mainBtnsRect.DOScale(0.6f, 0.5f);
            if(openPanel)
                openPanel.SetActive(false);
            rankPanel.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            rankPanel.transform.DOScale(1, 0.35f).SetEase(Ease.OutCubic);
            openPanel = rankPanel;
            SetRank();
        }else
        {
            mainBtnsRect.DOScale(1f, 0.5f);
            mainPanel.SetActive(true);
            mainPanel.transform.localScale = new Vector3(0.8f,0.8f,0.8f);
            mainPanel.transform.DOScale(1, 0.35f).SetEase(Ease.OutCubic);
            openPanel = mainPanel;
        }
        Debug.Log("openPanel   " + openPanel);
    }

    public void SetRank()
    {
        int currentScore = PlayerPrefs.GetInt("Current Score");
        if (curScore.Equals(currentScore)) 
            return;
        else
            curScore = currentScore;
        curScoreTxt.text =  currentScore.ToString("#,##0");

        int[] scores = GameData.LoadScores();
        int rank = 1;
        for(int i = 0; i < scores.Length; i++)
        {
            if( i < scoreList.Count)
            {
                scoreList[i].Init(rank.ToString(), scores[i].ToString("#,##0"), currentScore == scores[i]);
            }
            else
            {
                GameObject scoreObj = Instantiate(scorePrefab, rankContent.transform);
                ScorePanel scorePanel = scoreObj.GetComponent<ScorePanel>();
                scorePanel.Init(rank.ToString(), scores[i].ToString("#,##0"), currentScore == scores[i]);
                scoreList.Add(scorePanel);
            }
            rank++;
        }
    }

    public void MoveToMain()
    {
        mainCanvas.SetActive(true);
        gameCavas.SetActive(false);
        PopupManager.instance.CloseAllPopups();

        SoundManager.instance.PlayBackground(E_AUDIO_TYPE.main_01);
    }

    public void SetBottomPanel(E_FIELD_TYPE type, int num = 0)
    {
        switch (type)
        {
            case E_FIELD_TYPE.Default_Pink:
                bottomDeco.sprite = bottomDecoSprites[0];
                bottomDecoNumbers[0].gameObject.SetActive(false);
                bottomDecoNumbers[1].gameObject.SetActive(false);
                break;
            case E_FIELD_TYPE.Default_Blue:
                bottomDeco.sprite = bottomDecoSprites[1];
                bottomDecoNumbers[0].gameObject.SetActive(false);
                bottomDecoNumbers[1].gameObject.SetActive(false);
                break;
            case E_FIELD_TYPE.Fever:
                bottomDeco.sprite = bottomDecoSprites[2];
                bottomDecoNumbers[0].gameObject.SetActive(false);
                bottomDecoNumbers[1].gameObject.SetActive(false);
                break;
            case E_FIELD_TYPE.Combo:
                bottomDeco.sprite = bottomDecoSprites[3];

                bottomDecoNumbers[0].gameObject.SetActive(true);
                bottomDecoNumbers[1].gameObject.SetActive(true);

                bottomDecoNumbers[0].sprite = bottomDecoNumbersSprites[num % 10];
                bottomDecoNumbers[1].sprite = bottomDecoNumbersSprites[num / 10];
                break;
            case E_FIELD_TYPE.Reset:
                bottomDeco.sprite = bottomDecoSprites[4];
                bottomDecoNumbers[0].gameObject.SetActive(false);
                bottomDecoNumbers[1].gameObject.SetActive(false);
                break;
        }
    }
}
