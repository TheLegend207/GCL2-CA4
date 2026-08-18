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

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            fadeColor.a = alpha;
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
            float alpha = Mathf.Clamp01(elapsed / textDuration);
            
            textColor.a = alpha;
            deathText.color = textColor;

            yield return null;
        }

        textColor.a = 1f;
        deathText.color = textColor;

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the current level
    }
}
