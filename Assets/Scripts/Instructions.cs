using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Instructions : MonoBehaviour
{
	void Start()
	{
		StartCoroutine("FadeIn");
	}

	IEnumerator FadeIn()
	{
		HOTween.From(guiText, 2f, "color", Camera.main.backgroundColor);
		yield return new WaitForSeconds(10f);
	}
}