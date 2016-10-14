using UnityEngine;
using System.Collections;

public class OptionSelector : MonoBehaviour
{
	private AGameRules[] _rulesArray;
	private int _targetRulesetIndex = 0;

	void Start()
	{
		DynamicConfig.Instance.GetConfigs(ref _rulesArray);
	}

	public void SetTargetRuleset(int rulesetIndex)
	{
		_targetRulesetIndex = rulesetIndex;
	}

	public AGameRules GetTargetRuleset()
	{
		return _rulesArray[_targetRulesetIndex];
	}

	public void SetIsMatchRoundBased()
	{
		//GetTargetRuleset().IsMatchRoundBased = !GetTargetRuleset().IsMatchRoundBased;
	}

	public void SetCanFalledTilesRespawn()
	{
		//GetTargetRuleset().CanFalledTilesRespawn = !GetTargetRuleset().CanFalledTilesRespawn;
	}

	public void SetCanPlayerRespawn()
	{
		//GetTargetRuleset().CanPlayerRespawn = !GetTargetRuleset().CanPlayerRespawn;
	}

	public void SetNumberOfRounds(int direction)
	{
		//GetTargetRuleset().NumberOfRounds += direction;
	}

	public void SetMatchDuration(int direction)
	{
		//GetTargetRuleset().MatchDuration += direction;
	}

	public void SetTileRegerationTime(int direction)
	{
		//GetTargetRuleset().TileRegerationTime += direction;
	}

	public void SetPointsGainPerKill(int direction)
	{
		//GetTargetRuleset().PointsGainPerKill += direction;
	}

	public void SetPointsLoosePerSuicide(int direction)
	{
		//GetTargetRuleset().PointsLoosePerSuicide += direction;
	}

	public void SetTimeBeforeSuicide(int direction)
	{
		//GetTargetRuleset().TimeBeforeSuicide += direction;
	}
}
