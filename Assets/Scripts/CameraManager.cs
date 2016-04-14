using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	[SerializeField]
	private Vector3 _basePosition;
	private float _currentZoom = 0;
	private Vector3 _target;
	private Camera _camera;

	public void Start()
	{
		_camera = GetComponent<Camera>();
		_target = GameManager.instance.Arena.transform.position;
		_basePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

		transform.LookAt(_target);
	}

	public void Reset ()
	{
		StopAllCoroutines();
		transform.position = _basePosition;
	}

	public void OnZoom()
	{
		StartCoroutine(SmoothZoom());
	}

	IEnumerator SmoothZoom ()
	{
		float timer = 0;
		while(timer < 1)
		{
			timer += Time.deltaTime;
			_camera.transform.Translate(new Vector3(0, 0, timer * 0.2f), Space.Self);
			yield return null;
		}

		yield return null;
	}

}
