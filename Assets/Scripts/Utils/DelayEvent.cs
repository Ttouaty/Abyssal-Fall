using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DelayEvent : MonoBehaviour
{
	public float Delay = 1;
	public bool AutoStart = true;
	[Space]
	public bool Repeat = false;
	[Space]
	public UnityEvent Callback;

	void Start()
	{
		if(AutoStart)
			Activate();
	}

	public void Activate()
	{
		if(Repeat)
			InvokeRepeating("LaunchCallback", 0, Delay);
		else
			Invoke("LaunchCallback", Delay);
	}

	public void Stop()
	{
		CancelInvoke("LaunchCallback");
	}

	void LaunchCallback()
	{
		Callback.Invoke();
	}
}
