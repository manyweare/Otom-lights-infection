using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class PowerUpText : MonoBehaviour
{
	public Color MyColor;
	private Transform _myTransform;
	private Vector3 _originalPosition, _hidePosition;

	void Awake()
	{
		_myTransform = transform;
		_originalPosition = _myTransform.position;
		_hidePosition = _originalPosition - new Vector3(0f, 1f, 0f);
		// Changed to black, but can be used to set it to different colors.
		guiText.color = Color.black;
		guiText.text = "";
	}

	IEnumerator HideSelf()
	{
		HOTween.To(_myTransform, 0.2f, new TweenParms().Prop("position", _hidePosition).Ease(EaseType.EaseInExpo));
		HOTween.To(guiText, 0.2f, "color", Color.white);
		yield return new WaitForSeconds(0.2f);
	}

	IEnumerator ShowSelf()
	{
		HOTween.To(_myTransform, 0.2f, new TweenParms().Prop("position", _originalPosition).Ease(EaseType.EaseOutExpo));
		HOTween.To(guiText, 0.2f, "color", Color.black);
		yield return new WaitForSeconds(0.2f);
	}

	public void UpdateCount(int i)
	{
		StartCoroutine(CoUpdateCount(i));
	}

	IEnumerator CoUpdateCount(int count)
	{
		yield return StartCoroutine("HideSelf");
		guiText.text = count.ToString();
		yield return StartCoroutine("ShowSelf");
	}
}
