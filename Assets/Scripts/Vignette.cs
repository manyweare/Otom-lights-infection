using UnityEngine;
using System.Collections;

public class Vignette : MonoBehaviour
{
	// SINGLETON
	public static Vignette Instance {
		get {
			if (instance == null)
				instance = new Vignette();
			return instance;
		}
	}

	private static Vignette instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
