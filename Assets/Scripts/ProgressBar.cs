using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ProgressBar : MonoBehaviour
{
	[HideInInspector]
	public static ProgressBar Instance;
	private Transform _myTransform;
	private Vector3 originalScale, originalPos;
	private float maxWidth = 2.2f;
	// iPhone 4 width is 2.2f, iPad Retina is 2.5f.
	private TweenParms hideParms = new TweenParms();
	private TweenParms showParms = new TweenParms();

	void Awake()
	{
		Instance = this;
		_myTransform = transform;

		originalPos = _myTransform.position;
		originalScale = _myTransform.localScale;
		
		hideParms.Prop("position", originalPos + new Vector3(0f, _myTransform.localScale.y, _myTransform.position.z)).Ease(EaseType.EaseInExpo);
		showParms.Prop("position", originalPos).Ease(EaseType.EaseOutExpo);
	}

	void Start()
	{
		renderer.material.color = ArtManager.Instance.LockedColor;
		maxWidth = originalScale.x * ArtManager.Instance.screenRatio;
		UpdateSize();
	}

	public void UpdateSize()
	{
		float targetWidth = ((float)GameManager.Instance.Score / (float)GameManager.Instance.ScorePerLevel) * maxWidth;
		HOTween.To(_myTransform, 0.4f, "localScale", new Vector3(targetWidth, originalScale.y, originalScale.z));
	}

	public void HideSelf()
	{
		HOTween.To(_myTransform, 0.3f, hideParms);
	}

	public void ShowSelf()
	{
		HOTween.To(_myTransform, 0.3f, showParms);
	}
}
