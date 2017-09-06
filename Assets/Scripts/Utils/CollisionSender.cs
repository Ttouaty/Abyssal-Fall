using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Collider))]
public class CollisionSender : MonoBehaviour
{
	public UnityEventCollision OnCollisionEnterEvent;
	public UnityEventCollision OnCollisionStayEvent;
	public UnityEventCollision OnCollisionExitEvent;

	void OnCollisionEnter(Collision colli) { OnCollisionEnterEvent.Invoke(colli); }
	void OnCollisionStay(Collision colli) { OnCollisionStayEvent.Invoke(colli); }
	void OnCollisionExit(Collision colli) { OnCollisionExitEvent.Invoke(colli); }

}
[Serializable]
public class UnityEventCollision : UnityEvent<Collision> { }