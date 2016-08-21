using UnityEngine;
using System.Collections;

public class VikingController : PlayerController
{
	protected override void SpecialAction()
	{
		_animator.SetTrigger("Throw");
		_audioSource.PlayOneShot(_characterData.SoundList.OnSpecialActivate);
		_specialCooldown.Set(_characterData.CharacterStats.specialCooldown);
		_playerProp.PropRenderer.enabled = false;
		GameObject hammer = GameObjectPool.GetAvailableObject("Hammer");
        hammer.GetComponent<Hammer>().Launch(transform.position + transform.forward, transform.forward, _playerRef.PlayerNumber);
        hammer.transform.parent = ArenaManager.Instance.SpecialsRoot;
    }
}
