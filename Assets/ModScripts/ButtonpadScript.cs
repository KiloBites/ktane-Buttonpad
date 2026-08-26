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
	public KMColorblindMode Colorblind;

	public KMSelectable[] Buttons;

	public MeshRenderer[] ButtonMeshes, LEDMeshes;
	public SpriteRenderer[] SymbolDisplays;
	public Sprite[] KeypadSprites;

	public TextMesh[] CBButtonTexts, LEDButtonTexts;

	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved;

	private bool _correctButtonPressed;
	private bool _cbActive;

	private ButtonpadGenerator _generator;
	private ButtonInfo[] _buttonInfos;
	private LEDInfo[] _ledInfos;

	private void Awake()
	{
		_cbActive = Colorblind.ColorblindModeActive;

		_moduleId = _moduleIdCounter++;

		foreach (var button in Buttons)
		{
			button.OnInteract += () => { ButtonPress(button); return false; };
			button.OnInteractEnded += () => { ButtonRelease(button); };
		}

    }

	private void ButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);

		if (_moduleSolved)
			return;

		var ix = (ButtonPosition)Array.IndexOf(Buttons, button);
	}

	private void ButtonRelease(KMSelectable button)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);

		if (_moduleSolved)
			return;
		
		
	}
	
	private void Start()
	{
		_generator = new ButtonpadGenerator(Bomb);
		_ledInfos = _generator.GetLEDS();
		_buttonInfos = _ledInfos.Select(x => x.Button).ToArray();
		SetupButtons();
	}

	private void SetupButtons()
	{
		for (int i = 0; i < 4; i++)
		{
			SymbolDisplays[i].sprite = KeypadSprites[(int)_buttonInfos[i].ButtonSymbol];
			ButtonMeshes[i].material.color = _buttonInfos[i].GetButtonColor();
			CBButtonTexts[i].text = _cbActive && _buttonInfos[i].ButtonColor != ButtonColor.White ? _buttonInfos[i].ButtonColor.ToString() : string.Empty;
			CBButtonTexts[i].color = _buttonInfos[i].ButtonColor == ButtonColor.Yellow ? Color.black : Color.white;
		}
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
		var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	private IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}