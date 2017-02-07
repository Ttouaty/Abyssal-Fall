using UnityEngine;
using System.Collections;

public class VikingController : PlayerController
{
	protected override void SpecialAction()
	{
		_characterProp.PropRenderer.enabled = false;
		CmdLaunchProjectile("Hammer", transform.position + transform.forward, transform.forward);
    }
}
