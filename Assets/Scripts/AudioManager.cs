using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class AudioItem
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
    }

    [SerializeField] private AudioItem[] audioItems;

    private Dictionary<string, AudioSource> _loopSources = new();
    private Dictionary<string, AudioClip> _clips = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (var item in audioItems)
        {
            if (item.clip == null) continue;

            _clips[item.id] = item.clip;

            if (item.loop)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.clip = item.clip;
                src.loop = true;
                src.volume = item.volume;
                _loopSources[item.id] = src;
            }
        }
    }

    public void Play(string id, float volume = 1f)
    {
        if (!_clips.ContainsKey(id) || _clips[id] == null) return;
        AudioSource.PlayClipAtPoint(_clips[id], Vector3.zero, volume);
    }

    public void PlayRandom(params string[] ids)
    {
        if (ids == null || ids.Length == 0) return;
        int rand = Random.Range(0, ids.Length);
        Play(ids[rand]);
    }

    public void PlayLoop(string id)
    {
        if (_loopSources.TryGetValue(id, out var src))
        {
            if (!src.isPlaying)
                src.Play();
        }
    }

    public void StopLoop(string id)
    {
        if (_loopSources.TryGetValue(id, out var src))
        {
            if (src.isPlaying)
                src.Stop();
        }
    }
}