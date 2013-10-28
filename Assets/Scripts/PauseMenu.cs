using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class PauseMenu : MonoBehaviour
{
	public GameObject Background;
	private Transform _myTransform;
	private Vector3 hiddenPos;

	// SINGLETON
	public static PauseMenu Instance {
		get {
			if (instance == null)
				instance = new PauseMenu();
			return instance;
		}
	}

	private static PauseMenu instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
		{
			instance = this;
		}
		_myTransform = transform;
	}

	void Start()
	{
		_myTransform.position += new Vector3(1.5f, 0f, 0f);
		hiddenPos = _myTransform.position;
		Background.renderer.material.color -= new Color(0f, 0f, 0f, 0.1f);
	}

	void Update()
	{
		if (GameManager.Instance.isPaused)
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
					LayerMask menuLayer = 1 << LayerMask.NameToLayer("PauseMenu");
					if (Physics.Raycast(ray, out hit, 10f, menuLayer))
						PauseMenuActions(hit.collider.gameObject);
				}
			}
		} else
		{
			if (Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				LayerMask menuLayer = 1 << LayerMask.NameToLayer("PauseMenu");
				if (Physics.Raycast(ray, out hit, 10f, menuLayer))
					PauseMenuActions(hit.collider.gameObject);
			}
		}
	}

	void PauseMenuActions(GameObject button)
	{
		switch (button.name)
		{
		case "Continue":
			GUIManager.Instance.HidePauseMenu(true);
			break;
		case "Restart":
			GameManager.Instance.RestartGame();
			break;
		case "Quit":
			GameManager.Instance.LoadMainMenu();
			break;
		}
	}

	public IEnumerator Activate()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", Vector3.zero).Ease(EaseType.EaseOutExpo));
		yield return new WaitForSeconds(0.3f);
	}

	public IEnumerator Deactivate()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", hiddenPos).Ease(EaseType.EaseInExpo));
		yield return new WaitForSeconds(0.3f);
	}
}