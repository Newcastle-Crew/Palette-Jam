using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    static MusicController Instance;

    AudioSource source;
    bool muted = false;
    bool should_play = false;

    public AudioClip boss_music;

    public SpriteRenderer muted_sprite_renderer;
    public Sprite muted_sprite;
    public Sprite unmuted_sprite;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        Instance = this;

        muted = PlayerPrefs.HasKey("muted");

        if (Instance.muted_sprite_renderer != null)
            muted_sprite_renderer.sprite = muted ? muted_sprite : unmuted_sprite;

        Instance.should_play = true;
        if (!muted) {
            source.Play();
        }
    }

    public static void Mute() {
        if (Instance == null) return;

        Instance.source.Stop();
        Instance.muted = true;
        if (Instance.muted_sprite_renderer != null)
            Instance.muted_sprite_renderer.sprite = Instance.muted_sprite;
        PlayerPrefs.SetInt("muted", 42);
    }

    public static void Unmute() {
        if (Instance == null) return;

        Instance.muted = false;
        PlayerPrefs.DeleteKey("muted");
        if (Instance.muted_sprite_renderer != null)
            Instance.muted_sprite_renderer.sprite = Instance.unmuted_sprite;
        if (!Instance.should_play) return;
        Instance.source.Play();
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

        if (!Instance.muted) {
            Instance.source.Play();
        }
        Instance.should_play = true;
    }

    public static void StopBossMusic() {
        if (Instance == null) return;
        Instance.source.Stop();
        Instance.should_play = false;
    }
}
