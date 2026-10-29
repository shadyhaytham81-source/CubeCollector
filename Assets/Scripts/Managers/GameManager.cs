using UnityEngine;
using UnityEngine.SceneManagement;

// Central game rules: score, timer, and win / lose conditions.
// Other scripts reach it through GameManager.Instance (singleton pattern).
public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }

    public static GameManager Instance { get; private set; }

    public float timeLimit = 60f;

    public int Score { get; private set; }
    public int Collected { get; private set; }
    public int TotalCollectibles { get; private set; }
    public float TimeLeft { get; private set; }
    public GameState State { get; private set; } = GameState.Playing;
    public string EndMessage { get; private set; } = "";

    public bool IsPlaying => State == GameState.Playing;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        TimeLeft = timeLimit;
        TotalCollectibles = FindObjectsByType<Collectible>().Length;
    }

    void Update()
    {
        if (IsPlaying)
        {
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                Lose("Time's up!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    public void AddScore(int points)
    {
        if (!IsPlaying) return;

        Score += points;
        Collected++;

        if (Collected >= TotalCollectibles)
            Win();
    }

    public void Win()
    {
        if (!IsPlaying) return;
        State = GameState.Won;
        EndMessage = "You collected every cube!";
        Time.timeScale = 0f; // freeze the game
    }

    public void Lose(string reason)
    {
        if (!IsPlaying) return;
        State = GameState.Lost;
        EndMessage = reason;
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
