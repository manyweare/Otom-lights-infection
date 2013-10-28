using UnityEngine;
using System;
using System.Collections;
using Holoville.HOTween;

public class MeshRandomizer : MonoBehaviour
{
	#region SINGLETON

	public static MeshRandomizer Instance {
		get {
			if (instance == null)
				instance = new MeshRandomizer();
			return instance;
		}
	}

	private static MeshRandomizer instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;
	}

	#endregion
	
	// Struct to hold all animation parameters so StartCourtine can be called with a string.
	public struct AnimationObject
	{
		public Mesh mesh;
		public Vector3[] targetVerts;
		public float duration;
	}
	
	public Vector3[] RandomizeVerts2D(Vector3[] verts, float range)
	{
		Vector3[] results = new Vector3[verts.Length];
		System.Array.Copy(verts, results, verts.Length);
		for (int i = 0; i < verts.Length; i++)
		{
			results[i].x += UnityEngine.Random.Range(-range, range);
			results[i].y += UnityEngine.Random.Range(-range, range);
		}
		return results;
	}
	
	public void AnimateVerts(Mesh m, Vector3[] targetVerts, float duration)
	{
		AnimationObject ao = new AnimationObject();
		ao.mesh = m;
		ao.targetVerts = targetVerts;
		ao.duration = duration;
		StartCoroutine("CoAnimateVerts", ao);
	}

	IEnumerator CoAnimateVerts(AnimationObject ao)
	{
		TweenVar[] xVars = new TweenVar[ao.mesh.vertexCount];
		TweenVar[] yVars = new TweenVar[ao.mesh.vertexCount];
		// Initialize TweenVars.
		for (int i = 0; i < ao.mesh.vertexCount; i++)
		{
			xVars[i] = new TweenVar(ao.mesh.vertices[i].x, ao.targetVerts[i].x, ao.duration, EaseType.EaseInOutExpo);
			yVars[i] = new TweenVar(ao.mesh.vertices[i].y, ao.targetVerts[i].y, ao.duration, EaseType.EaseInOutExpo);
		}
		Vector3[] tempVerts = new Vector3[ao.mesh.vertices.Length];
		var startTime = Time.time;
		while(Time.time - startTime < ao.duration)
		{
			for (int i = 0; i < ao.mesh.vertices.Length; i++)
			{
				tempVerts[i] = new Vector3(xVars[i].Update(Time.time - startTime), yVars[i].Update(Time.time - startTime), ao.mesh.vertices[i].z);
			}
			ao.mesh.vertices = tempVerts;
			ao.mesh.RecalculateBounds();
			yield return null;
		}
	}
	
	public void RandomizeMesh2D(Mesh m, Vector3[] verts, float maxDist, float duration)
	{
		AnimateVerts(m, RandomizeVerts2D(verts, maxDist), duration);
	}

	public void StopAnimation()
	{
		StopCoroutine("CoAnimateVerts");
	}
}