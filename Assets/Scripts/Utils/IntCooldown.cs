using UnityEngine;
using System.Collections;

public class IntCooldown : CoolDown {

	/// <summary>
	/// time between Progress (1 = 1 progress/s, 60 => 60 progress/s).
	/// onProgress() may vary upon framerate.
	/// </summary>
	public float Interval = 1; 
	private int valueLeft = 0;

	private float _nextTimeStamp = 0;

	public IntCooldown(MonoBehaviour linkedParent):base(linkedParent)
	{
	}

	public override void Start()
	{
		base.Start();
		_nextTimeStamp = Time.time;
	}

	protected override void Update()
	{
		if (Interval / Time.deltaTime < 1)
			Debug.LogWarning("IntCoolDown linked to object " + parent.name + " has skipped a OnProgress() due to framerate too low.");

		if (_nextTimeStamp <= Time.time)
		{
			if (valueLeft == 0 && !isFinished)
			{
				isFinished = true;
				onFinish();
			}
			else
			{
				_nextTimeStamp = Time.time + Interval;
				--valueLeft;
				onProgress();
			}
		}
	}

	public void Add(int value)
	{
		if (_nextTimeStamp < Time.time)
			_nextTimeStamp = Time.time + Interval;
		valueLeft += value;
		isFinished = false;
	}

	public void Set(int value)
	{
		_nextTimeStamp = Time.time + Interval;
		valueLeft = value;
		isFinished = false;
	}
}
