using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    static MusicController Instance;

    AudioSource source;

    public AudioClip boss_music;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        Instance = this;
    }

    public static void SuperSpeed() {
        if (Instance == null) return;

        Instance.source.pitch = 1.3f;
    }

    public static void StopSuperSpeed() {
        if (Instance == null) return;

        Instance.source.pitch = 1.0f;
    }

    public static void StartBossMusic() {
        if (Instance == null) return;
        Instance.source.Stop();
        Instance.source.clip = Instance.boss_music;
        Instance.source.Play();
    }

    public static void StopBossMusic() {
        if (Instance == null) return;
        Instance.source.Stop();
    }
}
