using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndStageManager : GenericSingleton<EndStageManager>
{
    public bool IsOpen = false;
    public PlayerScore[] ScoresFields = new PlayerScore[4];
    public Text StageText;

    void Update()
    {
        if(IsOpen)
        {
            for (int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
            {
                Player player = GameManager.Instance.RegisteredPlayers[i];
                if (player != null)
                {
                    if (InputManager.GetButtonDown("Dash", player.JoystickNumber))
                    {
                        ArenaManager.Instance.ResetMap(false);
                        Close();
                    }
                }
            }
        }
    }

    public override void Init()
    {
        Close();

        for (int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
        {
            if (GameManager.Instance.RegisteredPlayers[i] != null)
            {
                ScoresFields[i].CurrentPlayer = GameManager.Instance.RegisteredPlayers[i];
            }
            Debug.Log("Init slot " + i);
            ScoresFields[i].Init();
        }
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        MenuPauseManager.Instance.CanPause = false;
        TimeManager.Pause();
        for (int i = 0; i < ScoresFields.Length; ++i)
        {
            ScoresFields[i].DisplayScore();
        }
        StageText.GetComponent<Localizator.LocalizedText>().OnChangeLanguage();
        StageText.text = StageText.text.Replace("%COUNT%", GameManager.Instance.CurrentStage.ToString());
        transform.GetChild(0).gameObject.SetActive(true);
        IsOpen = true;
    }

    public void Close()
    {
        TimeManager.Resume();
        transform.GetChild(0).gameObject.SetActive(false);
        IsOpen = false;
        MenuPauseManager.Instance.CanPause = true;
    }
}
