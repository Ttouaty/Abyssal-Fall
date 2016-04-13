using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour {

	private int _playerNumber;
	[SerializeField]
	private int _speed;
	[SerializeField]
	private int _stunInflicted;

	private Rigidbody _rigidB;
	void Start () {
		_rigidB = GetComponent<Rigidbody>();
		GetComponent<Collider>().isTrigger = true;
	}

	public void Launch(Vector3 Direction, int ShooterNumber)
	{
		gameObject.SetActive(true);
		_playerNumber = ShooterNumber;
		transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);
		_rigidB.velocity = Direction.normalized * _speed;
	}


	protected void Stop()
	{
		if (this.gameObject.activeSelf)
		{
			this._rigidB.velocity = Vector3.zero;
			this.gameObject.SetActive(false);
		}
	}

	private void OnTriggerEnter(Collider Collider)
	{
		if (Collider.tag == "Player")
		{
			if (Collider.GetComponent<PlayerController>().PlayerNumber == _playerNumber)
				return;
			
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
