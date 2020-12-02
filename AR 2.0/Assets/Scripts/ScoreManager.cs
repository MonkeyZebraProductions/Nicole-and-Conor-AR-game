using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int score, currentScore,topScore, multiplyer;
    private bool _increaseScore,_comboStart,_increaseMultiplyer;
    public TMP_Text Combo, CurrentScore, TopCombo;
    // Start is called before the first frame update
    void Start()
    {
        Combo.text = "Combo: " + score;
        CurrentScore.text="Current Score: " + currentScore;
        TopCombo.text = "Top Score" + topScore;
        _increaseMultiplyer = true;
    }

    // Update is called once per frame
    void Update()
    {
        Combo.text = "Combo: " + score + "x" + multiplyer;
        if(_increaseScore==true)
        {
            score += 1;
        }
        Debug.Log(_increaseMultiplyer);
        if (_comboStart==false)
       
        {
            
            currentScore += score;
            if(score>topScore)
            {
                topScore = score;
            }
            score = 0;
            multiplyer = 0;
        }
    }

    public void ScoreIncrease()
    {
        _increaseScore = true;
    }
    public void ComboStart()
    {
        _comboStart = true;
    }

    public void MultiplierInrease()
    {
        if(_increaseMultiplyer)
        {
            multiplyer += 1;
            _increaseMultiplyer = false;
        }
    }

    public void ScoreStop()
    {
        _increaseScore = false;
        _increaseMultiplyer = true;

    }
    public void ComboStop()
    {
        _comboStart = false;
    }
}
