using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Countdown : MonoBehaviour
{
	public GameObject CountdownPrefab, PipePrefab;
	private GameObject countdownText, pipe;
	private Transform _myTransform, countdownTransform;

	void Awake()
	{
		_myTransform = transform;
		countdownText = Instantiate(CountdownPrefab, _myTransform.position, Quaternion.identity) as GameObject;
		countdownTransform = countdownText.transform;
		countdownTransform.parent = _myTransform;
		pipe = Instantiate(PipePrefab, _myTransform.position, Quaternion.identity) as GameObject;
		pipe.transform.parent = _myTransform;
	}
	
	void Start()
	{
		pipe.renderer.material.color = _myTransform.parent.renderer.material.color;
		AnimateBirth();
	}
	
	void Update()
	{
		countdownTransform.position = Camera.main.WorldToViewportPoint(_myTransform.parent.transform.position);
	}
	
	void AnimateBirth()
	{
		HOTween.From(pipe.transform, 0.2f, "localScale", Vector3.zero);
	}
}
