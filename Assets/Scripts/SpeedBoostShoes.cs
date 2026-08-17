using System.Collections;
using UnityEngine;

public class SpeedBoostShoes : MonoBehaviour
{
    public PlayerController thePlayerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 45f * Time.deltaTime, 0f); //shoes will spin
    }

    
    }

