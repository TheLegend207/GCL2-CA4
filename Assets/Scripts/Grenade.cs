using System.Collections;
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
            rb = GetComponent<Rigidbody>();
        }

        audioSource =
            GetComponent<AudioSource>();

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

    private void Start()
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

    private void LateUpdate()
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

        if (matchCameraRotation)
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

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
        {
            return;
        }

        if (other.CompareTag("Environment") ||
            other.CompareTag("Zombie"))
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        if (hasExploded)
        {
            yield break;
        }

        hasExploded = true;

        PlayExplosionSound();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.isKinematic = true;
        }

        if (model != null)
        {
            model.SetActive(false);
        }

        if (explosion != null)
        {
            explosion.SetActive(true);
        }

        if (explosionSprite != null)
        {
            explosionSprite.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log(
            "Grenade destroyed on environment."
        );

        Destroy(gameObject);
    }

    private void PlayExplosionSound()
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