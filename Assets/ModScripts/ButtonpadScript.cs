using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

	private bool _inSubmission;
	private bool _cbActive;

	private ButtonpadGenerator _generator;
	private ButtonInfo[] _buttonSet;
	private LEDInfo[] _ledSet;
	
	private readonly Coroutine[] _buttonAnims = new Coroutine[4];
	private Coroutine _holding, _startingHold;
	
	private readonly List<ButtonInfo> _submittedButtons = new List<ButtonInfo>();
	private ButtonInfo? _lastButtonLit;
	private readonly bool[] _isBeingHeld = new bool[4];
	

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
		var ix = (ButtonPosition)Array.IndexOf(Buttons, button);

		if (!_submittedButtons.Contains(_buttonSet[(int)ix]))
		{
			button.AddInteractionPunch(0.4f);
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
		}

		if (_buttonAnims[(int)ix] != null && !_inSubmission)
		{
			StopCoroutine(_buttonAnims[(int)ix]);
			_buttonAnims[(int)ix] = null;
		}
		
		if (_moduleSolved || _submittedButtons.Contains(_buttonSet[(int)ix]))
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
		
		Log($"[Buttonpad #{_moduleId}] {pos} has been held and {_ledSet[(int)pos]}");
		Log($"[Buttonpad #{_moduleId}] Release the button when the timer contains a {ButtonpadGenerator.CalculateDigitalRoot(_ledSet[(int)pos])}");
		_isBeingHeld[(int)pos] = true;

		LEDMeshes[(int)_ledSet[(int)pos].Position].material = LEDMats[1];

		CBLEDTexts[(int)_ledSet[(int)pos].Position].text = _cbActive && _ledSet[(int)pos].LEDColor != LEDColor.White ? _ledSet[(int)pos].LEDColor.ToString() : string.Empty;
		CBLEDTexts[(int)_ledSet[(int)pos].Position].color = _ledSet[(int)pos].Button.ButtonColor == ButtonColor.Yellow ? Color.black : Color.white;

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

			if (!_inSubmission)
				_inSubmission = true;
		}

		if (_buttonAnims[(int)ix] != null && !_inSubmission)
		{
			StopCoroutine(_buttonAnims[(int)ix]);
			_buttonAnims[(int)ix] = null;
		}

		if (_moduleSolved || _submittedButtons.Contains(_buttonSet[(int)ix]))
			return;

		var timer = Bomb.GetFormattedTime().Where(char.IsDigit).Select(x => x - '0').ToArray();
		var areWrong = new List<string>();

		if (!_inSubmission && (_lastButtonLit == null ? _generator.GetFirstButtonToHold().Contains(_buttonSet[(int)ix]) : _buttonSet[(int)ix] == _lastButtonLit) && timer.Contains(ButtonpadGenerator.CalculateDigitalRoot(_ledSet[(int)ix])))
		{
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
			
			StopCoroutine(_holding);
			_holding = null;

			_isBeingHeld[(int)ix] = false;
			
			_buttonAnims[(int)ix] = StartCoroutine(ButtonAnimation(ix, false));

			LEDMeshes[(int)_ledSet[(int)ix].Position].material = LEDMats[0];
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
				
				Log($"[Buttonpad #{_moduleId}] All four buttons have been pressed correctly. Solved!");
				
				_moduleSolved = true;
				Module.HandlePass();

			}
			else
			{
				var pressedButtons = _buttonSet.IndicesOf(_submittedButtons.Contains).ToArray();

				foreach (var index in pressedButtons)
				{
					Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Buttons[index].transform);
					_buttonAnims[index] = StartCoroutine(ButtonAnimation((ButtonPosition)index, false));
					LEDMeshes[index].material = LEDMats[0];
				}
				
				if (!_generator.CheckButtons(_submittedButtons, _submittedButtons.Count))
					areWrong.Add($"expected {_generator.GetExpectedPosition(_submittedButtons.Count - 1)}");
				
				if (!timer.Contains(_generator.GetDigitForSubmission(_buttonSet[(int)ix])))
					areWrong.Add($"the timer doesn't contain a {_generator.GetDigitForSubmission(_buttonSet[(int)ix])} after releasing");
				
				Log($"[Buttonpad #{_moduleId}] {ix} is pressed, but {areWrong.Join(", and ")}. Strike!");
				
				_submittedButtons.Clear();
				_inSubmission = false;
				_lastButtonLit = null;
				Module.HandleStrike();
			}
		}
		else
		{
			StopCoroutine(_holding);
			_holding = null;
			_isBeingHeld[(int)ix] = false;
			
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
			_buttonAnims[(int)ix] = StartCoroutine(ButtonAnimation(ix, false));
			
			LEDMeshes[(int)_ledSet[(int)ix].Position].material = LEDMats[0];
			CBLEDTexts[(int)_ledSet[(int)ix].Position].text = string.Empty;
			
			if (_lastButtonLit == null && !_generator.GetFirstButtonToHold().Contains(_buttonSet[(int)ix]))
				areWrong.Add($"expected to hold either {_generator.GetFirstButtonToHold().Select((x, i) => i == _generator.GetFirstButtonToHold().Count() - 1 ? $"or {_generator.GetExpectedPositionFromButton(x)}" : _generator.GetExpectedPositionFromButton(x).ToString()).Join(", ")}");
			else if (_lastButtonLit != null && _buttonSet[(int)ix] != _lastButtonLit)
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
		
		Log($"[Buttonpad #{_moduleId}] The buttons in reading order: {_buttonSet.Select(x => $"[{x}]").Join(", ")}");
		
		Log($"[Buttonpad #{_moduleId}] {_generator}");
		
		Log($"[Buttonpad #{_moduleId}] {_generator.GetColoredSymbols()}");
		
		var correctPositions = Enumerable.Range(0, 4).Select(_generator.GetExpectedPosition).ToArray();
		var buttonDigits = correctPositions.Select(x => _generator.GetDigitForSubmission(_buttonSet[(int)x])).ToArray();
		
		Log($"[Buttonpad #{_moduleId}] The correct buttons and times are: {Enumerable.Range(0, 4).Select(x => $"[{correctPositions[x]} when the timer contains a {buttonDigits[x]}]").Join(", ")}");
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

	private void ToggleColorblind()
	{
		_cbActive ^= true;

		for (int i = 0; i < 4; i++)
		{
			CBButtonTexts[i].text = _cbActive && _buttonSet[i].ButtonColor != ButtonColor.White ? _buttonSet[i].ButtonColor.ToString() : string.Empty;
			CBButtonTexts[i].color = _buttonSet[i].ButtonColor == ButtonColor.Yellow ? Color.black : Color.white;
		}

		if (!_isBeingHeld.Any(x => x))
			return;
		
		var heldButtonIndex = _isBeingHeld.IndexOf(x => x);
			
		CBLEDTexts[(int)_ledSet[heldButtonIndex].Position].text = _cbActive && _ledSet[heldButtonIndex].LEDColor != LEDColor.White ? _ledSet[heldButtonIndex].LEDColor.ToString() : string.Empty;
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} hold TL/TR/BL/BR [holds the button at that position] || release # [releases the button held when the timer contains that digit] || tap TL/TR/BL/BR # [presses the button when the timer contains that digit] || cb [toggles colorblind]";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
		var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		
		var buttonPositions = ((ButtonPosition[])Enum.GetValues(typeof(ButtonPosition))).Select(x => x.ToString()).ToArray();

		switch (split[0])
		{
			case "HOLD":
				if (_inSubmission)
				{
					yield return "sendtochaterror The module is in submission mode!";
					yield break;
				}

				if (split.Length == 1)
				{
					yield return "sendtochaterror Please specify what button to hold!";
					yield break;
				}
				
				if (split.Length > 2)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}
				
				if (_isBeingHeld.Any(x => x))
				{
					yield return $"sendtochaterror You cannot hold any other buttons since {(ButtonPosition)Enumerable.Range(0, 4).First(x => _isBeingHeld[x])} is held!";
					yield break;
				}
				
				if (!buttonPositions.Contains(split[1]))
				{
					yield return $"sendtochaterror {split[1]} is not a valid button position!";
					yield break;
				}

				yield return null;
				
				Buttons[Array.IndexOf(buttonPositions, split[1])].OnInteract();
				
				yield return new WaitForSeconds(0.5f);

				yield break;
			case "RELEASE":
				if (_isBeingHeld.All(x => !x))
				{
					yield return "sendtochaterror There is no button to release!";
					yield break;
				}
				
				if (_inSubmission)
				{
					yield return "sendtochaterror The module is in submission mode!";
					yield break;
				}

				if (split.Length == 1)
				{
					yield return "sendtochaterror Please specify when to release the button held!";
					yield break;
				}

				if (split.Length > 2)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}


				int releaseDigit;

				if (split[1].Length > 1 || !int.TryParse(split[1], out releaseDigit))
				{
					yield return "sendtochaterror Either the digit is invalid or cannot be more than one digit!";
					yield break;
				}

				yield return null;
				
				while (!Bomb.GetFormattedTime().Where(char.IsDigit).Select(x => x - '0').Contains(releaseDigit))
					yield return "trycancel Button release command has been canceled!";

				Buttons[_isBeingHeld.IndexOf(x => x)].OnInteractEnded();
				yield return new WaitForSeconds(0.1f);
				
				yield break;
			case "TAP":
				if (_isBeingHeld.Any(x => x))
				{
					yield return "sendtochaterror You cannot tap a button while the current button is being held!";
					yield break;
				}
				
				switch (split.Length)
				{
					case 1:
						yield return "sendtochaterror Please specify what button to tap!";
						yield break;
					case 2:
						yield return "sendtochaterror Please specify when to tap the button!";
						yield break;
				}

				if (split.Length > 3)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				if (!buttonPositions.Contains(split[1]))
				{
					yield return $"sendtochaterror {split[1]} is not a valid button position!";
					yield break;
				}

				int tapDigit;

				if (split[2].Length > 1 || !int.TryParse(split[2], out tapDigit))
				{
					yield return "sendtochaterror Either the digit is invalid or cannot be more than one digit!";
					yield break;
				}
				
				
				var index = Array.IndexOf(buttonPositions, split[1]);

				if (_submittedButtons.Contains(_buttonSet[index]))
				{
					yield return "sendtochaterror The button is already submitted!";
					yield break;
				}
				
				yield return null;

				
				while (!Bomb.GetFormattedTime().Where(char.IsDigit).Select(x => x - '0').Contains(tapDigit))
					yield return "trycancel Button tap command has been canceled!";

				Buttons[index].OnInteract();
				yield return new WaitForSeconds(0.1f);
				Buttons[index].OnInteractEnded();
				yield return new WaitForSeconds(0.1f);
				
				yield break;
			case "CB":
				if (split.Length > 1)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				yield return null;
				ToggleColorblind();
				yield return new WaitForSeconds(0.1f);
				yield break;
			default:
				yield return "sendtochaterror Invalid command!";
				yield break;
		}
    }

	private IEnumerator TwitchHandleForcedSolve()
    {
	    if (_isBeingHeld.Any(x => x))
	    {
		    var index = _isBeingHeld.IndexOf(x => x);

		    while (!Bomb.GetFormattedTime().Where(char.IsDigit).Select(x => x - '0').Contains(ButtonpadGenerator.CalculateDigitalRoot(_ledSet[index])))
			    yield return true;

		    Buttons[index].OnInteractEnded();
		    yield return new WaitForSeconds(0.1f);
	    }

	    var correctButtonPositions = Enumerable.Range(0, 4).Select(_generator.GetExpectedPosition).ToArray();
	    var correctDigits = correctButtonPositions.Select(x => _generator.GetDigitForSubmission(_buttonSet[(int)x])).ToArray();

	    for (int i = 0; i < 4; i++)
	    {
		    if (_submittedButtons.Contains(_buttonSet[(int)correctButtonPositions[i]]))
			    continue;

		    while (!Bomb.GetFormattedTime().Where(char.IsDigit).Select(x => x - '0').Contains(correctDigits[i]))
			    yield return true;

		    Buttons[(int)correctButtonPositions[i]].OnInteract();
		    yield return new WaitForSeconds(0.1f);
		    Buttons[(int)correctButtonPositions[i]].OnInteractEnded();
		    yield return new WaitForSeconds(0.1f);
	    }
    }


}