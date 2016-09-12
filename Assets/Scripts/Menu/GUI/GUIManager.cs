using UnityEngine;
using System;
using System.Collections;

public class GUIManager : GenericSingleton<GUIManager>
{
	public GUITimer			Timer;
	public GUIRoundCount	RoundCount;

	public override void Init ()
	{
		base.Init();
		Timer.gameObject.SetActive(false);
		RoundCount.gameObject.SetActive(false);
	}

	public void RunTimer (float duration)
	{
		Timer.gameObject.SetActive(true);
		Timer.Run(duration);
	}

	public void StopTimer ()
	{
		Timer.gameObject.SetActive(false);
		Timer.Stop();
	}

	public void RunRoundCount ()
	{
		RoundCount.gameObject.SetActive(true);
		UpdateRoundCount(0);
	}

	public void UpdateRoundCount (int round)
	{
		RoundCount.SetRound(round);
	}

	public void StopRoundCount ()
	{
		RoundCount.gameObject.SetActive(false);
	}
}
