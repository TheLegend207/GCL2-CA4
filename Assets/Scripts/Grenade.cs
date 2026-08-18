using System;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class Grenade : MonoBehaviour
{
    public GameObject explosion;
    public GameObject model;
    public Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        explosion.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == ("Environment"))
        {
            StartCoroutine(Explode());
        }
        if (other.tag == ("Zombie"))
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        rb.linearVelocity = Vector3.zero;
        model.SetActive(false);
        explosion.SetActive(true);
        yield return new WaitForSeconds (0.5f);
        Destroy(gameObject);
        Debug.Log("Grenade destroyed on environment.");
    }
}
