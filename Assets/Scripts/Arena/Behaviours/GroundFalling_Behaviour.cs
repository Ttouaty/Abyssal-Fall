using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GroundFalling_Behaviour : ABaseBehaviour
{
	private List<Tile>			_availableGrounds   = new List<Tile>();
	public int                  NumberOfTiles       = 0;

	protected override IEnumerator Run_Implementation()
	{
		_availableGrounds = ArenaManager.Instance.Tiles.ToList();
		if(_availableGrounds.Count > 0)
		{
			_availableGrounds.Shuffle();

			int nbTilesToFall = Mathf.Min(_availableGrounds.Count, NumberOfTiles);
			for (int i = 0; i < nbTilesToFall; ++i)
			{
				_availableGrounds.ShiftRandomElement().ActivateFall();
			}
		}

		yield return null;
	}
}
