using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public static UnityEvent pauseEvent = new UnityEvent();
    public static UnityEvent resumeEvent = new UnityEvent();

    [Tooltip("The canvas holding the pause menu")] [SerializeField]
    Canvas pauseMenu;

    [SerializeField] AudioMixerSnapshot unpausedAudioMixerSnapshot;
    [SerializeField] AudioMixerSnapshot pausedAudioMixerSnapshot;

    static bool _paused;
    static float _oldTimeScale = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        Time.timeScale = 1f;
    }

    private void Start()
    {
        pauseMenu.gameObject.SetActive(false);
        _paused = false;
    }

    public static void Pause()
    {
        _oldTimeScale = Time.timeScale;
        _paused = true;
        SetMixerSnapshot(Instance.pausedAudioMixerSnapshot);
        Time.timeScale = 0f;
        Instance.pauseMenu.gameObject.SetActive(true);
        pauseEvent.Invoke();
    }

    public static void Resume()
    {
        if (_oldTimeScale < 0)
            return;
        _paused = false;
        SetMixerSnapshot(Instance.unpausedAudioMixerSnapshot);
        Time.timeScale = _oldTimeScale;
        Instance.pauseMenu.gameObject.SetActive(false);
        resumeEvent.Invoke();
    }

    public static bool IsPaused() => _paused;

    static void SetMixerSnapshot(AudioMixerSnapshot snapshot)
    {
        snapshot.TransitionTo(0f);
    }
}
