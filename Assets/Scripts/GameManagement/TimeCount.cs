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
    bool stop = false;
    
    [SerializeField] float timeStop = 0;
    
    
    void Update()  // 改为 Update
    {
        if (!stop)
        {
            timeCount += Time.deltaTime;  // 使用 deltaTime
            timeText.text = System.TimeSpan.FromSeconds(timeCount).ToString(@"mm\:ss\:ff");
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
        stop = true;
       
    }

    void LevelStart()
    {
        stop = false;
    }

    public void LevelStop()
    {
        stop = true;
    }


    
}
