using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Manages all game audio including button clicks, sound effects, and background music.
/// For best button click responsiveness, ensure Unity's Audio Settings have:
/// - DSP Buffer Size: Best Latency (Edit > Project Settings > Audio)
/// - Button click audio clips should have no leading silence
/// </summary>
public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource bgMusicSource;
    public AudioSource buttonClickSource; // Dedicated for instant button feedback

    [Header("Audio Clips")]
    public AudioClip walkSound;
    public AudioClip pickupSound;
    public AudioClip placeSound;
    public AudioClip buttonSellSound;
    public AudioClip pickupFailSound;
    public AudioClip idScanned;
    public AudioClip mainBackgroundMusic;
    public AudioClip continueButtonClick;
    public AudioClip genericButtonClick; // For all other buttons


    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        // Configure button click source for instant, responsive playback
        if (buttonClickSource != null)
        {
            buttonClickSource.priority = 0; // Highest priority (0-255, lower = higher priority)
            buttonClickSource.volume = 1f;
            buttonClickSource.spatialBlend = 0f; // 2D sound
            buttonClickSource.playOnAwake = false;
            buttonClickSource.bypassEffects = true; // Skip audio processing for instant playback
            buttonClickSource.bypassListenerEffects = true;
            buttonClickSource.bypassReverbZones = true;
        }

        PlayKioskBackgroundMusic();
    }

    public void PlaySound(AudioClip clip) {
        if (clip != null) {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayWalkSound() => PlaySound(walkSound);
    public void PlayPickupFailSound() {
        if (sfxSource != null && pickupFailSound != null) {
            sfxSource.PlayOneShot(pickupFailSound);
        }
    }
    public void PlayPickupSound() {
        if (sfxSource != null && pickupSound != null) {
            sfxSource.PlayOneShot(pickupSound, 1f); // quieter sound for pickup cause it's fucking earrape
        }
    }
    public void PlayPlaceSound() => PlaySound(placeSound);
    public void PlayButtonSellSound() {
        if (buttonClickSource != null && buttonSellSound != null) {
            buttonClickSource.clip = buttonSellSound;
            buttonClickSource.Play();
        }
    }
    public void PlayIDScanned()
    {
        if (sfxSource != null && idScanned != null) {
            sfxSource.PlayOneShot(idScanned, 0.3f); // these sounds are loud as shit bruv
        }
    }
    public void PlayContinueButtonClick()
    {
        if (buttonClickSource != null && continueButtonClick != null) {
            buttonClickSource.clip = continueButtonClick;
            buttonClickSource.Play();
        }
    }
    public void PlayGenericButtonClick()
    {
        if (buttonClickSource != null && genericButtonClick != null) {
            buttonClickSource.clip = genericButtonClick;
            buttonClickSource.Play();
        }
    }
    public void PlayKioskBackgroundMusic()
    {
        if (bgMusicSource != null && mainBackgroundMusic != null) {
            bgMusicSource.clip = mainBackgroundMusic;
            bgMusicSource.loop = true;
            bgMusicSource.volume = 2.5f;
            bgMusicSource.priority = 0;
            bgMusicSource.spatialBlend = 0f;
            bgMusicSource.enabled = true;
            bgMusicSource.Play();
        }
    }
}
