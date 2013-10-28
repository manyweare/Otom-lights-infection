using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class LevelComplete : MonoBehaviour
{
	[HideInInspector]
	public static LevelComplete Instance;
	public GameObject SpherePrefab;
	private GameObject sphere;
	private TweenParms activateParms = new TweenParms();
	private TweenParms deactivateParms = new TweenParms();

	void Awake()
	{
		Instance = this;
	}
	
	void Start()
	{
		sphere = Instantiate(SpherePrefab, Vector3.zero, Quaternion.identity) as GameObject;
		sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		sphere.renderer.material.color = Color.white;
		sphere.SetActive(false);
		gameObject.SetActive(false);
		activateParms.Prop("localScale", new Vector3(15f, 15f, 15f)).Ease(EaseType.EaseOutExpo);
		deactivateParms.Prop("localScale", new Vector3(0.1f, 0.1f, 0.1f)).Ease(EaseType.EaseInExpo);
	}

	public IEnumerator Activate()
	{
		gameObject.SetActive(true);
		sphere.SetActive(true);
		HOTween.To(sphere.transform, 0.3f, activateParms);
		yield return new WaitForSeconds(2f);
		HOTween.To(sphere.transform, 0.3f, deactivateParms);
		yield return new WaitForSeconds(0.3f);
		sphere.SetActive(false);
		gameObject.SetActive(false);
	}
}
