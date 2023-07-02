using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
	public static Settings instance;
	public AudioMixer mainMixer;
	public Slider masterSlider;
	public Slider musicSlider;
	public Slider sfxSlider;
	public Dropdown screenMode;

	private void Awake() {
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
	}

	private void Start() {
		LoadSettings();
	}

	public void SetScreenMode(int optionSelected) {
		switch (optionSelected) {
			case 0:
				Screen.fullScreen = true;
				break;

			case 1:
				Screen.fullScreen = false;
				break;
		}
		PlayerPrefs.SetInt("screenMode", optionSelected);
	}

	public void SetMasterVolume(float value) {
		mainMixer.SetFloat("master", Mathf.Log10(value) * 20);
		PlayerPrefs.SetFloat("masterVolume", value);
	}

	public void SetMusicVolume(float value) {
		mainMixer.SetFloat("music", Mathf.Log10(value) * 20);
		PlayerPrefs.SetFloat("musicVolume", value);
	}

	public void SetSFXVolume(float value) {
		mainMixer.SetFloat("sfx", Mathf.Log10(value) * 20);
		PlayerPrefs.SetFloat("sfxVolume", value);
	}

	public void LoadSettings() {
		if (PlayerPrefs.HasKey("masterVolume")) {
			masterSlider.value = PlayerPrefs.GetFloat("masterVolume");
		}

		if (PlayerPrefs.HasKey("musicVolume")) {
			musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
		}

		if (PlayerPrefs.HasKey("sfxVolume")) {
			sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
		}

		if (PlayerPrefs.HasKey("screenMode")) {
			screenMode.value = PlayerPrefs.GetInt("screenMode");
		}
	}
}
