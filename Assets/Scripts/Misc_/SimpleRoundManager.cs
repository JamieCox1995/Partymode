using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleRoundManager : MonoBehaviour
{
    [Header("Prefabs: ")]
    public GameObject player;
    public int playersToSpawn = 1;

    [Header("Teams: ")]
    public int numberOfTeams = 2;
    public List<TeamSettings> teams;
    public List<Avatar> allAvatars;

    [Header("Round Settings: ")]
    public float MaximumRoundTime = 300f;
    private float currentRoundTime = 0f;
    [Space]
    public string roundTimerDisplayString;
    public TextMeshProUGUI timerTextDisplay;

    public GameState gameState = GameState.PreGame;

    private void Start()
    {
        currentRoundTime = MaximumRoundTime;

        SetupTeams();

        gameState = GameState.PreGame;
    }

    public void SetupTeams()
    {
        //teams = new List<TeamSettings>(numberOfTeams);
    }

    public void SpawnPlayers()
    {
        for(int index = 0; index < playersToSpawn; index++)
        {
            // Creating a random index for the team to join
            int teamIndex = UnityEngine.Random.Range(0, teams.Count);

            if (teams[teamIndex].isFull == false)
            {
                GameObject playerGO = Instantiate(player);
                Avatar playerAvatar = playerGO.GetComponentInChildren<Avatar>();
                playerAvatar.m_AvatarHealth.OnDeath += OnAvatarDeath;

                teams[teamIndex].TeamMembers.Add(playerAvatar);
                allAvatars.Add(playerAvatar);
            }
        }
    }

    private void Update()
    {
        if (gameState == GameState.PreGame)
        {
            // Checking for a simple Key Press at the moment to start spawning in 
            // all of the players.
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Spawning");

                SpawnPlayers();

                gameState = GameState.GameRunning;
            }
        }
        else if (gameState == GameState.GameRunning)
        {
            GameTimerLoop();

            CheckGameOverCondition();
        }
    }

    private void GameTimerLoop()
    {
        currentRoundTime -= Time.deltaTime;

        UpdateRoundTimer(); 
    }

    private void UpdateRoundTimer()
    {
        timerTextDisplay.enabled = true;
        string minutes = "", seconds = "";

        if (currentRoundTime > 0f)
        {
            minutes = Mathf.Floor(currentRoundTime / 60f).ToString("00");
            seconds = (currentRoundTime % 60f).ToString("00");
        }
        else if (currentRoundTime <= 0f)
        {
            minutes = seconds = "00";
        }

        timerTextDisplay.text = string.Format(roundTimerDisplayString, minutes, seconds);
    }

    private void CheckGameOverCondition()
    {
        if (currentRoundTime <= 0f)
        {
            // Call GameOver
            GameOver();
        }

        // Checking if either team has living players
        foreach(TeamSettings team in teams)
        {
            if (team.TeamMembers.Count == 0)
            {
                // One of the teams has no players left alive, so we are calling GameOver
                GameOver();
            }
        }
    }

    public event Action OnGameOver;

    public void GameOver()
    {
        if (gameState != GameState.PostGame)
        {
            gameState = GameState.PostGame;

            if (OnGameOver != null)
            {
                OnGameOver();
            }

            Debug.LogWarning("Game has finished!");
        }
    }

    /*=======================================
     * Avatar Event Listeners
     ======================================*/
    public void OnAvatarDeath(Avatar avatar)
    {
        Debug.Log("Someone died!");

        //living.Remove(avatar);

        CheckGameOverCondition();
    }
}

[Serializable]
public class TeamSettings
{
    public string TeamName = "Team";
    public int MaximumPlayers = int.MaxValue / 2;
    public List<Avatar> TeamMembers;

    public bool isFull
    {
        get
        {
            return TeamMembers.Count == MaximumPlayers;
        }

        private set
        {
            isFull = value;
        }
    }
}

public enum GameState
{
    PreGame, GameRunning, PostGame
}
