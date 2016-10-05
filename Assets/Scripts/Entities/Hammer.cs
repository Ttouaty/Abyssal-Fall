using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Hammer : MonoBehaviour {

	public AudioClip OnHitPlayer;
	public AudioClip OnHitObstacle;

	private DamageDealer _playerShooting;
	[SerializeField]
	private int _speed = 10;
	[SerializeField]
	private float _stunInflicted = 0.3f;

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

		transform.localScale = Vector3.one * 2;
		transform.position = Position;
		transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);

		_rigidB.velocity = Direction.normalized * _speed;
	}


	protected void Stop()
	{
		if (this.gameObject.activeSelf)
		{
			this._rigidB.velocity = Vector3.zero;
			GameObjectPool.AddObjectIntoPool(gameObject);
		}
	}

	private void OnTriggerEnter(Collider Collider)
	{
		if (Collider.tag == "Player")
		{
			if (Collider.gameObject.GetInstanceID() == _playerShooting.gameObject.GetInstanceID() || Collider.GetComponent<PlayerController>()._isInvul)
				return;

			Collider.GetComponent<PlayerController>().Damage(Quaternion.FromToRotation(Vector3.right, _rigidB.velocity.ZeroY()) * _playerShooting.PlayerRef.Controller._characterData.Dash.force, _stunInflicted, _playerShooting);
			// explosion particules
			_audioSource.PlayOneShot(OnHitPlayer);
			this.Stop();
		}
		else if(Collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
		{
			_audioSource.PlayOneShot(OnHitObstacle);
			this.Stop();
		}
	}

	private void OnBecameInvisible()
	{
		if (this.gameObject.activeInHierarchy)
			StartCoroutine("delayStop");
	}

	private IEnumerator delayStop()
	{
		yield return new WaitForSeconds(0.5f);
		if (this.gameObject.activeSelf)
			if (!GetComponent<Renderer>().isVisible)
				this.Stop();
	}
}
