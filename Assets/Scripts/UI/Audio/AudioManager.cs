using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] string musicParam = "MusicVol";
    [SerializeField] string sfxParam = "SFXVol";

    AudioSettingsSO D;
    void Awake()
    {
        D = AudioSettingsHolder.Data;
        Apply();
        D.OnChanged += Apply;
    }
    void OnDestroy() => D.OnChanged -= Apply;

    void Apply()
    {
        mixer.SetFloat(musicParam, D.musicDb);
        mixer.SetFloat(sfxParam, D.sfxDb);
    }
}