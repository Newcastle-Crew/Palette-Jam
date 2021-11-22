using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundEffect {
    public AudioClip[] clips;
    public float volume;

    public void Play() {
        SoundEffectPlayer.Play(ref this);
    }
}

[RequireComponent(typeof(AudioSource))]
public class SoundEffectPlayer : MonoBehaviour
{
    static SoundEffectPlayer Instance;
    AudioSource audio_source;

    void Awake()
    {
        audio_source = GetComponent<AudioSource>();
        Instance = this;
    }

    public static void Play(ref SoundEffect effect)
    {
        if(Instance == null) return;
        if (effect.clips == null) return;
        if (effect.clips.Length == 0) return;

        var i = Random.Range(0, effect.clips.Length);
        var clip = effect.clips[i];
        Instance.audio_source.PlayOneShot(clip, effect.volume);
    }
}
