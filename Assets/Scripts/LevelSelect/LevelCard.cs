using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{
    public string level_name;

    public TMP_Text time_label;
    public TMP_Text level_label;

    public Button level_card_button;
    public Button game_diff_L;
    public Button game_diff_R;
    public Button touniu_diff_L;
    public Button touniu_diff_R;

    public SpriteList game_diff;
    public SpriteList touniu_diff;

    public GameObject level_setting;

    private int game_diff_v = 1;
    private int touniu_diff_v = 1;


    void Start()
    {
        level_setting.SetActive(false);

        level_card_button.onClick.AddListener(() =>
        {
            level_setting.SetActive(!level_setting.activeSelf);
        });


        // ========================================================
        // Game Difficulty
        // ========================================================

        game_diff_L.onClick.AddListener(() =>
        {
            game_diff_v = Mathf.Clamp(
                game_diff_v - 1,
                1,
                8
            );

            game_diff.index = game_diff_v;
        });

        game_diff_R.onClick.AddListener(() =>
        {
            game_diff_v = Mathf.Clamp(
                game_diff_v + 1,
                1,
                8
            );
            game_diff.index = game_diff_v;
        });


        // ========================================================
        // Touniu Difficulty
        // ========================================================

        touniu_diff_L.onClick.AddListener(() =>
        {
            touniu_diff_v = Mathf.Clamp(
                touniu_diff_v - 1,
                1,
                8
            );

            touniu_diff.index = touniu_diff_v;
        });

        touniu_diff_R.onClick.AddListener(() =>
        {
            touniu_diff_v = Mathf.Clamp(
                touniu_diff_v + 1,
                1,
                8
            );

            touniu_diff.index = touniu_diff_v;
        });


        // ========================================================
        // Initial
        // ========================================================

        game_diff.index = game_diff_v;
        touniu_diff.index = touniu_diff_v;
    }


    void OnValidate()
    {
        if (level_label != null)
            level_label.text = level_name;
    }
}