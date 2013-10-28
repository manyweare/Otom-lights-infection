using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ProgressBarContainer : MonoBehaviour
{
	public GameObject Track;
	private Transform _myTransform;
	private Vector3 originalPos;

	void Awake()
	{
		_myTransform = transform;
		originalPos = _myTransform.position;
	}

	void Start()
	{
		Track.transform.localScale *= ArtManager.Instance.screenRatio;
	}

	public void HideSelf()
	{
		HOTween.To(_myTransform, 0.3f,
			new TweenParms().Prop("position", originalPos + new Vector3(0f, _myTransform.localScale.y, _myTransform.position.z)).Ease(EaseType.EaseInExpo));
	}

	public void ShowSelf()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", originalPos).Ease(EaseType.EaseOutExpo));
	}
}
