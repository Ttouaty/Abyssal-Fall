using UnityEngine;
using System.Collections;

public class VikingController : PlayerController {

	protected override void SpecialAction()
	{
		_animator.SetTrigger("Throw");
		_audioSource.PlayOneShot(_playerData.SoundList.OnSpecialActivate);
		_specialCooldown.Set(_playerData.CharacterStats.specialCooldown);
		_playerProp.PropRenderer.enabled = false;
		GameObjectPool.GetAvailableObject("Hammer").GetComponent<Hammer>().Launch(transform.position + transform.forward, transform.forward, PlayerNumber);
	}

}
