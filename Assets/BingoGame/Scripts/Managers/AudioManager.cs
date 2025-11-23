using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BingoGame.Network
{

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Background Music")]
        [SerializeField] private AudioClip lobbyMusic;
        [SerializeField] private AudioClip gameSceneMusic;
        [SerializeField] private float musicVolume = 0.5f;

        [Header("Button Sounds")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private float buttonClickVolume = 0.7f;

        [Header("Audio Sources")]
        private AudioSource musicSource;
        private AudioSource sfxSource;

        private void Awake()
        {

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudioSources()
        {

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;


            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = buttonClickVolume;

            Debug.Log("[AudioManager] Audio sources initialized");
        }

        private void Start()
        {
            Debug.Log("[AudioManager] Start() called");

            // Detect which scene we're in and play appropriate music
            DetectAndPlaySceneMusic();

            // Find and setup all buttons in the scene
            SetupButtonSounds();
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {

            // Play appropriate music for the new scene
            DetectAndPlaySceneMusic();

            StartCoroutine(SetupButtonSoundsDelayed(0.5f));
        }

        private void DetectAndPlaySceneMusic()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Check for Lobby/Menu scenes (including SampleScene as default lobby)
            if (currentSceneName.Contains("Lobby") || currentSceneName.Contains("Menu") ||
                currentSceneName.Contains("lobby") || currentSceneName.Contains("menu") ||
                currentSceneName == "SampleScene")
            {
                PlayMusic(lobbyMusic);
            }
            else if (currentSceneName.Contains("Game") || currentSceneName.Contains("game"))
            {
                PlayMusic(gameSceneMusic);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Unknown scene: '{currentSceneName}', no music assigned");
            }
        }

        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("clip is NULL");
                return;
            }

            if (musicSource == null)
            {
                Debug.LogError("Music source is NULL");
                InitializeAudioSources();
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                Debug.Log($"Already playing music");
                return;
            }

            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PlayButtonClick()
        {
            if (buttonClickSound == null)
            {
                Debug.LogWarning("No button click sound assigned");
                return;
            }

            sfxSource.PlayOneShot(buttonClickSound, buttonClickVolume);
        }

        public void SetupButtonSounds()
        {
            // Find all Button components in the scene
            Button[] allButtons = FindObjectsOfType<Button>(true); // includeInactive = true

            int count = 0;
            foreach (Button button in allButtons)
            {

                bool alreadyHasListener = false;

                button.onClick.AddListener(PlayButtonClick);
                count++;
            }

            Debug.Log($"[AudioManager] Setup click sounds for {count} buttons");
        }

        private IEnumerator SetupButtonSoundsDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetupButtonSounds();

            // In GameScene, we need to wait for players to spawn
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName.Contains("Game"))
            {
                // Keep checking for new buttons (player UI spawns later)
                StartCoroutine(CheckForNewButtonsRoutine());
            }
        }

        private IEnumerator CheckForNewButtonsRoutine()
        {
            float checkDuration = 10f; // Check for 10 seconds
            float checkInterval = 1f; // Check every second
            float elapsed = 0f;

            Debug.Log("[AudioManager] Started checking for new buttons in GameScene");

            while (elapsed < checkDuration)
            {
                yield return new WaitForSeconds(checkInterval);
                elapsed += checkInterval;

                // Setup sounds for any new buttons
                SetupButtonSounds();
            }

            Debug.Log("[AudioManager] Finished checking for new buttons");
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            buttonClickVolume = Mathf.Clamp01(volume);
            sfxSource.volume = buttonClickVolume;
        }
    }
}
