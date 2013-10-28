using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class Messages : MonoBehaviour
{
	[HideInInspector]
	public static Messages Instance;
	private Transform _myTransform;
	private Vector3 _originalPosition;
	private Color _originalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
	private Color _warningColor = Color.red;
	private List<string> GreetingMessageList = new List<string>();
	private List<string> RegularMessageList = new List<string>();
	private List<string> BreatherMessageList = new List<string>();
	private List<string> WarningMessageList = new List<string>();
	private List<string> CongratulationsMessageList = new List<string>();
	private List<string> GameOverImminentList = new List<string>();

	void Awake()
	{
		Instance = this;
		_myTransform = transform;
		_originalPosition = _myTransform.position;

		GreetingMessageList.Add("Hey.");
		GreetingMessageList.Add("Hello.");
		GreetingMessageList.Add("Hi.");

		RegularMessageList.Add("Every power up is important.");
		RegularMessageList.Add("Loops are good.");
		RegularMessageList.Add("It's all about give and take.");
		RegularMessageList.Add("Don't let your guard down.");
		RegularMessageList.Add("Keep it up.");
		RegularMessageList.Add("Remember to use your power ups.");
		RegularMessageList.Add("Each color is a different musical scale.");
		RegularMessageList.Add("My codename is Otom.");
		RegularMessageList.Add("I'm in beta testing.");
		RegularMessageList.Add("Every color is important.");
		RegularMessageList.Add("It's OK if I keep talking, right?");
		RegularMessageList.Add("Balance is essential.");
		RegularMessageList.Add("Power ups will keep you alive.");
		RegularMessageList.Add("If you're greedy you won't last long.");
		RegularMessageList.Add("I hear level 50 is amazing.");
		RegularMessageList.Add("Long chains build Power Ups.");
		RegularMessageList.Add("Power Ups help you make Loops.");
		RegularMessageList.Add("Loops help you make long chains.");
		RegularMessageList.Add("Remember your power ups.");

		BreatherMessageList.Add("Ah... Finally a break.");
		BreatherMessageList.Add("Easier level, but play smart.");
		BreatherMessageList.Add("Enjoy! (For now...)");
		BreatherMessageList.Add("White dots never get locked.");
		BreatherMessageList.Add("White dots chain with any color.");

		WarningMessageList.Add("Careful with the grey dots.");
		WarningMessageList.Add("Be careful, grey dots are pesky.");
		WarningMessageList.Add("A bit harder now.");
		WarningMessageList.Add("Grey dots incoming!");
		WarningMessageList.Add("Loops don't unlock grey dots.");
		WarningMessageList.Add("Here come the grey dots.");

		CongratulationsMessageList.Add("You're good at this.");
		CongratulationsMessageList.Add("Thank you.");
		CongratulationsMessageList.Add("Very nice.");
		CongratulationsMessageList.Add("Impressive.");
		CongratulationsMessageList.Add("That's a lot of levels.");
		
		GameOverImminentList.Add("Careful.");
		GameOverImminentList.Add("Uh oh.");
		GameOverImminentList.Add("Watch out.");

		gameObject.guiText.text = "";
	}

	string SelectNewMessage(List<string> list)
	{
		var shuffledList = list;
		shuffledList.Shuffle();
		return(shuffledList[0]);
	}

	public void ShowNewMessage(string listName)
	{
		string s;
		switch (listName)
		{
		case "Greeting":
			s = SelectNewMessage(GreetingMessageList);
			break;
		case "Regular":
			s = SelectNewMessage(RegularMessageList);
			break;
		case "Breather":
			s = SelectNewMessage(BreatherMessageList);
			break;
		case "Warning":
			s = SelectNewMessage(WarningMessageList);
			break;
		case "Congratulations":
			s = SelectNewMessage(CongratulationsMessageList);
			break;
		case "Game Over Imminent":
			s = SelectNewMessage(GameOverImminentList);
			StartCoroutine(CoShowNewMessage(s, true));
			return;
		default:
			s = "";
			break;
		}
		StartCoroutine(CoShowNewMessage(s, false));
	}

	IEnumerator CoShowNewMessage(string s, bool isWarning)
	{
		if (s == null) s = "";
		HOTween.To(guiText, 0.3f, "color", Camera.main.backgroundColor);
		var parms = new TweenParms().Prop("position", _originalPosition + new Vector3(0f, 0.1f, 0f)).Ease(EaseType.EaseInExpo);
		HOTween.To(transform, 0.2f, parms);
		yield return new WaitForSeconds(0.3f);
		gameObject.guiText.text = s;
		if (isWarning)
			HOTween.To(guiText, 0.2f, "color", _warningColor);
		else
			HOTween.To(guiText, 0.2f, "color", _originalColor);
		parms.NewProp("position", _originalPosition).Ease(EaseType.EaseOutExpo);
		HOTween.To(transform, 0.3f, parms);
	}

	public void HideSelf()
	{
		HOTween.To(guiText, 0.3f, "color", Camera.main.backgroundColor);
		var parms = new TweenParms().Prop("position", _originalPosition + new Vector3(0f, 0.1f, 0f)).Ease(EaseType.EaseInExpo);
		HOTween.To(transform, 0.2f, parms);
	}

	public void ShowSelf()
	{
		HOTween.To(guiText, 0.2f, "color", _originalColor);
		var parms = new TweenParms().Prop("position", _originalPosition).Ease(EaseType.EaseOutExpo);
		HOTween.To(transform, 0.3f, parms);
	}
}
