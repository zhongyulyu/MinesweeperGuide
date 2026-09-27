using System;
using UnityEngine;

[Serializable]
public class SelectEvent : EventBase
{
    public string content;
    public Color color = Color.white;
    public string opt_a;
    public string opt_b;
    public bool default_choise; // true 表示默认选 B
}
