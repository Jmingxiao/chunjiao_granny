using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeCount : MonoBehaviour
{
    [SerializeField] Text timeText;

   //SerializeField] VoidEventChannel levelStartedEventChannel;
   //SerializeField] VoidEventChannel levelClearedEventChannel;

    float timeCount = 0;
    // Start is called before the first frame update
    bool stop = true;
    
    [SerializeField] float timeStop = 0;
    
    
    void FixedUpdate()
    {
        //if (stop) return;
        if (stop)
        {
            timeCount += Time.fixedDeltaTime;
            timeText.text = System.TimeSpan.FromSeconds(value: timeCount).ToString( @"mm\:ss\:ff");
            if (timeCount >= timeStop)
            {
                LevelClear();
            }
        }
        else
        {
            return;
        }
    }

    void LevelClear()
    {
        timeCount = 0;
        stop = false;
       
    }

    void LevelStart()
    {
        stop = false;
    }


    
}
