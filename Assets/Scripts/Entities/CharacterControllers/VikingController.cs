using UnityEngine;
using System.Collections;

public class VikingController : PlayerController
{
	protected override void SpecialAction()
	{
		_animator.SetTrigger("Special");
		_characterProp.PropRenderer.enabled = false;
		CmdLaunchProjectile("Hammer", transform.position + transform.forward, transform.forward);
    }
}
