using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ContainmentUnit : MonoBehaviour
{
	private Transform _myTransform;
	private Color _originalColor;
	private TweenParms rotationParms;

	void Awake()
	{
		_myTransform = transform;
	}

	void Start()
	{
		if (_myTransform.parent.GetComponent<Sphere>() != null)
			_originalColor = _myTransform.parent.GetComponent<Sphere>().CurrentColor;
		else
			_originalColor = _myTransform.parent.gameObject.renderer.material.color;

		rotationParms = new TweenParms();
		rotationParms.Ease(EaseType.EaseInOutBack);

		StartCoroutine("Contain");
	}
	
	public IEnumerator Contain()
	{
		while(true)
		{
            // Rotate in a random direction.
			float duration = UnityEngine.Random.Range(0.2f, 2f);
			rotationParms.Prop("eulerAngles", new Vector3(0f, 0f, _myTransform.eulerAngles.z + UnityEngine.Random.Range(-180f, 180f)));
			HOTween.To(_myTransform, duration, rotationParms);

			yield return new WaitForSeconds(duration);

            // Flash white.
			gameObject.renderer.material.color = Color.white;
			HOTween.To(gameObject.renderer.material, 0.3f, "color", _originalColor);

			yield return new WaitForSeconds(0.3f);
		}
	}
}
