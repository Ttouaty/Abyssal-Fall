using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerScore : MonoBehaviour
{
	[HideInInspector]
    public Player CurrentPlayer;
    public Image SpriteActive;
    public Image SpriteFill;
    public Text ScoreText;
    public Text PlayerText;
	private int startingScore = 0;
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

		startingScore = CurrentPlayer.Score;
		
		SpriteActive.sprite = CurrentPlayer.CharacterUsed._characterData.Icon;
		SpriteFill.sprite = CurrentPlayer.CharacterUsed._characterData.LightIcon;

		if (GameManager.Instance.GameRules.ScoreToWin != 0)
			SpriteFill.fillAmount = startingScore / GameManager.Instance.GameRules.ScoreToWin;

		SpriteActive.color = CurrentPlayer.PlayerColor; //Pwetty
		PlayerText.text = "P" + CurrentPlayer.PlayerNumber;
		PlayerText.color = CurrentPlayer.PlayerColor;

		ScoreText.text = ""+CurrentPlayer.Score;
	}

	public void DisplayScore ()
	{
		if (Active)
			MenuPauseManager.Instance.StartCoroutine(DisplayCoroutine());
		else
			gameObject.SetActive(false);
	}

	private IEnumerator DisplayCoroutine()
	{
		ScoreText.CrossFadeAlpha(0, 0, true);
		yield return new WaitForSeconds(0.5f); // Wait 0.5f second before showing up

		ScoreText.text = "" + CurrentPlayer.Score;
		ScoreText.CrossFadeAlpha(1, 0.5f, true);

		float eT = 0;
		float timeToFillUp = 1;
		while(eT < timeToFillUp)
		{
			eT += Time.deltaTime;
			SpriteFill.fillAmount = Mathf.Lerp(startingScore / GameManager.Instance.GameRules.ScoreToWin, CurrentPlayer.Score / GameManager.Instance.GameRules.ScoreToWin, eT / timeToFillUp);
			yield return null;
		}

		startingScore = CurrentPlayer.Score;
	}
}
