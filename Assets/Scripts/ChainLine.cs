using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Vectrosity;

public class ChainLine : MonoBehaviour
{
	private LineRenderer lineRenderer;
	private VectorLine line;

	void Awake()
	{
		lineRenderer = gameObject.GetComponent<LineRenderer>() as LineRenderer;
		lineRenderer.SetWidth(0.05f, 0.05f);
		lineRenderer.SetVertexCount(4);
		lineRenderer.SetColors((Color)ArtManager.Instance.ChainColor, (Color)ArtManager.Instance.ChainColor);

	}

	void Start()
	{
		line.lineWidth = 1f;
	}

//	void OnDestroy()
//	{
//        DestroyImmediate(renderer.material);
//    }

	public void DrawLine(Vector3 startPosition, Vector3 endPosition)
	{
		// Offsets Z to make sure lines render in front of spheres.
		startPosition += new Vector3(0f, 0f, 0.6f);
		endPosition += new Vector3(0f, 0f, 0.6f);
//		lineRenderer.SetPosition(0, startPosition);
//		lineRenderer.SetPosition(2, startPosition);
//		lineRenderer.SetPosition(1, endPosition);
//		lineRenderer.SetPosition(3, endPosition);

		line = VectorLine.SetLine3D(ArtManager.Instance.ChainColor, startPosition, endPosition);
	}

	public void FadeLine()
	{
		HOTween.To(lineRenderer.material, 0.1f, "color", Camera.main.backgroundColor);
	}

	public void ZoomLine()
	{
		var zoomParms = new TweenParms().Ease(EaseType.EaseInOutExpo);
		zoomParms.Prop("localScale", lineRenderer.transform.localScale + new Vector3(1f, 1f, 1f));
		HOTween.To(lineRenderer.transform, 1f, zoomParms);
	}
}