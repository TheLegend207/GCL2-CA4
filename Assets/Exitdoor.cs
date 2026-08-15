using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Win screen");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


}