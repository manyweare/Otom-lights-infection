using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class LevelTitle : MonoBehaviour
{
	[HideInInspector]
	public static LevelTitle Instance;
	private Transform _myTransform;
	private Vector3 _originalPosition;
	private Color CurrentColor;

	void Awake()
	{
		Instance = this;
		_myTransform = transform;
		_originalPosition = _myTransform.position;
		CurrentColor = guiText.color;
		guiText.text = "";
	}

	public IEnumerator Activate()
	{
		HOTween.To(guiText, 0.3f, "color", Camera.main.backgroundColor);
		HideSelf();
		yield return new WaitForSeconds(0.3f);
		gameObject.guiText.text = "Level " + GameManager.Instance.Level.ToString();
		HOTween.To(guiText, 0.2f, "color", CurrentColor);
		ShowSelf();
	}

	public void HideSelf()
	{
		var hideParms = new TweenParms().Prop("position", _originalPosition + new Vector3(0f, 0.2f, 0f)).Ease(EaseType.EaseInExpo);
		HOTween.To(transform, 0.2f, hideParms);
	}

	public void ShowSelf()
	{
		var showParms = new TweenParms().Prop("position", _originalPosition).Ease(EaseType.EaseOutExpo);
		HOTween.To(transform, 0.3f, showParms);
	}
}