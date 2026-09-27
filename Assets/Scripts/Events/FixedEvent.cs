    using System;
    using UnityEngine;

    [Serializable]
    public class FixedEvent : EventBase
    {
        public string content;
        public Color color = Color.white;
        public bool ca_interupt = false;
    }
