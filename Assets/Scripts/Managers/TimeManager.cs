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
    public static TimeEvent OnPause
    {
        get { return Instance.onPause; }
    }
    public static TimeEvent OnResume
    {
        get { return Instance.onResume; }
    }
    public static TimeEvent OnTimeScaleChange
    {
        get { return Instance.onTimeScaleChange; }
    }

    public static void Pause()                      { TimeScale = 0.0f; OnPause.Invoke(TimeScale); }
    public static void Resume()                     { TimeScale = 1.0f; OnResume.Invoke(TimeScale); }
    public static void SetTimeScale(float value)    { TimeScale = value; OnTimeScaleChange.Invoke(TimeScale); }


    [HideInInspector] public TimeEvent onPause;
    [HideInInspector] public TimeEvent onResume;
    [HideInInspector] public TimeEvent onTimeScaleChange;
    [HideInInspector] public float timeScale              = 1.0f;
    [HideInInspector] public float deltaTime              = 0.0f;
    [HideInInspector] public float currentTime            = 0.0f;

    void Update ()
    {
        deltaTime = Time.deltaTime * timeScale;
        currentTime += deltaTime;
    }
}
