using UnityEngine;
using System;
using System.Collections;
using Holoville.HOTween;
using Vectrosity;

public class MainMenu : MonoBehaviour
{
	public GameObject NewGameButton, StatsButton, SoundButton, AboutButton;
	public GUIText VersionText;
	public string VersionNumber;
	public Material LineMaterial;
	private Transform _myTransform;
	private Vector3 hiddenPos;
	private bool isHidden = true;
	private bool soundIsOn = true;
	private AudioListener _listener;
    //private VectorLine titleLine, versionLine, newGameLine, statsLine, soundLine, aboutLine;

	#region SINGLETON

	public static MainMenu Instance {
		get {
			if (instance == null)
				instance = new MainMenu();
			return instance;
		}
	}

	private static MainMenu instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		_myTransform = transform;
	}

	#endregion

	void Start()
	{
		_myTransform.position -= new Vector3(1f, 0f, 0f);
		hiddenPos = _myTransform.position;

		// Makes Sound button display the right text.
		var soundStatus = AudioListener.volume > 0 ? "On" : "Off";
		SoundButton.guiText.text = "Sound " + soundStatus;

		VersionText.text = VersionNumber;

		StartCoroutine("ShowSelf");

        //titleLine = new VectorLine("Title line", new Vector2[32], LineMaterial, 1f);
        //var startingPos = new Vector2(40f, Screen.height - 40f);
        //titleLine.MakeText("Otom", startingPos, 100f);
        //titleLine.SetColor(ArtManager.Instance.ColorList[0]);
        //titleLine.Draw();

        //versionLine = new VectorLine("Version line", new Vector2[32], LineMaterial, 1f);
        //versionLine.MakeText(VersionNumber, startingPos + new Vector2(0f, -120f), 10f);
        //versionLine.SetColor(ArtManager.Instance.ColorList[0]);
        //versionLine.Draw();

        //BuildMainMenu();
	}

	void Update()
	{
		if (!isHidden)
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
					LayerMask menuLayer = 1 << LayerMask.NameToLayer("MainMenu");
					if (Physics.Raycast(ray, out hit, 10f, menuLayer))
						MainMenuActions(hit.collider.transform.parent.gameObject);
				}
			}
		} else
		{
			if (Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				LayerMask menuLayer = 1 << LayerMask.NameToLayer("MainMenu");
				if (Physics.Raycast(ray, out hit, 10f, menuLayer))
					MainMenuActions(hit.collider.transform.parent.gameObject);
			}
		}
	}

    //void BuildMainMenu()
    //{
    //    newGameLine = BuildMenuItem(newGameLine, "New Game", 50f, new Vector2(10f, Screen.height - 200f), Color.white);
    //    statsLine = BuildMenuItem(statsLine, "Stats", 50f, new Vector2(10f, Screen.height - 280f), Color.white);
    //    soundLine = BuildMenuItem(soundLine, "Sound On", 50f, new Vector2(10f, Screen.height - 360f), Color.white);
    //    aboutLine = BuildMenuItem(aboutLine, "About", 50f, new Vector2(10f, Screen.height - 440f), Color.white);
    //}

    //VectorLine BuildMenuItem(VectorLine vectorLine, string buttonText, float size, Vector2 position, Color color)
    //{
    //    vectorLine = new VectorLine(buttonText + "line", new Vector2[32], LineMaterial, 1f);
    //    vectorLine.MakeText(buttonText, position, size);
    //    vectorLine.SetColor(color);
    //    vectorLine.Draw();
    //    return vectorLine;
    //}

	void MainMenuActions(GameObject button)
	{
		switch (button.name)
		{
		case "New Game":
			StartCoroutine("LoadNewGame");
			break;
		case "Statistics":
			StartCoroutine("LoadStatistics");
			break;
		case "Sound":
			ToggleSound();
			break;
		case "About":
			StartCoroutine("LoadAbout");
			break;
		}
	}

	IEnumerator LoadNewGame()
	{
		HideSelf();
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("Gameboard");
	}

	IEnumerator LoadAbout()
	{
		HideSelf();
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("About");
	}
	
	IEnumerator LoadStatistics()
	{
		HideSelf();
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("Statistics");
	}

	void ToggleSound()
	{
		soundIsOn = !soundIsOn;
		AudioListener.volume = soundIsOn == true ? 100 : 0;
		var soundStatus = soundIsOn == true ? "On" : "Off";
		SoundButton.guiText.text = "Sound " + soundStatus;
	}

	// Animation Methods.

	public IEnumerator ShowSelf()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", Vector3.zero).Ease(EaseType.EaseOutExpo));
		yield return new WaitForSeconds(0.3f);
		isHidden = false;
	}

	void HideSelf()
	{
		isHidden = true;
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", hiddenPos).Ease(EaseType.EaseInExpo));
	}
}