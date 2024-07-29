using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Burst.Intrinsics;
using System.Net.Http;
using System.Text.RegularExpressions;

public class FieldManager : Singleton<FieldManager>
{
    public Jelly[] jellies;
    public Jelly[] items;
    public Effect[] effects;

    public int width;
    public int height;
    public GameObject board;

    public bool isFever = false;

    Jelly[,] field;
    Dictionary<E_EFFECT_TYPE, Effect> effectDic;

    Coroutine coJellyMove = null;
    List<Jelly> matchedJellies;
    Jelly hintJelly;

    bool itemFlag;

    float moveJellyDuration;

    bool isActive = false;
    bool isMatching = false;

    public void InitField()
    {
        moveJellyDuration = GameData.GameSpeed;
        //SetFieldActive(true);

        effectDic = new Dictionary<E_EFFECT_TYPE, Effect>();

        foreach (var arr in effects)
        {
            if (!effectDic.ContainsKey(arr.GetEffectType()))
            {
                effectDic.Add(arr.GetEffectType(), arr);
                Debugger.PrintLog("key = " + arr.GetEffectType() + " / value = " + arr);
            }
        }

        field = new Jelly[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float interval = 150f;
                string nameObj = "( " + i + "," + j + " )";
                Vector2 tempPosition = new Vector2((i + -3f) * interval, (j + -4f) * interval);

                GameObject backgroundTile = Instantiate(board) as GameObject;

                backgroundTile.transform.SetParent(UIManager.instance.boardMother);
                backgroundTile.name = "board_" + nameObj;
                backgroundTile.transform.localScale = Vector3.one;
                backgroundTile.transform.localPosition = tempPosition;
            }
        }
    }

    public void InitJelly()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float interval = 150f;
                string nameObj = "( " + i + "," + j + " )";
                Vector2 tempPosition = new Vector2((i + -3f) * interval, (j + -4f) * interval);

                E_JELLY_TYPE jellyType;
                int randomNumber;
                int maxIterations = 0;
                do
                {
                    randomNumber = Random.Range(0, jellies.Length);
                    jellyType = jellies[randomNumber].GetJellyType();
                    maxIterations++;

                    if (maxIterations >= 100)
                    {
                        Debugger.PrintLog("Exceeded max iterations while generating the field!", LogType.Error);
                        break;
                    }
                } while (CheckJellyDuplicates(i, j, jellyType));

                //Debugger.PrintLog("[Duplitcate Count] :" + maxIterations);
                maxIterations = 0;

                GameObject jellyInstant = Instantiate(jellies[randomNumber].gameObject);

                jellyInstant.transform.SetParent(UIManager.instance.jellyMother);
                jellyInstant.name = "jelly_" + nameObj;
                jellyInstant.transform.localScale = Vector3.one;
                jellyInstant.transform.localPosition = tempPosition;

                field[i, j] = jellyInstant.GetComponent<Jelly>();

                Color color = field[i, j].GetComponent<Image>().color;
                color.a = 0f;
                field[i, j].GetComponent<Image>().color = color;
                field[i, j].GetComponent<Image>().DOFade(1f, 3f).SetEase(Ease.InQuart);
            }
        }
    }

    public void ReleaseField()
    {
        SetFieldActive(false);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                field[i, j].DOKill(true);
                if (field[i, j])
                {
                    Destroy(field[i, j].gameObject);
                    field[i, j] = null;
                }
            }
        }
    }

    bool CheckBoard()
    {
        matchedJellies = new List<Jelly>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (field[i, j].GetJellyType() != E_JELLY_TYPE.Empty)
                {
                    if (j == 0)
                    {
                        if (field[i, j].GetJellyType() == field[i, j + 1].GetJellyType() && field[i, j].GetJellyType() == field[i, j + 2].GetJellyType())
                        {
                            matchedJellies.Add(field[i, j]);
                            matchedJellies.Add(field[i, j + 1]);
                            matchedJellies.Add(field[i, j + 2]);
                        }
                    }
                    else if (j == field.GetLength(1) - 1)
                    {
                        if (field[i, j].GetJellyType() == field[i, j - 1].GetJellyType() && field[i, j].GetJellyType() == field[i, j - 2].GetJellyType())
                        {
                            matchedJellies.Add(field[i, j]);
                            matchedJellies.Add(field[i, j - 1]);
                            matchedJellies.Add(field[i, j - 2]);
                        }
                    }
                    else
                    {
                        if (field[i, j].GetJellyType() == field[i, j + 1].GetJellyType() && field[i, j].GetJellyType() == field[i, j - 1].GetJellyType())
                        {
                            matchedJellies.Add(field[i, j]);
                            matchedJellies.Add(field[i, j + 1]);
                            matchedJellies.Add(field[i, j - 1]);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (field[j, i].GetJellyType() != E_JELLY_TYPE.Empty)
                {
                    if (j == 0)
                    {
                        if (field[j, i].GetJellyType() == field[j + 1, i].GetJellyType() && field[j, i].GetJellyType() == field[j + 2, i].GetJellyType())
                        {
                            matchedJellies.Add(field[j, i]);
                            matchedJellies.Add(field[j + 1, i]);
                            matchedJellies.Add(field[j + 2, i]);
                        }
                    }
                    else if (j == field.GetLength(0) - 1)
                    {
                        if (field[j, i].GetJellyType() == field[j - 1, i].GetJellyType() && field[j, i].GetJellyType() == field[j - 2, i].GetJellyType())
                        {
                            matchedJellies.Add(field[j, i]);
                            matchedJellies.Add(field[j - 1, i]);
                            matchedJellies.Add(field[j - 2, i]);
                        }
                    }
                    else
                    {
                        if (field[j, i].GetJellyType() == field[j + 1, i].GetJellyType() && field[j, i].GetJellyType() == field[j - 1, i].GetJellyType())
                        {
                            matchedJellies.Add(field[j, i]);
                            matchedJellies.Add(field[j + 1, i]);
                            matchedJellies.Add(field[j - 1, i]);
                        }
                    }
                }
            }
        }

        return matchedJellies.Count > 2;
    }

    public bool FindPotentialMatches() 
    {
        if (GameManager.instance.IsLastPangMode())
        {
            return false;
        }

        for (int loop = 0; loop < width; loop++)
        {
            for (int loop2 = 0; loop2 < height - 2; loop2++)
            {
                Jelly hj = FindHintJellyByWidthSearch(field[loop, loop2], field[loop, loop2 + 1], field[loop, loop2 + 2]);

                if (hj != null)
                {
                    if (GameManager.instance.GetCombo() == 0)
                    {
                        hintJelly = hj;
                        hintJelly.SetHintSprtie();
                        return true;
                    }
                }
            }
        }

        for (int loop = 0; loop < height; loop++)
        {
            for (int loop2 = 0; loop2 < width - 2; loop2++)
            {
                Jelly hj = FindHintJellyByHeightSearch(field[loop2, loop], field[loop2 + 1, loop], field[loop2 + 2, loop]);

                if (hj != null)
                {
                    if (GameManager.instance.GetCombo() == 0)
                    {
                        hintJelly = hj;
                        hintJelly.SetHintSprtie();
                        return true;
                    }
                }
            }
        }

        return false;
        // for (int y = 0; y < height - 1; y++)
        // {
        //     for (int x = 0; x < width - 2; x++)
        //     {
        //         E_JELLY_TYPE type;

        //         type = field[x, y].GetJellyType();

        //         if(type == field[x + 1, y].GetJellyType() || type == field[x + 1, y + 1].GetJellyType())
        //         {
        //             if(type == field[x + 2, y].GetJellyType() || type == field[x + 2, y + 1].GetJellyType())
        //             {
        //                 hintJelly = field[x, y];
        //                 hintJelly.SetHintSprtie();
        //                 return true;
        //             }
        //         }

        //         type = field[x, y + 1].GetJellyType();

        //         if (type == field[x + 1, y].GetJellyType() || type == field[x + 1, y + 1].GetJellyType())
        //         {
        //             if (type == field[x + 2, y].GetJellyType() || type == field[x + 2, y + 1].GetJellyType())
        //             {
        //                 hintJelly = field[x, y + 1];
        //                 hintJelly.SetHintSprtie();
        //                 return true;
        //             }
        //         }
        //     }
        // }

        // for (int x = 0; x < width - 1; x++)
        // {
        //     for (int y = 0; y < height - 2; y++)
        //     {
        //         E_JELLY_TYPE type;

        //         type = field[x, y].GetJellyType();

        //         if (type == field[x, y + 1].GetJellyType() || type == field[x + 1, y + 1].GetJellyType())
        //         {
        //             if (type == field[x, y + 2].GetJellyType() || type == field[x + 1, y + 2].GetJellyType())
        //             {
        //                 hintJelly = field[x, y];
        //                 hintJelly.SetHintSprtie();
        //                 return true;
        //             }
        //         }

        //         type = field[x + 1, y].GetJellyType();

        //         if (type == field[x, y + 1].GetJellyType() || type == field[x + 1, y + 1].GetJellyType())
        //         {
        //             if (type == field[x, y + 2].GetJellyType() || type == field[x + 1, y + 2].GetJellyType())
        //             {
        //                 hintJelly = field[x + 1, y];
        //                 hintJelly.SetHintSprtie();
        //                 return true;
        //             }
        //         }
        //     }
        // }
        // return false; 
    }

    Jelly FindHintJellyByWidthSearch(Jelly j1, Jelly j2, Jelly j3)
    {
        int w = 0;
        int h = 0;

        if (j1.GetJellyType() == j2.GetJellyType())
        {
            w = GetJellyWidth(j3);
            h = GetJellyHeight(j3);

            if (h < height - 1)
            {
                if (field[w, h + 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h + 1];
                }
            }

            if (w <= 0)
            {
                if (field[w + 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w + 1, h];
                }
            }
            else if (w + 1 >= width)
            {
                if (field[w - 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w - 1, h];
                }
            }
            else
            {
                if (field[w + 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w + 1, h];
                }
                else if (field[w - 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w - 1, h];
                }
            }
        }

        if (j1.GetJellyType() == j3.GetJellyType())
        {
            w = GetJellyWidth(j2);
            h = GetJellyHeight(j2);

            if (w <= 0)
            {
                if (field[w + 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w + 1, h];
                }
            }
            else if (w + 1 >= width)
            {
                if (field[w - 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w - 1, h];
                }
            }
            else
            {
                if (field[w + 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w + 1, h];
                }
                else if (field[w - 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w - 1, h];
                }
            }
        }

        return null;
    }

    Jelly FindHintJellyByHeightSearch(Jelly j1, Jelly j2, Jelly j3)
    {
        int w = 0;
        int h = 0;

        if (j1.GetJellyType() == j2.GetJellyType())
        {
            w = GetJellyWidth(j3);
            h = GetJellyHeight(j3);

            if (w < width - 1)
            {
                if (field[w + 1, h].GetJellyType() == j1.GetJellyType())
                {
                    return field[w + 1, h];
                }
            }

            if (h <= 0)
            {
                if (field[w, h + 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h + 1];
                }
            }
            else if (h + 1 >= height)
            {
                if (field[w, h - 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h - 1];
                }
            }
            else
            {
                if (field[w, h + 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h + 1];
                }
                else if (field[w, h - 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h - 1];
                }
            }
        }

        if (j1.GetJellyType() == j3.GetJellyType())
        {
            w = GetJellyWidth(j2);
            h = GetJellyHeight(j2);

            if (h <= 0)
            {
                if (field[w, h + 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h + 1];
                }
            }
            else if (h + 1 >= height)
            {
                if (field[w, h - 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h - 1];
                }
            }
            else
            {
                if (field[w, h + 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h + 1];
                }
                else if (field[w, h - 1].GetJellyType() == j1.GetJellyType())
                {
                    return field[w, h - 1];
                }
            }
        }

        return null;
    }

    void ClearMatchedJellies()
    {
        if (matchedJellies != null)
        {
            if (matchedJellies.Count > 0)
            {
                if (GameManager.instance.GetFeverTime())
                {
                    SoundManager.instance.PlayEffect(E_AUDIO_TYPE.pop_fever);
                }
                else
                {
                    SoundManager.instance.PlayEffect(E_AUDIO_TYPE.pop);
                }
            }
        }

        foreach (Jelly jelly in matchedJellies)
        {
            if (jelly.IsItem)
            {
                continue;
            }

            int w = GetJellyWidth(jelly);
            int h = GetJellyHeight(jelly);
            field[w, h].SetJellyType(E_JELLY_TYPE.Empty);
            ShowNormalEffect(field[w, h]);

            jelly.Pop();
        }
    }

    void ShowNormalEffect(Jelly jelly, E_EFFECT_TYPE effectType = E_EFFECT_TYPE.Star)
    {
        Effect e = effectDic[effectType];
        Debugger.PrintLog("key = " + e.GetEffectType());

        if (e == null)
            return;

        GameObject tempEffectObj = Instantiate(e.gameObject) as GameObject;
        tempEffectObj.transform.SetParent(UIManager.instance.effectMother);
        tempEffectObj.transform.localScale = Vector3.one;

        Effect tempEffect = tempEffectObj.GetComponent<Effect>();

        if(GameManager.instance.GetFeverTime())
            tempEffect.StartFeverEffect(jelly.transform);
        else
            tempEffect.StartNormalEffect(jelly.transform);

        // if (GameManager.instance.GetFeverTime())
        // {
        //     tempEffect.StartFeverEffect(jelly.transform);
        // }
        // else
        // {
        //     tempEffect.StartNormalEffect(jelly.transform);
        // }
    }

    void RearrangmentBoard()
    {
        for (int w = 0; w < width; w++)
        {
            int minY = -1;
            int count = 0;

            for (int h = 0; h < height; h++)
            {
                if (field[w, h].GetJellyType() == E_JELLY_TYPE.Empty)
                {
                    Destroy(field[w, h].gameObject);
                    field[w, h] = null;
                    if (minY == -1)
                        minY = h;

                    count++;
                }
                else if (field[w, h] == null)
                {
                    if (minY == -1)
                        minY = h;

                    count++;
                }
                else if (!field[w, h].gameObject.activeSelf)
                {
                    Destroy(field[w, h].gameObject);
                    field[w, h] = null;
                    if (minY == -1)
                        minY = h;

                    count++;
                }
            }

            if (count != 0)
            {
                for (int h = minY + 1; h < height; h++)
                {
                    //null check 
                    if (field[w, h] != null)
                    {
                        field[w, h].IsMove = false;
                        MoveJellyLocal(field[w, h], new Vector3((w - 3f) * 150f, (minY - 4f) * 150f, 0f));
                        field[w, minY] = field[w, h];
                        field[w, h] = null;
                        minY++;
                    }
                }

                count = 0;

                for (int h = minY; h < height; h++)
                {
                    int randomNumber;

                    GameObject jellyInstant;

                    if (itemFlag)
                    {
                        randomNumber = Random.Range(0, items.Length);

                        jellyInstant = Instantiate(items[randomNumber].gameObject);
                        jellyInstant.GetComponent<Jelly>().IsItem = true;

                        jellyInstant.transform.SetParent(UIManager.instance.jellyMother);

                        jellyInstant.name = "item";
                        jellyInstant.transform.localScale = Vector3.one;
                        jellyInstant.transform.localPosition = new Vector3((w - 3) * 150f, (height + count - 4) * 150f, 0);

                        field[w, h] = jellyInstant.GetComponent<Jelly>();

                        Vector3 temp = new Vector3((w - 3) * 150f, (h - 4) * 150f, 0f);
                        MoveJellyLocal(field[w, h], temp);

                        itemFlag = false;
                    }
                    else
                    {
                        randomNumber = Random.Range(0, jellies.Length);

                        jellyInstant = Instantiate(jellies[randomNumber].gameObject);
                        jellyInstant.GetComponent<Jelly>().IsItem = false;

                        jellyInstant.transform.SetParent(UIManager.instance.jellyMother);

                        jellyInstant.name = "jelly_( " + w + "," + h + " )";
                        jellyInstant.transform.localScale = Vector3.one;
                        jellyInstant.transform.localPosition = new Vector3((w - 3) * 150f, (height + count - 4) * 150f, 0);

                        field[w, h] = jellyInstant.GetComponent<Jelly>();

                        Vector3 temp = new Vector3((w - 3) * 150f, (h - 4) * 150f, 0f);
                        Debugger.PrintLog("to : " + temp);
                        MoveJellyLocal(field[w, h], temp);

                        count++;
                    }
                }

            }
        }
    }

    bool CheckJellyDuplicates(int w, int h, E_JELLY_TYPE type)
    {
        if (w >= 2 && field[w - 1, h].GetJellyType() == type && field[w - 2, h].GetJellyType() == type)
        {
            return true;
        }

        if (h >= 2 && field[w, h - 1].GetJellyType() == type && field[w, h - 2].GetJellyType() == type)
        {
            return true;
        }

        return false;
    }

    int GetJellyWidth(Jelly jelly)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (field[i, j].GetInstanceID() == jelly.GetInstanceID())
                {
                    return i;
                }
            }
        }

        Debugger.PrintLog("can't find width of " + jelly.name, LogType.Error);
        return -1;
    }

    int GetJellyHeight(Jelly jelly)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (field[i, j].GetInstanceID() == jelly.GetInstanceID())
                {
                    return j;
                }
            }
        }

        Debugger.PrintLog("can't find height of " + jelly.name, LogType.Error);
        return -1;
    }

    void ReplaceJelly(Jelly a, Jelly b)
    {
        int aWidth = GetJellyWidth(a);
        int bWidth = GetJellyWidth(b);
        int aHeight = GetJellyHeight(a);
        int bHeight = GetJellyHeight(b);

        Jelly temp = null;

        temp = field[aWidth, aHeight];
        field[aWidth, aHeight] = field[bWidth, bHeight];
        field[bWidth, bHeight] = temp;

        a.name = "jelly_( " + bWidth + "," + bHeight + " )";
        b.name = "jelly_( " + aWidth + "," + aHeight + " )";
    }

    IEnumerator CoChangeJellyPos(Jelly a, Jelly b)
    {
        isMatching = true;
        Debug.Log("매칭 엑티브 " + isMatching);
        Vector3 aPos = a.transform.position;
        Vector3 bPos = b.transform.position;
        WaitForSeconds ss = new WaitForSeconds(moveJellyDuration);

        MoveJelly(a, bPos);
        MoveJelly(b, aPos);

        SoundManager.instance.PlayEffect(E_AUDIO_TYPE.jelly);

        yield return ss;
        if (CheckBoard())
        {
            coJellyMove = null;

            if (hintJelly != null)
            {
                hintJelly.SetNormalSprtie();
            }

            ClearMatchedJellies();
            yield return ss;
            RearrangmentBoard();
            //RearrangementBoard();

            Debugger.PrintLog("3 매치가 발생함!!");


            GameManager.instance.AddCombo(1);

            while (CheckBoard())
            {
                yield return ss;
                ClearMatchedJellies();
                yield return ss;
                RearrangmentBoard();

                GameManager.instance.AddCombo(1);
            }
        }
        else
        {
            Debugger.PrintLog("3 매치가 발생하지않음!!");

            MoveJelly(a, aPos);
            MoveJelly(b, bPos);
            ReplaceJelly(a, b);

            yield return ss;
            coJellyMove = null;
        }

        yield return ss;

        isMatching = false;
        Debug.Log("매칭 엑티브 " + isMatching);
        yield return null;
    }

    void MoveJelly(Jelly a, Vector3 changeVec)
    {
        if (a.IsMove)
            return;

        a.IsMove = true;
        a.transform.DOMove(changeVec, moveJellyDuration).OnComplete(() =>
        {
            if (!a.IsActive)
            {
                Destroy(a.gameObject);
            }
            else
            {
                a.IsMove = false;
            }
        });

        //a.transform.localPosition = changeVec;
    }

    void MoveJellyLocal(Jelly a, Vector3 changeVec)
    {
        if (a.IsMove)
            return;
        a.IsMove = true;
        a.transform.DOLocalMove(changeVec, moveJellyDuration).OnComplete(() =>
        {
            if (!a.IsActive)
            {
                Destroy(a.gameObject);
            }
            else
            {
                a.IsMove = false;
            }
        });

        //a.transform.localPosition = changeVec;
    }

    bool CheckFieldIsNull()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (field[i, j].GetJellyType() != E_JELLY_TYPE.Empty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void ChangeJellyPos(Jelly a, int dw, int dh)
    {
        if (coJellyMove != null)
            return;

        int aWidth = GetJellyWidth(a);
        int aHeight = GetJellyHeight(a);
        int bWidth = aWidth + dw;
        int bHeight = aHeight + dh;

        Jelly b = field[bWidth, bHeight];

        if ((bWidth < width && bWidth >= 0) && (bHeight < height && bHeight >= 0) && !b.IsMove)
        {
            ReplaceJelly(a, b);

            coJellyMove = StartCoroutine(CoChangeJellyPos(a, b));
        }
    }

    public bool GetFieldActive()
    {
        return isActive;
    }

    public void SetFieldActive(bool active)
    {
        isActive = active;
        Debug.Log("필드 엑티브 " + active);
    }

    public void UseItem(Jelly jelly)
    {
        GameManager.instance.AddScore(500);
        GameManager.instance.AddCombo(1);

        StartCoroutine(CoUseItem(jelly));
    }

    IEnumerator CoUseItem(Jelly jelly)
    {
        WaitForSeconds ss = new WaitForSeconds(moveJellyDuration);
        isMatching = true;
        Debug.Log("매칭 엑티브 " + isMatching);
        switch (jelly.GetJellyType())
        {
            case E_JELLY_TYPE.Item_Sword:
                int w = GetJellyWidth(jelly);

                jelly.SetJellyType(E_JELLY_TYPE.Empty);

                for (int loop = 0; loop < height; loop++)
                {
                    matchedJellies.Add(field[w, loop]);

                    if (!field[w, loop].IsItem)
                    {
                        field[w, loop].SetJellyType(E_JELLY_TYPE.Empty);
                    }
                }

                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.laser);
                ShowNormalEffect(field[w, height / 2], E_EFFECT_TYPE.LazerY);

                //ClearMatchedJellies();
                yield return ss;
                RearrangmentBoard();

                while (CheckBoard())
                {
                    yield return ss;
                    ClearMatchedJellies();
                    yield return ss;
                    RearrangmentBoard();

                    GameManager.instance.AddCombo(1);
                }
                break;

            case E_JELLY_TYPE.Item_Gun:
                int h = GetJellyHeight(jelly);

                jelly.SetJellyType(E_JELLY_TYPE.Empty);
                for (int loop = 0; loop < width; loop++)
                {
                    matchedJellies.Add(field[loop, h]);

                    if (!field[loop, h].IsItem)
                    {
                        field[loop, h].SetJellyType(E_JELLY_TYPE.Empty);
                    }
                }

                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.laser);
                ShowNormalEffect(field[width / 2, h], E_EFFECT_TYPE.LazerX);

                //ClearMatchedJellies();
                yield return ss;
                RearrangmentBoard();

                while (CheckBoard())
                {
                    yield return ss;
                    ClearMatchedJellies();
                    yield return ss;
                    RearrangmentBoard();

                    GameManager.instance.AddCombo(1);
                }
                break;

            case E_JELLY_TYPE.Item_Bomb:
                int pivot_w = GetJellyWidth(jelly);
                int pivot_h = GetJellyHeight(jelly);

                jelly.SetJellyType(E_JELLY_TYPE.Empty);

                matchedJellies.Add(field[pivot_w, pivot_h]);
                if (pivot_w + 1 < width)
                {
                    matchedJellies.Add(field[pivot_w + 1, pivot_h]);
                }

                if (pivot_h + 1 < height)
                {
                    matchedJellies.Add(field[pivot_w, pivot_h + 1]);
                }

                if (pivot_w - 1 >= 0)
                {
                    matchedJellies.Add(field[pivot_w - 1, pivot_h]);
                }

                if (pivot_h - 1 >= 0)
                {
                    matchedJellies.Add(field[pivot_w, pivot_h - 1]);
                }

                if (pivot_w + 1 < width && pivot_h + 1 < height)
                {
                    matchedJellies.Add(field[pivot_w + 1, pivot_h + 1]);
                }

                if (pivot_w - 1 >= 0 && pivot_h - 1 >= 0)
                {
                    matchedJellies.Add(field[pivot_w - 1, pivot_h - 1]);
                }

                if (pivot_w + 1 < width && pivot_h - 1 >= 0)
                {
                    matchedJellies.Add(field[pivot_w + 1, pivot_h - 1]);
                }

                if (pivot_w - 1 >= 0 && pivot_h + 1 < height)
                {
                    matchedJellies.Add(field[pivot_w - 1, pivot_h + 1]);
                }

                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.bomb);

                foreach (var j in matchedJellies)
                {
                    if (j.IsItem)
                    {
                        continue;
                    }

                    j.SetJellyType(E_JELLY_TYPE.Empty);
                    ShowNormalEffect(j, E_EFFECT_TYPE.Boom);
                }

                //ClearMatchedJellies();
                yield return ss;
                RearrangmentBoard();

                while (CheckBoard())
                {
                    yield return ss;
                    ClearMatchedJellies();
                    yield return ss;
                    RearrangmentBoard();

                    GameManager.instance.AddCombo(1);
                }

                break;
            case E_JELLY_TYPE.Item_TimeBomb:
                if (!GameManager.instance.IsLastPangMode())
                {
                    GameManager.instance.AddGameTime(5);
                }

                int Item_pivot_w = GetJellyWidth(jelly);
                int Item_pivot_h = GetJellyHeight(jelly);

                jelly.SetJellyType(E_JELLY_TYPE.Empty);

                matchedJellies.Add(field[Item_pivot_w, Item_pivot_h]);

                int rightW = Mathf.Min(Item_pivot_w + 2, width - 1);
                int leftW = Mathf.Max(Item_pivot_w - 2, 0);
                int upH = Mathf.Min(Item_pivot_h + 2, height - 1);
                int downH = Mathf.Max(Item_pivot_h - 2, 0);

                for (int i = leftW; i <= rightW; i++)
                {
                    for (int j = downH; j <= upH; j++)
                    {
                        if (field[i, j].IsItem)
                        {
                            continue;
                        }
                        matchedJellies.Add(field[i, j]);
                    }
                }

                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.bomb);

                foreach (var j in matchedJellies)
                {
                    if (j.IsItem)
                    {
                        continue;
                    }

                    j.SetJellyType(E_JELLY_TYPE.Empty);
                    ShowNormalEffect(j, E_EFFECT_TYPE.Boom);
                }

                //ClearMatchedJellies();
                yield return ss;
                RearrangmentBoard();

                while (CheckBoard())
                {
                    yield return ss;
                    ClearMatchedJellies();
                    yield return ss;
                    RearrangmentBoard();

                    GameManager.instance.AddCombo(1);
                }
                break;

            case E_JELLY_TYPE.Item_SuperBomb:

                int super_pivot_w = GetJellyWidth(jelly);
                int super_pivot_h = GetJellyHeight(jelly);

                jelly.SetJellyType(E_JELLY_TYPE.Empty);

                matchedJellies.Add(field[super_pivot_w, super_pivot_h]);

                int rw = Mathf.Min(super_pivot_w + 2, width-1);
                int lw = Mathf.Max(super_pivot_w - 2, 0);
                int  uh = Mathf.Min(super_pivot_h + 2, height-1);
                int dh = Mathf.Max(super_pivot_h - 2, 0);

                for(int i = lw; i <= rw; i++)
                {
                    for(int j = dh; j <= uh; j++)
                    {
                        if (field[i, j].IsItem)
                        {
                            continue;
                        }
                        matchedJellies.Add(field[i, j]);
                    }
                }

                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.bomb);

                foreach (var j in matchedJellies)
                {
                    if (j.IsItem)
                    {
                        continue;
                    }

                    j.SetJellyType(E_JELLY_TYPE.Empty);
                    ShowNormalEffect(j, E_EFFECT_TYPE.Boom);
                }

                //ClearMatchedJellies();
                yield return ss;
                RearrangmentBoard();

                while (CheckBoard())
                {
                    yield return ss;
                    ClearMatchedJellies();
                    yield return ss;
                    RearrangmentBoard();

                    GameManager.instance.AddCombo(1);
                }



                break;
        }
        isMatching = false;
        Debug.Log("매칭 엑티브 " + isMatching);
    }

    public void SetItemFlag(bool flag)
    {
        itemFlag = flag;
    }

    public void StartFeverTime()
    {
        GameManager.instance.SetMakeItem(false);
        StartCoroutine(CoProcFeverTime());
    }

    IEnumerator CoProcFeverTime()
    {
        WaitForSeconds ss = new WaitForSeconds(moveJellyDuration);

        yield return new WaitWhile(() => isMatching);

        UIManager.instance.CheerUP();

        matchedJellies.Clear();

        for (int j = 0; j < 5; j++)
        {
            
            int num = 8;
            for (int loop = 0; loop < num; loop++)
            {
                int a = Random.Range(0, field.GetUpperBound(0));
                int b = Random.Range(0, field.GetUpperBound(1));

                while (matchedJellies.Contains(field[a,b]))
                {
                    a = Random.Range(0, field.GetUpperBound(0) + 1);
                    b = Random.Range(0, field.GetUpperBound(1) + 1);

                    yield return null;
                }

                matchedJellies.Add(field[a, b]);
            }

            ClearMatchedJellies();

            yield return ss;

            RearrangmentBoard();

            yield return ss;

            CheckBoard();

            GameManager.instance.AddCombo(1);
           
        }

        GameManager.instance.StartFeverTimeCount();
        GameManager.instance.SetMakeItem(true);
        UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Default_Blue);

        SetFieldActive(true);

        while (CheckBoard())
        {
            yield return ss;
            ClearMatchedJellies();
            yield return ss;
            RearrangmentBoard();

            GameManager.instance.AddCombo(1);
        }

        /** 
         
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (field[i, j].IsItem)
                {
                    UseItem(field[i, j]);
                    yield return new WaitForSeconds(1.5f);
                }
            }
        }

        while (!CheckFieldIsNull())
        {
            matchedJellies.Clear();
            List<Jelly> popedJellies = new List<Jelly>();

            int num = Random.Range(5, 15);

            for (int loop = 0; loop < num; loop++)
            {
                int i = Random.Range(0, field.GetUpperBound(0));
                int j = Random.Range(0, field.GetUpperBound(1));

                while (field[i, j].GetJellyType() == E_JELLY_TYPE.Empty)
                {
                    i = Random.Range(0, field.GetUpperBound(0) + 1);
                    j = Random.Range(0, field.GetUpperBound(1) + 1);

                    yield return null;
                }

                matchedJellies.Add(field[i, j]);
                popedJellies.Add(field[i, j]);
            }

            ClearMatchedJellies();
            yield return ss;

            foreach (Jelly jelly in popedJellies)
            {
                jelly.gameObject.SetActive(false);
            }

            //yield return ss;
        }

        UIManager.instance.EndCheerUP();

        yield return new WaitForSeconds(2.5f);

        GameManager.instance.SetMakeItem(true);
        RearrangmentBoard();

        while (CheckBoard())
        {
            yield return ss;
            ClearMatchedJellies();
            yield return ss;
            RearrangmentBoard();

            GameManager.instance.AddCombo(1);
        }

        GameManager.instance.StartFeverTimeCount();
        UIManager.instance.SetBottomPanel(E_FIELD_TYPE.Default_Blue);
        SetFieldActive(true);
        **/

    }

    public bool IsItemExist()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; i < height; j++)
            {
                if (field[i, j].IsItem)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool GetFieldMatch()
    {
        return isMatching;
    }

    public void PlayLastPang()
    {
        isActive = false;

        if (hintJelly != null)
        {
            hintJelly.SetNormalSprtie();
        }
        
        StartCoroutine(CoPlayLastPang());
    }

    IEnumerator CoPlayLastPang()
    {
        int count = GameData.LastPangCount;

        yield return new WaitForSeconds(1f);

        UIManager.instance.DoLastPangAnimation();

        yield return new WaitForSeconds(5f);

        while (count > 0)
        {
            if (FindLastPangMatches())
            {
                count--;
                yield return new WaitWhile(() => isMatching);
            }
            else
            {
                yield return new WaitForSeconds(1f);
                break;
            }
        }

        yield return new WaitWhile(() => isMatching);

        while (true)
        {
            bool loop = false;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (field[i, j].IsItem)
                    {
                        loop = true;
                        GameManager.instance.AddScore(500);

                        yield return StartCoroutine(CoUseItem(field[i, j]));
                        yield return new WaitForSeconds(1f);
                    }
                }
            }

            if (!loop) break;

            yield return null;
        }

        yield return new WaitWhile(() => isMatching);
        yield return new WaitForSeconds(1f);

        GameManager.instance.FinishGame();
    }

    public bool FindLastPangMatches()
    {
        for (int loop = 0; loop < width; loop++)
        {
            for (int loop2 = 0; loop2 < height - 2; loop2++)
            {
                Jelly hj = FindHintJellyByWidthSearch(field[loop, loop2], field[loop, loop2 + 1], field[loop, loop2 + 2]);

                if (hj != null)
                {
                    int w = GetJellyWidth(hj);
                    int h = GetJellyHeight(hj);

                    if (w == loop)
                    {
                        hj.MoveJelly(0, -1);
                    }
                    else
                    {
                        if (w > loop)
                        {
                            hj.MoveJelly(-1, 0);
                        }
                        else
                        {
                            hj.MoveJelly(1, 0);
                        }
                    }
                    
                    return true;
                }
            }
        }

        for (int loop = 0; loop < height; loop++)
        {
            for (int loop2 = 0; loop2 < width - 2; loop2++)
            {
                Jelly hj = FindHintJellyByHeightSearch(field[loop2, loop], field[loop2 + 1, loop], field[loop2 + 2, loop]);

                if (hj != null)
                {
                    int w = GetJellyWidth(hj);
                    int h = GetJellyHeight(hj);

                    if (h == loop)
                    {
                        hj.MoveJelly(-1, 0);
                    }
                    else
                    {
                        if (h > loop)
                        {
                            hj.MoveJelly(0, -1);
                        }
                        else
                        {
                            hj.MoveJelly(0, 1);
                        }
                    }

                    return true;
                }
            }
        }
        return false;
    }

    private void Update() 
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameManager.instance.StopGame();
        }   
    }
}
