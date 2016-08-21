using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class LoadingScreen : GenericSingleton<LoadingScreen>
{
    public Image LoadBarProgress;
    public Localizator.LocalizedText LoadingStatusText;

    public void SetStateText(string fragment)
    {
        LoadingStatusText.SetText(fragment);
    }
}
