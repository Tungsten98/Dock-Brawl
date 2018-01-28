using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    // --------------------------------------------------------------

	// Score texts
    [SerializeField]
    Text m_Player1ScoreText;

    [SerializeField]
    Text m_Player2ScoreText;

	// Time limit and sudden death texts (best score in given time mode)
	public Text m_TimeText;
	public Text m_SuddenDeathText;

	// Score to achieve in order to beat the game (first to score game mode)
	public int m_WinScore;

	// The time limit of the game (best score in given time game mode)
	public float m_TimeLimit;

	// The start, select and game over screens
	public GameObject m_StartScreen;
	public GameObject m_SelectScoreScreen;
	public GameObject m_GameOverScreen;

	// The current game mode
	public GameModeSelect.GameModes m_CurrentGameMode;

    // --------------------------------------------------------------

	// The game states
	enum GameStates
	{
		START,
		SELECTING,
		PLAYING,
		GAME_OVER
	}

	// The current game state
	GameStates m_CurrentGameState;

	// Scores
    int m_Player1Score = 0;
    int m_Player2Score = 0;

	// The start time and time limit for each game (Score in time game mode)
	float m_StartTime;
	float m_TimeLeft;

	// The winner's player ID
	int m_WinnerID = 0;

	// Each player's win state
	bool m_Player1Win;
	bool m_Player2Win;

    // --------------------------------------------------------------

	void Start()
	{
		ShowStartScreen ();
	}

	// Reset the game
	public void RestartGame()
	{
		m_Player1Score = 0;
		m_Player2Score = 0;
		m_Player1ScoreText.text = "" + m_Player1Score;
		m_Player2ScoreText.text = "" + m_Player2Score;
		m_TimeText.text = "";
		m_SuddenDeathText.text = "";
		m_WinnerID = 0;
		m_Player1Win = false;
		m_Player2Win = false;
		m_CurrentGameState = GameStates.PLAYING;
		m_StartScreen.gameObject.SetActive (false);
		m_SelectScoreScreen.gameObject.SetActive (false);
		m_GameOverScreen.gameObject.SetActive (false);

		if (m_CurrentGameMode == GameModeSelect.GameModes.SCORE_IN_TIME) 
		{
			m_TimeLeft = m_TimeLimit;
			m_StartTime = Time.time;
			m_TimeText.text += m_TimeLimit;
		}
	}

	void Update()
	{
		if (m_CurrentGameState != GameStates.PLAYING)
			return;

		// Update the timer and trigger the sudden death flag if needed
		if (m_CurrentGameMode == GameModeSelect.GameModes.SCORE_IN_TIME) 
		{
			if (m_TimeLeft > 0) {
				m_TimeLeft = m_TimeLimit - Time.time + m_StartTime;
				m_TimeText.text = "" + ((int)m_TimeLeft + 1);
			} 
			else 
			{
				m_TimeText.text = "0";
				m_Player1Win = m_Player1Score > m_Player2Score;
				m_Player2Win = m_Player2Score > m_Player1Score;

				if (!m_Player1Win && !m_Player2Win)
					m_SuddenDeathText.text = "SUDDEN DEATH!";
			}
		}

		// Check if a player has won
		m_WinnerID = m_Player1Win ? 1 : (m_Player2Win ? 2 : 0);

		if (m_WinnerID != 0) 
			ShowWinScreen ();
	}

    void OnEnable()
    {
		DeathTrigger.OnPlayerDeath += OnUpdateScore;
    }

    void OnDisable()
    {
        DeathTrigger.OnPlayerDeath -= OnUpdateScore;
    }

    void OnUpdateScore(int playerNum)
    {
		if (m_CurrentGameState != GameStates.PLAYING)
			return;

		if (playerNum == 1) 
		{
			m_Player2Score += 1;
			m_Player2ScoreText.text = "" + m_Player2Score;
		} 
		else if (playerNum == 2) 
		{
			m_Player1Score += 1;
			m_Player1ScoreText.text = "" + m_Player1Score;
		}

		// Check if a player has won the game
		if (m_CurrentGameMode == GameModeSelect.GameModes.FIRST_TO_SCORE) 
		{
			m_Player1Win = m_Player1Score == m_WinScore;
			m_Player2Win = m_Player2Score == m_WinScore;
		} 
    }

	void ShowStartScreen()
	{
		m_Player1ScoreText.text = "";
		m_Player2ScoreText.text = "";
		m_TimeText.text = "";
		m_SuddenDeathText.text = "";
		m_CurrentGameState = GameStates.START;
		m_StartScreen.gameObject.SetActive (true);
		m_SelectScoreScreen.gameObject.SetActive (false);
		m_GameOverScreen.gameObject.SetActive (false);
	}

	public void ShowSelectScoreScreen()
	{
		m_Player1ScoreText.text = "";
		m_Player2ScoreText.text = "";
		m_TimeText.text = "";
		m_SuddenDeathText.text = "";
		m_CurrentGameState = GameStates.SELECTING;
		m_StartScreen.gameObject.SetActive (false);
		m_SelectScoreScreen.gameObject.SetActive (true);
		m_GameOverScreen.gameObject.SetActive (false);
	}

	void ShowWinScreen()
	{
		m_CurrentGameState = GameStates.GAME_OVER;

		// Load the colours associated with each player
		Color[] playerColours = new Color[2];
		playerColours[0] = new Color (0.0f, 118.0f, 255.0f, 255.0f);
		playerColours[1] = new Color (186.0f, 0.0f, 0.0f, 255.0f);

		// Set the contents on the screen
		m_GameOverScreen.gameObject.SetActive (true);

		string winString = "Player " + m_WinnerID + " Wins!";
		Text winText = m_GameOverScreen.GetComponentInChildren<Text> ();
		winText.color = playerColours [m_WinnerID - 1];
		winText.text = winString;
	}
}
