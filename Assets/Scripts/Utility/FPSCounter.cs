using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    int screenLongSide;
    Rect boxRect;
    GUIStyle style = new GUIStyle();

    int frameCount;
    float elapsedTime;
    double frameRate;
    
    void Awake()
    {
        //Application.targetFrameRate = 60;

        //DontDestroyOnLoad(gameObject);
        //UpdateUISize();
    }

    void Start()
    {
        UpdateUISize();
    }

    void Update()
    {
        frameCount++;
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime > 0.5f)
        {
            frameRate = System.Math.Round(frameCount / elapsedTime, 1, System.MidpointRounding.AwayFromZero);
            frameCount = 0;
            elapsedTime = 0;
        }
    }

    void UpdateUISize()
    {
        screenLongSide = Mathf.Max(Screen.width, Screen.height);
        var rectLongSide = screenLongSide / 8;
        boxRect = new Rect(1, 1, rectLongSide, rectLongSide / 3);
        style.fontSize = (int)(screenLongSide / 30);
        style.normal.textColor = Color.blue;
    }

    void OnGUI()
    {
        GUI.Box(boxRect, "");
        GUI.Label(boxRect, " " + frameRate + "fps", style);
    }
}
