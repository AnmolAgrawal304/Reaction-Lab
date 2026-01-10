using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject TilePrefab;
    public Transform GridContainer;
    public TMP_Text ScoreText;

    [Header("UI Game State")]
    private int _currentScore = 0;

    [Header("Color Rule Settings")]
    public Color NormalColor = Color.white;
    public Color OddColor = Color.red;

    [Header("Scoring Settings")]
    public int BaseScorePerRound = 100; // Max points for instant click
    public int MinScorePerRound = 10; // Minimum guaranteed score if clicked late
    public int WrongClickPenalty = 50; // Score penalty

    private float _roundStartTime; // Track reaction time

    [Header("Difficulty Settings")]
    public float TileFadeDuration = 1.0f; // Tiles disapper in 3 seconds

    private List<GameObject> _activeTiles = new List<GameObject>();

    private void Start()
    {
        _currentScore = 0;
        UpdateScoreUI();    
        StartNewRound();
    }

    public void StartNewRound()
    {
        ClearGrid();
        _roundStartTime = Time.time;

        // Decide which index is the odd one (0 to 8)
        int oddIndex = Random.Range(0, 9);

        // Loop 9 times to create the 3x3 grid
        for (int i = 0; i < 9; i++)
        {
            // Instantiate the tile into the grid container
            GameObject newTileObj = Instantiate(TilePrefab, GridContainer);
            TileController tileScript = newTileObj.GetComponent<TileController>();

            // Determine if this specific iteration is odd one
            bool isOdd = (i == oddIndex);
            Color colorToUse = isOdd ? OddColor : NormalColor;

            // Setup the tile's data and visuals
            tileScript.SetupTile(isOdd, colorToUse, this, TileFadeDuration);

            // Keep track of it
            _activeTiles.Add(newTileObj);
        }
    }

    // Handle input received from tiles
    public void TileClicked(bool wasOddOne)
    {
        if (wasOddOne)
        {
            CalculateAndAddScore(); // Calculate score based on time
            StartNewRound(); // Immediate restart loop
        }
        else
        {
            _currentScore -= WrongClickPenalty; // Apply penalty
            UpdateScoreUI();
            Debug.Log("WRONG CLICK! Penalty applied.");
        }
    }

    // Scoring Logic
    private void CalculateAndAddScore()
    {
        float TimeTaken = Time.time - _roundStartTime;

        // Calculate how much of the fade duration has passed as a percentage (0.0 to 1.0).
        // Use current time divided by total allowed fade duration.
        float timePercentage = TimeTaken / TileFadeDuration;

        // Ensure the percentage never goes above 1.0 (100%), even if they wait longer.
        timePercentage = Mathf.Clamp01(timePercentage);

        // Calculate score based on that percentage.
        // Mathf.Lerp finds a value between A and B based on t , If t=0 (start instant), score is baseScorePerRound , If t=1 (fully faded), score is exactly minScorePerRound.
        float calculatedScore = Mathf.Lerp(BaseScorePerRound, MinScorePerRound, timePercentage);

        // Round to nearest whole number for the final score
        int finalRoundScore = Mathf.RoundToInt(calculatedScore);

        _currentScore += finalRoundScore;
        UpdateScoreUI();

        // Debug to confirm it works
        if (timePercentage >= 1.0f)
        {
            Debug.Log($"Tiles finished fading. Awarding minimum score: {finalRoundScore}");
        }
        else
        {
            Debug.Log($"Reacted in {TimeTaken}s. Score: {finalRoundScore}");
        }
    }

    private void UpdateScoreUI()
    {
        if (ScoreText != null)
        {
            ScoreText.text = $"Score: {_currentScore}";
        }
    }

    // Clean up before spawning the new ones
    private void ClearGrid()
    {
        foreach (GameObject tile in _activeTiles)
        {
            Destroy(tile);
        }
        _activeTiles.Clear();
    }
}
