using UnityEngine;
using UnityEngine.UI;

public class AudioSlidersUI : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    AudioSettingsSO D;

    static float LinearToDb(float x) => Mathf.Log10(Mathf.Clamp(x, .0001f, 1f)) * 20f;
    static float DbToLinear(float db) => Mathf.Pow(10f, db / 20f);

    void Awake()
    {
        D = AudioSettingsHolder.Data;
        if (D == null)
        {
            Debug.LogError("[AudioSlidersUI] AudioSettingsHolder.Data not found!");
            enabled = false;
            return;
        }

        if (musicSlider == null || sfxSlider == null)
        {
            var sliders = GetComponentsInChildren<Slider>(true);
            if (musicSlider == null && sliders.Length > 0) musicSlider = sliders[0];
            if (sfxSlider == null && sliders.Length > 1) sfxSlider = sliders[1];
        }
        if (musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("[AudioSlidersUI] Sliders not assigned!");
            enabled = false;
            return;
        }

        musicSlider.SetValueWithoutNotify(DbToLinear(D.musicDb));
        sfxSlider.SetValueWithoutNotify(DbToLinear(D.sfxDb));

        musicSlider.onValueChanged.AddListener(OnMusic);
        sfxSlider.onValueChanged.AddListener(OnSfx);
    }

    void OnMusic(float valLin)
    {
        D.musicDb = LinearToDb(valLin);
        Push();
    }
    void OnSfx(float valLin)
    {
        D.sfxDb = LinearToDb(valLin);
        Push();
    }
    void Push()
    {
        D.Save();
        D.Raise();
    }
}