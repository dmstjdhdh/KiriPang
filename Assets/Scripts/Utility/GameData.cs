using System;
using System.Linq;
using UnityEngine;


public static class GameData
{
    public static float GameTime { get; set; }

    public static float FeverTime { get; set; }

    public static int Combo { get; set; }

    public static float GameSpeed { get; set; }

    public static int LastPangCount { get; set; }

    public static void Initialize()
    {
        GameTime = 100f;
        FeverTime = 15f;
        Combo = 10;
        GameSpeed = 0.2f;
        LastPangCount = 5;
    }

    public static int[] LoadScores()
    {
        string scoresString = PlayerPrefs.GetString("Player Scores", ",");
        string[] scoreStrings = scoresString.Split(',');

        int[] scores = new int[scoreStrings.Length];

        for (int i = 0; i < scoreStrings.Length; i++)
        {
            if (int.TryParse(scoreStrings[i], out int score))
            {
                scores[i] = score;
            }
            else
            {
                Debug.LogWarning("Failed to parse score: " + scoreStrings[i]);
            }
        }

        Array.Sort(scores);
        Array.Reverse(scores);

        if (scores.Length > 10)
        {
            return scores.Take(10).ToArray();
        }
        else
        {
            return scores;
        }

    }

    public static void SaveScore(int score)
    {
        PlayerPrefs.SetInt("Current Score", score);

        String scores = PlayerPrefs.GetString("Player Scores", "");
        scores += scores.Equals("") ? score : "," + score;

        PlayerPrefs.SetString("Player Scores", scores);
    }
}
