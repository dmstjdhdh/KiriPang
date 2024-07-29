using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ScorePanel : MonoBehaviour
{
    [SerializeField]
    TMP_Text rankTxt;

    [SerializeField]
    TMP_Text scoreTxt;

    [SerializeField]
    Sprite myScoreSprite;

    [SerializeField]
    Sprite scoreSprite;

    public void Init(string rank = null, string score = null, bool isCurrentScore = false)
    {
        if(scoreTxt)
            scoreTxt.text = score;

        if (isCurrentScore)
        {
            gameObject.GetComponent<Image>().sprite = myScoreSprite;
            if (rankTxt)
                rankTxt.text = " ";
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = scoreSprite;
            if (rankTxt)
                rankTxt.text = rank;
        }
    }

}
