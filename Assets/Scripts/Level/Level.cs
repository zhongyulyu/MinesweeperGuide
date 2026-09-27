using System;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public EventManager e;
    public CellManager cellManager;

    public void InitLevel()
    { 
        LevelData levelData =
            LevelDataLoader.GetLevelData( 
                LevelInput.level_index 
            );
        
        LevelConfig levelConfig = levelData.levelConfig;

        cellManager.CreateCells(levelConfig.width, levelConfig.height, levelConfig.mine[LevelInput.level_game_diff], levelConfig.time_max, levelConfig.times);
        
        e.ClearEvents();

        if (levelData == null)
        {
            Debug.LogError(
                $"找不到关卡：{LevelInput.level_index}"
            );

            return;
        }


        // ========================================================
        // Source
        // ========================================================

        Dictionary<string, EventBase> source = new();
        Dictionary<string, Trigger[]> triggers = new();
        Dictionary<string, Timer> timers = new()
        {
            {"LevelTimer", e.level_timer},
            {"RoundCounter", e.round_timer},
            {"TimeLeft", e.time_left}
        };

        foreach (
            KeyValuePair<string, SourceData>
            pair in levelData.Source
        )
        {
            string name = pair.Key;
            SourceData data =
                pair.Value;

            int diff_index = LevelInput.level_touniu_diff;
            diff_index = Mathf.Clamp(
                diff_index,
                0,
                data.ID.Count - 1
            );
            int ID = data.ID[diff_index];
            EventBase @event = null;
            switch (data.EventType)
            {
                case 0:
                @event = e.GetEvent(0, ID);
                break;
                case 1:
                @event = e.GetEvent(1, ID);
                break;
                case 2:
                @event = new HandleEvent(ID, data.args);

                break;
            }

            source.Add(
                name,
                @event
            );
        }

        print(triggers);

        foreach (
            KeyValuePair<string, SetupData>
            pair in levelData.Setup
        )
        {
            string name = pair.Key;
            SetupData setupData = pair.Value;

            TriggerData triggerData = setupData.trigger;

            Trigger trigger = null;
            // 在Timers和Triggers里查找
            Trigger[] t = null;
            Timer t2 = null;

            triggers.TryGetValue(triggerData.source, out t);
            if (t != null)
            { 
                trigger = t[(int)triggerData.prm];
            }
            timers.TryGetValue(triggerData.source, out t2);
            if (t2 != null)
            {
                trigger = t2.TimePass(triggerData.prm);
            }


            switch (setupData.type)
            {
                case 0:
                    Trigger[] event_res = e.SetupEvent(source[setupData.source], Touniu(setupData.target), trigger, setupData.time);
                    triggers[name] = event_res;
                    break;
                case 1:
                    Timer timer = e.CreateTimer(name, trigger);
                    timers[name] = timer;
                    break;
                    
                case 2:
                    break;
            }
        }
    }


    #region Shortcut

    Trigger OnTime(float time)
    {
        return e.level_timer.TimePass(time);
    }

    TouniuHandler Touniu(int index)
    {
        return e.GetTouniuHandler(index);
    }

    #endregion
}