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

		foreach (Transform child in transform)
		{
			child.SetParent(PanelRefs["Main"].transform, false);
			child.localPosition = Vector3.zero;
		}
		PanelRefs["Main"].Open();

		SetGlobalDelay(100000000);// deactivated by inputlistener
		yield return new WaitUntil(() => PanelRefs["Main"].transform.Find("Title").GetComponentInChildren<Animator>());
		PanelRefs["Main"].transform.Find("Title").GetComponentInChildren<Animator>().SetTrigger("wait");

		SetGlobalDelay(100000000);// deactivated by inputlistener
	}
}
