using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuPanel : MonoBehaviour {
	public string MenuName;
	public Button PreSelectedButton;
	[Space]
	public MenuPanel ParentMenu;
	[HideInInspector]
	public Button LastButtonSelected; // NOT YET IMPLEMENTED
}
