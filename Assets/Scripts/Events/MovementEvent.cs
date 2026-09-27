using System;
using UnityEngine;

[Serializable]
public class MovementEvent : EventBase
{
    // ============================================================
    // 目标
    // ============================================================

    public bool random_move = true;

    public Vector2 target = Vector2.zero;


    // ============================================================
    // 移动
    // ============================================================

    public float move_time = 5f;

    public bool instant = false;


    // ============================================================
    // 随机漫步
    // ============================================================

    public float random_min_distance = 3f;

    public float random_max_distance = 5f;

    public float random_min_interval = 0.5f;

    public float random_max_interval = 2f;


    // ============================================================
    // 行为
    // ============================================================

    public bool interrupt = true;

    public bool force = false;


    // ============================================================
    // 初始化
    // ============================================================

    public MovementEvent(
        bool random_move = false,
        Vector2 target = default,

        float move_time = 5f,
        bool instant = false,

        float random_min_distance = 3f,
        float random_max_distance = 5f,

        float random_min_interval = 0.5f,
        float random_max_interval = 2f,

        bool interrupt = true,
        bool force = false
    )
    {
        this.random_move =
            random_move;

        this.target =
            target;

        this.move_time =
            move_time;

        this.instant =
            instant;

        this.random_min_distance =
            random_min_distance;

        this.random_max_distance =
            random_max_distance;

        this.random_min_interval =
            random_min_interval;

        this.random_max_interval =
            random_max_interval;

        this.interrupt =
            interrupt;

        this.force =
            force;
    }
}