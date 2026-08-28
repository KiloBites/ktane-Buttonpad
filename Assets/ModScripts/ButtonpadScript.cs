using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class ButtonpadScript : MonoBehaviour 
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMColorblindMode Colorblind;

	public KMSelectable[] Buttons;

	public Material[] LEDMats;
	public MeshRenderer[] ButtonMeshes, LEDMeshes;
	public SpriteRenderer[] SymbolDisplays;
	public Sprite[] KeypadSprites;

	public TextMesh[] CBButtonTexts, CBLEDTexts;

	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved;

	private bool _inSubmission, _isHeld;
	private bool _cbActive;

	private ButtonpadGenerator _generator;
	private ButtonInfo[] _buttonSet;
	private LEDInfo[] _ledSet;
	
	private readonly Coroutine[] _buttonAnims = new Coroutine[4];
	private Coroutine _holding, _startingHold;
	
	private readonly List<ButtonInfo> _submittedButtons = new List<ButtonInfo>();
	private ButtonInfo? _lastButtonLit;
	

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

		var ix = (ButtonPosition)Array.IndexOf(Buttons, button);

		if (_buttonAnims[(int)ix] != null)
		{
			StopCoroutine(_buttonAnims[(int)ix]);
			_buttonAnims[(int)ix] = null;
		}
		
		if (_moduleSolved || (_inSubmission && _submittedButtons.Contains(_buttonSet[(int)ix])))
			return;
		
		_buttonAnims[(int)ix] = StartCoroutine(ButtonAnimation(ix, true));
		_holding = StartCoroutine(ToggleLED(ix));
	}

	private IEnumerator ButtonAnimation(ButtonPosition pos, bool isBeingPressed)
	{
		const float duration = 0.1f;
		var elapsed = 0f;

		var from = Buttons[(int)pos].transform.localPosition;
		var to = isBeingPressed ? -0.003f : 0f;

		while (elapsed < duration)
		{
			Buttons[(int)pos].transform.localPosition = new Vector3(from.x, Easing.InOutQuad(elapsed, from.y, to, duration), from.z);
			yield return null;
			elapsed += Time.deltaTime;
		}
		Buttons[(int)pos].transform.localPosition = new Vector3(from.x, to, from.z);
		
		_buttonAnims[(int)pos] = null;
	}

	private IEnumerator Hold()
	{
		const float duration = 0.5f;
		var elapsed = 0f;

		while (elapsed < duration)
		{
			yield return null;
			elapsed += Time.deltaTime;
		}

		_startingHold = null;
	}

	private IEnumerator ToggleLED(ButtonPosition pos)
	{
		_startingHold = StartCoroutine(Hold());

		yield return new WaitWhile(() => _startingHold != null);

		LEDMeshes[(int)_ledSet[(int)pos].Position].material = LEDMats[1];

		CBLEDTexts[(int)_ledSet[(int)pos].Position].text = _cbActive && _ledSet[(int)pos].LEDColor != LEDColor.White ? _ledSet[(int)pos].LEDColor.ToString() : string.Empty;
		CBLEDTexts[(int)_ledSet[(int)pos].Position].color = new[] { ButtonColor.Yellow, ButtonColor.White }.Contains(_buttonSet[(int)_ledSet[(int)pos].Position].ButtonColor) ? Color.black : Color.white;

		var color = _ledSet[(int)pos].GetLEDColor();
		var darkerColor = color.GetDarkerShade();

		var reverse = false;

		while (true)
		{
			const float duration = 0.5f;
			var elapsed = 0f;

			while (elapsed < duration)
			{
				LEDMeshes[(int)_ledSet[(int)pos].Position].material.color = Color.Lerp(reverse ? darkerColor : color, reverse ? color : darkerColor, elapsed);
				yield return null;
				elapsed += Time.deltaTime;
			}

			reverse ^= true;
		}
	}

	private void ButtonRelease(KMSelectable button)
	{
		var ix = (ButtonPosition)Array.IndexOf(Buttons, button);

		if (_startingHold != null)
		{
			StopCoroutine(_holding);
			_holding = null;
			StopCoroutine(_startingHold);
			_startingHold = null;
			_inSubmission = true;
		}

		if (_buttonAnims[(int)ix] != null && !_inSubmission)
		{
			StopCoroutine(_buttonAnims[(int)ix]);
			_buttonAnims[(int)ix] = null;
		}

		if (_moduleSolved || (_inSubmission && _submittedButtons.Contains(_buttonSet[(int)ix])))
			return;

		var timer = Bomb.GetFormattedTime().Where(x => x != ':' || x != '.').Select(x => x - '0').ToArray();
		var areWrong = new List<string>();

		if (!_inSubmission && (_lastButtonLit == null ? _generator.GetFirstButtonToHold().Contains(_buttonSet[(int)ix]) : _buttonSet[(int)ix] == _lastButtonLit) && timer.Contains(ButtonpadGenerator.CalculateDigitalRoot(_ledSet[(int)ix])))
		{
			StopCoroutine(_holding);
			_holding = null;

			LEDMeshes[(int)ix].material = LEDMats[0];
			CBLEDTexts[(int)_ledSet[(int)ix].Position].text = string.Empty;
			
			_lastButtonLit = _buttonSet[(int)_ledSet[(int)ix].Position];
		}
		else if (_inSubmission)
		{
			_submittedButtons.Add(_buttonSet[(int)ix]);

			if (_generator.CheckButtons(_submittedButtons, _submittedButtons.Count) && timer.Contains(_generator.GetDigitForSubmission(_buttonSet[(int)ix])))
			{
				LEDMeshes[(int)ix].material = LEDMats[1];
				LEDMeshes[(int)ix].material.color = Color.green;

				if (_submittedButtons.Count != 4)
					return;
				
				_moduleSolved = true;
				Module.HandlePass();

			}
			else
			{
				var pressedButtons = _buttonSet.IndicesOf(_submittedButtons.Contains).ToArray();

				foreach (var index in pressedButtons)
				{
					_buttonAnims[index] = StartCoroutine(ButtonAnimation((ButtonPosition)index, false));
					LEDMeshes[index].material = LEDMats[0];
				}
				
				if (!_generator.CheckButtons(_submittedButtons, _submittedButtons.Count))
					areWrong.Add($"expected {_generator.GetExpectedPosition(_submittedButtons.Count - 1)}");
				
				if (!timer.Contains(_generator.GetDigitForSubmission(_buttonSet[(int)ix])))
					areWrong.Add($"the timer doesn't contain a {_generator.GetDigitForSubmission(_buttonSet[(int)ix])} after releasing");
				
				Log($"[Buttonpad #{_moduleId}] {ix} is pressed, but {areWrong.Join(", and ")}. Strike!");
				
				_submittedButtons.Clear();
				
				Module.HandleStrike();
			}
		}
		else
		{
			StopCoroutine(_holding);
			_holding = null;
			
			LEDMeshes[(int)_ledSet[(int)ix].Position].material = LEDMats[0];
			CBLEDTexts[(int)_ledSet[(int)ix].Position].text = string.Empty;
			
			if (_lastButtonLit == null && !_generator.GetFirstButtonToHold().Contains(_buttonSet[(int)ix]))
				areWrong.Add($"expected to hold either {_generator.GetFirstButtonToHold().Select((x, i) => i == _generator.GetFirstButtonToHold().Count() - 1 ? $"or {_generator.GetExpectedPositionFromButton(x)}" : _generator.GetExpectedPositionFromButton(x).ToString()).Join(", ")}");
			else if (_buttonSet[(int)ix] != _lastButtonLit)
				areWrong.Add($"expected to hold {_generator.GetExpectedPositionFromButton(_lastButtonLit.Value)}");
			
			if (!timer.Contains(ButtonpadGenerator.CalculateDigitalRoot(_ledSet[(int)ix])))
				areWrong.Add($"the timer doesn't contain the digital root ({ButtonpadGenerator.CalculateDigitalRoot(_ledSet[(int)ix])}) after releasing");
			
			Log($"[Buttonpad #{_moduleId}] {ix} is pressed and released, but {areWrong.Join(", and ")}. Strike!");

			Module.HandleStrike();
		}

	}
	
	
	
	private void Start()
	{
		_generator = new ButtonpadGenerator(Bomb);
		_ledSet = _generator.GetLEDS();
		_buttonSet = _ledSet.Select(x => x.Button).ToArray();
		SetupButtons();
	}

	private void SetupButtons()
	{
		for (int i = 0; i < 4; i++)
		{
			SymbolDisplays[i].sprite = KeypadSprites[(int)_buttonSet[i].ButtonSymbol];
			ButtonMeshes[i].material.color = _buttonSet[i].GetButtonColor();
			CBButtonTexts[i].text = _cbActive && _buttonSet[i].ButtonColor != ButtonColor.White ? _buttonSet[i].ButtonColor.ToString() : string.Empty;
			CBButtonTexts[i].color = _buttonSet[i].ButtonColor == ButtonColor.Yellow ? Color.black : Color.white;
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