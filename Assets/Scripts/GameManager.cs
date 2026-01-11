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
    public TMP_Text timerText;

    [Header("UI Game State")]
    private int _currentScore = 0;

    [Header("Game Rules State")]
    private RuleType currentRoundRule; // Tracks current rule

    [Header("Color Rule Settings")]
    public Color NormalColor = Color.white;
    public Color OddColor = Color.red;

    [Header("Rotation Rule Settings")]
    public float minRotationAngle = 0f;
    public float maxRotationAngle = 90f;
    public float minRotationDifference = 30f; // Minimum gap required between normal and odd

    [Header("Scoring Settings")]
    public int BaseScorePerRound = 100; // Max points for instant click
    public int MinScorePerRound = 10; // Minimum guaranteed score if clicked late

    private float _roundStartTime; // Track reaction time

    [Header("Difficulty Settings")]
    public float TileFadeDuration = 2.0f; // Tiles disapper seconds
    public float minFadeDuration = 0.5f; // Hardest possible setting (fastest fade)
    public float maxFadeDuration = 3.0f; // Easiest possible setting (slowest fade)
    public float difficultyStep = 0.25f; // How much to change duration by each time

    [Header("Adaptive Difficulty Logic")]
    public int historySize = 5; // How many past rounds to remember
    public int thresholdsScoreHigh = 80; // Average score above this = get harder
    public int thresholdScoreLow = 40;  // Average score below this = get easier

    [Header("Time Limit Settings")]
    public float startingTime = 60.0f; // Total game time in seconds
    public float timePenaltyWrongClick = 5.0f; // Time lost for a mistake
    public float timeRewardHighPerformance = 10.0f; // Time gained for doing well

    private float currentTimeRemaining; // Tracks current time

    [Header("UI References")]
    public Button stopButton;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;

    private bool isGameActive = false; // Tracks if we should accept input
    private int roundsPlayedCount = 0; // Track stats

    private List<int> scoreHistory = new List<int>(); // List to store past scores

    private List<GameObject> _activeTiles = new List<GameObject>();

    private void Start()
    {
        // Ensure UI is in correct starting state
        gameOverPanel.SetActive(false);
        stopButton.interactable = true;

        SetupNewGame();
    }

    // Helper to reset everything for a fresh game
    private void SetupNewGame()
    {
        isGameActive = true;
        _currentScore = 0;
        roundsPlayedCount = 0;
        scoreHistory.Clear();
        // Reset difficulty to default for new game, Whatever your desired start speed is
        TileFadeDuration = 2.0f;
        currentTimeRemaining = startingTime;

        UpdateScoreUI();
        UpdateTimerUI();
        StartNewRound();
    }

    private void Update()
    {
        if (isGameActive)
        {
            currentTimeRemaining -= Time.deltaTime;
            if (currentTimeRemaining <= 0)
            {
                currentTimeRemaining = 0;
                EndGame("TIME'S UP!");
            }
            UpdateTimerUI();
        }
    }

    // Function for the timer text
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + currentTimeRemaining.ToString();
        }
    }

    public void StartNewRound()
    {
        if (!isGameActive) return; // Don't start rounds if game is over

        roundsPlayedCount++; // Increment stat

        ClearGrid();
        _roundStartTime = Time.time;

        // Pick a random rule for this round. Random.value returns between 0.0 and 1.0. 50/50 chance.
        currentRoundRule = (Random.value > 0.5f) ? RuleType.ColorDifference : RuleType.RotationDifference;
        Debug.Log("Starting new round with rule: " + currentRoundRule); // Debug log to help us test


        // ---ROTATION LOGIC START---
        // We need to define these variables outside the loop
        float generatedNormalAngle = 0f;
        float generatedOddAngle = 0f;

        // Only do the math if it's actually a rotation round
        if (currentRoundRule == RuleType.RotationDifference)
        {
            // Generate the first angle randomly within range
            float angleA = Random.Range(minRotationAngle, maxRotationAngle);
            float angleB = angleA;

            // Generate the second angle. If it's too close to angleA, try again.
            // We use a loop to ensure they are distinct by at least minRotationDifference.
            int safetyCounter = 0; // Prevent infinite loops if settings are bad
            while (Mathf.Abs(angleA - angleB) < minRotationDifference && safetyCounter < 100)
            {
                angleB = Random.Range(minRotationAngle, maxRotationAngle);
                safetyCounter++;
            }

            // Randomly assign which angle is "normal" and which is "odd"
            if (Random.value > 0.5f)
            {
                generatedNormalAngle = angleA;
                generatedOddAngle = angleB;
            }
            else
            {
                generatedNormalAngle = angleB;
                generatedOddAngle = angleA;
            }
            Debug.Log($"Rotation Round. Normal Angle: {generatedNormalAngle}, Odd Angle: {generatedOddAngle}");
        }
        // ---ROTATION LOGIC END---

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

            // Color Logic
            Color colorToUse = NormalColor;
            if (currentRoundRule == RuleType.ColorDifference)
            {
                // If color rule active, check if odd
                colorToUse = isOdd ? OddColor : NormalColor;
            }

            // Rotation Logic
            float rotationToUse = generatedNormalAngle;
            if (currentRoundRule == RuleType.RotationDifference)
            {
                // If rotation rule active, check if odd
                rotationToUse = isOdd ? generatedOddAngle : generatedNormalAngle;
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
        if (!isGameActive) return; // Ignore clicks if game over

        if (wasOddOne)
        {
            int roundScore = CalculateAndAddScore(); // Calculate score and get the result

            AdjustDifficulty(roundScore); // Adjust difficulty based on that result

            StartNewRound(); // Start next round
        }
        else
        {
            scoreHistory.Add(MinScorePerRound); // Treat a wrong click as a "bad round" for difficulty
            currentTimeRemaining = Mathf.Max(0, currentTimeRemaining - timePenaltyWrongClick);
            Debug.Log($"Wrong click! Penalized by {timePenaltyWrongClick} seconds.");
            UpdateTimerUI();
        }
    }

    // Scoring Logic
    private int CalculateAndAddScore()
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

        return finalRoundScore;
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

    // Called at the end of a round to adjust difficulty for the next one
    private void AdjustDifficulty(int latestRoundScore)
    {
        // Add latest score to history
        scoreHistory.Add(latestRoundScore);

        // Keep history at the defined size (remove oldest if too big)
        if (scoreHistory.Count > historySize)
        {
            scoreHistory.RemoveAt(0);
        }

        // Need enough history to make a decision
        if (scoreHistory.Count < historySize) return;

        // Calculate Average Score
        float averageScore = 0f;
        foreach (int score in scoreHistory)
        {
            averageScore += score;
        }
        averageScore /= scoreHistory.Count;

        Debug.Log($"Current Average Score (last {historySize} rounds): {averageScore}");


        if (averageScore >= thresholdsScoreHigh)
        {
            // Doing well: Make it harder (decrease duration)
            TileFadeDuration = Mathf.Max(minFadeDuration, TileFadeDuration - difficultyStep);
            Debug.Log("Difficulty Increased! New fade duration: " + TileFadeDuration);

            // Grant bonus time instead:
            currentTimeRemaining += timeRewardHighPerformance;
            Debug.Log("*** High Performance Streak! Bonus time added: " + timeRewardHighPerformance + "s ***");


            scoreHistory.Clear();
        }
        else if (averageScore <= thresholdScoreLow)
        {
            //Make it easier (increase duration)
            TileFadeDuration = Mathf.Min(maxFadeDuration, TileFadeDuration + difficultyStep);
            Debug.Log("Difficulty Decreased. New fade duration: " + TileFadeDuration);

            scoreHistory.Clear();
        }
    }

    // Linked to the "Stop Button"
    public void OnStopButtonPressed()
    {
        EndGame("GAME STOPPED");
    }

    // Linked to the "Restart Button" on the end panel
    public void OnRestartButtonPressed()
    {
        // Hide the end panel
        gameOverPanel.SetActive(false);
        // Re-enable the stop button
        stopButton.interactable = true;
        // Start fresh
        SetupNewGame();
    }

    // The logic that runs when the game finishes
    private void EndGame(string reason)
    {
        isGameActive = false;
        ClearGrid();

        if (gameOverPanel.transform.Find("GameOverTitle") != null)
        {
            gameOverPanel.transform.Find("GameOverTitle").GetComponent<TMP_Text>().text = reason;
        }

        finalScoreText.text = "Final Score: " + _currentScore + "\nRounds Played: " + roundsPlayedCount;
        gameOverPanel.SetActive(true);
        stopButton.interactable = false;
    }
}
