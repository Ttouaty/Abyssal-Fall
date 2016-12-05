using UnityEngine;
using System.Collections;

public class PortNotification : MonoBehaviour {

	void Update () {
		gameObject.SetActive(!Network.HavePublicAddress());
	}
}
