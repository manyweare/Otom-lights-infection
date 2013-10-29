using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class PowerUpsManager : MonoBehaviour
{
	public GameObject ExplosionPrefab, Button, Bar, Circle;
	public PowerUpText Text01, Text02, Text03, Text04, Text05;
    public Material TrackMaterial;
	[HideInInspector]
	public int[] PowerUpArray = new int[] { 0, 0, 0, 0, 0 };
	[HideInInspector]
	public List<int> ColorScoreList = new List<int>();
	[HideInInspector]
	public List<PowerUpText> textList = new List<PowerUpText>();
	[HideInInspector]
	public List<GameObject> CircleList = new List<GameObject>();
	
	private Transform _myTransform;
	private Vector3 originalPos;
	private Vector3 _originalCircleScale;
	private TweenParms explosionParms = new TweenParms();
	private TweenParms hideParms = new TweenParms();
	private TweenParms showParms = new TweenParms();
	private List<GameObject> buttonList = new List<GameObject>();
	private List<GameObject> barList = new List<GameObject>();
	private List<GameObject> trackList = new List<GameObject>();
	private const float PROGRESS_BAR_HEIGHT = 0.1f;
	
	// SINGLETON
	public static PowerUpsManager Instance {
		get {
			if (instance == null)
				instance = new PowerUpsManager();
			return instance;
		}
	}

	private static PowerUpsManager instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		_myTransform = transform;

		for (int i = 0; i < 8; i++)
		{
			ColorScoreList.Add(0);
		}
		
		originalPos = _myTransform.position;
		explosionParms.Prop("localScale", new Vector3(50f, 50f, 50f)).Ease(EaseType.EaseInExpo);
		hideParms.Prop("position", originalPos - new Vector3(0f, 0.5f, 0f)).Ease(EaseType.EaseInExpo);
		showParms.Prop("position", originalPos).Ease(EaseType.EaseOutExpo);
	}

	void Start()
	{
		// Color 01 Power Up.
		Text01.MyColor = ArtManager.Instance.ColorList[0];
		textList.Add(Text01);
		// Color 02 Power Up.
		Text02.MyColor = ArtManager.Instance.ColorList[1];
		textList.Add(Text02);
		// Color 03 Power Up.
		Text03.MyColor = ArtManager.Instance.ColorList[2];
		textList.Add(Text03);
		// Color 04 Power Up.
		Text04.MyColor = ArtManager.Instance.ColorList[3];
		textList.Add(Text04);
		// Color 05 Power Up.
		Text05.MyColor = ArtManager.Instance.ColorList[4];
		textList.Add(Text05);

        // Generate background for power up texts.
        for (int i = 0; i < textList.Count; i++)
        {
            // Circle number background.
            var circle = Instantiate(Circle, Vector3.zero, Quaternion.identity) as GameObject;
            circle.transform.position = Camera.main.ViewportToWorldPoint(textList[i].transform.position);
            // Ensure circle is in front of all other button layers.
            circle.transform.position = new Vector3(circle.transform.position.x, circle.transform.position.y + 0.005f, -0.3f);
            circle.transform.localScale *= ArtManager.Instance.screenRatio * 2f;
            _originalCircleScale = circle.transform.localScale;
            circle.transform.parent = textList[i].transform;
            circle.renderer.material = TrackMaterial;
            circle.renderer.material.SetTexture("_Detail", ArtManager.Instance.Patterns[i]);
            circle.renderer.material.SetFloat("_DetailTiling", 15f * ArtManager.Instance.screenRatio);
            circle.renderer.material.color = ArtManager.Instance.ColorList[i];
            circle.renderer.material.SetColor("_RimColor", ArtManager.Instance.ColorList[i]);
            CircleList.Add(circle);
        }

		GenerateButtons(Button, Bar, 5);

		UpdateAllSizes();
	}

	void Update()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount == 1 && !GameManager.Instance.isRunningSomething)
				HandleTouchInput();
		} else
		{
			if (!GameManager.Instance.isRunningSomething)
				HandleMouseInput();
		}
	}

	void GenerateButtons(GameObject buttonPrefab, GameObject barPrefab, int numberOfButtons)
	{
		var xOffset = Camera.main.pixelWidth / numberOfButtons;

		for (int i = 1; i <= numberOfButtons; i++)
		{
			// Instantiate power up button.
			var myButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			myButton.transform.position = Camera.main.ScreenToWorldPoint(new Vector3((i * xOffset) - xOffset, 0f, 0f));
			myButton.transform.position = new Vector3(myButton.transform.position.x, myButton.transform.position.y, 0f);
			myButton.transform.localScale = new Vector3(myButton.transform.localScale.x * ArtManager.Instance.screenRatio, 1f, 1f);
			myButton.transform.parent = _myTransform;
			myButton.name = "Button0" + i.ToString();
			myButton.layer = LayerMask.NameToLayer("PowerUps");
			myButton.renderer.material.color = ArtManager.Instance.ColorList[i - 1];
			buttonList.Add(myButton);
			
			// Instantiate power up bar.
			var myBar = Instantiate(barPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			myBar.transform.position = Camera.main.ScreenToWorldPoint(new Vector3((i * xOffset) - xOffset, 0f, 0f));
			myBar.transform.position = new Vector3(myBar.transform.position.x, myBar.transform.position.y, -0.2f);
			myBar.transform.localScale = new Vector3(myBar.transform.localScale.x * ArtManager.Instance.screenRatio, PROGRESS_BAR_HEIGHT, 1f);
			myBar.transform.parent = _myTransform;
			myBar.renderer.material.color = Color.white;
			myBar.name = "Bar0" + i.ToString();
			barList.Add(myBar);
			
			// Instantiate track.
			var myTrack = Instantiate(barPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			myTrack.transform.position = Camera.main.ScreenToWorldPoint(new Vector3((i * xOffset) - xOffset, 0f, 0f));
			myTrack.transform.position = new Vector3(myTrack.transform.position.x, myTrack.transform.position.y, -0.1f);
			myTrack.transform.localScale = new Vector3(myTrack.transform.localScale.x * ArtManager.Instance.screenRatio, 1f, 1f);
			myTrack.transform.parent = _myTransform;
			myTrack.name = "Track0" + i.ToString();
            myTrack.renderer.material = TrackMaterial;
            myTrack.renderer.material.SetTexture("_Detail", ArtManager.Instance.Patterns[i - 1]);
            myTrack.renderer.material.SetFloat("_DetailTiling", 15f * ArtManager.Instance.screenRatio);
            myTrack.renderer.material.color = ArtManager.Instance.ColorList[i - 1];
            myTrack.renderer.material.SetColor("_RimColor", ArtManager.Instance.ColorList[i - 1]);
			trackList.Add(myTrack);

            // Alternate track colors to divide buttons.
            if (i % 2 == 0)
                myTrack.renderer.material.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            else
                myTrack.renderer.material.color = new Color(0.1f, 0.1f, 0.1f, 1f);
		}
	}

	void HandleTouchInput()
	{
		var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
		RaycastHit hit;
		LayerMask powerUpsLayer = 1 << LayerMask.NameToLayer("PowerUps");
		if (Input.GetTouch(0).phase == TouchPhase.Ended && Physics.Raycast(ray, out hit, 10f, powerUpsLayer))
			StartCoroutine(ActivatePowerUp(hit.collider));
	}

	void HandleMouseInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			LayerMask powerUpsLayer = 1 << LayerMask.NameToLayer("PowerUps");
			if (Physics.Raycast(ray, out hit, 10f, powerUpsLayer))
				StartCoroutine(ActivatePowerUp(hit.collider));
		}
	}

	IEnumerator ActivatePowerUp(Collider powerUp)
	{
//		int i = buttonList.IndexOf(powerUp.gameObject);
		yield return StartCoroutine(AnimateTap(powerUp.gameObject));
		GameManager.Instance.IsGameOver();
	}

	public void UpdateAllSizes()
	{
		StartCoroutine("CoUpdateAllSizes");
	}

	IEnumerator CoUpdateAllSizes()
	{
		int i = 0;
		while (i < barList.Count)
		{
			textList[i].UpdateCount(PowerUpArray[i]);
			UpdateSize(i);
			++i;
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void UpdateSize(int colorIndex)
	{
		if (colorIndex < 0 || colorIndex >= barList.Count)
			return;
		
		if (ColorScoreList[colorIndex] >= GameManager.Instance.ColorScorePerPower)
		{
			ColorScoreList[colorIndex] = 0;
			++PowerUpArray[colorIndex];
			textList[colorIndex].UpdateCount(PowerUpArray[colorIndex]);
		}
		float s = ((float)ColorScoreList[colorIndex] / (float)GameManager.Instance.ColorScorePerPower) * 1.05f;
		s *= ArtManager.Instance.screenRatio;
		HOTween.To(barList[colorIndex].transform, 0.2f, "localScale", new Vector3(s, PROGRESS_BAR_HEIGHT, 1f));
        //AnimateCircle(colorIndex);
	}

    // Animation that plays when the power up button gets a new circle.
    void AnimateCircle(int i)
    {
        var circleSequence = new Sequence();
        circleSequence.Append(HOTween.To(CircleList[i].transform, 0.2f,
            new TweenParms().NewProp("localScale", _originalCircleScale * 1.3f).Ease(EaseType.EaseInExpo)));
        circleSequence.Append(HOTween.To(CircleList[i].transform, 0.6f,
            new TweenParms().NewProp("localScale", _originalCircleScale).Ease(EaseType.EaseOutExpo)));
        circleSequence.Play();
    }

	IEnumerator AnimateTap(GameObject button)
	{
		var buttonColor = button.renderer.material.color;
		// IndexOf returns -1 if no index found.
		int i = ArtManager.Instance.OriginalColors.IndexOf(buttonColor);
		var originalTrackColor = CircleList[i].renderer.material.color;
		// Checks if there's enough power up points for an explosion.
		if (i >= 0 && PowerUpArray[i] > 0)
		{
			HOTween.To(CircleList[i].renderer.material, 0.2f, "color", buttonColor);
			StartCoroutine(AnimateExplosion(buttonColor, button.transform.position));
			yield return new WaitForSeconds(0.2f);
			GameManager.Instance.PowerUpActions(buttonColor);
			PowerUpArray[i] -= 1;
			textList[i].UpdateCount(PowerUpArray[i]);
		} else
		{
			// If not enough points, animate button warning.
			HOTween.To(CircleList[i].renderer.material, 0.2f, "color", Color.red);
			yield return new WaitForSeconds(0.2f);
		}
		HOTween.To(CircleList[i].renderer.material, 0.2f, "color", originalTrackColor);
	}

	IEnumerator AnimateExplosion(Color color, Vector3 position)
	{
		AudioManager.Instance.PlayExplosionSound(color);
		var explosion = Instantiate(ExplosionPrefab, position + new Vector3(0.1f, 0f, 0f), Quaternion.identity) as GameObject;
		explosion.renderer.material.color = color;
		HOTween.To(explosion.transform, 0.6f, explosionParms);
		yield return new WaitForSeconds(0.4f);
		HOTween.To(explosion.renderer.material, 0.2f, "color", explosion.renderer.material.color - new Color32(0, 0, 0, 255));
		yield return new WaitForSeconds(0.2f);
		Destroy(explosion);
	}

	public void HideSelf()
	{
		HOTween.To(_myTransform, 0.4f, hideParms);
	}

	public void ShowSelf()
	{
		StartCoroutine("CoShowSelf");
	}

	IEnumerator CoShowSelf()
	{
		HOTween.To(_myTransform, 0.4f, showParms);
		UpdateAllSizes();
		yield return null;
	}
}