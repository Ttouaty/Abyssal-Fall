using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class TimeEvent : UnityEvent<float> { }

public class TimeManager : GenericSingleton<TimeManager>
{
    public static float DeltaTime
    {
        get { return Instance.deltaTime; }
    }
    public static float CurrentTime
    {
        get { return Instance.currentTime; }
    }
    public static float TimeScale
    {
        get { return Instance.timeScale; }
        set { Instance.timeScale = value; }
    }
    public static bool IsPaused
    {
        get { return TimeScale == 0.0f; }
    }

    public static void Pause()								{ TimeScale = 0.0f; Instance.OnPause.Invoke(TimeScale); }
    public static void Resume()								{ TimeScale = 1.0f; Instance.OnResume.Invoke(TimeScale); }
    public static void SetTimeScale(float value)			{ TimeScale = value; Instance.OnTimeScaleChange.Invoke(TimeScale); }


    [HideInInspector] public TimeEvent OnPause;
    [HideInInspector] public TimeEvent OnResume;
    [HideInInspector] public TimeEvent OnTimeScaleChange;
    [HideInInspector] public float timeScale				= 1.0f;
    [HideInInspector] public float deltaTime				= 0.0f;
    [HideInInspector] public float currentTime				= 0.0f;

    void Update ()
    {
        deltaTime = Time.deltaTime * timeScale;
        currentTime += deltaTime;
    }
}
