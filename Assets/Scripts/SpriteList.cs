using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteList : MonoBehaviour
{
    public List<Sprite> sprites = new();

    [Range(0, 99)]
    public int index = 0;

    private SpriteRenderer sprite_renderer;

    void OnEnable()
    {
        UpdateSprite();
    }

    void OnValidate()
    {
        UpdateSprite();
    }

    void Update()
    {
        UpdateSprite();
    }
    void UpdateSprite()
    {
        if (sprite_renderer == null)
            sprite_renderer = GetComponent<SpriteRenderer>();

        if (sprites == null || sprites.Count == 0)
        {
            sprite_renderer.sprite = null;
            return;
        }

        index = Mathf.Clamp(index, 0, sprites.Count - 1);

        sprite_renderer.sprite = sprites[index];
    }
}