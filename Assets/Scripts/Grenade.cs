using System.Collections;
using Unity.Jobs;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Grenade : MonoBehaviour
{
    [Header("Explosion")]
    public GameObject explosion;
    public GameObject model;

    [Header("Explosion sprite")]
    public GameObject explosionSprite;
    public Camera targetCamera;
    public bool matchCameraRotation = true;
    public bool flipSprite = false;

    [Header("Physics")]
    public Rigidbody rb;

    [Header("Explosion audio")]
    public AudioClip explosionSound;

    [Range(0f, 4f)]
    public float explosionVolume = 4f;

    [Tooltip("If enabled, the sound is played at the grenade's world position.")]
    public bool playSoundAtWorldPosition = true;

    private AudioSource audioSource;
    private Transform cameraTransform;
    private bool hasExploded;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>(); //get rigidbody of grenade
        }

        audioSource =
            GetComponent<AudioSource>(); //link audio source for explosion

        audioSource.playOnAwake = false;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            cameraTransform =
                targetCamera.transform;
        }
    }

    private void Start() //set explosion and explosion sprite to false
    {
        if (explosion != null)
        {
            explosion.SetActive(false);
        }

        if (explosionSprite != null)
        {
            explosionSprite.SetActive(false);
        }
    }

    private void LateUpdate() //return or reassign if any below variables are null
    {
        if (!hasExploded ||
            explosionSprite == null ||
            !explosionSprite.activeSelf)
        {
            return;
        }

        if (cameraTransform == null)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                cameraTransform =
                    targetCamera.transform;
            }
        }

        if (cameraTransform == null)
        {
            return;
        }

        if (matchCameraRotation) //for explosion to match camera
        {
            explosionSprite.transform.rotation =
                cameraTransform.rotation;
        }
        else
        {
            Vector3 direction =
                cameraTransform.position -
                explosionSprite.transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                explosionSprite.transform.rotation =
                    Quaternion.LookRotation(
                        direction.normalized,
                        cameraTransform.up
                    );
            }
        }

        if (flipSprite)
        {
            explosionSprite.transform.localRotation *=
                Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void OnTriggerEnter(Collider other) // when colliding with something else
    {
        if (hasExploded) //if exploded, retunr
        {
            return;
        }

        if (other.CompareTag("Environment") ||
            other.CompareTag("Zombie")) //if tag is envrionemtn or zombie, run explode function
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode() //explode function
    {
        if (hasExploded)
        {
            yield break; //end if the grenade has already exploded
        }

        hasExploded = true;

        PlayExplosionSound(); //explosion sound

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.isKinematic = true;
        }

        if (model != null) //force model to be false and hidden
        {
            model.SetActive(false);
        }

        if (explosion != null) //force explosion to be true and appear
        {
            explosion.SetActive(true);
        }
        
        if (explosionSprite != null) //force explosion sprite to be true and appear
        {
            explosionSprite.SetActive(true);
        }

        yield return new WaitForSeconds(0.7f); //destory grenade after exploding

        Debug.Log(
            "Grenade destroyed on environment."
        );

        Destroy(gameObject);
    }

    private void PlayExplosionSound() //exploding sound on grenade explosion
    {
        if (explosionSound == null)
        {
            return;
        }

        if (playSoundAtWorldPosition)
        {
            AudioSource.PlayClipAtPoint(
                explosionSound,
                transform.position,
                explosionVolume
            );
        }
        else
        {
            audioSource.PlayOneShot(
                explosionSound,
                explosionVolume
            );
        }
    }
}