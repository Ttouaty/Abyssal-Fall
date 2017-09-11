﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

//Automaticaly added to playercontrollers, this class just serves as a identifier for harmful objects.
public class DamageDealer
{
	public Player PlayerRef;
	private GameObject _objectRef;
	public int InstanceId;
	public GameObject ObjectRef
	{
		get { return _objectRef; }
		set { InstanceId = value.GetInstanceID(); _objectRef = value; }
	}
	public DamageData DamageData;
	public string InGameName;
	public Sprite Icon;
}

[Serializable]
public struct DamageData
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

	public DamageData Copy(DamageDealer newDealer)
	{
		DamageData newDamageData = new DamageData();
		newDamageData.Dealer = newDealer;
		newDamageData.Dealer.ObjectRef = newDealer.ObjectRef;
		newDamageData.Dealer.PlayerRef = newDealer.PlayerRef;
		newDamageData.Dealer.InstanceId = newDealer.InstanceId;
		newDamageData.Dealer.DamageData = newDamageData;
		newDamageData.Dealer.Icon = newDealer.Icon;
		newDamageData.AttackTypeUsed = AttackTypeUsed;
		newDamageData.StunInflicted = StunInflicted;
		newDamageData.IsParryable = IsParryable;
		newDamageData.Projectile = Projectile;

		return newDamageData;
	}
}
