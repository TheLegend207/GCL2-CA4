using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) //if tag is player
        {
            SceneManager.LoadScene("Win screen"); //go to win screen
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


}