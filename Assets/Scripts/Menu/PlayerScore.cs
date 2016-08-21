using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerScore : MonoBehaviour
{
    public Player CurrentPlayer;
    public GameObject PrefabPoint;
    public Sprite ImageActive;
    public Sprite ImageInactive;

    public RectTransform Root;
    public RectTransform Label;

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
        if(Active)
        {
            Label.gameObject.SetActive(true);
            Label.GetComponent<Text>().text = "P" + (CurrentPlayer.PlayerNumber + 1);
        }
        else
        {
            Label.gameObject.SetActive(false);
        }

        float width     = Root.sizeDelta.x;
        float ratio     = width / GameManager.Instance.CurrentGameConfiguration.NumberOfStages;
        Points          = new Image[GameManager.Instance.CurrentGameConfiguration.NumberOfStages];

        for (int i = 0; i < Points.Length; ++i)
        {
            GameObject scoreGo                  = Instantiate(PrefabPoint);
            scoreGo.transform.position          = Root.position + new Vector3(ratio * (i + 0.5f), 0, 0);
            Image image                         = scoreGo.GetComponent<Image>();
            image.sprite                        = (Active) ? ImageActive : ImageInactive;
            Points[i]                           = image;

            scoreGo.transform.SetParent(Root.transform);
        }
    }

    public void DisplayScore ()
    {
        if(Active)
        {
            // END DEBUG
            for (int i = 0; i < Points.Length; ++i)
            {
                if(Points[i] != null)
                {
                    Points[i].color = i < Score ? Color.yellow : Color.white;
                }
            }
        }
    }
}
