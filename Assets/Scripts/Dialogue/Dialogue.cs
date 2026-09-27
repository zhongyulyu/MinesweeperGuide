using System;
using UnityEngine;

public class Dialogue
{
    public string content;
    public string yes_text = "是";
    public string no_text = "否";

    public bool show_choise = false;
    public bool defult_choise = false;
    public Color content_color = Color.white;
    public float exist_time = 10f;

    
 
    public Dialogue(
        string content,
        string yes_text = "是",
        string no_text = "否",
        bool defult_choise = false,
        Color? content_color = null,
        float exist_time = 10f,
        bool show_choise = false
    )
    {
        this.content = content;
        this.yes_text = yes_text;
        this.no_text = no_text;
        this.defult_choise = defult_choise;
        this.content_color = content_color ?? Color.white;
        this.exist_time = exist_time;
        this.show_choise = show_choise;
    }
}