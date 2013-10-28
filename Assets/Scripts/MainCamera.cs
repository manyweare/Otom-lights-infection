using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class MainCamera : MonoBehaviour
{
	public float screenRatio = 1f;

	// SINGLETON
	public static MainCamera Instance {
		get {
			if (instance == null)
				instance = new MainCamera();
			return instance;
		}
	}

	private static MainCamera instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		// Set screen ratio for art assets.
		if (Mathf.Approximately(Camera.main.aspect, 0.5625f)
			|| Screen.height == 1136
			|| iPhone.generation == iPhoneGeneration.iPhone5
			|| iPhone.generation == iPhoneGeneration.iPhoneUnknown
			|| iPhone.generation == iPhoneGeneration.iPodTouch5Gen
			|| iPhone.generation == iPhoneGeneration.iPodTouchUnknown)
			screenRatio = 0.85f; // iPhone 5
		else if (Mathf.Approximately(Camera.main.aspect, 0.66666667f)
			|| Screen.height == 960
			|| iPhone.generation == iPhoneGeneration.iPhone4
			|| iPhone.generation == iPhoneGeneration.iPhone4S
			|| iPhone.generation == iPhoneGeneration.iPodTouch4Gen)
			screenRatio = 1f; // iPhone 4
		else if (Mathf.Approximately(Camera.main.aspect, 0.75f)
			|| Screen.height == 2048
			|| iPhone.generation == iPhoneGeneration.iPad3Gen
			|| iPhone.generation == iPhoneGeneration.iPad4Gen)
			screenRatio = 1.1222221f; // iPad Retina

		Application.targetFrameRate = 60;

		// If player has quit out of the game and gone back to main menu, kill all tween objects.
		if (HOTween.totTweens > 0)
			HOTween.Kill();
		HOTween.Init(true, true, true);
//		HOTween.EnableOverwriteManager(false);
	}
}
