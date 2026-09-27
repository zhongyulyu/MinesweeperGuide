using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Slider main_volume_slider;
    public Slider music_slider;
    public Slider sfx_slider;
    public Slider vo_slider;

    public TMP_Dropdown resolution;
    public Toggle fullscreen;

    private Resolution[] resolutions;

    void Start()
    {
        // =========================
        // 音量
        // =========================

        main_volume_slider.value = PlayerPrefs.GetFloat("MainVolume", 1f);
        music_slider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfx_slider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        vo_slider.value = PlayerPrefs.GetFloat("VOVolume", 1f);

        main_volume_slider.onValueChanged.AddListener(SetMainVolume);
        music_slider.onValueChanged.AddListener(SetMusicVolume);
        sfx_slider.onValueChanged.AddListener(SetSFXVolume);
        vo_slider.onValueChanged.AddListener(SetVOVolume);

        SetMainVolume(main_volume_slider.value);
        SetMusicVolume(music_slider.value);
        SetSFXVolume(sfx_slider.value);
        SetVOVolume(vo_slider.value);


        // =========================
        // 分辨率
        // =========================

        resolutions = Screen.resolutions;

        resolution.ClearOptions();

        var options = new System.Collections.Generic.List<string>();

        int current_resolution = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option =
                resolutions[i].width + " x " +
                resolutions[i].height;

            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                current_resolution = i;
            }
        }

        resolution.AddOptions(options);

        resolution.value = PlayerPrefs.GetInt(
            "Resolution",
            current_resolution
        );

        resolution.RefreshShownValue();

        resolution.onValueChanged.AddListener(SetResolution);

        SetResolution(resolution.value);


        // =========================
        // 全屏
        // =========================

        bool is_fullscreen = PlayerPrefs.GetInt(
            "Fullscreen",
            Screen.fullScreen ? 1 : 0
        ) == 1;

        fullscreen.isOn = is_fullscreen;

        fullscreen.onValueChanged.AddListener(SetFullscreen);

        SetFullscreen(is_fullscreen);
    }


    // =========================
    // 音量
    // =========================

    void SetMainVolume(float value)
    {
        PlayerPrefs.SetFloat("MainVolume", value);
    }

    void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void SetVOVolume(float value)
    {
        PlayerPrefs.SetFloat("VOVolume", value);
    }


    // =========================
    // 分辨率
    // =========================

    void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length)
            return;

        Resolution selected = resolutions[index];

        Screen.SetResolution(
            selected.width,
            selected.height,
            Screen.fullScreen
        );

        PlayerPrefs.SetInt("Resolution", index);
    }


    // =========================
    // 全屏
    // =========================

    void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;

        PlayerPrefs.SetInt(
            "Fullscreen",
            value ? 1 : 0
        );

        PlayerPrefs.Save();
    }
}