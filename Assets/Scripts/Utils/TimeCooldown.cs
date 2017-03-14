using UnityEngine;
using System.Collections;

public class TimeCooldown : CoolDown{
	private float targetTimeStamp = 0;

	public float TimeLeft
	{
		get 
		{
			return Mathf.Clamp(targetTimeStamp - Time.time, 0, 32768);
		}
	}

	public TimeCooldown(MonoBehaviour linkedParent) : base(linkedParent) { }

	protected override void Update()
	{
		if (targetTimeStamp <= Time.time)
		{
			if (!isFinished)
			{
				isFinished = true;
				if (onFinish != null)
					onFinish();
			}
		}
		else if(onProgress != null)
			onProgress();
	}

	public void Add(float value)
	{
		if (targetTimeStamp < Time.time)
			targetTimeStamp = Time.time;
		
		targetTimeStamp += value;
		isFinished = false;
	}

	public void Set(float value)
	{
		targetTimeStamp = Time.time + value;
		isFinished = value == 0;
	}

}
