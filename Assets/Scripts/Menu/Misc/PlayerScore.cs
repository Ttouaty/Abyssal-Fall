using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerScore : MonoBehaviour
{
	[HideInInspector]
    public Player CurrentPlayer;
    private Sprite SpriteActive;
    private Sprite SpriteInactive;

    public RectTransform Root;

	[HideInInspector]
    public Image[] Points;

    public bool Active
    {
        get { return CurrentPlayer != null; }
    }

    public int Score
    {
        get { return Active ? CurrentPlayer.Score : 0; }
    }

    public void Init ()
    {
		gameObject.SetActive(Active);

		if (!Active)
			return;

		SpriteActive	= CurrentPlayer.CharacterUsed._characterData.Icon;
		SpriteInactive	= CurrentPlayer.CharacterUsed._characterData.DarkIcon;

		float ratio     = Root.sizeDelta.x / GameManager.Instance.CurrentGameConfiguration.NumberOfStages;
        Points          = new Image[GameManager.Instance.CurrentGameConfiguration.NumberOfStages];

        for (int i = 0; i < Points.Length; ++i)
        {
            GameObject scoreGo = new GameObject(name+ " point "+i, typeof(Image));
			scoreGo.GetComponent<RectTransform>().pivot = new Vector2(0,0.5f);
            scoreGo.transform.SetParent(Root.transform, false);
			scoreGo.GetComponent<RectTransform>().sizeDelta	= new Vector2(ratio, GetComponent<RectTransform>().sizeDelta.y);
			scoreGo.transform.localPosition		= new Vector3(ratio * i, 0, 0);
            Image image                         = scoreGo.GetComponent<Image>();
            image.sprite                        = SpriteInactive;
            Points[i]                           = image;
        }
    }

    public void DisplayScore ()
    {
        if(Active)
        {
            for (int i = 0; i < Points.Length; ++i)
            {
                if(Points[i] != null)
                {
                    Points[i].sprite = i < Score ? SpriteActive : SpriteInactive;
                }
            }
        }
    }
}
