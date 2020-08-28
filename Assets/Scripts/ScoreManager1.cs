using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager1 : MonoBehaviour
{
    public static ScoreManager1 instance;
    public TextMeshProUGUI text;
    public static int score;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void ChangeScore(int coinValue)
    {
        score += coinValue;
        text.text = "x " + score.ToString();
        // This is the number of coins the level has.
        if(score >= 4)
        {
            text.text = "x 4"; 
        }


    }
}
