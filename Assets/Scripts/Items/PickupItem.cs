using System.Collections;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemEffect effect;
    private Animator animator;
    public AudioClip pickUpClip;
    AudioSource sfx;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 1f;
        sfx.rolloffMode = AudioRolloffMode.Linear;
        sfx.maxDistance = 20f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (InventoryManager.Instance.AddItem(effect))
        {
            animator.SetBool("Picked", true);
            PlaySFX(pickUpClip, 1f);

            StartCoroutine(DestroyAfterDelay(0.5f));
        }
    }

    IEnumerator DestroyAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    protected void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfx.pitch = Random.Range(0.9f, 1.1f); // невеличка варіація, щоб не монотонно
        sfx.PlayOneShot(clip, volume);
    }
}