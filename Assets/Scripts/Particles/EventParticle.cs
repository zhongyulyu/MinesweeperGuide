using System.Collections;
using UnityEngine;

public class EventParticle : MonoBehaviour
{
    // ============================================================
    // Particle
    // ============================================================

    public ParticleSystem tracking_particle;
    public ParticleSystem target_particle;

    public AudioSource audioSource;


    // ============================================================
    // 设置位置
    // ============================================================

    public void Play(
        Vector2 tracking_position,
        Vector2 target_position
    )
    {
        if (tracking_particle == null ||
            target_particle == null)
        {
            Destroy(gameObject);
            return;
        }

        // ========================================================
        // 设置初始位置
        // ========================================================

        tracking_particle.transform.position =
            tracking_position;

        target_particle.transform.position =
            target_position;


        // ========================================================
        // 播放粒子
        // ========================================================

        tracking_particle.Play();
        target_particle.Play();
        audioSource.Play();


        // ========================================================
        // 开始移动
        // ========================================================

        StartCoroutine(
            MoveCoroutine(
                tracking_particle.transform,
                tracking_position,
                target_position
            )
        );
    }


    // ============================================================
    // 移动
    // ============================================================

    private IEnumerator MoveCoroutine(
        Transform tracking,
        Vector2 start,
        Vector2 target
    )
    {
        const float duration = 0.5f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            tracking.position =
                Vector2.Lerp(
                    start,
                    target,
                    t
                );

            yield return null;
        }


        // ========================================================
        // 确保最终位置准确
        // ========================================================

        tracking.position = target;


        // ========================================================
        // 自动销毁
        // ========================================================

        yield return new WaitForSeconds(0.5f);

        Destroy(this);
    }
}