using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public List<TouniuHandler> tounius = new();

    public Timer level_timer;
    public Timer round_timer;
    public Timer time_left;

    public ToolManager toolManager;

    // ============================================================
    // 已登记事件
    // ============================================================

    private class EventData
    {
        public EventBase @event;
        public TouniuHandler target;
        public Trigger trigger;
        public float exist_time;

        // 事件触发后产生的结果 Trigger
        public Trigger[] result;
    }

    private List<EventData> events = new();


    // ============================================================
    // Unity
    // ============================================================

    void Awake()
    {
        EventDatabase.Init();
        
        level_timer = CreateTimer(
            "LevelTimer",
            new Trigger(() => true)
        );
        time_left = CreateTimer(
            "TimeLeft",
            new Trigger(() => false)
        );
        time_left.revesed = true;
        round_timer = CreateTimer(
            "RoundTimer",
            new Trigger(() => false)
        );
    }



    void Update()
    {
        for (int i = events.Count - 1; i >= 0; i--)
        {
            EventData e = events[i];

            if (e.trigger == null)
                continue;

            if (e.trigger.Triggered == null)
                continue;

            if (!e.trigger.Triggered())
                continue;


            // ====================================================
            // 触发事件
            // ====================================================

            e.target.PushEvent(
                e.@event,
                e.exist_time,
                e.result
            );


            // ====================================================
            // 事件已经触发
            // ====================================================

            events.RemoveAt(i);
        }
    }

    // ============================================================
    // 创建 Timer
    // ============================================================

    public Timer CreateTimer(
        string name,
        Trigger trigger
    )
    {
        GameObject obj = new GameObject(name);

        Timer timer = obj.AddComponent<Timer>();

        timer.Init(trigger);

        return timer;
    }


    // ============================================================
    // 登记事件
    //
    // 返回值：
    //
    // FixedEvent
    // [0] = 事件结束
    //
    // SelectEvent
    // [0] = Yes
    // [1] = No
    //
    // 其他事件
    // null
    // ============================================================

    public Trigger[] SetupEvent(
        EventBase @event,
        TouniuHandler target,
        Trigger trigger,
        float exist_time
    )
    {
        if (@event == null)
            return null;

        if (target == null)
            return null;

        if (trigger == null)
            return null;


        // ========================================================
        // 创建结果 Trigger
        //
        // 这里先全部设置为 false
        // 等事件真正开始时，再由 PushEvent 替换
        // ========================================================

        Trigger[] result = null;


        // ========================================================
        // FixedEvent
        // ========================================================

        if (@event is FixedEvent)
        {
            FixedEvent fixedEvent = (FixedEvent)@event;
            result = new Trigger[]
            {
                new Trigger(() => false),

                // Interupt
                new Trigger(() => false)
            };
        }


        // ========================================================
        // SelectEvent
        // ========================================================

        else if (@event is SelectEvent)
        {
            result = new Trigger[]
            {
                // Yes
                new Trigger(() => false),

                // No
                new Trigger(() => false),

                // Interupt
                new Trigger(() => false)
            };
        }


        // ========================================================
        // 登记
        // ========================================================

        events.Add(
            new EventData
            {
                @event = @event,
                target = target,
                trigger = trigger,
                exist_time = exist_time,
                result = result
            }
        );


        return result;
    }


    // ============================================================
    // 获取 TouniuHandler
    // ============================================================

    public TouniuHandler GetTouniuHandler(int index)
    {
        if (index < 0 || index >= tounius.Count)
            return null;

        return tounius[index];
    }


    // ============================================================
    // 获取事件
    // ============================================================

    public EventBase GetEvent(
        int type,
        int id
    )
    {
        if (type < 0 ||
            type >= EventDatabase.Events.Count)
        {
            return null;
        }

        foreach (EventBase @event in EventDatabase.Events[type])
        {
            if (@event != null &&
                @event.id == id)
            {
                return @event;
            }
        }

        return null;
    }

    public void ClearEvents()
    {
        events.Clear();
    }

    public void GameOver()
    {
        ClearEvents();
        foreach (TouniuHandler touniuHandler in tounius)
        {
            touniuHandler.audio_source.Stop();
        }
    }
}