using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum E_AUDIO_TYPE
{
    bike,
    bomb,
    click,
    door,
    eya,
    game,
    gameover,
    jelly,
    laser,
    lastpang,
    main_01,
    main_02,
    main_03,
    pop,
    pop_fever,
    scroll,
    start,
    start_total,
    three_two_one,
    time_up,
    touch,
    warning
}

public class SoundManager : Singleton<SoundManager>
{
    private AudioSource background;
    public AudioSource _background
    {
        get
        {
            return background;
        }
    }

    private AudioSource[] effects = new AudioSource[8];
    public AudioSource[] _effects
    {
        get
        {
            return effects;
        }
    }

    private readonly string AUDIO_BASE_PATH = "Sounds/{0}";
    private readonly Array Audio_Types_ = Enum.GetValues(typeof(E_AUDIO_TYPE));
    private Dictionary<E_AUDIO_TYPE, AudioClip> audios = new Dictionary<E_AUDIO_TYPE, AudioClip>();

    float bgmVolume;
    float fxVolume;

    //     private void Update()
    //     {
    // #if UNITY_EDITOR
    //         if (Input.GetKeyDown(KeyCode.Return))
    //         {
    //             PlayMuyaho();
    //         }
    // #else
    //         if (Input.touchCount >= 4)
    //         {
    //             PlayMuyaho();
    //         }
    // #endif // UNITY_EDITOR
    //     }

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    void Init()
    {
        InitBackground();
        InitEffect();

        InitAudios();
    }

    private void InitBackground()
    {
        background = gameObject.AddComponent<AudioSource>();
        if (background != null)
        {
            background.playOnAwake = false;
            background.loop = true;
            background.reverbZoneMix = 0.0f;
            background.dopplerLevel = 0.0f;

            background.mute = false;

            bgmVolume = GetBGMVolume();
            background.volume = bgmVolume;
        }
    }

    private void InitEffect()
    {
        for (var i = 0; i < effects.Length; i++)
        {
            effects[i] = gameObject.AddComponent<AudioSource>();
            if (effects[i] != null)
            {
                effects[i].playOnAwake = false;
                effects[i].loop = false;
                effects[i].reverbZoneMix = 0.0f;
                effects[i].dopplerLevel = 0.0f;

                effects[i].mute = false;

                fxVolume = GetFXVolume();
                effects[i].volume = fxVolume;
            }
        }
    }

    private void InitAudios()
    {
        var Iterator = Audio_Types_.GetEnumerator();
        while (Iterator.MoveNext())
        {
            var Object = Iterator.Current;
            if (Object == null)
            {
                continue;
            }

            var AudioClip = Resources.Load<AudioClip>(string.Format(AUDIO_BASE_PATH, Object.ToString()));
            if (AudioClip == null)
            {
                continue;
            }

            audios.Add((E_AUDIO_TYPE)Object, AudioClip);
        }
    }

    public void ApplyBGMVolumeOption(float bgmVol)
    {
        PlayerPrefs.SetFloat("BGM_VOLUME", bgmVol);
        background.volume = bgmVol;
    }

    public void ApplyFXVolumeOption(float fxVol)
    {
        PlayerPrefs.SetFloat("FX_VOLUME", fxVol);

        foreach (AudioSource source in effects)
        {
            source.volume = fxVol;
        }
    }

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
    }

    public float GetFXVolume()
    {
        return PlayerPrefs.GetFloat("FX_VOLUME", 0.5f);
    }

    public void PlayBackground(E_AUDIO_TYPE _Audio_Type, float volume = 1f)
    {
        if (!audios.ContainsKey(_Audio_Type))
        {
            return;
        }

        if (background.isPlaying && background.clip == audios[_Audio_Type])
        {
            return;
        }

        background.Stop();
        background.clip = audios[_Audio_Type];
        background.volume = GetBGMVolume();
        background.Play();
    }

    public void PlayBackground(AudioClip _Audio_Type, float volume = 1f)
    {
        if (background.isPlaying && background.clip == _Audio_Type)
        {
            return;
        }

        background.Stop();
        background.clip = _Audio_Type;
        background.volume = GetBGMVolume();
        background.Play();
    }

    public void StopBackground()
    {
        background.Stop();
    }

    public void PlayEffect(E_AUDIO_TYPE _Audio_Type, bool loop = false)
    {
        if (!audios.ContainsKey(_Audio_Type))
        {
            Debug.LogErrorFormat("Error PlayEffect {0}", _Audio_Type);
            return;
        }

        bool bPlay = false;
        for (var i = 0; i < effects.Length; i++)
        {
            if (effects[i].isPlaying)
            {
                continue;
            }

            effects[i].clip = audios[_Audio_Type];
            effects[i].loop = loop;
            effects[i].volume = GetFXVolume();

            effects[i].Play();

            bPlay = true;
            break;
        }

        if (!bPlay)
        {
            Debug.LogErrorFormat("Error PlayEffect {0}, Played Audio Count {1}", _Audio_Type, effects.Length);
        }
    }

    public void StopAllEffect()
    {
        for (var i = 0; i < effects.Length; i++)
        {
            if (effects[i].isPlaying)
            {
                effects[i].Stop();
            }
        }
    }

    public void StopEffect()
    {
        for (var i = 0; i < effects.Length; i++)
        {
            effects[i].Stop();
        }
    }

    public void StopEffect(bool onlyLoop = false)
    {
        if (onlyLoop)
        {
            for (var i = 0; i < effects.Length; i++)
            {
                if (effects[i].loop)
                {
                    effects[i].Stop();
                }
            }
        }
        else
        {
            StopEffect();
        }
    }

    public void StopEffect(E_AUDIO_TYPE _Audio_Type)
    {
        for (var i = 0; i < effects.Length; i++)
        {
            if (effects[i].clip == audios[_Audio_Type])
            {
                effects[i].Stop();
            }
        }
    }
}
