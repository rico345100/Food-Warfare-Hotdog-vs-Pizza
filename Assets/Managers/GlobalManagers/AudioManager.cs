using UnityEngine;

public class AudioManager : GlobalSingleton<AudioManager> {
    private AudioSource m_MusicChannel;
    private AudioSource m_SFXChannel;

    [Header("Music")]
    [SerializeField] private AudioClip m_MainMusic = null;
    [SerializeField] private AudioClip m_BattleMusic = null;
    [SerializeField] private AudioClip m_GameOverMusic = null;

    [Header("SFX - UI")]
    [SerializeField] private AudioClip m_ClickSound = null;
    [SerializeField] private AudioClip m_CancelSound = null;
    [SerializeField] private AudioClip m_ErrorSound = null;

    [Header("SFX - Misc")]
    [SerializeField] private AudioClip[] m_FireSounds = null;
    [SerializeField] private AudioClip[] m_ExplosionSounds = null;
    
    public AudioClip MainMusic => m_MainMusic;
    public AudioClip BattleMusic => m_BattleMusic;
    public AudioClip GameOverMusic => m_GameOverMusic;
    
    protected override void OnInit() {
        m_MusicChannel = gameObject.AddComponent<AudioSource>();
        m_MusicChannel.loop = true;

        m_SFXChannel = gameObject.AddComponent<AudioSource>();
    }

    public static void PlaySound(AudioClip clip) {
        Instance.m_SFXChannel.clip = clip;
        Instance.m_SFXChannel.Play();
    }

    public static void PlayMusic(AudioClip clip) {
        Instance.m_MusicChannel.clip = clip;
        Instance.m_MusicChannel.Play();

        if (Instance.m_MusicChannel.loop == false) {
            Instance.m_MusicChannel.loop= true;
        }
    }

    public static void PlayMusicOnce(AudioClip clip) {
        PlayMusic(clip);
        Instance.m_MusicChannel.loop = false;
    }

    public static void StopMusic() {
        Instance.m_MusicChannel.Stop();
    }

    public static void CreateAudioObject(Vector3 position, AudioClip clip, float time) {
        GameObject audioObject = InGamePoolManager.Instance.Get("AudioObject", position, Quaternion.identity);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 0.5f;
        audioSource.Stop();
        audioSource.Play();

        InGamePoolManager.Instance.Return(audioObject, time);
    }

    public static AudioClip GetFireSound() {
        return Instance.m_FireSounds[Random.Range(0, Instance.m_FireSounds.Length)];
    }

    public static AudioClip GetExplosionSound() {
        return Instance.m_ExplosionSounds[Random.Range(0, Instance.m_ExplosionSounds.Length)];
    }

    public static void PlayClickSound() {
        Instance.m_SFXChannel.PlayOneShot(Instance.m_ClickSound);
    }

    public static void PlayCancelSound() {
        Instance.m_SFXChannel.PlayOneShot(Instance.m_CancelSound);
    }

    public static void PlayErrorSound() {
        Instance.m_SFXChannel.PlayOneShot(Instance.m_ErrorSound);
    }
}
