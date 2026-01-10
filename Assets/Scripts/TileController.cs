using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    private bool _isOddOne;
    private GameManager _gameManager;

    // [Header("Fade Settings")]
    private Image _tileImage;
    private float _fadeDuration;
    private float _currentFadeTime;
    private bool _isFading = false;

    // This function is called when Button component is clicked
    public void OnClick()
    {
        // Tell the manager if this is odd tile or not
        _gameManager.TileClicked(_isOddOne);
    }

    // Setup function called by GameManager when spawning the tile 
    public void SetupTile(bool isOddOne, Color tileColor, GameManager gameManager, float duration)
    {
        _isOddOne = isOddOne;
        _gameManager = gameManager;
        _fadeDuration = duration;

        _tileImage = GetComponent<Image>();
        _tileImage.color = tileColor;

        if (!_isOddOne)
        {
            _currentFadeTime = 0;
            _isFading = true;
        }
    }

    // Updating every frame to handle smooth fading animation
    private void Update()
    {
        if (_isFading)
        {
            _currentFadeTime += Time.deltaTime;
            float fadeProgress = _currentFadeTime / _fadeDuration;

            // Lerp alpha from 1 (visible) to 0 (invisible)
            Color newColor = _tileImage.color;
            newColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
            _tileImage.color = newColor;

            // Stop fading once invisible
            if (fadeProgress >= 1f) _isFading = false;
        }
    }
}
