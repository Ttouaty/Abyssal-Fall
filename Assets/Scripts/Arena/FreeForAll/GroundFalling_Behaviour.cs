using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundFalling_Behaviour : ABaseBehaviour
{
    private List<GameObject>    _availableGrounds   = new List<GameObject>();
    public int                  NumberOfTiles       = 0;

    protected override IEnumerator Run_Implementation()
    {
        _availableGrounds = ArenaManager.Instance.Tiles;
        _availableGrounds.Shuffle();

        for(int i = 0; i < NumberOfTiles; ++i)
        {
            GameObject go = _availableGrounds.ShiftRandomElement();
            Tile tile = go.GetComponent<Tile>();
            if (tile != null)
            {
                tile.ActivateFall();
            }
        }

        yield return null;
    }
}
