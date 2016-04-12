using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ArenaGenerator))]
public class Arena : MonoBehaviour
{
	private ArenaGenerator _generator;

	// Use this for initialization
	public void Init () {
		_generator = GetComponent<ArenaGenerator>();
		_generator.Init();
	}
}
