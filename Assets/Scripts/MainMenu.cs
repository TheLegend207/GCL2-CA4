using UnityEngine;

public class MenuCam : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 10f * Time.deltaTime, 0f); //Rotate camera constantly
    }
}