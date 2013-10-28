using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class About : MonoBehaviour
{
	public GameObject AboutContainer;
	private Transform _myTransform;
	private Vector3 hiddenPos;

	// SINGLETON
	public static About Instance {
		get {
			if (instance == null)
				instance = new About();
			return instance;
		}
	}

	private static About instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		_myTransform = AboutContainer.transform;
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

	// Animation Methods.

	void ShowSelf()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", Vector3.zero).Ease(EaseType.EaseOutExpo));
	}

	IEnumerator HideSelf()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", hiddenPos).Ease(EaseType.EaseInExpo));
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("Main Menu");
	}
}