using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
	public GameObject Floor;
	[HideInInspector]
	public bool isRunningSomething = false;
	[HideInInspector]
	public bool isPaused = false;
	[HideInInspector]
	public int Level = 1;
	[HideInInspector]
	public int Turn = 1;
	[HideInInspector]
	public int TurnsPerLevel = 6;
	[HideInInspector]
	public int Score = 0;
	[HideInInspector]
	public int FullScore = 0;
	[HideInInspector]
	public int Loops = 0;
	[HideInInspector]
	public int PowerUpsUsed = 0;
	[HideInInspector]
	public int PurpleScore, BlueScore, PinkScore, TealScore, YellowScore, GreyScore, WhiteScore = 0;
	[HideInInspector]
	public int ScorePerLevel = 12;
	[HideInInspector]
	public int LongestChain = 0;
	[HideInInspector]
	public int MostUnlocks = 0;
	[HideInInspector]
	public int ColorScorePerPower = 12;
	[HideInInspector]
	// Acceptable distance has to be 0.3f or higher for diagonals.
	public float ACCEPTABLE_DISTANCE = 0.3f;
	[HideInInspector]
	public string levelMessage = "";
	private string currentMessage = "";

	#region SINGLETON

	public static GameManager Instance {
		get {
			if (instance == null)
				instance = new GameManager();
			return instance;
		}
	}

	private static GameManager instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		ACCEPTABLE_DISTANCE *= ArtManager.Instance.screenRatio;

		GameEventManager.NextTurn += NextTurn;
	}

	#endregion

	void Start()
	{
		LoadNewGame();
	}

	#region INPUT

	void Update()
	{
		// Pause button input.
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount == 1 && !GameOver.Instance.gameObject.activeSelf && !isPaused)
			{
				var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
				RaycastHit hit;
				LayerMask layer = 1 << LayerMask.NameToLayer("PauseMenu");
				if (Physics.Raycast(ray, out hit, 10f, layer))
					if (hit.collider.name == "Pause Button")
						GUIManager.Instance.LoadPauseMenu();
			}
		} else
			{
			// Test Input for Mac/PC.
			if (!isRunningSomething && !isPaused)
			{
				if (Input.GetKeyDown(KeyCode.G))
					LoadGameOver();
				else if (Input.GetKeyDown(KeyCode.L))
					LoadNextLevel();
				else if (Input.GetKeyDown(KeyCode.R))
					StartCoroutine("ResetBoard");
			}

			if (!GameOver.Instance.gameObject.activeSelf)
			{
				if (Input.GetKeyDown(KeyCode.P))
				{
					if (!isPaused)
					{
						GUIManager.Instance.LoadPauseMenu();
					} else if (isPaused)
					{
						GUIManager.Instance.HidePauseMenu(true);
					}
				}
				if (Input.GetMouseButton(0) && !isPaused)
				{
					var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					LayerMask layer = 1 << LayerMask.NameToLayer("PauseMenu");
					if (Physics.Raycast(ray, out hit, 10f, layer) && hit.collider.name == "Pause Button")
					{
						GUIManager.Instance.LoadPauseMenu();
					}
				}
			}
		}
	}

	#endregion

	private void NextTurn()
	{
		Turn++;
		FullScore += Score;

		if (!IsGameOver())
		{
			CheckIfNextLevel();

			// Message that warns player if game over is imminent.
			if (!IsGameOverImminent())
			{
				// Only replace current message if it has changed.
				if (currentMessage != levelMessage)
				{
					GUIManager.Instance.UpdateMessage(levelMessage);
					currentMessage = levelMessage;	
				}
			}
		}
	}

	public void CalculatePoints(List<Sphere> c, List<Sphere> w, bool l)
	{
		// c is a list with dots in the chain.
		// w is a list with dots that are not supposed to be in the chain.
		// l is true if a loop was activated.
		if (c.Count == 0 && w.Count == 0)
			return;
		StartCoroutine(CoCalculatePoints(c, w, l));
	}

    IEnumerator CoCalculatePoints(List<Sphere> chainList, List<Sphere> wrongChainList, bool loopActivated)
    {
        isRunningSomething = true;

        //		StartCoroutine(DotManager.Instance.DeleteChainLines(loopActivated));

        // If there are any wrong dots in the chain, or just one dot, clear the chains.
        if (wrongChainList.Count > 0 || chainList.Count <= 1)
        {
            for (int i = 0; i < wrongChainList.Count; i++)
            {
                wrongChainList[i].DeactivateHighlight();
            }
            for (int i = 0; i < chainList.Count; i++)
            {
                chainList[i].DeactivateHighlight();
            }
            ChainlineManager.Instance.FadeOutLine();
            DotManager.Instance.wrongChainList.Clear();
            DotManager.Instance.chainList.Clear();
            isRunningSomething = false;
            yield break;
        }

        // If a loop was formed, unlock all locked dots, if not, unlock chainList.Count - 1.
        if (loopActivated)
        {
            ++Loops;
            ChainlineManager.Instance.ZoomLine(chainList.Count, DotManager.Instance.ChainColor, true);
            ChainlineManager.Instance.FadeOutLine();
            yield return StartCoroutine(DotManager.Instance.UnlockAllSpheres());
        }
        else
        {
            ChainlineManager.Instance.ZoomLine(chainList.Count, DotManager.Instance.ChainColor);
            ChainlineManager.Instance.FadeOutLine();
            yield return StartCoroutine(DotManager.Instance.UnlockSpheres(chainList.Count - 1));
        }

        // Destroy spheres in chainList.
        for (int i = 0; i < chainList.Count; i++)
        {
            // Add to total score, one by one, as dots are destroyed.
            ++Score;

            // Add to color score, also one by one.
            AddColorScore(chainList[i].MyMesh.renderer.material.color);
            yield return StartCoroutine(DotManager.Instance.DestroySphere(chainList[i]));
            yield return null;
        }

        // Add to record if applicable.
        if ((chainList.Count - 1) > GameManager.Instance.MostUnlocks)
            GameManager.Instance.MostUnlocks = chainList.Count - 1;
        // Keep record of longest chain.
        if (chainList.Count > LongestChain)
            LongestChain = chainList.Count;

        // Only lock spheres this turn if chain wasn't a loop.
        if (!loopActivated)
            yield return StartCoroutine(DotManager.Instance.SpreadInfection());

        GameEventManager.TriggerNextTurn();

        isRunningSomething = false;
    }
	
	public void PowerUpActions(Color32 color)
	{
		var unlockedList = new List<Sphere>();
		unlockedList.AddRange(DotManager.Instance.UnlockSpheresOfColor(color));
		// Add to record if appropriate.
		if (unlockedList.Count > MostUnlocks)
			MostUnlocks = unlockedList.Count;
		++PowerUpsUsed;
		IsGameOverImminent();
	}
	
	bool IsGameOverImminent()
	{
		// Check if game over is imminent and if so, warn player.
		if (CheckIfGameOverIsImminent(DotManager.Instance.cleanList))
		{
			GUIManager.Instance.UpdateMessage("Game Over Imminent");
			DotManager.Instance.PulsateDots();
			return true;
		} else if (DotManager.Instance.dotsPulsating)
		{
			GUIManager.Instance.UpdateMessage(levelMessage);
			DotManager.Instance.StopPulsatingDots();
		}
		return false;
	}

	#region Level and Game Over Methods.

	public void CheckIfNextLevel()
	{
		if (Score >= ScorePerLevel)
			LoadNextLevel();
	}
	
	public bool IsGameOver()
	{
		var list = DotManager.Instance.cleanList;
		foreach (Sphere dot in list)
		{
			var thisDot = dot.MyMesh;
			LayerMask layer = 1 << LayerMask.NameToLayer("Spheres");
			Collider[] hitColliders = Physics.OverlapSphere(dot.transform.position, ACCEPTABLE_DISTANCE - 0.05f, layer);
			foreach (Collider hit in hitColliders)
			{
				Sphere hitDot = hit.GetComponent<Sphere>();
				bool isLocked = hitDot.isLocked;
				bool isSelf = hit.transform.parent.gameObject.name == dot.transform.parent.name;
				bool hasSameColorNeighbor = (Color)thisDot.renderer.material.color == (Color)hitDot.CurrentColor;
				bool hasWhiteNeighbor = (Color)hitDot.CurrentColor == (Color)ArtManager.Instance.WhiteDotColor;
				if (!isLocked && !isSelf && (hasSameColorNeighbor || hasWhiteNeighbor))
					return false;
			}
		}
		LoadGameOver();
		return true;
	}
	
	public bool CheckIfGameOverIsImminent(List<Sphere> cleanList)
	{
		if (cleanList.Count <= 16)
			return true;
		
		var result = false;
		{
			foreach (Color32 c in ArtManager.Instance.OriginalColors)
			{
				var colorCount = 0;
				// Finds how many of each color there are in the clean list.
				foreach (Sphere dot in cleanList)
				{
					if ((Color)dot.CurrentColor == (Color)c)
						++colorCount;
				}
				// Less than 2 means game over is imminent.
				if (colorCount > 2)
					return false;
				else
					result = true;
			}
		}
		return result;
	}
	
	#endregion

	#region Level Manager Methods

	public void LoadNewGame()
	{
		StartCoroutine("CoLoadNewGame");
	}

	IEnumerator CoLoadNewGame()
	{
		isRunningSomething = true;
		yield return new WaitForSeconds(0.5f);
		ResetScore();
		ManageDifficultyCurve(Level);
		GUIManager.Instance.ShowGUI();
		yield return StartCoroutine(LevelTitle.Instance.Activate());
		yield return new WaitForSeconds(0.2f);
		GUIManager.Instance.UpdateMessage(levelMessage);
		if (ArtManager.Instance.ColorList.Count > 5)
			ArtManager.Instance.RemoveColors(ArtManager.Instance.ColorList.Count - 5);
		yield return StartCoroutine(DotManager.Instance.UnlockAllSpheres());
		yield return StartCoroutine(DotManager.Instance.HideAllSpheres());
		DotManager.Instance.DestroyAllSpheres();
		yield return StartCoroutine(DotManager.Instance.Spawn(6, 6));
		yield return StartCoroutine(DotManager.Instance.SpreadInfection());
		isRunningSomething = false;
	}

	public void RestartGame()
	{
		StartCoroutine("CoRestartGame");
	}

	IEnumerator CoRestartGame()
	{
		isRunningSomething = true;
		WritePlayerPrefs();
		ResetScore();
		ManageDifficultyCurve(Level);
		GUIManager.Instance.UpdateInterface();

		if (GameOver.Instance.gameObject.activeSelf)
		{
			StartCoroutine(GameOver.Instance.Deactivate());
			yield return StartCoroutine(LevelTitle.Instance.Activate());
		} else if (isPaused)
		{
			GUIManager.Instance.HidePauseMenu(true);
			yield return StartCoroutine(LevelTitle.Instance.Activate());
		}
		// Remove grey spheres.
		if (ArtManager.Instance.ColorList.Count > 5)
			ArtManager.Instance.RemoveColors(ArtManager.Instance.ColorList.Count - 5);
		yield return StartCoroutine("ResetBoard");
		GUIManager.Instance.ShowGUI();
		isRunningSomething = false;
	}

	void ResetScore()
	{
		Level = 1;
		Turn = 1;
		TurnsPerLevel = 6;
		Score = 0;
		FullScore = 0;
		ScorePerLevel = 16;
		LongestChain = 0;
		MostUnlocks = 0;
		for (int i = 0; i < ArtManager.Instance.ColorList.Count; i++)
		{
			PowerUpsManager.Instance.ColorScoreList[i] = 0;
		}
		for (int i = 0; i < PowerUpsManager.Instance.PowerUpArray.Length; i++)
		{
			PowerUpsManager.Instance.PowerUpArray[i] = 1;
		}
		GUIManager.Instance.UpdateInterface();
	}

	public void AddColorScore(Color32 c)
	{
		GUIManager.Instance.UpdateInterface();
		
		// Grey spheres aren't added to the specific color score, just logged to stats.
		if ((Color)c == (Color)ArtManager.Instance.GreyDotColor)
		{
			++GreyScore;
			return;
		}
		// If dot is white, add to all color scores.
		else if ((Color)c == (Color)ArtManager.Instance.WhiteDotColor)
		{
			++WhiteScore;
			for (int i = 0; i < PowerUpsManager.Instance.ColorScoreList.Count; i++)
			{
				PowerUpsManager.Instance.ColorScoreList[i] += 1;
				PowerUpsManager.Instance.UpdateSize(i);
			}
		} else
		{
			int colorIndex = ArtManager.Instance.ColorList.FindIndex(go => (Color)go == (Color)c);
			if (colorIndex < 6)
			{
				PowerUpsManager.Instance.ColorScoreList[colorIndex] += 1;

				// HACK: This is hard coded to the colors in ArtManager. Very bad.
				Color tempC = ArtManager.Instance.ColorList[colorIndex];
				if (tempC == ArtManager.Instance.Color01)
					++PurpleScore;
				else if (tempC == ArtManager.Instance.Color02)
					++PinkScore;
				else if (tempC == ArtManager.Instance.Color03)
					++TealScore;
				else if (tempC == ArtManager.Instance.Color04)
					++BlueScore;
				else if (tempC == ArtManager.Instance.Color05)
					++YellowScore;
				else
					Debug.Log("WARNING: No color score added!");
			}
			PowerUpsManager.Instance.UpdateSize(colorIndex);	
		}
	}

	IEnumerator ResetBoard()
	{
		Debug.Log("*** RESET BOARD ***");
		isRunningSomething = true;
		yield return StartCoroutine(DotManager.Instance.UnlockAllSpheres());
		yield return StartCoroutine(DotManager.Instance.HideAllSpheres());
		yield return StartCoroutine(DotManager.Instance.ShowAllSpheres());
		yield return StartCoroutine(DotManager.Instance.SpreadInfection());
		isRunningSomething = false;
	}

	public void LoadNextLevel()
	{
		Debug.Log("*** LOAD NEXT LEVEL ***");
		++Level;
		Score = 0;
		ScorePerLevel += 2;
		ManageDifficultyCurve(Level);
		GUIManager.Instance.UpdateInterface();
		WritePlayerPrefs();
		StartCoroutine(LevelTitle.Instance.Activate());
	}

	public void LoadGameOver()
	{
		StartCoroutine("CoLoadGameOver");
	}

	IEnumerator CoLoadGameOver()
	{
		Debug.Log("*** GAME OVER ***");
		isRunningSomething = true;
		GUIManager.Instance.UpdateMessage("");
		yield return StartCoroutine(DotManager.Instance.GameOverDots());
		yield return new WaitForSeconds(1f);
		GUIManager.Instance.HideGUI();
		yield return StartCoroutine(GameOver.Instance.Activate());
		WritePlayerPrefs();
	}

	// Can only happen during pause menu.
	public void LoadMainMenu()
	{
		StartCoroutine("CoLoadMainMenu");
	}

	IEnumerator CoLoadMainMenu()
	{
		isRunningSomething = true;
		GUIManager.Instance.HidePauseMenu(false);
		GUIManager.Instance.HideGUI();
		WritePlayerPrefs();
		yield return StartCoroutine(DotManager.Instance.HideAllSpheres());
		yield return StartCoroutine(DotManager.Instance.DestroyAllSpheres());
		Application.LoadLevel("Main Menu");
	}

	#endregion

	void WritePlayerPrefs()
	{
		// RECORDS

		// Retrieve records.
		var recordLevel = PlayerPrefs.GetInt("Level", 1);
		var recordTurn = PlayerPrefs.GetInt("Turn", 0);
		var recordChain = PlayerPrefs.GetInt("Longest Chain", 0);
		var recordUnlocks = PlayerPrefs.GetInt("Most Unlocks", 0);
		// Compare current with record, add if larger.
		if (Level > recordLevel)
			PlayerPrefs.SetInt("Level", Level);
		if (Turn > recordTurn)
			PlayerPrefs.SetInt("Turn", Turn);
		if (LongestChain > recordChain)
			PlayerPrefs.SetInt("Longest Chain", LongestChain);
		if (MostUnlocks > recordUnlocks)
			PlayerPrefs.SetInt("Most Unlocks", MostUnlocks);

		// STATISTICS

		// Retrieve totals.
		var totalScore = PlayerPrefs.GetInt("Score", 0);
		var totalLoops = PlayerPrefs.GetInt("Total Loops", 0);
		var totalPowerUps = PlayerPrefs.GetInt("Total Power Ups", 0);
		var totalPurple = PlayerPrefs.GetInt("Purple", 0);
		var totalBlue = PlayerPrefs.GetInt("Blue", 0);
		var totalTeal = PlayerPrefs.GetInt("Teal", 0);
		var totalPink = PlayerPrefs.GetInt("Pink", 0);
		var totalYellow = PlayerPrefs.GetInt("Yellow", 0);
		var totalGrey = PlayerPrefs.GetInt("Grey", 0);
		var totalWhite = PlayerPrefs.GetInt("White", 0);
		// Add to totals.
		PlayerPrefs.SetInt("Score", FullScore + totalScore);
		PlayerPrefs.SetInt("Total Loops", Loops + totalLoops);
		PlayerPrefs.SetInt("Total Power Ups", PowerUpsUsed + totalPowerUps);
		PlayerPrefs.SetInt("Purple", PurpleScore + totalPurple);
		PlayerPrefs.SetInt("Blue", BlueScore + totalBlue);
		PlayerPrefs.SetInt("Teal", TealScore + totalTeal);
		PlayerPrefs.SetInt("Pink", PinkScore + totalPink);
		PlayerPrefs.SetInt("Yellow", YellowScore + totalYellow);
		PlayerPrefs.SetInt("Grey", GreyScore + totalGrey);
		PlayerPrefs.SetInt("White", WhiteScore + totalWhite);
		// Reset current numbers.
		FullScore = 0;
		Loops = 0;
		PowerUpsUsed = 0;
		PurpleScore = 0;
		BlueScore = 0;
		TealScore = 0;
		PinkScore = 0;
		YellowScore = 0;
		GreyScore = 0;
		WhiteScore = 0;
	}
	
	void ManageDifficultyCurve(int level)
	{
		switch (level)
		{
		case 1:
			levelMessage = "Greeting";
			break;
		case 4:
			ArtManager.Instance.AddGreyDot();
			levelMessage = "Warning";
			break;
		case 5:
			ArtManager.Instance.RemoveGreyDot();
			levelMessage = "Regular";
			break;
		case 6:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Breather";
			break;
		case 7:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 8:
			ArtManager.Instance.AddGreyDot();
			levelMessage = "Warning";
			break;
		case 9:
			ArtManager.Instance.RemoveGreyDot();
			levelMessage = "Regular";
			break;
		case 12:
			ArtManager.Instance.AddGreyDot();
			levelMessage = "Warning";
			break;
		case 14:
			ArtManager.Instance.RemoveGreyDot();
			levelMessage = "Regular";
			break;
		case 16:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Breather";
			break;
		case 18:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 20:
			levelMessage = "Congratulations";
			break;
		case 22:
			ArtManager.Instance.AddGreyDot();
			levelMessage = "Warning";
			break;
		case 24:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Breather";
			break;
		case 26:
			ArtManager.Instance.RemoveGreyDot();
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 30:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 31:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 36:
			ArtManager.Instance.AddGreyDot(); // PERMANENT GREY DOT!
			levelMessage = "Warning";
			break;
		case 40:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 42:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 50:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 52:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 60:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 62:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 70:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 72:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 80:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 82:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 90:
			ArtManager.Instance.AddWhiteDot(); // BREATHER
			levelMessage = "Congratulations";
			break;
		case 94:
			ArtManager.Instance.RemoveWhiteDot();
			levelMessage = "Regular";
			break;
		case 100:
			ArtManager.Instance.AddWhiteDot(); // PERMANENT WHITE DOT!
			levelMessage = "Congratulations";
			break;
		default:
			levelMessage = "Regular";
			break;
		}
	}
}