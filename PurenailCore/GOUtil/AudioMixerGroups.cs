using UnityEngine;
using UnityEngine.Audio;

namespace PurenailCore.GOUtil;

public static class AudioMixerGroups
{
    static AudioMixerGroups()
    {
        foreach (var mixer in Resources.FindObjectsOfTypeAll<AudioMixer>())
        {
            if (mixer.name == "Actors") _actorsGroup = mixer.outputAudioMixerGroup;
            if (mixer.name == "Music") _musicGroup = mixer.outputAudioMixerGroup;
        }
    }

    // Master, UI, Actors, EnviroEffects, ShadeMixer, Music Options, Atmos, DamageEffects, Sound Options, Music, Music Effects
    private static readonly AudioMixerGroup? _actorsGroup;
    private static readonly AudioMixerGroup? _musicGroup;

    public static AudioMixerGroup Actors() => _actorsGroup!;

    public static AudioMixerGroup Music() => _musicGroup!;
}
