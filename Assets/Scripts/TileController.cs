using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    private bool _isOddOne;
    private GameManager _gameManager;

    // This function is called when Button component is clicked
    public void OnClick()
    {
        // Tell the manager if this is odd tile or not
        _gameManager.TileClicked(_isOddOne);
    }

    // Setup function called by GameManager when spawning the tile 
    public void SetupTile(bool isOddOne, Color tileColor, GameManager gameManager)
    {
        _isOddOne = isOddOne;
        _gameManager = gameManager;

        // Change color visually based on the rule
        GetComponent<Image>().color = tileColor;
    }
}
