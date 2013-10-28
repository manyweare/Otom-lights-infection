using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class Sphere : MonoBehaviour
{
    public GameObject HighlightPrefab, LockedPrefab, ShadowPrefab;
    public Color32 CurrentColor = Color.black;
    public Color WrongColor = Color.red;
    public GameObject MyMesh, Highlight, Locked;
    public Material PlainMaterial;
    private GameObject myShadow;
    private Transform _myTransform, _lockedTransform;
    private Sequence pulseSequence;
    private GameObject[] powerUpAnimPool = new GameObject[36];
    private GameObject myLight;
    private int numTurnsUnlockable = 0;

    // This property controls weather or not a circle is lockable.
    private bool _isLockable = true;
    public bool IsLockable
    {
        get
        {
            if ((Color)CurrentColor == (Color)ArtManager.Instance.WhiteDotColor)
            {
                _isLockable = false;
                return _isLockable;
            }
            else if (numTurnsUnlockable <= 0)
                _isLockable = true;
            else if (numTurnsUnlockable > 0)
                _isLockable = false;
            return _isLockable;
        }
        set
        {
            if ((Color)CurrentColor == (Color)ArtManager.Instance.WhiteDotColor)
            {
                _isLockable = false;
                return;
            }
            else if (value)
            {
                numTurnsUnlockable = 0;
                _isLockable = true;
            }
            else if (!value)
            {
                numTurnsUnlockable = 2;
                _isLockable = false;
            }
        }
    }

    // Mesh randomizer cache variables.
    private Mesh _originalMesh;
    private Vector3[] _originalVerts;

    private bool stopMorph = false;
    private float _lightIntensity = 3f;
    private float _lightRange = 0.2f;

    void Awake()
    {
        _myTransform = transform;
        _myTransform.localScale *= ArtManager.Instance.screenRatio;
        var _originalSize = _myTransform.localScale;

        GameEventManager.NextTurn += NextTurn;

        // Sequence with pulse that warns player that game over is imminent.
        pulseSequence = new Sequence(new SequenceParms().Loops(-1, LoopType.Restart));
        var firstPulseTween = HOTween.To(_myTransform, 0.4f,
            new TweenParms().Prop("localScale", _myTransform.localScale + new Vector3(0.15f, 0.15f, 0.15f)).Ease(EaseType.EaseInExpo));
        var backPulseTween = HOTween.To(_myTransform, 0.2f,
            new TweenParms().Prop("localScale", _originalSize).Ease(EaseType.EaseOutExpo));
        var secondPulseTween = HOTween.To(_myTransform, 0.2f,
            new TweenParms().Prop("localScale", _myTransform.localScale + new Vector3(0.1f, 0.1f, 0.1f)).Ease(EaseType.EaseInExpo));
        pulseSequence.Append(firstPulseTween);
        pulseSequence.Append(backPulseTween);
        pulseSequence.Append(secondPulseTween);
        pulseSequence.Append(backPulseTween);
        pulseSequence.AppendInterval(1f);
        pulseSequence.Pause();
    }

    void Start()
    {
        InstantiateDotMesh(ArtManager.Instance.DataMeshList[0]);

        // Change main Dot prefab collider radius based on screen ratio.
        _myTransform.parent.transform.GetComponent<BoxCollider>().size *= ArtManager.Instance.screenRatio;

        _originalMesh = MyMesh.GetComponent<MeshFilter>().mesh;
        _originalVerts = new Vector3[_originalMesh.vertexCount];
        System.Array.Copy(_originalMesh.vertices, _originalVerts, _originalMesh.vertexCount);
    }

    private void NextTurn()
    {
        if (numTurnsUnlockable > 0)
            --numTurnsUnlockable;
    }

    #region CONSTRUCTOR METHODS

    public void InstantiateDotMesh(GameObject prefab)
    {
        MyMesh = Instantiate(prefab, _myTransform.parent.position, Quaternion.identity) as GameObject;
        MyMesh.name = "Dot";
        MyMesh.transform.localScale = ArtManager.Instance.OriginalScale;
        MyMesh.transform.parent = _myTransform;
        MyMesh.AddComponent<MeshNoise>();

        InstantiateHighlightMesh(HighlightPrefab);
        InstantiateLockedMesh(LockedPrefab);
        //InstantiateShadowMesh(ShadowPrefab);
        InstantiatePowerUpAnimPool(MyMesh);
        InstantiateLight();

        //StartCoroutine(Helper.PanTextureLinear(MyMesh, new Vector2(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f)), 0f));

        _myTransform.localScale = Vector3.zero;
        ShowSelf();
    }

    void InstantiateLight()
    {
        myLight = new GameObject("Light");
        myLight.AddComponent<Light>();
        myLight.light.intensity = 0f;
        myLight.transform.position = _myTransform.position + new Vector3(0f, 0f, 0.1f);
        myLight.transform.parent = _myTransform;
        myLight.light.range = _lightRange;
        HOTween.To(myLight.light, 0.4f, "intensity", _lightIntensity);
    }

    public void InstantiateShadowMesh(GameObject prefab)
    {
        myShadow = Instantiate(prefab, _myTransform.parent.position, Quaternion.identity) as GameObject;
        myShadow.name = "Shadow";
        myShadow.transform.localScale *= 0.5f;
        myShadow.transform.localScale *= ArtManager.Instance.screenRatio;
        myShadow.transform.parent = _myTransform;
    }

    public void InstantiateHighlightMesh(GameObject prefab)
    {
        // This is the prefab that activates when player taps on dot.
        Highlight = Instantiate(prefab, _myTransform.position, Quaternion.identity) as GameObject;
        Highlight.name = "Highlight";
        Highlight.transform.localScale *= ArtManager.Instance.screenRatio;
        Highlight.transform.parent = _myTransform;
        Highlight.SetActive(false);
    }

    public void InstantiateLockedMesh(GameObject prefab)
    {
        Locked = Instantiate(prefab, _myTransform.position, Quaternion.identity) as GameObject;
        Locked.name = "Locked";
        _lockedTransform = Locked.transform;
        _lockedTransform.localScale = Vector3.zero;
        _lockedTransform.parent = _myTransform;
        Locked.SetActive(false);
    }

    void InstantiatePowerUpAnimPool(GameObject prefab)
    {
        for (int i = 0; i < powerUpAnimPool.Length; i++)
        {
            powerUpAnimPool[i] = Instantiate(prefab, _myTransform.position + new Vector3(0f, 0f, -2f), Quaternion.identity) as GameObject;
            powerUpAnimPool[i].name = "power up obj" + i.ToString();
            powerUpAnimPool[i].renderer.material.color = CurrentColor;
            powerUpAnimPool[i].transform.localScale *= 0.5f;
            powerUpAnimPool[i].transform.parent = _myTransform;
            powerUpAnimPool[i].SetActive(false);
        }
    }

    #endregion

    public void AssignNewRandomColor(List<Color32> colorList, GameObject targetObject)
    {
        // Random index for material and color lists.
        int i = (int)UnityEngine.Random.Range(0, colorList.Count);
        //// Assign new material.
        //if (ArtManager.Instance.Patterns[i] != null)
        //{
        //    targetObject.renderer.material = ArtManager.Instance.Patterns[i];
        //}
        // Assign new color.
        targetObject.renderer.material.color = colorList[i];
        CurrentColor = colorList[i];

        myLight.light.color = CurrentColor;
        MyMesh.renderer.material.SetColor("_RimColor", CurrentColor);
    }

    #region ANIMATION METHODS

    public void AnimateBirth()
    {
        pulseSequence.Pause();
        AssignNewRandomColor(ArtManager.Instance.ColorList, MyMesh);
        Highlight.renderer.material.color = MyMesh.renderer.material.color;
        ReturnShapeToNormalInstantly();
        HOTween.To(_myTransform, 0.2f, "localScale", ArtManager.Instance.OriginalScale);
        IsLockable = true;
    }

    public void AnimateDeath()
    {
        ReturnShapeToNormalInstantly();
        pulseSequence.Pause();
        DeactivateHighlight();
        HOTween.To(_myTransform, 0.1f, ArtManager.Instance.deathParms);
    }

    public void ActivateHighlight()
    {
        BrightenLight(2f);
    }

    public void DeactivateHighlight()
    {
        RestoreLight();
    }

    public void WrongSphere()
    {
        myLight.light.color = WrongColor;
    }

    public void SquareSphere()
    {
        myLight.light.color = ArtManager.Instance.ChainColor;
        BrightenLight(2f);
    }

    public void LockSelf()
    {
        IsLockable = false;
        pulseSequence.Pause();
        Locked.SetActive(true);
        var nObj = new NoiseObject();
        nObj.obj = MyMesh;
        nObj.scale = 0.05f;
        nObj.speed = 1f;
        nObj.duration = 0f;
        DimLight();
        MyMesh.GetComponent<MeshNoise>().Morph();
    }

    public void UnlockSelf()
    {
        IsLockable = true;
        Locked.SetActive(false);
        StartCoroutine("ReturnShapeToNormal");
        RestoreLight();
    }

    public void HighlightLock()
    {
        myLight.light.color = ArtManager.Instance.ChainColor;
        BrightenLight(1f);
    }

    public void ResetLock()
    {
        myLight.light.color = CurrentColor;
        DimLight();
    }

    public void HideSelf()
    {
        pulseSequence.Pause();
        DeactivateHighlight();
        HOTween.To(_myTransform, 0.2f, ArtManager.Instance.hideParms);
    }

    public void ShowSelf()
    {
        AssignNewRandomColor(ArtManager.Instance.ColorList, MyMesh);
        HOTween.To(_myTransform, 0.2f, ArtManager.Instance.showParms);
        RestoreLight();
    }

    void RestoreLight()
    {
        myLight.SetActive(true);
        myLight.light.color = CurrentColor;
        HOTween.To(myLight.light, 0.4f, "intensity", _lightIntensity);
        HOTween.To(myLight.light, 0.4f, "range", _lightRange);
    }

    void BrightenLight(float multiplier)
    {
        HOTween.To(myLight.light, 0.4f, "intensity", _lightIntensity * multiplier);
        HOTween.To(myLight.light, 0.4f, "range", _lightRange * multiplier);
    }

    void DimLight()
    {
        HOTween.To(myLight.light, 0.4f, "intensity", 0f);
    }

    // Method to animate the death of the chained dot and send a copy to the corresponding power up color.
    // This helps inform the player that the dots she chained are added to power up score.
    public IEnumerator AnimateToPowerUp()
    {
        if (MyMesh.renderer.material.color == ArtManager.Instance.GreyDotColor)
        {
            AnimateDeath();
            yield break;
        }

        // Find position of power up button of the same color as chain to know where to send dot.
        Vector3 powerUpButtonPos = Vector3.zero;
        int colorIndex = ArtManager.Instance.OriginalColors.IndexOf(MyMesh.renderer.material.color);
        if (colorIndex >= 0)
            powerUpButtonPos = PowerUpsManager.Instance.textList[colorIndex].transform.position;

        Vector3[] positions = new Vector3[5];
        int numAnims = 1;

        // If dot is white, we're gonna animate 5 dots going towards all power up colors.
        if (MyMesh.renderer.material.color == ArtManager.Instance.WhiteDotColor)
        {
            numAnims = 5;
            for (int j = 0; j < numAnims; j++)
            {
                // Make an array with the positions of all power up buttons.
                var targetPos = Vector3.zero;
                targetPos = PowerUpsManager.Instance.textList[j].transform.position;
                positions[j] = Camera.main.ViewportToWorldPoint(targetPos);
            }
        }
        else
            positions[0] = Camera.main.ViewportToWorldPoint(powerUpButtonPos);

        // Animate power up circles.
        for (int i = 0; i < numAnims; i++)
        {
            // Activate a circle from the pool. This guy will animate towards the power up button.
            powerUpAnimPool[i].SetActive(true);
            Destroy(powerUpAnimPool[i], 0.5f);

            HOTween.To(powerUpAnimPool[i].transform, 0.5f,
                new TweenParms().Prop("position", positions[i] + new Vector3(0f, 0f, 5f)).Ease(EaseType.EaseInOutExpo));
            HOTween.To(powerUpAnimPool[i].renderer.material, 0.4f,
                new TweenParms().Prop("color", powerUpAnimPool[i].renderer.material.color - new Color(0f, 0f, 0f, 1f)).Ease(EaseType.EaseInExpo));

            yield return null;
        }

        // Replenish pool.
        for (int i = 0; i < numAnims; i++)
        {
            GameObject temp = Instantiate(powerUpAnimPool[i], _myTransform.position, Quaternion.identity) as GameObject;
            powerUpAnimPool[i] = temp;
            powerUpAnimPool[i].SetActive(false);
        }

        // Hide current circle.
        AnimateDeath();
    }

    public void Pulsate()
    {
        pulseSequence.Restart();
    }

    public void StopPulse()
    {
        pulseSequence.Rewind();
        pulseSequence.Pause();
    }

    #endregion

    #region MESH MORPH METHODS

    IEnumerator RandomizeShape()
    {
        stopMorph = false;
        float duration = 0.4f;
        RandomizeMesh2D(_originalMesh, _originalVerts, UnityEngine.Random.Range(0.01f, 0.02f), duration);
        yield return new WaitForSeconds(duration);
        StartCoroutine("RandomizeShapeOnInterval");
    }

    IEnumerator RandomizeShapeOnInterval()
    {
        stopMorph = false;
        var frequency = UnityEngine.Random.Range(2f, 5f);
        while (!stopMorph)
        {
            RandomizeMesh2D(_originalMesh, _originalVerts, UnityEngine.Random.Range(0.01f, 0.04f), UnityEngine.Random.Range(frequency, frequency + 0.5f));
            yield return new WaitForSeconds(frequency);
        }
    }

    public IEnumerator ReturnShapeToNormal()
    {
        StopAnimation();

        //StopCoroutine("RandomizeShape");
        AnimateVerts(MyMesh.GetComponent<MeshFilter>().mesh, _originalVerts, 0.3f);
        yield return new WaitForSeconds(0.3f);
    }

    void ReturnShapeToNormalInstantly()
    {
        StopAnimation();

        //StopCoroutine("RandomizeShape");
        MyMesh.GetComponent<MeshFilter>().mesh.vertices = _originalVerts;
        MyMesh.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }

    IEnumerator ShapeSquirm()
    {
        stopMorph = false;
        var frequency = UnityEngine.Random.Range(0.01f, 0.05f);
        while (!stopMorph)
        {
            RandomizeMesh2D(_originalMesh, _originalVerts, UnityEngine.Random.Range(0.01f, 0.05f), frequency);
            yield return new WaitForSeconds(frequency);
        }
    }

    // Struct to hold all animation parameters so StartCourtine can be called with a string.
    public struct AnimationObject
    {
        public Mesh mesh;
        public Vector3[] targetVerts;
        public float duration;
    }

    public struct NoiseObject
    {
        public GameObject obj;
        public float scale;
        public float speed;
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
        // Initialize TweenVars.
        TweenVar[] xVars = new TweenVar[ao.mesh.vertexCount];
        TweenVar[] yVars = new TweenVar[ao.mesh.vertexCount];
        for (int i = 0; i < ao.mesh.vertexCount; i++)
        {
            xVars[i] = new TweenVar(ao.mesh.vertices[i].x, ao.targetVerts[i].x, ao.duration, EaseType.EaseOutExpo);
            yVars[i] = new TweenVar(ao.mesh.vertices[i].y, ao.targetVerts[i].y, ao.duration, EaseType.EaseOutExpo);
        }
        // Teporary vertex array.
        Vector3[] tempVerts = new Vector3[ao.mesh.vertices.Length];
        // Log start time so the tween vars can be updated correctly.
        var startTime = Time.time;
        while (Time.time - startTime < ao.duration)
        {
            // Updates all vertices for this frame tween values.
            for (int i = 0; i < ao.mesh.vertexCount; i++)
            {
                tempVerts[i] = new Vector3(
                    xVars[i].Update(Time.time - startTime),
                    yVars[i].Update(Time.time - startTime),
                    ao.mesh.vertices[i].z);
            }
            ao.mesh.vertices = tempVerts;
            ao.mesh.RecalculateBounds();
            yield return null;
        }
    }

    IEnumerator AnimateVertex(AnimationObject ao, int i)
    {
        TweenVar xVar = new TweenVar(ao.mesh.vertices[i].x, ao.targetVerts[i].x, ao.duration, EaseType.EaseInOutBack);
        TweenVar yVar = new TweenVar(ao.mesh.vertices[i].y, ao.targetVerts[i].y, ao.duration, EaseType.EaseInOutBack);
        var startTime = Time.time;
        Vector3[] tempVerts = ao.mesh.vertices;
        while (Time.time - startTime < ao.duration)
        {
            tempVerts[i] = new Vector3(
                xVar.Update(Time.time - startTime),
                yVar.Update(Time.time - startTime),
                ao.mesh.vertices[i].z);
            ao.mesh.vertices[i] = tempVerts[i];
            ao.mesh.RecalculateBounds();
            yield return null;
        }
    }

    public void RandomizeMesh2D(Mesh m, Vector3[] verts, float maxDist, float duration)
    {
        if (!stopMorph)
            AnimateVerts(m, RandomizeVerts2D(verts, maxDist), duration);
    }

    public void StopAnimation()
    {
        //stopMorph = true;
        //StopCoroutine("CoAnimateVerts");
        //StopCoroutine("GenerateNoise");
    }

    #endregion
}