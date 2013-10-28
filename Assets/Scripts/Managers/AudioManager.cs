using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
	public AudioClip Pop;
	public AudioClip C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B, C2, C2Sharp, D2, D2Sharp, E2, F2, F2Sharp, G2, G2Sharp, A2, A2Sharp, B2;
	public AudioClip CLong, DLong, ELong, FLong, GLong, ALong, BLong, C2Long;
	private List<AudioClip> CScale = new List<AudioClip>();
	private List<AudioClip> DScale = new List<AudioClip>();
	private List<AudioClip> EScale = new List<AudioClip>();
	private List<AudioClip> FScale = new List<AudioClip>();
	private List<AudioClip> GScale = new List<AudioClip>();
	private List<AudioClip> BmScale = new List<AudioClip>();
	private List<AudioClip> LongScale = new List<AudioClip>();
	private List<List<AudioClip>> Scales = new List<List<AudioClip>>();

	// SINGLETON
	public static AudioManager Instance {
		get {
			if (instance == null)
				instance = new AudioManager();
			return instance;
		}
	}

	private static AudioManager instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		gameObject.AddComponent("AudioSource");

		CScale.Add(C);
		CScale.Add(D);
		CScale.Add(E);
		CScale.Add(F);
		CScale.Add(G);
		CScale.Add(A);
		CScale.Add(B);
		CScale.Add(C2);

		DScale.Add(D);
		DScale.Add(E);
		DScale.Add(FSharp);
		DScale.Add(G);
		DScale.Add(A);
		DScale.Add(B);
		DScale.Add(C2Sharp);
		DScale.Add(D2);

		EScale.Add(E);
		EScale.Add(FSharp);
		EScale.Add(GSharp);
		EScale.Add(A);
		EScale.Add(B);
		EScale.Add(C2Sharp);
		EScale.Add(D2Sharp);
		EScale.Add(E2);

		FScale.Add(F);
		FScale.Add(G);
		FScale.Add(A);
		FScale.Add(ASharp);
		FScale.Add(C2);
		FScale.Add(D2);
		FScale.Add(E2);
		FScale.Add(F2);

		GScale.Add(G);
		GScale.Add(A);
		GScale.Add(B);
		GScale.Add(C2);
		GScale.Add(D2);
		GScale.Add(E2);
		GScale.Add(F2Sharp);
		GScale.Add(G2);

		BmScale.Add(B);
		BmScale.Add(C2Sharp);
		BmScale.Add(D2);
		BmScale.Add(E2);
		BmScale.Add(F2Sharp);
		BmScale.Add(G2);
		BmScale.Add(A2);
		BmScale.Add(B2);

		LongScale.Add(CLong);
		LongScale.Add(DLong);
		LongScale.Add(ELong);
		LongScale.Add(FLong);
		LongScale.Add(GLong);
		LongScale.Add(ALong);
		LongScale.Add(BLong);
		LongScale.Add(C2Long);

		Scales.Add(CScale);
		Scales.Add(DScale);
		Scales.Add(EScale);
		Scales.Add(FScale);
		Scales.Add(GScale);
		Scales.Add(BmScale);
		Scales.Add(LongScale);
	}
	
	// TODO Long notes for all scales.

	public void PlayDotSound(int count, Color chainColor, bool isCorrectChain)
	{
		if (AudioListener.volume == 0)
			return;

		audio.pitch = isCorrectChain ? 1f : 0.83f;

		int colorIndex = ArtManager.Instance.OriginalColors.IndexOf(chainColor);
		// If the color is not in OriginalColors, then it's grey or white.
		// If the chain isn't white, use Bm scale. Else, use long C scale.
		if (colorIndex < 0)
		{
			if (chainColor != ArtManager.Instance.WhiteDotColor)
				colorIndex = 5;
			else
				colorIndex = 6;
		}
		var myScale = Scales[colorIndex];
		// Ascending notes.
		if (count <= 7 || (count > 14 && count < 22) || count >= 36)
		{
			int i = (count + 6) % 7;
			audio.PlayOneShot(Pop, 0.3f);
			audio.PlayOneShot(myScale[i], 0.2f);
		}
		// Descending notes.
		else if ((count > 7 && count <= 14) || (count >= 22 && count < 35))
		{
			int i = (count + 6) % 7;
			audio.PlayOneShot(Pop, 0.3f);
			audio.PlayOneShot(myScale[7 - i], 0.2f);
		}
	}

	public void PlayExplosionSound(Color c)
	{
		if (AudioListener.volume == 0)
			return;

		audio.pitch = 1f;
		int i = ArtManager.Instance.OriginalColors.IndexOf(c);
		audio.PlayOneShot(LongScale[i]);
	}

	public void PlayLoopSound(Color c)
	{
		if (AudioListener.volume == 0)
			return;

		audio.pitch = 1f;
		var numArpeggioNotes = 3;
		int firstNoteIndex = ArtManager.Instance.OriginalColors.IndexOf(c);
		// If i = -1 then the Loop is grey or white.
		// Grey is B, White is C.
		if (firstNoteIndex < 0)
		{
			if (c != ArtManager.Instance.WhiteDotColor)
				firstNoteIndex = 6;
			else
			{
				firstNoteIndex = 0;
				numArpeggioNotes = 5;
			}
		}
		StartCoroutine(Arpeggio(firstNoteIndex, numArpeggioNotes));
	}

	IEnumerator Arpeggio(int firstNote, int numNotes)
	{
		while (numNotes > 0)
		{
			audio.PlayOneShot(LongScale[firstNote]);
			--numNotes;
			audio.pitch += 0.333333f;
			yield return new WaitForSeconds(0.1f);
		}
	}
}
