using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{

	private Vector3 _target;

	[SerializeField]
	private Vector3 _camDirection;

	[SerializeField]
	private Vector3 _baseOffset = Vector3.zero;
	private Vector3 _playerOffset = Vector3.zero;

	private GameObject[] _playersRef;

	void Start()
	{
		_playersRef = GameObject.FindGameObjectsWithTag("Player");
	}

	void Update() {
		CalculateCameraPosAndSize();
	}


	void CalculateCameraPosAndSize()
	{ 

		Vector3 cameraCenter = Vector3.zero;
		for (int i = 0; i < _playersRef.Length; ++i)
		{
			if (!_playersRef[i].GetComponent<PlayerController>()._isDead)
				cameraCenter += _playersRef[i].transform.position;
		}

		if (cameraCenter.sqrMagnitude == 0)
			return;

		cameraCenter /= _playersRef.Length;



		transform.position = new Vector3(Mathf.Lerp(transform.position.x, (_playerOffset + _baseOffset + cameraCenter).x, 5 * Time.deltaTime), transform.position.y, Mathf.Lerp(transform.position.z, (_playerOffset + _baseOffset + cameraCenter).z, 5 * Time.deltaTime));
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cameraCenter - transform.position, _camDirection), 1 * Time.deltaTime);
	}

}
