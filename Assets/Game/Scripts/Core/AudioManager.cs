using System;
using TowerDefense.Enemies;
using TowerDefense.World;
using UnityEngine;

namespace TowerDefense.Core
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] [Range(0f, 1f)] private float menuMusicVolume = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float gameMusicVolume = 0.2f;
        [SerializeField] private float musicTransitionDuration = 1.0f;

        [Header("UI & Global SFX")]
        [SerializeField] private AudioClip uiClickSuccess;
        [SerializeField] private AudioClip uiClickError;
        [SerializeField] private AudioClip towerBuildSound;
        [SerializeField] private AudioClip pvpSpendGoldSound;
        [SerializeField] [Range(0f, 1f)] private float uiSfxVolume = 0.8f;
        [SerializeField] [Range(0f, 1f)] private float combatSfxVolume = 1.0f;

        private AudioSource musicSource1;
        private AudioSource musicSource2;
        private bool isUsingSource1 = true;
        private Coroutine crossfadeCoroutine;

        private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSources()
        {
            EnsureAudioListener();

            musicSource1 = gameObject.AddComponent<AudioSource>();
            musicSource1.loop = true;
            musicSource1.playOnAwake = false;
            musicSource1.volume = menuMusicVolume;

            musicSource2 = gameObject.AddComponent<AudioSource>();
            musicSource2.loop = true;
            musicSource2.playOnAwake = false;
            musicSource2.volume = 0f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = uiSfxVolume;
        }

        private void EnsureAudioListener()
        {
            if (FindAnyObjectByType<AudioListener>() == null)
            {
                if (Camera.main != null)
                {
                    Camera.main.gameObject.AddComponent<AudioListener>();
                    Debug.Log("AudioManager: Added AudioListener to Main Camera.");
                }
                else
                {
                    gameObject.AddComponent<AudioListener>();
                    Debug.Log("AudioManager: Added AudioListener to AudioManager object.");
                }
            }
        }

        public void PlayMenuMusic() => PlayMusic(menuMusic, menuMusicVolume);
        public void PlayGameMusic() => PlayMusic(gameMusic, gameMusicVolume);

        private void PlayMusic(AudioClip clip, float targetVolume)
        {
            if (clip == null) return;
            
            AudioSource activeSource = isUsingSource1 ? musicSource1 : musicSource2;
            if (activeSource.clip == clip && activeSource.isPlaying)
            {
                activeSource.volume = targetVolume;
                return;
            }

            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }
            crossfadeCoroutine = StartCoroutine(CrossfadeMusic(clip, targetVolume));
        }

        private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, float targetVolume)
        {
            AudioSource activeSource = isUsingSource1 ? musicSource1 : musicSource2;
            AudioSource newSource = isUsingSource1 ? musicSource2 : musicSource1;

            float startActiveVolume = activeSource.volume;

            isUsingSource1 = !isUsingSource1;

            newSource.clip = newClip;
            newSource.volume = 0f;
            newSource.Play();

            float t = 0f;
            while (t < musicTransitionDuration)
            {
                t += Time.deltaTime;
                float normalizedTime = t / musicTransitionDuration;
                
                activeSource.volume = Mathf.Lerp(startActiveVolume, 0f, normalizedTime);
                newSource.volume = Mathf.Lerp(0f, targetVolume, normalizedTime);
                
                yield return null;
            }

            activeSource.volume = 0f;
            activeSource.Stop();
            newSource.volume = targetVolume;
            crossfadeCoroutine = null;
        }

        public void StopMusic()
        {
            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            musicSource1.Stop();
            musicSource2.Stop();
        }

        // UI & Global Interactions
        public void PlayClickSuccess() => PlayUI(uiClickSuccess);
        public void PlayClickError() => PlayUI(uiClickError);
        public void PlayTowerBuild() => PlayUI(towerBuildSound);
        public void PlaySpendGold() => PlayUI(pvpSpendGoldSound);

        private void PlayUI(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, uiSfxVolume);
            }
        }

        // Entity Specific Positional Sounds
        public void PlayPositionalSFX(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, combatSfxVolume);
            }
        }

        public void PlayEnemyDeath(EnemyConfig config, Vector3 position)
        {
            if (config != null && config.DeathSound != null)
            {
                PlayPositionalSFX(config.DeathSound, position);
            }
        }

        public void PlayTowerShoot(TowerConfig config, Vector3 position)
        {
            if (config != null && config.ShootSound != null)
            {
                PlayPositionalSFX(config.ShootSound, position);
            }
        }
        
        public void PlayProjectileHit(TowerConfig config, Vector3 position)
        {
            if (config != null && config.ProjectileHitSound != null)
            {
                PlayPositionalSFX(config.ProjectileHitSound, position);
            }
        }
    }
}
