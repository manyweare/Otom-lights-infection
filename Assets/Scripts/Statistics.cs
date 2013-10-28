using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Statistics : MonoBehaviour
{
	public GameObject StatisticsContainer;
	public GUIText StatsText;
	private Transform _myTransform;
	private Vector3 hiddenPos;
	// Player prefs ints.
	private int recordLevel, recordChain, recordUnlocks;
	private int totalDotsChained, totalPoweUpsUsed, totalLoops;
	private int purpleDots, blueDots, tealDots, pinkDots, yellowDots, greyDots, whiteDots;

	// SINGLETON
	public static Statistics Instance {
		get {
			if (instance == null)
				instance = new Statistics();
			return instance;
		}
	}

	private static Statistics instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;
		
		_myTransform = StatisticsContainer.transform;
	}

	void Start()
	{
		_myTransform.position += new Vector3(1f, 0f, 0f);
		hiddenPos = _myTransform.position;
		
		ShowSelf();
	}

	void Update()
	{
		HandleInput();
	}

	void HandleInput()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount == 1)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 10f))
						StartCoroutine("HideSelf");
				}
			}
		} else
		{
			if (Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 10f))
					StartCoroutine("HideSelf");
			}
		}
	}
	
	void CheckPlayerPrefs()
	{
		// defaultValue is added to check if the value exists in the file or not.
		recordLevel = PlayerPrefs.GetInt("Level", 1);
		recordChain = PlayerPrefs.GetInt("Longest Chain", 0);
		recordUnlocks = PlayerPrefs.GetInt("Most Unlocks", 0);
		totalDotsChained = PlayerPrefs.GetInt("Score", 0);
		purpleDots = PlayerPrefs.GetInt("Purple", 0);
		blueDots = PlayerPrefs.GetInt("Blue", 0);
		tealDots = PlayerPrefs.GetInt("Teal", 0);
		pinkDots = PlayerPrefs.GetInt("Pink", 0);
		yellowDots = PlayerPrefs.GetInt("Yellow", 0);
		greyDots = PlayerPrefs.GetInt("Grey", 0);
		whiteDots = PlayerPrefs.GetInt("White", 0);
		totalPoweUpsUsed = PlayerPrefs.GetInt("Total Power Ups", 0);
		totalLoops = PlayerPrefs.GetInt("Total Loops", 0);
	}

	// Animation Methods.

	void ShowSelf()
	{
		CheckPlayerPrefs();
		
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", Vector3.zero).Ease(EaseType.EaseOutExpo));
		
		StatsText.text = "Highest level  <b><color=white>" + recordLevel.ToString() + "</color></b> \n" +
			"Longest chain  <b><color=white>" + recordChain.ToString() + "</color></b> \n" +
			"Most unlocks in one move  <b><color=white>" + recordUnlocks.ToString() + "</color></b> \n" +
			"Total power ups used  <b><color=white>" + totalPoweUpsUsed.ToString() + "</color></b> \n" +
			"Total loops  <b><color=white>" + totalLoops.ToString() + "</color></b> \n" +
			"Total dots  <b><color=white>" + totalDotsChained.ToString() + "</color></b> \n" +
			"Purple dots  <b><color=" + Helper.ColorToHex(ArtManager.Instance.Color01) + ">" + purpleDots.ToString() + "</color></b> \n" +
			"Pink dots  <b><color=" + Helper.ColorToHex(ArtManager.Instance.Color02) + ">" + pinkDots.ToString() + "</color></b> \n" +
			"Teal dots  <b><color=" + Helper.ColorToHex(ArtManager.Instance.Color03) + ">" + tealDots.ToString() + "</color></b> \n" +
			"Blue dots  <b><color=" + Helper.ColorToHex(ArtManager.Instance.Color04) + ">" + blueDots.ToString() + "</color></b> \n" +
			"Yellow dots  <b><color=" + Helper.ColorToHex(ArtManager.Instance.Color05) + ">" + yellowDots.ToString() + "</color></b> \n" +
			"Grey dots  <b><color=" + Helper.ColorToHex(ArtManager.Instance.GreyDotColor) + ">" + greyDots.ToString() + "</color></b> \n" +
			"White dots  <b><color=white>" + whiteDots.ToString() + "</color></b>";
	}

	IEnumerator HideSelf()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", hiddenPos).Ease(EaseType.EaseInExpo));
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("Main Menu");
	}
}