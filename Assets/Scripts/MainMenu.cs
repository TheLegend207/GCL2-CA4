using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource audioSource;
    public AudioClip hoverSound;
    void Update()
    {
        transform.Rotate(0f, 10f * Time.deltaTime, 0f); //Rotate camera constantly
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.PlayOneShot(hoverSound);
    }

    //Load to different scenes when pressing their respective buttons
    public void StartGame() 
    {
        SceneManager.LoadScene("Level 01");
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}