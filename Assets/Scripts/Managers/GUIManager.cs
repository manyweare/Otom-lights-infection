using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
	public GameObject PauseButton, ProgressBarGUI;

	// SINGLETON
	public static GUIManager Instance {
		get {
			if (instance == null)
				instance = new GUIManager();
			return instance;
		}
	}

	private static GUIManager instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
		{
			instance = this;
		}
	}

	public void UpdateInterface()
	{
		ProgressBar.Instance.UpdateSize();
//		PowerUpsManager.Instance.UpdateAllSizes();
	}
	
	public void UpdateMessage(string message)
	{
		Messages.Instance.ShowNewMessage(message);
	}

	public void HideGUI()
	{
		Messages.Instance.HideSelf();
		PowerUpsManager.Instance.HideSelf();
		ProgressBarGUI.GetComponent<ProgressBarContainer>().HideSelf();
		LevelTitle.Instance.HideSelf();
		PauseButton.SetActive(false);
	}

	public void ShowGUI()
	{
		Messages.Instance.ShowSelf();
		PowerUpsManager.Instance.ShowSelf();
		ProgressBarGUI.GetComponent<ProgressBarContainer>().ShowSelf();
		LevelTitle.Instance.ShowSelf();
		PauseButton.SetActive(true);
	}

	public void LoadPauseMenu()
	{
		StartCoroutine("CoLoadPauseMenu");
	}

	IEnumerator CoLoadPauseMenu()
	{
		HideGUI();
		yield return StartCoroutine(PauseMenu.Instance.Activate());
		GameManager.Instance.isPaused = true;
	}

	public void HidePauseMenu(bool s)
	{
		StartCoroutine(CoHidePauseMenu(s));
	}

	IEnumerator CoHidePauseMenu(bool show)
	{
		if (show)
			ShowGUI();
		yield return StartCoroutine(PauseMenu.Instance.Deactivate());
		GameManager.Instance.isPaused = false;
	}
}
