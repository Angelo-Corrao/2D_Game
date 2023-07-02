using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager Instance {
		get; private set;
	}
	public AudioSource musicSource;
	public AudioSource sfxSource;
	public Sound[] musicSounds;
	public Sound[] sfxSounds;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
	}

	public void PlayMusic(string clipName) {
		Sound sound = Array.Find(musicSounds, x => x.name == clipName);

		if (sound != null) {
			musicSource.clip = sound.clip;
			musicSource.Play();
		}
	}

	public void PlaySFX(string clipName) {
		Sound sound = Array.Find(sfxSounds, x => x.name == clipName);

		if (sound != null) {
			sfxSource.PlayOneShot(sound.clip);
		}
	}
}
