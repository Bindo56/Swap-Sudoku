using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{

    public static UI_Manager instance;
    // Start is called before the first frame update


    private void Awake()
    {
        if(instance == null)
        {
          instance = this;
        }
    }
    public void OpenGame()
    {
        SceneManager.LoadSceneAsync(0);
    }
    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button musicButton;
    public Button soundButton;
    public Button quitButton;
    public Button moreGamesButton;

    [Header("Button Icons")]
    public Image musicIcon;
    public Image soundIcon;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private const string MUSIC_KEY = "MusicEnabled";
    private const string SOUND_KEY = "SoundEnabled";

    private bool isMusicOn;
    private bool isSoundOn;

    void Start()
    {
        // Load settings
        isMusicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        isSoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;

        ApplyAudioSettings();
        UpdateIcons();

        // Button listeners
        if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
        if (soundButton != null) soundButton.onClick.AddListener(ToggleSound);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (moreGamesButton != null) moreGamesButton.onClick.AddListener(OpenMoreGames);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt(MUSIC_KEY, isMusicOn ? 1 : 0);
        ApplyAudioSettings();
        UpdateIcons();
    }

    void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt(SOUND_KEY, isSoundOn ? 1 : 0);
        ApplyAudioSettings();
        UpdateIcons();
    }

    void UpdateIcons()
    {
        if (musicIcon != null)
            musicIcon.sprite = isMusicOn ? musicOnSprite : musicOffSprite;

        if (soundIcon != null)
            soundIcon.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    void ApplyAudioSettings()
    {
        // Mute global audio listener for sound
        AudioListener.volume = isSoundOn ? 1f : 0f;

        // Find music source by tag and mute
        GameObject bgMusic = GameObject.FindWithTag("BGMusic");
        if (bgMusic != null)
        {
            AudioSource audio = bgMusic.GetComponent<AudioSource>();
            if (audio != null)
                audio.mute = !isMusicOn;
        }
    }

    void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    void OpenMoreGames()
    {
        Application.OpenURL("https://play.google.com/store/apps/developer?id=YourDeveloperName");
    }




}
