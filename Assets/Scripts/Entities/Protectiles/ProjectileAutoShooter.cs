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
		OwnDamageDealer.InGameName = CharacterDamaging.IngameName;
		
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
