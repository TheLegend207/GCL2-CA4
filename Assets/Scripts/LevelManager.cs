using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public TMP_Text deathText;
    public UnityEngine.UI.Image fadeImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make both text and black screen invisible until when player died
        Color textColor = deathText.color;
        textColor.a = 0f;
        deathText.color = textColor;

        Color fadeColor = fadeImage.color;
        fadeColor.a = 0f;
        fadeImage.color = fadeColor;
    }
    
    public void PlayerDied()
    {
        StartCoroutine(RestartGame());
    }
    
    // Coroutine is just screen fading to black and the text of "you died" fading in before restarting level
    private IEnumerator RestartGame()
    {
        // Make sure the death text starts invisible
        Color textColor = deathText.color;
        textColor.a = 0f;
        deathText.color = textColor;

        // Fade screen to black over 2 seconds
        float fadeDuration = 2f;
        float elapsed = 0f;

        Color fadeColor = fadeImage.color;
        
        // Continue fading until 2 seconds has passed
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration); // Convert the elapsed time to a value between 0 and 1

            fadeColor.a = alpha; // Set the fade's image transparancy 
            fadeImage.color = fadeColor;

            yield return null;
        }

        // Make sure screen is completely black
        fadeColor.a = 1f;
        fadeImage.color = fadeColor;

        float textDuration = 2f;
        elapsed = 0f;

        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / textDuration); // Convert the elapsed time to a value between 0 and 1
            
            textColor.a = alpha; // Set the fade in text's transparancy 
            deathText.color = textColor;

            yield return null;
        }

        textColor.a = 1f;
        deathText.color = textColor;

        yield return new WaitForSeconds(2f); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the current level
    }
}
