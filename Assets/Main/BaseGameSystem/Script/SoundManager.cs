using UnityEngine;
using System.Collections;

/// <summary>
/// Volume設定用の構造体
/// </summary>
[System.Serializable]
public class SoundVolume
{
	public float BGM = 1.0f;
	public float Voice = 1.0f;
	public float SE = 1.0f;
	public bool BGM_Mute = false;
	public bool SE_Mute = false;

	public void Init ()
	{
		BGM = 1.0f;
		Voice = 1.0f;
		SE = 1.0f;
		BGM_Mute = false;
		SE_Mute = false;
	}
}

[System.Serializable]
public class SoundSource
{
	public AudioClip clip;
	public string Name;

	[HideInInspector]
	public bool FadeFlag = false;
	public    float m_VolumeBefore = 0.0f;
	// フェード開始前の音量.
	public    float m_VolumeAfter = 0.0f;
	// フェード終了後の音量.
	public    float m_FadeTime = 0.0f;
	// フェード時間.

	public float m_Volume;
}

/// <summary>
/// 音源を扱うシングルトン.
/// どこからでも参照出来るのでプロパティを通してインスタンスを得られる.
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
	//BGM
	private AudioSource BGMSource;
	// SE
	private AudioSource[] SEsources = new AudioSource[16];
	//音量
	public SoundVolume volume = new SoundVolume ();
	//BGM
	public SoundSource[] BGMs;
	// SE
	public SoundSource[] SEs;

	void Start ()
	{

		BGMSource = gameObject.AddComponent<AudioSource> ();
		BGMSource.loop = true;

		for (int i = 0; i < SEsources.Length; i++) {
			SEsources [i] = gameObject.AddComponent<AudioSource> ();
		}


	}

	void Update ()
	{
	
		foreach (SoundSource ss in BGMs) {
			if (ss.FadeFlag == true && ss.m_FadeTime >= 0.0f) {
				// 音量を調整する.
				ss.m_Volume += (ss.m_VolumeAfter - ss.m_VolumeBefore) / ss.m_FadeTime / 60.0f;
				BGMSource.volume = ss.m_Volume;

				// フェード処理が完了したらフェード時間を初期化.
				if (ss.m_Volume >= 1.0f) {
					ss.m_Volume = 1.0f;
					ss.m_FadeTime = 0.0f;
				} else if (ss.m_Volume <= 0.0f) {
					ss.m_Volume = 0.0f;
					ss.m_FadeTime = 0.0f;
					BGMSource.Stop ();
				}
			}
		}

	}

	public void StopBGM (string name)
	{

		foreach (SoundSource ss in BGMs) {
			if (ss.Name.Equals (name) == true) {
				BGMSource.clip = ss.clip;
				BGMSource.Stop ();
			}
		}

	}

	public void FadeOutBGM (string name)
	{

		foreach (SoundSource ss in BGMs) {
			if (ss.Name.Equals (name) == true) {
				ss.FadeFlag = true;
			}
		}

	}

	public void PlayBGM (string name)
	{

		foreach (SoundSource ss in BGMs) {
			if (ss.Name.Equals (name) == true) {
				BGMSource.clip = ss.clip;
				BGMSource.volume = volume.BGM;
				BGMSource.Play ();

				ss.m_Volume = volume.BGM;
			}
		}

	}

	public void PlaySE (string name)
	{

		foreach (SoundSource ss in SEs) {
			if (ss.Name.Equals (name) == true) {

				foreach (AudioSource audiosource in SEsources) {
					if (audiosource.isPlaying == false) {
						audiosource.clip = ss.clip;
						audiosource.volume = volume.SE;
						audiosource.Play ();
						return;
					}
				}
			}
		}
	}
}