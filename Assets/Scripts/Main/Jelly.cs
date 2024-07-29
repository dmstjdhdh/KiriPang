
using UnityEngine;
using UnityEngine.UI;

public class Jelly : MonoBehaviour, IFieldObject
{
    [SerializeField]
    E_JELLY_TYPE type;
    
    public bool IsActive { get; set; }
    public bool IsMove { get; set; }
    public bool IsItem { get; set; }

    public Sprite[] jellySprites;

    private Vector2 firstClickPosition;
    private Vector2 finalTouchPosition;

    private float swipeResist = 1f;

    Image jellyImage;

    private void Awake() 
    {
        jellyImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        IsActive = true;
        IsMove = false;

        SetNormalSprtie();
    }

    public E_JELLY_TYPE GetJellyType()
    {
        return type;
    }

    public void SetJellyType(E_JELLY_TYPE ty)
    {
        type = ty;
    }

    public void Pop()
    {
        GameManager.instance.AddScore(100);
        SetBombSprtie();
    }

    void CalculateAngle(float swipeAngle)
    {
        int degreeWidth = 0;
        int degreeHeight = 0;

        if (swipeAngle > -45f && swipeAngle <= 45f)
        {
            degreeWidth = 1;
            degreeHeight = 0;
        }
        else if (swipeAngle > 45f && swipeAngle <= 135f)
        {
            degreeWidth = 0;
            degreeHeight = 1;
        }
        else if (swipeAngle > 135f || swipeAngle <= -135f)
        {
            degreeWidth = -1;
            degreeHeight = 0;
        }
        else if (swipeAngle < -45f && swipeAngle >= -135f)
        {
            degreeWidth = 0;
            degreeHeight = -1;
        }

        if(!IsMove && !IsItem)
        {
            //Debugger.PrintLog("[width/height] :" + degreeWidth + " /" + degreeHeight);
            FieldManager.instance.ChangeJellyPos(this, degreeWidth, degreeHeight);
        }
    }

    void OnMouseUp()
    {
        if (!FieldManager.instance.GetFieldActive() || FieldManager.instance.GetFieldMatch())
            return;

        finalTouchPosition = Input.mousePosition;

        if (IsItem)
        {
            float dist = Vector3.Distance(firstClickPosition, finalTouchPosition);

            if (dist < 140f)
            {
                FieldManager.instance.UseItem(this);
            }
        }
        else
        {
            if (Mathf.Abs(finalTouchPosition.y - firstClickPosition.y) > swipeResist ||
              Mathf.Abs(finalTouchPosition.x - firstClickPosition.x) > swipeResist)
            {
                float swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstClickPosition.y, finalTouchPosition.x - firstClickPosition.x) * 180f / Mathf.PI;
                CalculateAngle(swipeAngle);
            }
            else
            {
                SoundManager.instance.PlayEffect(E_AUDIO_TYPE.touch);
            }
        }
    }

    private void OnMouseDown()
    {
        firstClickPosition = Input.mousePosition;
    }
    
    public void MoveJelly(int width, int height)
    {
        FieldManager.instance.ChangeJellyPos(this, width, height);
    }

    public void SetHintSprtie()
    {
        if (jellySprites.Length <= 0) return;

        jellyImage.sprite = jellySprites[2];
    }

    public void SetBombSprtie()
    {
        if (jellySprites.Length <= 0) return;

        jellyImage.sprite = jellySprites[1];
    }

    public void SetNormalSprtie()
    {
        if (jellySprites.Length <= 0) return;
        
        jellyImage.sprite = jellySprites[0];
    }
}

