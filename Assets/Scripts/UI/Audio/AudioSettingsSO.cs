using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Runtime Settings (sliders only)")]
public class AudioSettingsSO : ScriptableObject
{
    [Range(-80, 0)] public float musicDb = 0;
    [Range(-80, 0)] public float sfxDb = 0;

    public delegate void SettingsChanged();
    public event SettingsChanged OnChanged;
    public void Raise() => OnChanged?.Invoke();

    const string K_MUSICDB = "A_MusicDb";
    const string K_SFXDB = "A_SfxDb";

    public void Save()
    {
        PlayerPrefs.SetFloat(K_MUSICDB, musicDb);
        PlayerPrefs.SetFloat(K_SFXDB, sfxDb);
    }
    public void Load()
    {
        musicDb = PlayerPrefs.GetFloat(K_MUSICDB, 0);
        sfxDb = PlayerPrefs.GetFloat(K_SFXDB, 0);
    }
}