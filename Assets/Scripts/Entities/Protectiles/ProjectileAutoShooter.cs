using UnityEngine;
using System.Collections;

public class ProjectileAutoShooter : MonoBehaviour
{
	private TimeCooldown _shootTimer;

	public string _projectilePoolName = "Hammer";
	public float Interval = 2;

	[Tooltip("Usually SO_Environnement here.")]
	public SO_Character CharacterDamaging;
	private DamageDealer OwnDamageDealer;

	void Start()
	{
		if (CharacterDamaging == null)
			return;

		OwnDamageDealer = new DamageDealer();
		OwnDamageDealer.InGameName = gameObject.name;
		OwnDamageDealer.PlayerRef = new Player();
		OwnDamageDealer.PlayerRef.Controller = gameObject.AddComponent<PlayerController>();
		OwnDamageDealer.PlayerRef.Controller._characterData = CharacterDamaging;
		OwnDamageDealer.PlayerRef.Controller.GetComponent<Rigidbody>().isKinematic = true;
		OwnDamageDealer.PlayerRef.Controller._isNPC = true;

		_shootTimer = new TimeCooldown(this);
		_shootTimer.onFinish = () =>
		{
			_shootTimer.Set(Interval);
			GameObjectPool.GetAvailableObject(_projectilePoolName).GetComponent<ABaseProjectile>().Launch(transform.position, transform.forward, OwnDamageDealer);
		};
		_shootTimer.Set(Interval);
	}

	void Update()
	{

	}
}
