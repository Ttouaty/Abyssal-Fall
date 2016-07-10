using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	private Vector3 _basePosition;
	[SerializeField]
	private float _cameraWinZoomSpeed = 3;
	private Vector3 _target;
	private Camera _camera;
	private float _cameraOffset = 5;
	public void Start()
	{
		_camera = GetComponent<Camera>();

		InitPosition();
		
		_basePosition = transform.position;

		GameManager.instance.OnPlayerWin.AddListener(OnPlayerWin);
		Reset();
		StartCoroutine(SmoothZoom());
	}

	private void InitPosition()
	{
		RecalculateTarget();
		
		float distanceToArena = Vector3.Distance(_target, _camera.transform.position);
		_camera.transform.position = _target - _camera.transform.forward * distanceToArena;
		transform.LookAt(_target);
	}

	private void RecalculateTarget()
	{
		Vector3 directionToCam = (transform.position - GameManager.instance.Arena.transform.position).ZeroY().normalized;
		_target = GameManager.instance.Arena.transform.position + directionToCam * (Arena.instance.Size * 0.25f * (Mathf.Cos((_camera.transform.rotation.eulerAngles.x) * Mathf.Deg2Rad)));
	}

	public void Reset ()
	{
		StopAllCoroutines();
		transform.position = _basePosition;
		transform.LookAt(_target);
		InitPosition();
	}

	public void OnZoom()
	{
		StartCoroutine(SmoothZoom());
	}

	IEnumerator SmoothZoom ()
	{
		RecalculateTarget();
		float timer = 0;
		//Vector3 endPosition = _camera.transform.forward * 3 + _camera.transform.transform.position;
		Vector3 endPosition = _target - _camera.transform.forward * (((Arena.instance.Size * Mathf.Sin((_camera.transform.rotation.eulerAngles.x) *Mathf.Deg2Rad)) * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad)) + _cameraOffset);
		// Thanks unity doc for distance calculation :)
		while(timer < 1)
		{
			timer += Time.deltaTime;
			_camera.transform.position = Vector3.Lerp(_camera.transform.position, endPosition, timer);
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
		winner.GetComponent<PlayerController>()._playerProp.PropRenderer.enabled = false;

		Vector3 startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		winner.transform.Rotate(Vector3.up, Vector3.Angle(winner.transform.position, transform.position));

		Vector3 endPos = winner.transform.position + winner.transform.forward * 6 + winner.transform.up * 5;

		float timer = 0;
		while(timer < 1)
		{
			timer += Time.deltaTime * (1 / _cameraWinZoomSpeed);
			transform.position = Vector3.Lerp(startPos, endPos, timer);
			transform.LookAt(winner.transform);
			yield return null;
		}

		winner.GetComponent<PlayerController>()._animator.SetTrigger("Win");
		yield return new WaitForSeconds(3);
		int winnerId = winner.GetComponent<PlayerController>().PlayerNumber -1;
		EndStageScreen endScreen = GameManager.instance.EndStageScreen.GetComponent<EndStageScreen>();
		endScreen.ShowPanel();
		yield return endScreen.StartCoroutine(endScreen.StartCountdown(winnerId));

		Reset();
		GameManager.instance.Arena.ClearArena();
		GameManager.instance.Restart();

		yield return null;
	}

}
