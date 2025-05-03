using System.Collections;
using System.Collections.Generic;
using ACG;
using UnityEngine;

public class StartMenuUIListener : MObject
{
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.mute = false;
        PlayerPrefs.SetInt("Mute", 0);
        UnRegistListener<IconType>(EvenDefine.StartMenuUI, OnStartMenuUI); 
        RegistListener<IconType>(EvenDefine.StartMenuUI, OnStartMenuUI);  
    }

    void OnStartMenuUI(IconType iconType)
    {
        switch (iconType)
        {
            case IconType.Trigger:
                Debug.Log("Start Menu UI Triggered");
                break;
            case IconType.Boolean:
                audioSource.mute = !audioSource.mute;
                PlayerPrefs.SetInt("Mute", audioSource.mute ? 1 : 0);
                break;
            default:
                break;
        }
    }
}
