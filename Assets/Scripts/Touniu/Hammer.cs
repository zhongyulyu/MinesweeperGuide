using UnityEngine;

public class Hammer : MonoBehaviour
{
    // ============================================================
    // Inspector
    // ============================================================

    public AudioSource audio_source;

    public AudioClip audio_clip_1;
    public AudioClip audio_clip_2;


    // ============================================================
    // Unity
    // ============================================================

    private void Awake()
    {
        if (audio_source == null)
        {
            audio_source = GetComponent<AudioSource>();
        }
    }


    // ============================================================
    // Public API
    // ============================================================

    public void PlaySFX()
    {
        if (audio_source == null)
            return;

        if (audio_clip_1 == null ||
            audio_clip_2 == null)
            return;

        audio_source.Stop();

        audio_source.clip = audio_clip_1;
        audio_source.Play();

        Invoke(
            nameof(PlaySecond),
            audio_clip_1.length
        );
    }


    // ============================================================
    // 播放第二段
    // ============================================================

    private void PlaySecond()
    {
        if (audio_source == null)
            return;

        audio_source.clip = audio_clip_2;
        audio_source.Play();
    }
}