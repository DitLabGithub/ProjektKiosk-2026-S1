using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource bgMusicSource;

    [Header("Audio Clips")]
    public AudioClip walkSound;
    public AudioClip pickupSound;
    public AudioClip placeSound;
    public AudioClip buttonSellSound;
    public AudioClip pickupFailSound;
    public AudioClip idScanned;
    public AudioClip mainBackgroundMusic;
    public AudioClip continueButtonClick;


    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
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
        if (sfxSource != null && buttonSellSound != null) {
            sfxSource.PlayOneShot(buttonSellSound, 1f); // these sounds are loud as shit bruv
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
        if (sfxSource != null && continueButtonClick != null) {
            sfxSource.PlayOneShot(continueButtonClick, 1f); // these sounds are loud as shit bruv
        }
    }
    public void PlayKioskBackgroundMusic()
    {
        if (bgMusicSource != null && mainBackgroundMusic != null) {
            bgMusicSource.clip = mainBackgroundMusic;
            bgMusicSource.loop = true;
            bgMusicSource.volume = 1.5f;
            bgMusicSource.enabled = true;
            bgMusicSource.Play();
        }
    }
}
