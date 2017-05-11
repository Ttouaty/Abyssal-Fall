using UnityEngine;
using System.Collections;

public class NinjaController : PlayerController {

	public int ShurikenNumber = 3;
	public float AngleSpread = 90; 

	protected override void SpecialAction()
	{
		//_animator.SetTrigger("Special");
		_characterData.SoundList["OnSpecialActivate"].Play(gameObject); // need trigger across players

		Vector3 newDirection = Quaternion.AngleAxis(- AngleSpread * 0.5f, Vector3.up) * transform.forward;
		for (int i = 0; i < ShurikenNumber; i++)
		{
			ThrowShuriken(newDirection);
			newDirection = Quaternion.AngleAxis(AngleSpread / (ShurikenNumber - 1), Vector3.up) * newDirection;
		}
	}

	private void ThrowShuriken(Vector3 direction)
	{
		CmdLaunchProjectile("Shuriken", transform.position + transform.forward, direction);
	}
}
