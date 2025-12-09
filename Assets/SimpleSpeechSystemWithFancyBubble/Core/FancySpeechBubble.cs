using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(TMP_Text))]
public class FancySpeechBubble : MonoBehaviour
{
	/// <summary>
	/// Character start font size.
	/// </summary>
	public float characterStartSize = 1;

	/// <summary>
	/// Character size animate speed.
	/// Unit: delta font size / second
	/// </summary>
	public float characterAnimateSpeed = 1000f;

	/// <summary>
	/// The bubble background (OPTIONAL).
	/// </summary>
	public Image bubbleBackground;

	/// <summary>
	/// Minimum height of background.
	/// </summary>
	public float backgroundMinimumHeight;

	/// <summary>
	/// Vertical margin (top + bottom) between label and background (OPTIONAL).
	/// </summary>
	public float backgroundVerticalMargin;

	/// <summary>
	/// A copy of raw text.
	/// </summary>
	private string _rawText;
	public string rawText
	{
		get { return _rawText; }
	}

	/// <summary>
	/// Processed version of raw text.
	/// </summary>
	private string _processedText;
	public string processedText
	{
		get { return _processedText; }
	}

	[HideInInspector]
	public bool _isTyping;

	[Header("音效设置(音效尽量短 < 1s)")]
	[Range(0, 1)]
	public float soundVolume = 0.5f; // 在Inspector中滑动调节

	private void Awake()
	{
		// 确保有AudioSource组件
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		label = transform.Find("SpeechBubbleCanvas").Find("FancySpeechBubble").Find("SpeechLabel").GetComponent<TMP_Text>();
		label.text = "";
	}

	/// <summary>
	/// Set the label text.
	/// </summary>
	/// <param name="text">Text.</param>
	public void Set(string text)
	{
		StopAllCoroutines();
		StartCoroutine(SetRoutine(text));
	}
	public void SetTextInstantly(string text)
	{
		StopAllCoroutines();
		//TMP_Text label = GetComponent<TMP_Text>();
		audioSource.Stop();
		label.text = text;
		_isTyping = false;
	}

	/// <summary>
	/// Set the label text.
	/// </summary>
	/// <param name="text">Text.</param>
	public IEnumerator SetRoutine(string text)
	{
		_rawText = text;
		yield return StartCoroutine(TestFit());
		yield return StartCoroutine(CharacterAnimation());
	}

	/// <summary>
	/// Test fit candidate text,
	/// set intended label height,
	/// generate processed version of the text.
	/// </summary>
	private IEnumerator TestFit()
	{
		// prepare targets
		//TMP_Text label = GetComponent<TMP_Text>();
		//ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();

		// change label alpha to zero to hide test fit
		float alpha = label.color.a;
		label.color = new Color(label.color.r, label.color.g, label.color.b, 0f);

		// configure fitter and set label text so label can auto resize height
		label.text = _rawText;

		// need to wait for a frame before label's height is updated
		yield return new WaitForEndOfFrame();
		// make sure label is anchored to center to measure the correct height
		float totalHeight = label.rectTransform.sizeDelta.y;

		// now it's time to test word by word
		_processedText = "";
		string buffer = "";
		string line = "";
		float currentHeight = -1f;
		// yes, sorry multiple spaces
		foreach (string word in _rawText.Split(' '))
		{
			buffer += word + " ";
			label.text = buffer;
			yield return new WaitForEndOfFrame();
			if (currentHeight < 0f)
			{
				currentHeight = label.rectTransform.sizeDelta.y;
			}
			if (currentHeight != label.rectTransform.sizeDelta.y)
			{
				currentHeight = label.rectTransform.sizeDelta.y;
				_processedText += line.TrimEnd(' ') + "\n";
				line = "";
			}
			line += word + " ";
		}
		_processedText += line;

		// prepare fitter and label for character animation
		label.text = "";
		label.rectTransform.sizeDelta = new Vector2(label.rectTransform.sizeDelta.x, totalHeight);
		label.color = new Color(label.color.r, label.color.g, label.color.b, alpha);
	}

	private IEnumerator CharacterAnimation()
	{
		// prepare target
		//TMP_Text label = GetComponent<TMP_Text>();

		// go through character in processed text
		string prefix = "";
		foreach (char c in _processedText.ToCharArray())
		{
			_isTyping = true;
			if (!char.IsWhiteSpace(c))
			{
				PlayCharacterSound();
			}
			// animate character size
			float size = characterStartSize;
			while (size < label.fontSize)
			{
				size += (int)(Time.deltaTime * characterAnimateSpeed);
				size = Mathf.Min(size, label.fontSize);
				label.text = prefix + "<size=" + size + ">" + c + "</size>";
				yield return new WaitForEndOfFrame();
			}
			prefix += c;
		}
		_isTyping = false;
		// set processed text
		label.text = _processedText;
	}

	// 新增音效相关变量
	public AudioClip charSoundEffect; // 在Inspector中拖入音效文件
	public float soundPitchRandomRange = 0.1f; // 音调随机变化范围
	private AudioSource audioSource;
	private TMP_Text label;
	private void PlayCharacterSound()
	{
		if (charSoundEffect == null) return;

		// 随机音调避免机械感
		audioSource.pitch = 1f + Random.Range(-soundPitchRandomRange, soundPitchRandomRange);
		audioSource.volume = soundVolume; // 使用公开变量

		if (!audioSource.isPlaying)
			audioSource.PlayOneShot(charSoundEffect);
	}
}
