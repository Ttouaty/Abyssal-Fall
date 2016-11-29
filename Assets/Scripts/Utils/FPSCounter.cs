using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour {

	private Text _textRef;

	private float _fpsDisplayed;

	void Start ()
	{
		_textRef = GetComponent<Text>();
	}
	
	void Update ()
	{
		_fpsDisplayed = Mathf.Lerp(_fpsDisplayed, 1 / Time.deltaTime, 10 * Time.deltaTime);
		_textRef.text = _fpsDisplayed.ToString("0.0");
	}

	public void Toggle()
	{
		_textRef = GetComponent<Text>();
		_textRef.text = "";
		enabled = !enabled;
	}
}
