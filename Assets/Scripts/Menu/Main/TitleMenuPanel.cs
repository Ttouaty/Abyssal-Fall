using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TitleMenuPanel : MenuPanelNew
{
	public override void FinishedEntering()
	{
		base.FinishedEntering();
		StartCoroutine(WaitForCameraMovement());
	}

	IEnumerator WaitForCameraMovement()
	{
		PanelRefs["Main"].Open();
		yield return new WaitUntil(() => PanelRefs["Main"].transform.Find("Title").GetComponentInChildren<Animator>());
		yield return new WaitUntil(() => InputEnabled);
		foreach (Transform child in transform)
		{
			child.SetParent(PanelRefs["Main"].transform, false);
		}
		PanelRefs["Main"].transform.Find("Title").GetComponentInChildren<Animator>().SetTrigger("wait");

		GlobalInputDelay = 100000000; // deactivated by inputlistener
	}

}
