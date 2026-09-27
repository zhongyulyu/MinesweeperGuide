using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NumberDisplay : MonoBehaviour
{
    public List<SpriteList> numbers = new(); // 从个位开始

    public int number = 0;

    void OnEnable()
    {
        UpdateNumber();
    }

    void OnValidate()
    {
        UpdateNumber();
    }

    void Update()
    {
        UpdateNumber();
    }

    public void SetNumber(int value)
    {
        number = value;
        UpdateNumber();
    }

    void UpdateNumber()
    {
        int value = Mathf.Max(0, number);

        for (int i = 0; i < numbers.Count; i++)
        {
            int digit = value % 10;

            numbers[i].index = digit;

            value /= 10;
        }
    }
}