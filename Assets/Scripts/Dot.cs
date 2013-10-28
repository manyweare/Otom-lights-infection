using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dot : MonoBehaviour
{
	// Editor objects.
	public GameObject DotPrefab, LockedPrefab;

	// CONSTRUCTOR
	public Dot(GameObject dotMesh, GameObject lockedMesh)
	{
		if (dotMesh && lockedMesh)
		{
			_mesh = dotMesh;
			_lockedMesh = lockedMesh;
			_highlightMesh = dotMesh;
			_mesh.AddComponent("Sphere");
			gameObject.AddComponent("Rigidbody");
			gameObject.AddComponent("Box Collider");
		} else
		{
			Debug.Log("Can't create Dot: Missing mesh.");
			return;
		}
	}

	#region PROPERTIES

	public Transform _transform { get { return transform; } }

	// Class with all behavior and animation methods.
	public Sphere _sphere {
		get {
			if (gameObject.GetComponentInChildren<Sphere>() == null)
				gameObject.AddComponent("Sphere");
			return gameObject.GetComponentInChildren<Sphere>();
		}
	}

	// The main mesh for the Dot.
	public GameObject _mesh {
		get {
			if (_sphere.MyMesh == null)
				_sphere.InstantiateDotMesh(DotPrefab);
			return _sphere.MyMesh;
		}
		set {
			if (value.GetType() == typeof(GameObject))
			{
				Destroy(_sphere.MyMesh);
				_sphere.InstantiateDotMesh(value);
			} else
				Debug.Log("Invalid type: Has to be a Game Object.");
		}
	}

	// Mesh the dot turns into when locked.
	public GameObject _lockedMesh {
		get {
			if (_sphere.Locked == null)
				_sphere.InstantiateLockedMesh(LockedPrefab);
			return _sphere.Locked;
		}
		set {
			if (value.GetType() == typeof(GameObject))
			{
				Destroy(_sphere.Locked);
				_sphere.InstantiateLockedMesh(value);
			} else
				Debug.Log("Invalid type: Has to be a Game Object.");
		}
	}

	// Mesh displayed when the dot is highlighted by tap or mouse press.
	public GameObject _highlightMesh {
		get {
			if (_sphere.Highlight == null)
				_sphere.InstantiateHighlightMesh(DotPrefab);
			return _sphere.Highlight;
		}
		set {
			if (value.GetType() == typeof(GameObject))
			{
				Destroy(_sphere.Highlight);
				_sphere.InstantiateHighlightMesh(value);
			} else
				Debug.Log("Invalid type: Has to be a Game Object.");
		}
	}

	public Color _color {
		get { return _mesh.renderer.material.color; }
		set { _mesh.renderer.material.color = value; }
	}

	#endregion

	#region PUBLIC METHODS

	public void RandomizeColor()
	{
		_sphere.AssignNewRandomColor(ArtManager.Instance.ColorList, _mesh);
	}

	public void ComeAlive()
	{
		_sphere.AnimateBirth();
	}

	public void Die()
	{
		_sphere.AnimateDeath();
	}

	public void Die(bool addToScore)
	{
		if (addToScore)
			AddToScore();
	}

	public void Show()
	{
		_sphere.ShowSelf();
	}

	public void Hide()
	{
		_sphere.HideSelf();
	}

	public void Highlight()
	{
		_sphere.ActivateHighlight();
	}

	public void Highlight(bool isWrong)
	{
		_sphere.ActivateHighlight();
		if (isWrong)
			_sphere.WrongSphere();
	}

	public void Lock()
	{
		_sphere.LockSelf();
	}

	public void Unlock()
	{
		_sphere.UnlockSelf();
	}

	public void HighlightLock()
	{
		_sphere.HighlightLock();
	}

	public void ResetLock()
	{
		_sphere.ResetLock();
	}

	#endregion

	private void AddToScore()
	{
		_sphere.AnimateToPowerUp();
	}
}