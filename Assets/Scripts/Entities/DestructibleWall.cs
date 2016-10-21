using UnityEngine;
using System;

public enum AttackType
{
	Dash,
	Special, 
	Any
}

[RequireComponent(typeof(BoxCollider))]
public class DestructibleWall : MonoBehaviour, IDamageable
{
	public AttackType AttackTypeToListen;

	//void OnCollisionEnter(Collision colli)
	//{
	//	if (AttackTypeToListen == AttackType.Dash)
	//	{
	//		if (colli.gameObject.GetComponent<PlayerController>() != null)
	//		{
	//			if (colli.gameObject.GetComponent<PlayerController>()._characterData.Dash.inProgress)
	//				DestroyWall();
	//		}
	//	}
	//	else if (AttackTypeToListen == AttackType.Special)
	//	{
			
	//	}
	//}

	public void DestroyWall(Vector3 position, float force)
	{
		Rigidbody[] childrenRigiB = GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < childrenRigiB.Length; i++)
		{
			childrenRigiB[i].isKinematic = false;
			childrenRigiB[i].useGravity = true;
			if (childrenRigiB[i].GetComponent<Collider>() != null)
				childrenRigiB[i].GetComponent<Collider>().enabled = true;
			childrenRigiB[i].AddExplosionForce(force, position, GetComponent<BoxCollider>().bounds.extents.magnitude * 2);

			childrenRigiB[i].gameObject.AddComponent<FadeDestroy>().Activate(5);
		}
		GetComponent<Collider>().enabled = false;
	}

	
	public void Damage(Vector3 direction, Vector3 impactPoint, DamageData Sender)
	{
		if (Sender.AttackTypeUsed == AttackTypeToListen || AttackTypeToListen == AttackType.Any)
		{
			DestroyWall(impactPoint, direction.magnitude * 50);
		}
	}
}
