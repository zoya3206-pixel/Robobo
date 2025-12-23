using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VolumeButtonsControl : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private TMP_Text volumeText;

    [SerializeField] private float volumeStep = 0.1f; 

    private const float DEFAULT_VOLUME = 0.2f;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", DEFAULT_VOLUME);

        if (musicSource != null)
        {
            musicSource.volume = savedVolume;
            musicSource.loop = true;

            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        if (minusButton != null)
            minusButton.onClick.AddListener(DecreaseVolume);

        if (plusButton != null)
            plusButton.onClick.AddListener(IncreaseVolume);

        UpdateVolumeText(savedVolume);
    }

    public void DecreaseVolume()
    {
        if (musicSource == null) return;

        float newVolume = Mathf.Clamp01(musicSource.volume - volumeStep);
        musicSource.volume = newVolume;
        PlayerPrefs.SetFloat("MusicVolume", newVolume);
        PlayerPrefs.Save();
        UpdateVolumeText(newVolume);
    }

    public void IncreaseVolume()
    {
        if (musicSource == null) return;

        float newVolume = Mathf.Clamp01(musicSource.volume + volumeStep);
        musicSource.volume = newVolume;
        PlayerPrefs.SetFloat("MusicVolume", newVolume);
        PlayerPrefs.Save();
        UpdateVolumeText(newVolume);
    }

    void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
            volumeText.text = $"Music: {Mathf.RoundToInt(volume * 100)}%";
    }

    // Если нужно сбросить на дефолт
    public void ResetToDefault()
    {
        if (musicSource != null)
        {
            musicSource.volume = DEFAULT_VOLUME;
            PlayerPrefs.SetFloat("MusicVolume", DEFAULT_VOLUME);
            PlayerPrefs.Save();
            UpdateVolumeText(DEFAULT_VOLUME);
        }
    }
}