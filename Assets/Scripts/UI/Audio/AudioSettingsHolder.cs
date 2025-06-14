using UnityEngine;
public class AudioSettingsHolder : MonoBehaviour
{
    public AudioSettingsSO data;
    public static AudioSettingsSO Data { get; private set; }

    void Awake()
    {
        if (Data == null)
        {
            Data = data;
            Data.Load();
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}