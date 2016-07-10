using UnityEngine;
using System.Collections;

public class IntCooldown : CoolDown {
	private int valueLeft = 0;

	public IntCooldown(MonoBehaviour linkedParent):base(linkedParent)
	{
	}

	protected override void Update()
	{
		if (valueLeft == 0 && !isFinished)
		{
			isFinished = true;
			onFinish();
		}
		else
		{ 
			--valueLeft;
			onProgress();
		}
	}

	public void Add(int value)
	{
		valueLeft += value;
		isFinished = false;
	}

	public void Set(int value)
	{
		valueLeft = value;
		isFinished = false;
	}
}
