using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Floor : MonoBehaviour
{
	private Transform _myTransform;
	private Vector3 _originalPosition;
	private TweenParms moveParms = new TweenParms();

	void Awake()
	{
		_myTransform = transform;
		moveParms.Prop("position", _myTransform.position + new Vector3(3.5f, 0f, 0f)).Ease(EaseType.Linear);
	}

	void Start()
	{
		_originalPosition = _myTransform.position;
	}

	public void Reset()
	{
		_myTransform.position = _originalPosition;
	}

	public void Move()
	{
		HOTween.To(_myTransform, 1.8f, moveParms);
	}
}
