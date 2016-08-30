using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReturnButton : InputButton {

	private Image CircleFill;
	private Image SpriteB;

	protected override void Start()
	{
		base.Start();
		
		CircleFill = transform.FindChild("Fill").GetComponent<Image>();
		SpriteB = transform.FindChild("B").GetComponent<Image>();
	}

	protected override void Update()
	{
		base.Update();

		CircleFill.fillAmount = Mathf.Lerp(CircleFill.fillAmount, _timeHeld / TimeToHold, 0.5f);
		if (_timeHeld == 0)
			CircleFill.fillAmount = 0;
	}

	protected override void LaunchCallback()
	{
		base.LaunchCallback();
	}
}
