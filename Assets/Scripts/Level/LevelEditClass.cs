using System;
using UnityEngine;

public class Trigger
{
    public Func<bool> Triggered;

    public Trigger(Func<bool> trigger)
    {
        Triggered = trigger;
    }

    // ============================================================
    // 更换 Trigger
    // ============================================================

    public void SetTrigger(Func<bool> trigger)
    {
        Triggered = trigger;
    }

    // ============================================================
    // 与
    // ============================================================

    public Trigger And(Trigger other)
    {
        return new Trigger(() =>
            Triggered != null &&
            other != null &&
            other.Triggered != null &&
            Triggered() &&
            other.Triggered()
        );
    }

    // ============================================================
    // 或
    // ============================================================

    public Trigger Or(Trigger other)
    {
        return new Trigger(() =>
            (Triggered != null && Triggered()) ||
            (other != null && other.Triggered != null && other.Triggered())
        );
    }

    // ============================================================
    // 非
    // ============================================================

    public Trigger Not()
    {
        return new Trigger(() =>
            Triggered != null &&
            !Triggered()
        );
    }
}

public class Timer : MonoBehaviour
{
    public Trigger start_timer;

    public float time;
    public bool started;
    public bool revesed;

    public void Init(Trigger start_timer)
    {
        this.start_timer = start_timer;
    }

    void Update()
    {
        if (!started)
        {
            if (start_timer != null && start_timer.Triggered())
            { 
                started = true;
            }
        }

        if (started)
        {
            time += Time.deltaTime;
        }
    }
    
   public void SetPulse(bool b)
    {
        started = b;
    }

    public Trigger TimePass(float t)
    {
        return new Trigger(() => revesed?(time <= t):(time >= t));
    }
}