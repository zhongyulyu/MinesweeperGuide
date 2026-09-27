using System.Collections.Generic;
using UnityEngine;

public class HandleEvent : EventBase
{
    public string name;
    public int func_id;
    public List<List<object>> args;

    public HandleEvent(
        int func_id,
        List<List<object>> args = null
    )
    {
        this.func_id = func_id;
        this.args = args ?? new List<List<object>>();
    } 
}