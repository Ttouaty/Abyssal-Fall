using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Collider))]
public class TriggerSender : MonoBehaviour
{
	public UnityEventTrigger OnTriggerEnterEvent;
	public UnityEventTrigger OnTriggerStayEvent;
	public UnityEventTrigger OnTriggerExitEvent;

	void OnTriggerEnter(Collider colli) { OnTriggerEnterEvent.Invoke(colli); }
	void OnTriggerStay(Collider colli) { OnTriggerStayEvent.Invoke(colli); }
	void OnTriggerExit(Collider colli) { OnTriggerExitEvent.Invoke(colli); }

}
[Serializable]
public class UnityEventTrigger : UnityEvent<Collider> { }
