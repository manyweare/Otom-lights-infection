using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DotManager : MonoBehaviour
{
    public GameObject DotPrefab, LinePrefab;
    [HideInInspector]
    public bool LoopActivated = false;
    [HideInInspector]
    public Color ChainColor;
    [HideInInspector]
    public List<Sphere> cleanList = new List<Sphere>();
    [HideInInspector]
    public List<Sphere> chainList = new List<Sphere>();
    [HideInInspector]
    public List<Sphere> wrongChainList = new List<Sphere>();
    [HideInInspector]
    public List<Sphere> lockedList = new List<Sphere>();
    [HideInInspector]
    public bool dotsPulsating = false;
    private Transform _myTransform;
    private List<Sphere> highlightList = new List<Sphere>();
    private const float CIRCLE_SIZE = 1.2f;

    #region SINGLETON

    public static DotManager Instance
    {
        get
        {
            if (instance == null)
                instance = new DotManager();
            return instance;
        }
    }

    private static DotManager instance = null;

    void Awake()
    {
        if (instance)
            DestroyImmediate(gameObject);
        else
            instance = this;

        _myTransform = transform;

        GameEventManager.NextTurn += NextTurn;
    }

    #endregion

    void Update()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount == 1 && !GameManager.Instance.isRunningSomething && !GameManager.Instance.isPaused)
                HandleTouchInput();
        }
        else
        {
            if (!GameManager.Instance.isRunningSomething && !GameManager.Instance.isPaused)
            {
                if (Input.GetMouseButton(0))
                    HandleMouseInput();
                else if (Input.GetMouseButtonUp(0))
                    GameManager.Instance.CalculatePoints(chainList, wrongChainList, LoopActivated);
            }
        }
    }

    void NextTurn()
    {
        // Clear chain lists.
        chainList.Clear();
        wrongChainList.Clear();
    }

    #region Input Methods

    void HandleTouchInput()
    {
        var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        RaycastHit hit;
        LayerMask spheresLayer = 1 << LayerMask.NameToLayer("Spheres");
        if (Physics.Raycast(ray, out hit, 10f, spheresLayer))
        {
            Sphere activeSphere = hit.collider.gameObject.GetComponent<Sphere>();
            // If the current dot is locked, do nothing.
            if (activeSphere.isLocked)
                return;

            GameObject activeMesh = activeSphere.MyMesh;

            // First dot in chain.
            if (Input.GetTouch(0).phase == TouchPhase.Began && chainList.Count == 0)
                FirstDotInChain(activeSphere);

            // Player moving finger.
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && chainList.Count > 0)
            {
                // Check for acceptable distance first.
                var distance = Vector3.Distance(hit.collider.transform.position, chainList[chainList.Count - 1].transform.position);
                if (distance > 0f && distance < GameManager.Instance.ACCEPTABLE_DISTANCE)
                {
                    // Then check for color.
                    if (ValidateChainColor(activeMesh.renderer.material.color) && (wrongChainList.Count == 0))
                        HandleCorrectChain(activeSphere);
                    else if (!wrongChainList.Contains(activeSphere))
                        HandleWrongChain(activeSphere);
                }
            }
        }

        // Player not touching screen anymore.
        if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
            GameManager.Instance.CalculatePoints(chainList, wrongChainList, LoopActivated);
    }

    void HandleMouseInput()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        LayerMask spheresLayer = 1 << LayerMask.NameToLayer("Spheres");
        if (Physics.Raycast(ray, out hit, 10f, spheresLayer))
        {
            Sphere activeSphere = hit.collider.gameObject.GetComponent<Sphere>();
            // If the current dot is locked, do nothing.
            if (activeSphere.isLocked)
                return;

            GameObject activeMesh = activeSphere.MyMesh;

            // If this is the first dot in the chain.
            if (chainList.Count == 0)
                FirstDotInChain(activeSphere);

            // Player moving mouse and hit another dot.
            else if (chainList.Count > 0)
            {
                // Check for acceptable distance first.
                var distance = Vector3.Distance(hit.collider.transform.position, chainList[chainList.Count - 1].transform.position);
                if (distance > 0f && distance < GameManager.Instance.ACCEPTABLE_DISTANCE)
                {
                    // Then check for color.
                    if (ValidateChainColor(activeMesh.renderer.material.color) && (wrongChainList.Count == 0))
                        HandleCorrectChain(activeSphere);
                    else if (!wrongChainList.Contains(activeSphere))
                        HandleWrongChain(activeSphere);
                }
            }
        }
    }

    bool ValidateChainColor(Color c)
    {
        var result = false;
        // If chain color was white, but the active dot isn't, set chain color to new color.
        if (ChainColor == ArtManager.Instance.WhiteDotColor && c != ChainColor)
        {
            ChainColor = c;
            result = true;
        }
        else if (c == ChainColor || c == ArtManager.Instance.WhiteDotColor)
            result = true;

        return result;
    }

    void FirstDotInChain(Sphere s)
    {
        if (s != null)
            s.ActivateHighlight();
        chainList.Add(s);
        ChainColor = chainList[0].MyMesh.renderer.material.color;

        ChainlineManager.Instance.AddPointToChainLine(s.transform.parent.transform.position, 0);

        AudioManager.Instance.PlayDotSound(1, ChainColor, true);
        LoopActivated = false;
    }

    void HandleCorrectChain(Sphere s)
    {
        // Add dot to chain.
        if (!chainList.Contains(s))
        {
            if (s != null)
                s.ActivateHighlight();
            chainList.Add(s);
            // If the current chain color is white, make this the active chain color.
            if (ChainColor == ArtManager.Instance.WhiteDotColor)
                ChainColor = chainList[chainList.Count - 1].MyMesh.renderer.material.color;
            AudioManager.Instance.PlayDotSound(chainList.Count, s.MyMesh.renderer.material.color, true);

            ChainlineManager.Instance.AddPointToChainLine(s.transform.position, chainList.Count - 1);

            HighlightUnlocks();
        }
        else if ((s == chainList[0]) && (chainList.Count >= 4) && !LoopActivated)
        {
            // Payer just formed a loop.
            Loop.Instance.gameObject.SetActive(true);
            Loop.Instance.Activate();

            ChainlineManager.Instance.AddPointToChainLine(s.transform.position, chainList.Count);

            AudioManager.Instance.PlayLoopSound(ChainColor);
            LoopActivated = true;
            HighlightUnlocks();
        }
        else if (chainList.Contains(s) && !LoopActivated)
        {
            // Dot has already been added, is not the first, and player is dragging back over it.
            RemoveLastHighlight();
            chainList[chainList.Count - 1].DeactivateHighlight();
            chainList.RemoveAt(chainList.Count - 1);
            AudioManager.Instance.PlayDotSound(chainList.Count, s.MyMesh.renderer.material.color, true);

            ChainlineManager.Instance.AddPointToChainLine(s.transform.position, chainList.Count - 1);

            LoopActivated = false;
        }
    }

    void HandleWrongChain(Sphere s)
    {
        // This dot is not the right color or the chain is already broken.
        if (s != null)
        {
            s.ActivateHighlight();
            s.WrongSphere();
        }
        if (highlightList.Count >= 0)
        {
            foreach (Sphere i in highlightList)
            {
                i.ResetLock();
            }
        }
        wrongChainList.Add(s);
        AudioManager.Instance.PlayDotSound(wrongChainList.Count, s.MyMesh.renderer.material.color, false);
    }

    #endregion

    #region View Methods

    public IEnumerator SpawnWithPhysics()
    {
        Debug.Log("*** SPAWNING SPHERES ***");
        cleanList.Clear();
        lockedList.Clear();
        wrongChainList.Clear();
        chainList.Clear();

        // Pool of spawned dots.
        var tempList = new List<GameObject>();
        // Determine where the game board will start to be generated.
        var spawnPoint = new Vector3(-1f * ArtManager.Instance.screenRatio / 2f, _myTransform.position.y + 2f, _myTransform.position.z);
        _myTransform.position = spawnPoint;

        // Instantiate all dots.
        for (int j = 0; j < 6; j++)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject dot = Instantiate(DotPrefab, _myTransform.position, Quaternion.identity) as GameObject;
                dot.rigidbody.useGravity = false;
                dot.name = j.ToString() + i.ToString();
                tempList.Add(dot);
                cleanList.Add(dot.GetComponentInChildren<Sphere>());
                _myTransform.position += new Vector3(0f, 0.2f, 0f);
            }
            _myTransform.position += new Vector3(0.2f * ArtManager.Instance.screenRatio, -1.2f, 0f);
        }

        // Release all dots.
        foreach (GameObject dot in tempList)
        {
            dot.rigidbody.useGravity = true;
            yield return new WaitForSeconds(0.05f);
            yield return null;
        }
        _myTransform.position = spawnPoint;
    }

    public IEnumerator Spawn(int rows, int columns)
    {
        Debug.Log("*** SPAWNING SPHERES ***");
        cleanList.Clear();
        lockedList.Clear();
        wrongChainList.Clear();
        chainList.Clear();

        // Pool of spawned dots.
        var tempList = new List<GameObject>();
        // Determine where the game board will start to be generated.
        var spawnPoint = new Vector3(-1f * ArtManager.Instance.screenRatio / 2f, _myTransform.position.y, _myTransform.position.z);
        _myTransform.position = spawnPoint;

        // Instantiate all dots.
        for (int j = 0; j < columns; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                GameObject dot = Instantiate(DotPrefab, _myTransform.position, Quaternion.identity) as GameObject;
                dot.name = j.ToString() + i.ToString();
                tempList.Add(dot);
                cleanList.Add(dot.GetComponentInChildren<Sphere>());
                _myTransform.position += new Vector3(0f, CIRCLE_SIZE / (float)rows * ArtManager.Instance.screenRatio, 0f);
                yield return null;
            }
            _myTransform.position += new Vector3(CIRCLE_SIZE / (float)columns * ArtManager.Instance.screenRatio, -((CIRCLE_SIZE / (float)rows) * (float)rows) * ArtManager.Instance.screenRatio, 0f);
        }
        _myTransform.position = spawnPoint;
    }

    public IEnumerator DestroySphere(Sphere dotToDestroy)
    {
        // Animate dot towards power up button of same color.
        yield return StartCoroutine(dotToDestroy.AnimateToPowerUp());
        yield return new WaitForSeconds(0.1f);
        dotToDestroy.gameObject.SetActive(true);
        dotToDestroy.AnimateBirth();
    }

    public IEnumerator DestroyAllSpheres()
    {
        Debug.Log("*** DESTROY ALL SPHERES ***");
        List<Sphere> completeList = new List<Sphere>();
        completeList.AddRange(cleanList);
        completeList.AddRange(lockedList);

        if (completeList.Count <= 0)
            yield break;

        foreach (Sphere deadDot in completeList)
        {
            Destroy(deadDot);
            yield return null;
        }
        completeList.Clear();
    }

    void HighlightUnlocks()
    {
        if (lockedList.Count == 0 || chainList.Count < 2)
            return;
        if (LoopActivated)
        {
            foreach (Sphere dot in lockedList)
            {
                dot.HighlightLock();
            }
            return;
        }
        var i = chainList.Count - 1;
        if (lockedList.Count - i >= 0)
        {
            lockedList[lockedList.Count - i].HighlightLock();
            highlightList.Add(lockedList[lockedList.Count - i]);
        }
    }

    void RemoveLastHighlight()
    {
        if (highlightList.Count <= 0)
            return;

        highlightList[highlightList.Count - 1].ResetLock();
        highlightList.RemoveAt(highlightList.Count - 1);
    }

    public IEnumerator SpreadInfection()
    {
        // If no circles are locked, run the random lock method.
        if (lockedList.Count == 0)
        {
            StartCoroutine("LockSpheres");
            yield break;
        }

        //// Create new list with random circles from the locked list.
        //List<Sphere> vectors = lockedList.Select(item => (Sphere)item).ToList();
        //vectors.Shuffle();
        //int max = UnityEngine.Random.Range(1, vectors.Count);
        //int k = UnityEngine.Random.Range(1, max);
        //vectors.Take(k);

        // Find all the lockable dots around each vector.
        var newLocks = new List<Sphere>();
        for (int i = 0; i < lockedList.Count; i++)
        {
            LayerMask layer = 1 << LayerMask.NameToLayer("Spheres");
            Collider[] neighbors = Physics.OverlapSphere(lockedList[i].transform.position, GameManager.Instance.ACCEPTABLE_DISTANCE - 0.05f, layer);
            foreach (Collider n in neighbors)
            {
                Sphere hitDot = n.GetComponent<Sphere>();
                if (hitDot.isLockable && !newLocks.Contains(hitDot))
                {
                    if (ShouldBeLocked())
                    {
                        hitDot.LockSelf();
                        newLocks.Add(hitDot);
                        cleanList.Remove(hitDot);
                        yield return null;
                    }
                }
            }
        }
        if (newLocks != null)
            lockedList.AddRange(newLocks);
    }

    bool ShouldBeLocked()
    {
        bool result = false;
        int max = 10 - Mathf.FloorToInt(GameManager.Instance.Level / 6);

        int k = UnityEngine.Random.Range(0, max);
        result = k == 0 ? true : false;

        return result;
    }

    public IEnumerator LockSpheres()
    {
        // Add all lockable dots to a new list of lockable dots.
        var lockableList = new List<Sphere>();
        for (int i = 0; i < cleanList.Count; i++)
        {
            if (cleanList[i].isLockable)
                lockableList.Add(cleanList[i]);
        }
        if (lockableList.Count == 0)
        {
            Debug.Log("No lockable dots.");
            yield break;
        }

        int maxLocked = 3 + Mathf.RoundToInt(GameManager.Instance.Level % 4);
        // More dots get locked if the board is cleaner.
        if (lockableList.Count >= 30)
            maxLocked += 6;
        else if (lockableList.Count < 30 && lockableList.Count >= 16)
            maxLocked += 4;

        int minLocked = 2;
        // Add one to minimum dots locked every 4 levels.
        minLocked += Mathf.RoundToInt(GameManager.Instance.Level / 4);

        int numDotsToLock = UnityEngine.Random.Range(minLocked, maxLocked + 1);
        // Add one to total dots locked every 6 levels.
        numDotsToLock += Mathf.RoundToInt(GameManager.Instance.Level / 6);
        if (numDotsToLock > lockableList.Count)
            numDotsToLock = lockableList.Count;

        Debug.Log("*** LOCKING " + numDotsToLock.ToString() + " DOTS ***");
        while (numDotsToLock > 0)
        {
            Sphere randDot = lockableList[(int)UnityEngine.Random.Range(0, lockableList.Count)];
            randDot.LockSelf();
            lockableList.Remove(randDot);
            // Update global lists.
            lockedList.Add(randDot);
            cleanList.Remove(randDot);
            --numDotsToLock;
            yield return new WaitForSeconds(0.1f);
            yield return null;
        }
        lockableList.Clear();
    }

    public IEnumerator LockAllSpheres()
    {
        foreach (Sphere dot in cleanList)
        {
            dot.LockSelf();
            lockedList.Add(dot);
            yield return new WaitForSeconds(0.05f);
            yield return null;
        }
        cleanList.Clear();
        GameManager.Instance.IsGameOver();
    }

    public IEnumerator UnlockSpheres(int i)
    {
        while (i > 0 && lockedList.Count > 0)
        {
            var lastLockedDot = lockedList[lockedList.Count - 1];
            cleanList.Add(lastLockedDot);
            lastLockedDot.UnlockSelf();
            lockedList.Remove(lastLockedDot);
            --i;
            yield return new WaitForSeconds(0.05f);
            yield return null;
        }
        yield return new WaitForSeconds(0.05f);
    }

    public IEnumerator UnlockAllSpheres()
    {
        Debug.Log("*** UNLOCK ALL SPHERES ***");
        for (int i = 0; i < lockedList.Count; i++)
        {
            lockedList[i].UnlockSelf();
            yield return null;
            //			yield return new WaitForSeconds(0.05f);
        }
        // UnlockSelf takes 0.2f.
        float duration = (float)lockedList.Count * 0.21f * 0.01f;
        yield return new WaitForSeconds(duration);

        cleanList.AddRange(lockedList);
        lockedList.Clear();
    }

    public List<Sphere> UnlockSpheresOfColor(Color32 c)
    {
        int i = ArtManager.Instance.OriginalColors.IndexOf(c);
        if (i < 0) return null;
        if (PowerUpsManager.Instance.PowerUpArray[i] == 0)
        {
            Debug.Log("Not enough power ups.");
            return null;
        }
        if (lockedList.Count == 0)
        {
            Debug.Log("No locked dots.");
            return null;
        }
        // Create temporary list to hold all locked dots of the same color.
        List<Sphere> cList = new List<Sphere>();
        foreach (Sphere lockedDot in lockedList)
        {
            if ((Color)lockedDot.CurrentColor == (Color)c)
                cList.Add(lockedDot);
        }
        // Animate dots unlocking.
        StartCoroutine(CoUnlockSpheresOfColor(cList));
        // Update lists.
        foreach (Sphere dot in cList)
        {
            cleanList.Add(dot);
            lockedList.Remove(dot);
        }
        return cList;
    }

    IEnumerator CoUnlockSpheresOfColor(List<Sphere> list)
    {
        if (list == null) yield break;

        foreach (Sphere dot in list)
        {
            dot.UnlockSelf();
            yield return new WaitForSeconds(0.05f);
        }
    }

    public IEnumerator HideAllSpheres()
    {
        Debug.Log("*** HIDE ALL SPHERES ***");
        List<Sphere> completeList = new List<Sphere>();
        completeList.AddRange(cleanList);
        completeList.AddRange(lockedList);
        completeList.Shuffle();

        if (completeList.Count <= 0)
            yield break;

        foreach (Sphere dot in completeList)
        {
            dot.HideSelf();
            yield return new WaitForSeconds(0.01f);
            yield return null;
        }
        // HideSelf takes 0.2f.
        float duration = (0.01f * (float)completeList.Count) + 0.2f;
        yield return new WaitForSeconds(duration);
    }

    public IEnumerator ShowAllSpheres()
    {
        Debug.Log("*** SHOW ALL SPHERES ***");
        List<Sphere> completeList = new List<Sphere>();
        completeList.AddRange(cleanList);
        completeList.AddRange(lockedList);
        completeList.Shuffle();

        if (completeList.Count <= 0)
            yield break;

        foreach (Sphere dot in completeList)
        {
            dot.ShowSelf();
            yield return new WaitForSeconds(0.01f);
            yield return null;
        }
        // ShowSelf takes 0.2f.
        float duration = (0.01f * (float)completeList.Count) + 0.2f;
        yield return new WaitForSeconds(duration);
    }

    // Method for highlighting dots after there are no more moves.
    public IEnumerator GameOverDots()
    {
        foreach (Sphere dot in cleanList)
        {
            dot.StopPulse();
            dot.ActivateHighlight();
            dot.WrongSphere();
            yield return new WaitForSeconds(0.1f);
            yield return null;
        }
    }

    public void PulsateDots()
    {
        foreach (Sphere dot in DotManager.Instance.cleanList)
        {
            dot.Pulsate();
        }
        dotsPulsating = true;
    }

    public void StopPulsatingDots()
    {
        foreach (Sphere dot in DotManager.Instance.cleanList)
        {
            dot.StopPulse();
        }
        dotsPulsating = false;
    }

    #endregion
}