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

	// Score to achieve in order to beat the game
	public int m_WinScore;

	// The start, select and game over screens
	public GameObject m_StartScreen;
	public GameObject m_SelectScoreScreen;
	public GameObject m_GameOverScreen;

    // --------------------------------------------------------------

	// The game states
	enum GameStates
	{
		START,
		SELECTING,
		PLAYING,
		GAME_OVER
	}

	GameStates m_CurrentGameState;

	public GameModeSelect.GameModes m_currentGameMode;

	// Scores
    int m_Player1Score = 0;
    int m_Player2Score = 0;

	// The winner's player ID
	int m_WinnerID = 0;

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
		m_WinnerID = 0;
		m_CurrentGameState = GameStates.PLAYING;
		m_StartScreen.gameObject.SetActive (false);
		m_SelectScoreScreen.gameObject.SetActive (false);
		m_GameOverScreen.gameObject.SetActive (false);
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
		bool player1Win = m_Player1Score == m_WinScore;
		bool player2Win = m_Player2Score == m_WinScore;
		m_WinnerID = player1Win ? 1 : (player2Win ? 2 : 0);

		if (m_WinnerID != 0) 
		{
			m_CurrentGameState = GameStates.GAME_OVER;
			ShowWinScreen ();
		}
    }

	void ShowStartScreen()
	{
		m_Player1ScoreText.text = "";
		m_Player2ScoreText.text = "";
		m_CurrentGameState = GameStates.START;
		m_StartScreen.gameObject.SetActive (true);
		m_SelectScoreScreen.gameObject.SetActive (false);
		m_GameOverScreen.gameObject.SetActive (false);
	}

	public void ShowSelectScoreScreen()
	{
		m_Player1ScoreText.text = "";
		m_Player2ScoreText.text = "";
		m_CurrentGameState = GameStates.SELECTING;
		m_StartScreen.gameObject.SetActive (false);
		m_SelectScoreScreen.gameObject.SetActive (true);
		m_GameOverScreen.gameObject.SetActive (false);
	}

	void ShowWinScreen()
	{
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
