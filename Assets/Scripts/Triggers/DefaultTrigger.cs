using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum CollisionType
{
	Enter,
	Stay,
	Exit
}
[RequireComponent(typeof(BoxCollider))]
public class DefaultTrigger : MonoBehaviour {
	public bool CanRepeat = false;
	public CollisionType TriggerType;

	[Space()]
	public UnityEvent EventToTrigger;

	void Start()
	{
		if (!GetComponent<Collider>().isTrigger)
		{
			Debug.Log("DefaultTrigger \"" + gameObject.name+"\" had a non trigger collider,\nforcing isTrigger = true!");
			GetComponent<Collider>().isTrigger = true;
		}

		if (gameObject.layer != LayerMask.NameToLayer("PlayerTrigger"))
		{
			Debug.LogWarning("Warning DefaultTrigger \"" + gameObject.name + "\" is not for player only.\nTo Make it player only, set its gameobject layer to PlayerTrigger.");
		}
				
	}

	void OnTriggerEnter()
	{
		if (TriggerType == CollisionType.Enter)
			LaunchCallBack();
	}

	void OnTriggerStay()
	{
		if (TriggerType == CollisionType.Stay)
			LaunchCallBack();
	}

	void OnTriggerExit()
	{
		if (TriggerType == CollisionType.Exit)
			LaunchCallBack();
	}

	void LaunchCallBack()
	{
		if (!CanRepeat)
			GetComponent<Collider>().enabled = false;

		EventToTrigger.Invoke();
	}
}
