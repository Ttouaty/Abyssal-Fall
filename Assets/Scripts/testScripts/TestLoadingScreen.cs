using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TestLoadingScreen : PersistantGenericSingleton<TestLoadingScreen>
{
    public Image LoadBarProgress;
    public LocalizedText LoadingStatusText;

    public void SetStateText(string lang)
    {
        LoadingStatusText.SetText(lang);
    }
}
