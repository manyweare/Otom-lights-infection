using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Loop : MonoBehaviour
{
	public static Loop Instance;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		gameObject.renderer.material.color = Camera.main.backgroundColor;
		gameObject.SetActive(false);
	}

	public void Activate()
	{
		StartCoroutine("CoActivate");
	}

	IEnumerator CoActivate()
	{
		Debug.Log("LOOP!");
		HOTween.To(gameObject.renderer.material, 0.3f, "color", DotManager.Instance.ChainColor);
		yield return new WaitForSeconds(0.3f);
		gameObject.renderer.material.color = Camera.main.backgroundColor;
		gameObject.SetActive(false);
	}
}
