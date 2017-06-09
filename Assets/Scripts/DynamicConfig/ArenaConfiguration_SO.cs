using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewArenaConfiguration", menuName = "Abyssal Fall/Arena Configuration", order = 1)]
public class ArenaConfiguration_SO : ScriptableObject
{
    public GameObject Ground;
    public GameObject Obstacle;
    public PoolConfiguration[] AdditionalPoolsToLoad;
    public SceneField BackgroundLevel;
    public BehaviourConfiguration[] Behaviours;
	public Sprite Artwork;
	public EArenaConfiguration TargetMapEnum;
	[Space]
	public VictoryPlatform VictoryPlatformGo;
	public Texture AmbientRamp;
	public string AmbianceSound;
}
