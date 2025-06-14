using System.Collections.Generic;
using UnityEngine;

public class DontDestroyManager : MonoBehaviour
{
    public string managerId;

    private static Dictionary<string, DontDestroyManager> instances = new();

    void Awake()
    {
        if (instances.ContainsKey(managerId))
        {
            Debug.LogWarning($"Duplicate manager with ID '{managerId}' detected. Destroying this.");
            Destroy(gameObject);
            return;
        }

        instances[managerId] = this;
        DontDestroyOnLoad(gameObject);
    }
}