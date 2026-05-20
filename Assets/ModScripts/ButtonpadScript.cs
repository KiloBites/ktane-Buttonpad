using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class ButtonpadScript : MonoBehaviour 
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMColorblindMode ColorblindMode;

	public KMSelectable[] Buttons;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	private ButtonInfo[] assignedButtons;

	private bool correctButtonPressed;

	void Awake()
    {

		moduleId = moduleIdCounter++;

    }

	void ButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);

		if (moduleSolved)
			return;

		var ix = (ButtonPosition)Array.IndexOf(Buttons, button);

		correctButtonPressed = ButtonpadTools.DetermineCorrectButton(Bomb, assignedButtons).Contains(ix);
	}

	void ButtonRelease(KMSelectable button)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);

		if (moduleSolved)
			return;

		if (correctButtonPressed) // This will eventually change to determine if the button's position is correct and if the bomb's timer has the digital root of the sum.
		{
			Log($"[Buttonpad #{moduleId}] {(ButtonPosition)Array.IndexOf(Buttons, button)} has been released at {Bomb.GetFormattedTime()}, and the timer contains the digital root of the sum. Solved!");
			moduleSolved = true;
			Module.HandlePass();
		}
		else
		{
			Log($"[Buttonpad # {moduleId}] {(ButtonPosition)Array.IndexOf(Buttons, button)} has been released at {Bomb.GetFormattedTime()}, but the timer doesn't contain the digital root of the sum. Strike!");
			Module.HandleStrike();
		}
	}

	
	void Start()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}