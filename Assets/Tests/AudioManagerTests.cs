using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SpaceShooter;

namespace SpaceShooter.Tests
{
    /// <summary>
    /// TDD tests for <see cref="AudioManager"/>.
    /// </summary>
    public class AudioManagerTests
    {
        private AudioManager CreateManager()
        {
            var go = new GameObject("AudioManager");
            var am = go.AddComponent<AudioManager>();
            am.Initialize();
            return am;
        }

        [UnityTest]
        public IEnumerator Singleton_InstanceExists()
        {
            var am = CreateManager();
            yield return null;
            Assert.IsNotNull(AudioManager.Instance);
            Assert.AreSame(am, AudioManager.Instance);
            Object.DestroyImmediate(am.gameObject);
        }

        [Test]
        public void PlaySFX_SetsLastPlayedClip()
        {
            var am = CreateManager();
            var clip = AudioClip.Create("sfx", 100, 1, 44100, false);
            am.PlaySFX(clip);
            Assert.AreSame(clip, am.LastPlayedSFX);
            Object.DestroyImmediate(am.gameObject);
        }

        [Test]
        public void PlayMusic_SetsLoopingClip()
        {
            var am = CreateManager();
            var clip = AudioClip.Create("music", 100, 1, 44100, false);
            am.PlayMusic(clip);
            Assert.AreSame(clip, am.MusicSource.clip);
            Assert.IsTrue(am.MusicSource.loop);
            Object.DestroyImmediate(am.gameObject);
        }

        [Test]
        public void StopMusic_StopsPlayback()
        {
            var am = CreateManager();
            var clip = AudioClip.Create("music", 100, 1, 44100, false);
            am.PlayMusic(clip);
            am.StopMusic();
            Assert.IsFalse(am.MusicSource.isPlaying);
            Object.DestroyImmediate(am.gameObject);
        }

        [Test]
        public void SetMusicVolume_AppliesAndPersists()
        {
            var am = CreateManager();
            am.SetMusicVolume(0.42f);
            Assert.AreEqual(0.42f, am.MusicSource.volume, 0.0001f);
            Assert.AreEqual(0.42f, PlayerPrefs.GetFloat(AudioManager.MusicVolumeKey), 0.0001f);
            Object.DestroyImmediate(am.gameObject);
        }

        [Test]
        public void SetSFXVolume_AppliesAndPersists()
        {
            var am = CreateManager();
            am.SetSFXVolume(0.33f);
            Assert.AreEqual(0.33f, am.SFXSource.volume, 0.0001f);
            Assert.AreEqual(0.33f, PlayerPrefs.GetFloat(AudioManager.SFXVolumeKey), 0.0001f);
            Object.DestroyImmediate(am.gameObject);
        }
    }
}
