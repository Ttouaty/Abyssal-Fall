using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	public float zoomSpeed = 20f;
	public float minZoomFOV = 10f;

	private Vector3 _target;
	private Camera _camera;

	[SerializeField]
	private Vector3 _camDirection;

	[SerializeField]
	private Vector3 _baseOffset = Vector3.zero;
	private Vector3 _playerOffset = Vector3.zero;

	private GameObject[] _playersRef;

	void Start()
	{
		_camera = Camera.main;
		_playersRef = GameObject.FindGameObjectsWithTag("Player");

		Vector3 cameraCenter = Vector3.zero;
		for (int i = 0; i < _playersRef.Length; ++i)
		{
			cameraCenter += _playersRef[i].transform.position;
		}

		cameraCenter /= _playersRef.Length;
		transform.position = new Vector3((_playerOffset + _baseOffset + cameraCenter).x, transform.position.y, (_playerOffset + _baseOffset + cameraCenter).z);

		transform.LookAt(cameraCenter);
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
		//transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cameraCenter - transform.position, _camDirection), 5 * Time.deltaTime);
	}

	public void OnZoom(int value)
	{

	}

}
