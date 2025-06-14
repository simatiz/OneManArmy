using UnityEngine.UI;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider effectHealthSlider;

    private float lerpSpeed = 0.03f;

    private float targetHealth;

    void Start()
    {
        targetHealth = healthSlider.maxValue;
    }

    void Update()
    {
        if (Mathf.Abs(effectHealthSlider.value - targetHealth) > 0.01f)
        {
            effectHealthSlider.value = Mathf.Lerp(effectHealthSlider.value, targetHealth, lerpSpeed);
        }
    }

    public void SetMaxHealth(float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        effectHealthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        effectHealthSlider.value = maxHealth;

        targetHealth = maxHealth;
    }

    public void UpdateHealth(float currentHealth)
    {
        targetHealth = currentHealth;
        healthSlider.value = currentHealth;
    }
}