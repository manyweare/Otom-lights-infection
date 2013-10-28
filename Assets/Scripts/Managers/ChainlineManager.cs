using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Vectrosity;

public class ChainlineManager : MonoBehaviour
{
	private VectorLine chainLine;
	private Vector3[] linePoints = new Vector3[36]; // Maximum # of points the line can have = # of Dots on board.
	private float chainLineWidth = 25f;

	#region SINGLETON

	public static ChainlineManager Instance {
		get {
			if (instance == null)
				instance = new ChainlineManager();
			return instance;
		}
	}

	private static ChainlineManager instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;
	}

	#endregion

	void Start()
	{
		chainLineWidth = chainLineWidth / ArtManager.Instance.screenRatio;
		chainLine = new VectorLine("Chain Line", linePoints, null, chainLineWidth, LineType.Continuous, Joins.Fill);
		chainLine.SetColor(ArtManager.Instance.ChainColor);
		chainLine.active = false;
	}

	public void AddPointToChainLine(Vector3 position, int pointIndex)
	{
		// Offsets Z to make sure chain lines render in front of dots.
		position += new Vector3(0f, 0f, 0.02f);
		linePoints[pointIndex] = position;
		// Fix for origin rogue point.
		if (pointIndex == 0)
		{
			chainLine.points3[0] = position;
			chainLine.active = true;
		}
		DrawLine(chainLine, pointIndex);
	}

	void DrawLine(VectorLine lineToDraw, int lastPoint)
	{
		lineToDraw.drawEnd = lastPoint;
//		var i = ArtManager.Instance.ColorList.IndexOf((Color)DotManager.Instance.ChainColor);
//		if (i >= 0)
//		{
//			lineToDraw.material = ArtManager.Instance.Patterns[i];
//			lineToDraw.SetTextureScale(1.25f);
//		}

        lineToDraw.SetColor(DotManager.Instance.ChainColor);
		lineToDraw.Draw3D();
	}

	public void FadeOutLine()
	{
		StartCoroutine(CoFadeOutLine(chainLine, 0.5f, false));
	}

	IEnumerator CoFadeOutLine(VectorLine lineToFade, float duration, bool destroy)
	{
		float a = 0f;
		while (a < 1f)
		{
			a += Time.deltaTime / duration;
			lineToFade.SetColor(lineToFade.color - new Color(0f, 0f, 0f, a));
			yield return null;
		}

		lineToFade.ZeroPoints();
		lineToFade.SetColor(ArtManager.Instance.ChainColor);

		if (destroy)
		{
			VectorLine.Destroy(ref lineToFade);
		} else
			lineToFade.active = false;
	}

	public void ZoomLine(int l, Color c)
	{
		StartCoroutine(CoZoomLine(l, c, false));
	}

	public void ZoomLine(int l, Color c, bool isLoop)
	{
		StartCoroutine(CoZoomLine(l, c, isLoop));
	}

	IEnumerator CoZoomLine(int lastPoint, Color chainColor, bool isLoop)
	{
		// Copy original points array.
		Vector3[] copyPoints = new Vector3[36];
		copyPoints = (Vector3[])linePoints.Clone();

		// Make new line.
		VectorLine copyLine = new VectorLine("temp line", copyPoints, null, chainLineWidth * 2f, LineType.Continuous, Joins.Fill);
		copyLine.SetColor(chainColor);

		// Make new empty game object and position it at the center of the lines mesh.
		var container = new GameObject("zoom container");
		container.transform.position = copyLine.mesh.bounds.center - copyLine.mesh.bounds.extents;
		for (int i = 0; i < copyPoints.Length; i++)
		{
			copyPoints[i] -= container.transform.position;
			copyPoints[i] += new Vector3(0f, 0f, 0.5f);
		}

		// If it's a loop, close shape.
		if (isLoop)
		{
			copyPoints[lastPoint + 1] = copyPoints[lastPoint];
			++lastPoint;
		}

		DrawLine(copyLine, lastPoint - 1);

		// Zoom.
		HOTween.To(copyLine.vectorObject.transform, 0.4f,
			new TweenParms().Prop("localScale", copyLine.vectorObject.transform.localScale * 4f).Ease(EaseType.EaseInExpo));

		StartCoroutine(CoFadeOutLine(copyLine, 0.4f, true));
		Destroy(container, 0.4f);
		yield return null;
	}
}