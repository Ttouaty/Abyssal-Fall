using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroundFalling_Behaviour : ABaseBehaviour
{
	//private List<Tile>			_availableGrounds   = new List<Tile>();
	//public int                  NumberOfTiles       = 0;

	protected override IEnumerator Run_Implementation()
	{
		/*
		#####################################
			Deactivated due to player rage
		#####################################
		*/

		//if (!NetworkServer.active)
		//	yield break;
		//_availableGrounds = ArenaManager.Instance.Tiles.Where(x => x != null).ToList();
		//if(_availableGrounds.Count > 0)
		//{
		//	_availableGrounds.Shuffle();

		//	int nbTilesToFall = Mathf.Min(_availableGrounds.Count, NumberOfTiles);
		//	for (int i = 0; i < nbTilesToFall; ++i)
		//	{
		//		Tile tempTile = _availableGrounds.ShiftRandomElement();
		//		if(tempTile != null)
		//			tempTile.ActivateFall();
		//		else
		//			Debug.Log("tile was null");
		//	}
		//}

		yield return null;
	}
}
