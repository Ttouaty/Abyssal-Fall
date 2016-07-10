﻿using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
	void Awake()
	{
		Transform myTransform = transform;
		for (int c = 0; c < myTransform.childCount; ++c)
		{
			myTransform.GetChild(c).gameObject.SetActive(false);
		}
	}
}
