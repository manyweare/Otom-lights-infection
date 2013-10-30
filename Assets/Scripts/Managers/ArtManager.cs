using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class ArtManager : MonoBehaviour
{
	[HideInInspector]
	public static ArtManager Instance;
	public GameObject CircleMesh;
	[HideInInspector]
	public List<GameObject> DataMeshList = new List<GameObject>();
	[HideInInspector]
	public Vector3 OriginalScale = Vector3.zero;
	[HideInInspector]
	public List<Color32> ColorList = new List<Color32>();
	[HideInInspector]
	public List<Color32> OriginalColors = new List<Color32>();
	[HideInInspector]
	public List<Color32> ExtraColors = new List<Color32>();
	public Color32 Color01 = new Color32(255, 0, 125, 255); // Pink
	public Color32 Color02 = new Color32(125, 0, 125, 255); // Purple
	public Color32 Color03 = new Color32(0, 255, 150, 255); // Teal
	public Color32 Color04 = new Color32(0, 125, 255, 255); // Blue
	public Color32 Color05 = new Color32(255, 230, 0, 255); // Yellow
	public Color32 GreyDotColor = new Color32(90, 90, 90, 255); // Grey
	public Color32 WhiteDotColor = new Color32(255, 255, 255, 255); // White
	public Color32 ChainColor = new Color32(255, 255, 255, 255);
	public Texture[] Patterns = new Texture[7];
	public TweenParms bigSphereParms = new TweenParms();
	public TweenParms deathParms = new TweenParms();
	public TweenParms lockParms = new TweenParms();
	public TweenParms unlockParms = new TweenParms();
	public TweenParms highlightLockParms = new TweenParms();
	public TweenParms resetLockParms = new TweenParms();
	public TweenParms lockLoopParms = new TweenParms();
	public TweenParms hideParms = new TweenParms();
	public TweenParms showParms = new TweenParms();
	public TweenParms explosionParms = new TweenParms();
	public Color LockedColor = Color.white;
	[HideInInspector]
	public float screenRatio = 1f;
	private const float LOCKED_SCALE = 0.008f;
	private Vector3 lockedVector = Vector3.zero;

	void Awake()
	{
		Instance = this;

		screenRatio = MainCamera.Instance.screenRatio;

		DataMeshList.Add(CircleMesh);

		OriginalScale = (new Vector3(1f, 1f, 1f) * screenRatio) / 100f;
		lockedVector = new Vector3(LOCKED_SCALE, LOCKED_SCALE, LOCKED_SCALE);

		ColorList.Add(Color01);
		ColorList.Add(Color02);
		ColorList.Add(Color03);
		ColorList.Add(Color04);
		ColorList.Add(Color05);
		ColorList.Shuffle();

		OriginalColors.AddRange(ColorList);

		ExtraColors.Add(GreyDotColor);
		ExtraColors.Add(WhiteDotColor);

		bigSphereParms.Prop("localScale", OriginalScale * 1.2f);
		bigSphereParms.Ease(EaseType.EaseOutBack);

		deathParms.Prop("localScale", Vector3.zero);
		deathParms.Ease(EaseType.EaseInBack);

		lockParms.Prop("localScale", lockedVector);
		lockParms.Ease(EaseType.EaseOutBack);

        resetLockParms.Prop("localScale", OriginalScale * 0.3f);
		resetLockParms.Ease(EaseType.EaseOutExpo);

		unlockParms.Prop("localScale", OriginalScale * 3f);
		unlockParms.Ease(EaseType.EaseInBack);
		
		highlightLockParms.Prop("localScale", lockedVector * 1.3f);
		highlightLockParms.Ease(EaseType.EaseOutBack);

		lockLoopParms.Prop("localScale", lockedVector * 1.1f);
		lockLoopParms.Loops(-1, LoopType.Yoyo);
		lockLoopParms.Ease(EaseType.EaseInOutCubic);

		hideParms.Prop("localScale", Vector3.zero);
		hideParms.Ease(EaseType.EaseInExpo);

		showParms.Prop("localScale", OriginalScale);
		showParms.Ease(EaseType.EaseOutExpo);
	}

	public void AddColors(int i)
	{
		if (i > ExtraColors.Count)
			return;
		var j = i;
		while (j > 0)
		{
			ColorList.Add(ExtraColors[ExtraColors.Count - 1]);
			ExtraColors.Remove(ExtraColors[ExtraColors.Count - 1]);
			--j;
		}
	}

	public void RemoveColors(int i)
	{
		if (i > ColorList.Count)
			return;
		var j = i;
		while (j > 0)
		{
			ExtraColors.Add(ColorList[ColorList.Count - 1]);
			ColorList.Remove(ColorList[ColorList.Count - 1]);
			--j;
		}
	}
	
	public void AddGreyDot()
	{
		if (!ColorList.Contains(GreyDotColor))
		{
			ColorList.Add(GreyDotColor);
			ExtraColors.Remove(GreyDotColor);
			Debug.Log("*** ADDED Grey Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to add GREY Dot but it's already active. ***");
	}
	
	public void RemoveGreyDot()
	{
		if (ColorList.Contains(GreyDotColor))
		{
			ExtraColors.Add(GreyDotColor);
			ColorList.Remove(GreyDotColor);
			Debug.Log("*** REMOVED Grey Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to REMOVE GREY Dot but it's already been removed. ***");
	}
	
	public void AddWhiteDot()
	{
		if (!ColorList.Contains(WhiteDotColor))
		{
			ColorList.Add(WhiteDotColor);
			ExtraColors.Remove(WhiteDotColor);
			Debug.Log("*** ADDED White Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to ADD WHITE Dot but it's already active. ***");
	}
	
	public void RemoveWhiteDot()
	{
		if (ColorList.Contains(WhiteDotColor))
		{
			ExtraColors.Add(WhiteDotColor);
			ColorList.Remove(WhiteDotColor);
			Debug.Log("*** REMOVED White Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to REMOVE WHITE Dot but it's already been removed. ***");
	}
}