using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class GameOver : MonoBehaviour
{
	[HideInInspector]
	public static GameOver Instance;
	public GameObject Background;
	public GUIText RecordText;
	private Transform _myTransform;
	private Vector3 hiddenPos = Vector3.zero;

	void Awake()
	{
		Instance = this;
		_myTransform = transform;
	}
	
	void Start()
	{
		_myTransform.position += new Vector3(1.5f, 0f, 0f);
		hiddenPos = _myTransform.position;

		gameObject.guiText.text = "No more moves.\n";
		gameObject.guiText.text += "Tap twice to try again.";

		RecordText.text = "";

		Background.renderer.material.color -= new Color(0f, 0f, 0f, 0.2f);

		gameObject.SetActive(false);
	}

	void Update()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount == 1 && gameObject.activeSelf)
				if (Input.GetTouch(0).tapCount == 2)
					StartCoroutine("Deactivate");
		} else
		{
			if (Application.platform == RuntimePlatform.OSXEditor && gameObject.activeSelf)
				if (Input.GetKeyDown(KeyCode.N))
					StartCoroutine("Deactivate");
		}
	}

	public IEnumerator Activate()
	{
		gameObject.SetActive(true);

		RecordText.text = "You reached Level " + GameManager.Instance.Level.ToString() + ".\n";

		// Check if this is a new record.
		var highestLevel = PlayerPrefs.GetInt("Level");
		if (GameManager.Instance.Level > highestLevel)
			RecordText.text += " That's a new personal best!\n";
		else
			RecordText.text += " Your personal best is Level " + highestLevel.ToString() + ".\n";
		RecordText.text += "\n";
		RecordText.text += "Longest chain: " + GameManager.Instance.LongestChain.ToString() + "\n";
		RecordText.text += "Most dots unlocked in one move: " + GameManager.Instance.MostUnlocks.ToString();

		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", new Vector3(0.1f, 0.5f, -1f)).Ease(EaseType.EaseOutExpo));

		yield return null;
	}

	public IEnumerator Deactivate()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", hiddenPos).Ease(EaseType.EaseInExpo));
		yield return new WaitForSeconds(0.5f);
		GameManager.Instance.RestartGame();
		gameObject.SetActive(false);
	}
}
