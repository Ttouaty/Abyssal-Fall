using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

//Automaticaly added to playercontrollers, this class just serves as a identifier for harmful objects.
public class DamageDealer
{
	public Player PlayerRef;
	public string InGameName;
	public Image Icon;
}

[Serializable]
public class DamageData
{
	[HideInInspector]
	public DamageDealer Dealer;
	public AttackType AttackTypeUsed;
	public float StunInflicted;
	public bool IsParryable;
	[HideInInspector]
	public ABaseProjectile Projectile;

	public DamageData SetProjectile(ABaseProjectile newProjo)
	{
		Projectile = newProjo;
		return this;
	}
}
