using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Hammer : MonoBehaviour {

	public AudioClip OnHitPlayer;
	public AudioClip OnHitObstacle;

	private DamageDealer _playerShooting;
	[SerializeField]
	private int _speed = 20;

	private Rigidbody _rigidB;
	private AudioSource _audioSource;

	void Start () {
		_audioSource = GetComponent<AudioSource>();
		_rigidB = GetComponent<Rigidbody>();
		GetComponent<Collider>().isTrigger = true;
	}

	public void Launch(Vector3 Position, Vector3 Direction, DamageDealer Shooter)
	{
		if (_rigidB == null)
			_rigidB = GetComponent<Rigidbody>();

		gameObject.SetActive(true);
		_playerShooting = Shooter;

		//transform.localScale = Vector3.one * 2;
		transform.position = Position;
		transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);

		_rigidB.velocity = Direction.normalized * _speed;
		StartCoroutine(DelayStop());

	}


	protected void Stop()
	{
		_rigidB.velocity = Vector3.zero;
		GameObjectPool.AddObjectIntoPool(gameObject);
	}

	private void OnTriggerEnter(Collider colli)
	{
		if (colli.GetComponent<IDamageable>() != null)
		{
			if (colli.gameObject.GetInstanceID() == _playerShooting.PlayerRef.Controller.gameObject.GetInstanceID())
				return;

			colli.GetComponent<IDamageable>().Damage(Quaternion.FromToRotation(Vector3.right, _rigidB.velocity.ZeroY()) * _playerShooting.PlayerRef.Controller._characterData.SpecialEjection.Multiply(Axis.x, _playerShooting.PlayerRef.Controller._characterData.CharacterStats.strength),
				transform.position,
				_playerShooting.PlayerRef.Controller._characterData.SpecialDamageData);
			// explosion particules
			_audioSource.PlayOneShot(OnHitPlayer);
			//Stop();
		}
		else if(colli.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			//_audioSource.PlayOneShot(OnHitObstacle);
			//Stop();
		}
	}

	private IEnumerator DelayStop()
	{
		yield return new WaitForSeconds(3f);
		Stop();
	}
}
