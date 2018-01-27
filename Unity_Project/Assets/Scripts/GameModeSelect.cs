using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelect : MonoBehaviour 
{
	// -------------------------------------------------

	public enum GameModes
	{
		FIRST_TO_SCORE,
		BEST_IN_TIME
	}

	UIManager m_MainUIManager;
	public GameModes m_CurrentGameMode;
	public GameObject[] m_GameModeSelectInterface;

	// Local variables to send off to the UIManager
	public class UIManagerInfo
	{
		public int m_MaxScore = 10;
		public int m_GameTimeInSeconds = 60;
	}
	public UIManagerInfo m_UIManagerInfo;

	// -------------------------------------------------

	void Awake()
	{
		m_MainUIManager = GetComponentInParent<UIManager> ();
		m_UIManagerInfo = new UIManagerInfo ();
	}

	void Start()
	{
		m_CurrentGameMode = GameModes.FIRST_TO_SCORE;
	}

	void Update()
	{
		switch (m_CurrentGameMode) {

		case GameModes.FIRST_TO_SCORE:
			m_GameModeSelectInterface [0].SetActive (true);
			break;

		default:
			m_GameModeSelectInterface [0].SetActive (false);
			break;
		}
	}

	public void TriggerModeChange()
	{

	}

	// Update the maximum score when the user updates information in the UI 
	public void ChangeMaxScore(string value)
	{
		int valueAsInt = 0;
		int.TryParse (value, out valueAsInt);

		if (valueAsInt < 5)
			m_UIManagerInfo.m_MaxScore = 5;
		else if (valueAsInt > 20)
			m_UIManagerInfo.m_MaxScore = 20;
		else
			m_UIManagerInfo.m_MaxScore = valueAsInt;

		UpdateMaxScoreUI();
	}

	public void ChangeMaxScore(int sign)
	{
		m_UIManagerInfo.m_MaxScore += sign;

		if (m_UIManagerInfo.m_MaxScore < 5)
			m_UIManagerInfo.m_MaxScore = 5;
		else if (m_UIManagerInfo.m_MaxScore > 20)
			m_UIManagerInfo.m_MaxScore = 20;
		
		UpdateMaxScoreUI();
	}

	void UpdateMaxScoreUI()
	{
		InputField inputField = GetComponentInChildren<InputField> ();
		inputField.text = "" + m_UIManagerInfo.m_MaxScore;
	}

	public void SendGameModeInfo()
	{
		m_MainUIManager.m_currentGameMode = m_CurrentGameMode;

		if (m_CurrentGameMode == GameModes.FIRST_TO_SCORE)
			m_MainUIManager.m_WinScore = m_UIManagerInfo.m_MaxScore;
	}
}
