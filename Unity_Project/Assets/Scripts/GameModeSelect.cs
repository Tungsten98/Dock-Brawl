using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelect : MonoBehaviour 
{
	// -------------------------------------------------

	// The game modes
	public enum GameModes
	{
		FIRST_TO_SCORE,
		SCORE_IN_TIME
	}
	int m_GameModesEnumSize = 2;

	// The current game mode
	public GameModes m_CurrentGameMode;

	// References to the property selection interfaces
	public GameObject[] m_GameModeSelectInterface;

	// References to the game mode toggles
	public Toggle[] m_GameModeToggles;

	// -------------------------------------------------

	// Local variables to send off to the UIManager
	class UIManagerInfo
	{
		public int m_MaxScore = 10;
		public int m_GameTimeInSeconds = 60;
	}

	// The UIManager that will take this information to load the appropriate game
	UIManager m_MainUIManager;

	// An instance of the information to send to the UIManager
	UIManagerInfo m_UIManagerInfo;

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
		// Check for which game mode is enabled, and load the appropriate settings interface
		for (int gameModeIndex = 0; gameModeIndex < m_GameModesEnumSize; gameModeIndex++) 
		{
			m_GameModeToggles [gameModeIndex].isOn = (GameModes) gameModeIndex == m_CurrentGameMode;
			m_GameModeSelectInterface [gameModeIndex].SetActive ((GameModes) gameModeIndex == m_CurrentGameMode);
		}
	}

	public void ChangeGameMode(int gameMode)
	{
		m_CurrentGameMode = (GameModes) gameMode;
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

	// Update the time limit when the user updates information in the UI 
	public void ChangeTimeLimit(string value)
	{
		int valueAsInt = 0;
		int.TryParse (value, out valueAsInt);

		if (valueAsInt < 30)
			m_UIManagerInfo.m_GameTimeInSeconds = 30;
		else if (valueAsInt > 120)
			m_UIManagerInfo.m_GameTimeInSeconds = 120;
		else
			m_UIManagerInfo.m_GameTimeInSeconds = valueAsInt;

		UpdateTimeLimitUI();
	}

	public void ChangeTimeLimit(int sign)
	{
		m_UIManagerInfo.m_GameTimeInSeconds += sign;

		if (m_UIManagerInfo.m_GameTimeInSeconds < 30)
			m_UIManagerInfo.m_GameTimeInSeconds = 30;
		else if (m_UIManagerInfo.m_GameTimeInSeconds > 120)
			m_UIManagerInfo.m_GameTimeInSeconds = 120;

		UpdateTimeLimitUI();
	}

	void UpdateTimeLimitUI()
	{
		InputField inputField = GetComponentInChildren<InputField> ();
		inputField.text = "" + m_UIManagerInfo.m_GameTimeInSeconds;
	}

	// Send the information to the UIManager to process and load the appropriate game
	public void SendGameModeInfo()
	{
		m_MainUIManager.m_CurrentGameMode = m_CurrentGameMode;

		if (m_CurrentGameMode == GameModes.FIRST_TO_SCORE)
			m_MainUIManager.m_WinScore = m_UIManagerInfo.m_MaxScore;
		else if (m_CurrentGameMode == GameModes.SCORE_IN_TIME)
			m_MainUIManager.m_TimeLimit = (float) m_UIManagerInfo.m_GameTimeInSeconds;
	}
}
