using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//  Different game types
public enum RuleType { ColorDifference, RotationDifference }


public class GameManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject TilePrefab;
    public Transform GridContainer;
    public TMP_Text ScoreText;

    [Header("UI Game State")]
    private int _currentScore = 0;

    [Header("Game Rules State")]
    private RuleType currentRoundRule; // Tracks current rule

    [Header("Color Rule Settings")]
    public Color NormalColor = Color.white;
    public Color OddColor = Color.red;

    [Header("Rotation Rule Settings")]
    public float normalRotation = 0f;
    public float oddRotation = 45f;

    [Header("Scoring Settings")]
    public int BaseScorePerRound = 100; // Max points for instant click
    public int MinScorePerRound = 10; // Minimum guaranteed score if clicked late
    public int WrongClickPenalty = 50; // Score penalty

    private float _roundStartTime; // Track reaction time

    [Header("Difficulty Settings")]
    public float TileFadeDuration = 0.5f; // Tiles disapper in 3 seconds

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

        // Pick a random rule for this round. Random.value returns between 0.0 and 1.0. 50/50 chance.
        currentRoundRule = (Random.value > 0.5f) ? RuleType.ColorDifference : RuleType.RotationDifference;

        // Debug log to help us test
        Debug.Log("Starting new round with rule: " + currentRoundRule);

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

            // Determine Color based on current rule
            Color colorToUse = NormalColor;
            if (currentRoundRule == RuleType.ColorDifference)
            {
                // If color rule active, check if odd
                colorToUse = isOdd ? OddColor : NormalColor;
            }

            // Determine Rotation based on current rule
            float rotationToUse = normalRotation; 
            if (currentRoundRule == RuleType.RotationDifference)
            {
                // If rotation rule active, check if odd
                rotationToUse = isOdd ? oddRotation : normalRotation;
            }

            // Pass all data to the tile
            tileScript.SetupTile(isOdd, this, TileFadeDuration, colorToUse, rotationToUse);

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
