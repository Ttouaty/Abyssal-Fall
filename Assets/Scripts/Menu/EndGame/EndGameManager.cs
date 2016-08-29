using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndGameManager : GenericSingleton<EndGameManager>
{
    public bool IsOpen = false;
    public int WinnerId = -1;

    public Text WinnerText;

    void Update()
    {
        if (IsOpen)
        {
            for (int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
            {
                Player player = GameManager.Instance.RegisteredPlayers[i];
                if (player != null)
                {
                    if (InputManager.GetButtonDown("Dash", player.JoystickNumber))
                    {
                        MainManager.Instance.LEVEL_MANAGER.UnloadScene(LevelManager.Instance.CurrentArenaConfig.BackgroundLevel);
                        MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig = null;
                        MainManager.Instance.LEVEL_MANAGER.OpenMenu();
                    }
                }
            }
        }
    }

    public override void Init()
    {
        Close();
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
        transform.GetChild(0).gameObject.SetActive(true);
        IsOpen = true;
        WinnerText.GetComponent<Localizator.LocalizedText>().OnChangeLanguage();
        WinnerText.text = WinnerText.text.Replace("%ID%", (WinnerId + 1).ToString());
    }

    public void Close()
    {
        IsOpen = false;
        transform.GetChild(0).gameObject.SetActive(false);
        TimeManager.Resume();
        MenuPauseManager.Instance.CanPause = true;
    }
}
