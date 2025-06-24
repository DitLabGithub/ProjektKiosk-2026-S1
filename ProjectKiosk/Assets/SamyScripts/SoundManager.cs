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
        sfxSource.PlayOneShot(pickupFailSound);
    }
    public void PlayPickupSound() {
        sfxSource.PlayOneShot(pickupSound, 0.3f); // quieter sound for pickup cause it's fucking earrape
    }
    public void PlayPlaceSound() => PlaySound(placeSound);
    public void PlayButtonSellSound() {
        sfxSource.PlayOneShot(buttonSellSound, 0.5f); // these sounds are loud as shit bruv
    }
    public void PlayIDScanned()
    {
        sfxSource.PlayOneShot(idScanned, 0.5f); // these sounds are loud as shit bruv
    }
    public void PlayContinueButtonClick()
    {
        sfxSource.PlayOneShot(continueButtonClick, 0.2f); // these sounds are loud as shit bruv
    }
    public void PlayKioskBackgroundMusic()
    {
        bgMusicSource.clip = mainBackgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.volume = 1f;
        bgMusicSource.Play();
    }
}
