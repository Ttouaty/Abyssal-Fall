using UnityEngine;
using System.Collections;

public class VikingController : PlayerController
{
	protected override void SpecialAction()
	{
		_animator.SetTrigger("Throw");
		_characterProp.PropRenderer.enabled = false;
		GameObject hammer = GameObjectPool.GetAvailableObject("Hammer");

		hammer.GetComponent<Hammer>().Launch(transform.position + transform.forward, transform.forward, _characterData.SpecialDamageData);
		_characterData.SoundList["OnSpecialActivate"].Play(hammer);
		if(ArenaManager.Instance != null)
			hammer.transform.parent = ArenaManager.Instance.SpecialsRoot;
    }
}
