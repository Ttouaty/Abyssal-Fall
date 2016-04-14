using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	[SerializeField]
	private Vector3 _basePosition;
	[SerializeField]
	private float _cameraWinZoomSpeed = 3;
	private float _currentZoom = 0;
	private Vector3 _target;
	private Camera _camera;

	public void Start()
	{
		_camera = GetComponent<Camera>();
		_target = GameManager.instance.Arena.transform.position;
		_basePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

		GameManager.instance.OnPlayerWin.AddListener(OnPlayerWin);
		Reset();
	}

	public void Reset ()
	{
		StopAllCoroutines();
		transform.position = _basePosition;
		transform.LookAt(_target);
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

	public void OnPlayerWin (GameObject winner)
	{
		StartCoroutine(ZoomOnWinner(winner));
	}

	IEnumerator ZoomOnWinner (GameObject winner)
	{
		winner.GetComponent<PlayerController>()._hammerPropModel.SetActive(false);
		Vector3 startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		Vector3 direction = (winner.transform.position - startPos).normalized;
		winner.transform.Rotate(Vector3.up, Vector3.Angle(winner.transform.position, transform.position));

		Vector3 endPos = winner.transform.position + winner.transform.forward * 6 + winner.transform.up * 4 ;

		float timer = 0;
		while(timer < 1)
		{
			timer += Time.deltaTime * (1 / _cameraWinZoomSpeed);
			transform.position = Vector3.Lerp(startPos, endPos, timer);
			transform.LookAt(winner.transform);
			yield return null;
		}

		winner.GetComponent<PlayerController>()._animator.SetTrigger("Win");

		yield return new WaitForSeconds(5);

		Reset();
		GameManager.instance.Arena.ClearArena();
		GameManager.instance.Restart();

		yield return null;
	}

}
