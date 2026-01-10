using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject TilePrefab;
    public Transform GridContainer;

    [Header("Color Rule Settings")]
    public Color NormalColor = Color.white;
    public Color OddColor = Color.red;

    private List<GameObject> _activeTiles = new List<GameObject>();

    private void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        ClearGrid();

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
            tileScript.SetupTile(isOdd, colorToUse, this);

            // Keep track of it
            _activeTiles.Add(newTileObj);
        }
    }

    // Handle input received from tiles
    public void TileClicked(bool wasOddOne)
    {
        if (wasOddOne)
        {
            Debug.Log("CORRECT CLICK! Moving to next round....");
            // Call StartNewRound() here instantly
        }
        else
        {
            Debug.Log("WRONG CLICK! Penalty applied.");
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
