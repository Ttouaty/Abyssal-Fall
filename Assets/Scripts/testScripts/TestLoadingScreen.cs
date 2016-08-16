using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TestLoadingScreen : PersistantGenericSingleton<TestLoadingScreen>
{
    public Image LoadBarProgress;
    public Localizator.LocalizedText LoadingStatusText;

    public void SetStateText(Localizator.EFragmentsEnum fragment)
    {
        LoadingStatusText.SetText(fragment);
    }
}
