using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Hammer : MonoBehaviour {

	private int _playerNumber;
	[SerializeField]
	private int _speed = 10;
	[SerializeField]
	private float _stunInflicted = 0.5f;

	private Rigidbody _rigidB;
	void Start () {
		_rigidB = GetComponent<Rigidbody>();
		GetComponent<Collider>().isTrigger = true;
	}

	public void Launch(Vector3 Position, Vector3 Direction, int ShooterNumber)
	{
		if (_rigidB == null)
			_rigidB = GetComponent<Rigidbody>();

		gameObject.SetActive(true);
		_playerNumber = ShooterNumber;

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
			if (Collider.GetComponent<PlayerController>().PlayerNumber == _playerNumber || Collider.GetComponent<PlayerController>()._isInvul)
				return;

			Collider.GetComponent<PlayerController>().Damage(_rigidB.velocity + Vector3.up * 3, _stunInflicted);
			// explosion particules
			this.Stop();
		}
		else if(Collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
		{
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
