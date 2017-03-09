using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileAutoShooter : MonoBehaviour
{
	private TimeCooldown _shootTimer;

	public string _projectilePoolName = "Hammer";
	public float Interval = 2;
	[Space()]
	public string DamagerName = "Environnement";
	public float StunInflicted = 0.3f;
	private DamageData _ownDamageData;

	void Start()
	{
		_ownDamageData = new DamageData();
		_ownDamageData.StunInflicted = StunInflicted;
		_ownDamageData.Dealer = new DamageDealer();
		_ownDamageData.Dealer.InGameName = DamagerName;
		_ownDamageData.Dealer.ObjectRef = gameObject;

		_shootTimer = new TimeCooldown(this);
		_shootTimer.onFinish = () =>
		{
			_shootTimer.Set(Interval);
			GameObjectPool.GetAvailableObject(_projectilePoolName).GetComponent<ABaseProjectile>().Launch(transform.position, transform.forward, _ownDamageData, gameObject.GetComponent<NetworkIdentity>().netId);
		};
		_shootTimer.Set(Interval);
	}
}
