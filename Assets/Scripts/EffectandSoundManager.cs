using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectandSoundManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips; // 効果音リストを公開フィールドとして定義
    [SerializeField] private List<ParticleSystem> particleEffects; // エフェクトリスト

    public Dictionary<string, AudioSource> audioSources; // 効果音名とAudioSourceの辞書
    public Dictionary<string, ParticleSystem> particleSystems; // エフェクト名とParticleSystemの辞書


    private float defaultVolume = 0.1f;

    public static EffectandSoundManager Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // インスタンスを設定
            DontDestroyOnLoad(gameObject); // シーン間でオブジェクトを保持
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は新しいオブジェクトを破棄
        }
    }

    void Start()
    {
        audioSources = new Dictionary<string, AudioSource>();

        foreach (AudioClip clip in audioClips)
        {
            GameObject audioObject = new GameObject(clip.name);
            audioObject.transform.SetParent(this.transform);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSources[clip.name] = audioSource;
            audioSource.volume = defaultVolume; // デフォルトの音量を設定

        }

        particleSystems = new Dictionary<string, ParticleSystem>();

        foreach (ParticleSystem effect in particleEffects)
        {
            if (effect != null) // null チェック
            {
                GameObject effectObject = Instantiate(effect.gameObject, this.transform);
                effectObject.name = effect.name;
                particleSystems[effect.name] = effectObject.GetComponent<ParticleSystem>();
            }
            else
            {
                Debug.LogWarning("One of the particleEffects is null.");
            }

        }
    }

    public void PlaySound(string clipName)
    {
        if (audioSources.ContainsKey(clipName))
        {
            audioSources[clipName].Play();
        }
        else
        {
            Debug.LogWarning("Sound: " + clipName + " not found!");
        }
    }

    public void PlayEffect(string effectName, Vector3 playposition)
    {
        if (particleSystems.ContainsKey(effectName))
        {
            if (particleSystems[effectName] != null)
            {
                //ParticleSystem effect = particleSystems[effectName];
                //effect.transform.position = playposition;  // 位置を設定

                //effect.Play();  // エフェクトの再生
                ParticleSystem effect = Instantiate(particleSystems[effectName], playposition, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }
            else
            {
                Debug.LogWarning($"Effect: {effectName} not found!");
            }
        }
        else
        {
            Debug.LogWarning($"Effect: {effectName} not found!");
        }
    }

    public void SetLoop(string clipName, bool loop)
    {
        if (audioSources.ContainsKey(clipName))
        {
            audioSources[clipName].loop = loop;
        }
        else
        {
            Debug.LogWarning("Sound: " + clipName + " not found!");
        }
    }

    public void StopSound(string clipName)
    {
        if (audioSources.ContainsKey(clipName))
        {
            audioSources[clipName].Stop();
        }
        else
        {
            Debug.LogWarning($"Sound: {clipName} not found!");
        }
    }

    public void StopEffect(string effectName)
    {
        if (particleSystems.ContainsKey(effectName))
        {
            ParticleSystem effect = particleSystems[effectName];
            effect.Stop();
        }
        else
        {
            Debug.LogWarning($"Effect: {effectName} not found!");
        }
    }

    public void SetVolume(string clipName, float volume)
    {
        if (audioSources.ContainsKey(clipName))
        {
            audioSources[clipName].volume = Mathf.Clamp(volume, 0f, 1f); // 0.0から1.0の範囲で音量を設定
        }
        else
        {
            Debug.LogWarning($"Sound: {clipName} not found!");
        }
    }
}
