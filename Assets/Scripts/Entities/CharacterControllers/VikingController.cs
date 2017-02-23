using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class VikingController : PlayerController
{
	protected override void SpecialAction()
	{
		CmdHideHammer();
		CmdLaunchProjectile("Hammer", transform.position + transform.forward, transform.forward);
    }

	[Command]
	protected void CmdHideHammer()
	{
		RpcHideHammer();
	}

	[ClientRpc]
	protected void RpcHideHammer()
	{
		_characterProp.PropRenderer.gameObject.SetActive(false);
	}
}
