using UnityEngine;
using System.Collections;

public class MenuPauseManager : GenericSingleton<MenuPauseManager>
{
    public bool IsOpen                  = false;
    public PlayerScore[] ScoresFields   = new PlayerScore[4];
    public bool CanPause                = true;

    void Update ()
    {
        for(int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
        {
            Player player = GameManager.Instance.RegisteredPlayers[i];
            if(player != null)
            {
                if(InputManager.GetButtonDown("Start", player.JoystickNumber) && CanPause)
                {
                    Toggle();
                }
            }
        }
    }

    public override void Init ()
    {
        Close();

        for (int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
        {
            if(GameManager.Instance.RegisteredPlayers[i] != null)
            {
                ScoresFields[i].CurrentPlayer = GameManager.Instance.RegisteredPlayers[i];
            }
            Debug.Log("Init slot " + i);
            ScoresFields[i].Init();
        }
    }

    public void Toggle ()
    {
        if(IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open ()
    {
        TimeManager.Pause();
        for (int i = 0; i < ScoresFields.Length; ++i)
        {
            ScoresFields[i].DisplayScore();
        }
        transform.GetChild(0).gameObject.SetActive(true);
        IsOpen = true;
    }

    public void Close ()
    {
        TimeManager.Resume();
        transform.GetChild(0).gameObject.SetActive(false);
        IsOpen = false;
    }
}
