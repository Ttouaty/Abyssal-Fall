using UnityEngine;
using System.Collections;

public class TutorialManager : GenericSingleton<TutorialManager> {

	public override void Init()
	{

		Debug.Log("Le tutoriel est initialisé ici.");
	}
}
