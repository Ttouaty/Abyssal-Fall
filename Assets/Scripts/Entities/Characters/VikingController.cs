using UnityEngine;
using System.Collections;

public class VikingController : PlayerController
{
	protected override void SpecialAction()
	{
		_animator.SetTrigger("Throw");
		_audioSource.PlayOneShot(_characterData.SoundList.OnSpecialActivate);
		_characterProp.PropRenderer.enabled = false;
		GameObject hammer = GameObjectPool.GetAvailableObject("Hammer");
        hammer.GetComponent<Hammer>().Launch(transform.position + transform.forward, transform.forward, _dmgDealerSelf);
		if(ArenaManager.Instance != null)
			hammer.transform.parent = ArenaManager.Instance.SpecialsRoot;
    }
}
