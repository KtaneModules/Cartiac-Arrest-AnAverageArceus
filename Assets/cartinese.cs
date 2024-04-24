using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class cartinese : MonoBehaviour
{
    public new KMAudio audio; //I sincerely apologize for most of the formatting on the text that wasn't already here, new editor means new formatting that I don't like
    private KMAudio.KMAudioRef audioRef;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] buttons;
    public KMSelectable screen;
    public Renderer[] buttonRenders;
    public TextMesh screenText;
    public Font solveFont;
    public Material solveMat;
	public GameObject everything;
	public Animator rotatoration;
	public GameObject thescreen;
	public Animator wackysolveanim;

    private int[] buttonColors = new int[4];
	private static string[] colorNames = {"magenta", "yellow", "cyan", "blue"};
	private static string[] abbreviations = {"M", "Y", "C", "B" };

	float timer = 0f;
	bool isplaying;
	bool solving;
	bool colored;
	bool ambulanced;

	bool rightear = false;

	int[,] environment = new int[16, 16]; //technically this isn't needed, but if I want to add more to this in the future, compatibility is key
	int[] forlogging = new int[6];
	int[] colorValues = new int[4];
	string[] textscroll = { "", "", "", "", " ", "", "", " ", "", "", " "};
	int textpart = 0;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
		rotatoration = everything.GetComponent<Animator> ();
		wackysolveanim = thescreen.GetComponent<Animator> ();
		screen.OnInteract += delegate () {
			Screened ();
			return false;
		};
		for (int i = 0; i < 6; i++)
			forlogging [i] = rnd.Range (0, 16);
		environment [forlogging[4], forlogging[5]] = 4; //ambulance, this being checked first means that it can be overwritten
		Debug.LogFormat("[Cartiac Arrest #{0}] The ambulance has been placed at ({1}, {2}).", moduleId, forlogging[4], forlogging[5]);
		environment [forlogging[0], forlogging[1]] = 1; //start pos
		environment [forlogging[2], forlogging[3]] = 2; //goal pos
		Debug.LogFormat("[Cartiac Arrest #{0}] Starting at ({1}, {2}) and ending at ({3}, {4}).", moduleId, forlogging[0], forlogging[1], forlogging[2], forlogging[3]);
		List<int> uniqueness = new List<int>();
		for (int i = 0; i < 4; i++)
			uniqueness.Add (i);
		for (int i = 0; i < 4; i++) {
			int temp = rnd.Range (0, 4 - i);
			colorValues[i] = uniqueness[temp];
			textscroll [i] = abbreviations [colorValues [i]];
			uniqueness.RemoveAt(temp);
		}
		textscroll[5] = textscroll[forlogging[4]/4];
		textscroll[6] = textscroll[forlogging[4]%4];
		textscroll[8] = textscroll[forlogging[5]/4];
		textscroll[9] = textscroll[forlogging[5]%4];
		Debug.LogFormat ("[Cartiac Arrest #{0}] From 0 to 3, each number is associated with the following colors: {1}, {2}, {3}, {4}.", moduleId, colorNames[colorValues[0]], colorNames[colorValues[1]], colorNames[colorValues[2]], colorNames[colorValues[3]]);
		buttonColors [0] = colorValues[forlogging [2] / 4];
		buttonColors [1] = colorValues[forlogging [2] % 4];
		buttonColors [2] = colorValues[forlogging [3] / 4];
		buttonColors [3] = colorValues[forlogging [3] % 4];
		for (byte i = 0; i < buttons.Length; i++)
		{
			KMSelectable inputs = buttons[i];
			inputs.OnInteract += delegate
			{
				Movement(inputs);
				return false;
			};
		}
    }

	void Start(){
		StartCoroutine (ColorButtons());
	}

	void Update(){
		timer -= Time.deltaTime;
		if (isplaying) {
			if (!solving && timer <= 31.65f && timer > 30f) {
				timer = 31.65f;
				audio.PlaySoundAtTransform ("go", transform);
				Debug.LogFormat ("[Cartiac Arrest #{0}] Crowd's cleared up! Get to your goal now!", moduleId);
				solving = true;
				StartCoroutine (ColorButtons ());
				buttonColors [0] = colorValues[forlogging [0] / 4];
				buttonColors [1] = colorValues[forlogging [0] % 4];
				buttonColors [2] = colorValues[forlogging [1] / 4];
				buttonColors [3] = colorValues[forlogging [1] % 4];
				screenText.text = "";
				screenText.fontSize = 140;
			}
			if (timer <= 31.35f && timer > 0f && solving) {
				if (!colored)
					StartCoroutine (ColorButtons ());
				screenText.text = ":" + (timer - 0.5f).ToString ("N0");
				screenText.text += System.Environment.NewLine;
				screenText.text += "." + ((timer % 1f) * 100f).ToString ("N0");
			}
			if (timer <= 0f && solving) {
				solving = false;
				StartCoroutine (EndAnim ());
			}
		} else {
			if (timer < 0f) {
				timer += 1f;
				screenText.text = textscroll [textpart % 11];
				textpart++;
			}
		}
	}

	IEnumerator EndAnim (){
		ambulanced = false;
		screenText.fontSize = 100;
		Debug.LogFormat ("[Cartiac Arrest #{0}] The crowd has gathered again! But did you make it?", moduleId);
		screenText.text = "di";
		yield return new WaitWhile (() => timer > -0.15f);
		screenText.text = "jack";
		yield return new WaitWhile (() => timer > -0.3f);
		screenText.text = "a";
		yield return new WaitWhile (() => timer > -0.45f);
		screenText.text = "from";
		yield return new WaitWhile (() => timer > -0.6f);
		screenText.text = "di";
		yield return new WaitWhile (() => timer > -0.75f);
		screenText.text = "pack";
		yield return new WaitWhile (() => timer > -0.9f);
		screenText.text = "a";
		yield return new WaitWhile (() => timer > -1.05f);
		screenText.text = "man";
		yield return new WaitWhile (() => timer > -1.2f);
		screenText.text = "a";
		yield return new WaitWhile (() => timer > -1.35f);
		screenText.text = "rule";
		yield return new WaitWhile (() => timer > -1.65f);
		screenText.text = "any";
		yield return new WaitWhile (() => timer > -1.95f);
		screenText.text = "flock";
		if (environment [forlogging [0], forlogging [1]] == 3)
			StartCoroutine (SolveAnim ());
		else
			StartCoroutine (StrikeAnim ());
		yield return new WaitWhile (() => timer > -2.1f);
		screenText.text = "";
	}

	void Screened(){
		if (!isplaying) {
			timer = 34.5f;
			isplaying = true;
			audio.PlaySoundAtTransform ("ready", transform);
			rotatoration.Play ("Base Layer.ro ta te", -1, 0f);
		}
	}

	void Movement(KMSelectable inputs)
	{
		int press = Array.IndexOf(buttons, inputs);
		buttons[press].AddInteractionPunch(0.2f);
		if (!ambulanced && solving) {
			switch (press) {
			case 0:
				if (forlogging [0] != 0) {
					environment [forlogging [0], forlogging [1]]--;
					forlogging [0]--;
					environment [forlogging [0], forlogging [1]]++;
					if (rightear)
						audio.PlaySoundAtTransform ("move-right", transform);
					else
						audio.PlaySoundAtTransform ("move-left", transform);
					rightear = !rightear;
				} else {
					if (rightear)
						audio.PlaySoundAtTransform ("bad-right", transform);
					else
						audio.PlaySoundAtTransform ("bad-left", transform);
					rightear = !rightear;
				}
				break;
			case 1:
				if (forlogging [1] != 15) {
					environment [forlogging [0], forlogging [1]]--;
					forlogging [1]++;
					environment [forlogging [0], forlogging [1]]++;
					if (rightear)
						audio.PlaySoundAtTransform ("move-right", transform);
					else
						audio.PlaySoundAtTransform ("move-left", transform);
					rightear = !rightear;
				} else {
					if (rightear)
						audio.PlaySoundAtTransform ("bad-right", transform);
					else
						audio.PlaySoundAtTransform ("bad-left", transform);
					rightear = !rightear;
				}
				break;
			case 2:
				if (forlogging [0] != 15) {
					environment [forlogging [0], forlogging [1]]--;
					forlogging [0]++;
					environment [forlogging [0], forlogging [1]]++;
					if (rightear)
						audio.PlaySoundAtTransform ("move-right", transform);
					else
						audio.PlaySoundAtTransform ("move-left", transform);
					rightear = !rightear;
				} else {
					if (rightear)
						audio.PlaySoundAtTransform ("bad-right", transform);
					else
						audio.PlaySoundAtTransform ("bad-left", transform);
					rightear = !rightear;
				}
				break;
			case 3:
				if (forlogging [1] != 0) {
					environment [forlogging [0], forlogging [1]]--;
					forlogging [1]--;
					environment [forlogging [0], forlogging [1]]++;
					if (rightear)
						audio.PlaySoundAtTransform ("move-right", transform);
					else
						audio.PlaySoundAtTransform ("move-left", transform);
					rightear = !rightear;
				} else {
					if (rightear)
						audio.PlaySoundAtTransform ("bad-right", transform);
					else
						audio.PlaySoundAtTransform ("bad-left", transform);
					rightear = !rightear;
				}
				break;
			}
			if (environment [forlogging [0], forlogging [1]] == 5) {
				ambulanced = true;
			}
		} else if (ambulanced) {
			if (rightear)
				audio.PlaySoundAtTransform ("bad-right", transform);
			else
				audio.PlaySoundAtTransform ("bad-left", transform);
			rightear = !rightear;
		}
	}

	IEnumerator SolveAnim(){
		yield return new WaitWhile (() => timer > -3f);
		timer = -3f;
		Debug.LogFormat ("[Cartiac Arrest #{0}] Yes, you did! Nice job! Now stick around a while...", moduleId);
		audio.PlaySoundAtTransform ("solve", transform);
		wackysolveanim.Play ("Base Layer.solvething", -1, 0f);
		yield return new WaitWhile (() => timer > -3.3f);
		screenText.fontSize = 70;
		screenText.text = "antwerp";
		yield return new WaitWhile (() => timer > -3.75f);
		screenText.text += System.Environment.NewLine + "massive";
		yield return new WaitWhile (() => timer > 4.05f);
		module.HandlePass ();
	}

	IEnumerator StrikeAnim(){
		audio.PlaySoundAtTransform ("strike", transform);
		Debug.LogFormat ("[Cartiac Arrest #{0}] No, you failed! You made it to ({1}, {2}), but that wasn't where you wanted to be!", moduleId, forlogging[0], forlogging[1]);
		timer = 10f;
		StartCoroutine (ColorButtons ());
		yield return new WaitWhile (() => timer > 8.95f);
		for (int i = 0; i < 8; i++) {
			screenText.fontSize = 280;
			screenText.text = "X";
			yield return new WaitWhile (() => timer > (8.875f-(Convert.ToSingle(i)*0.15f)));
			screenText.text = "";
			yield return new WaitWhile (() => timer > (8.8f-(Convert.ToSingle(i)*0.15f)));
		}
		yield return new WaitWhile (() => timer > 6.85f);
		module.HandleStrike ();
		yield return new WaitWhile (() => timer > 3f);
		environment [forlogging[4], forlogging[5]] = 0;
		environment [forlogging[0], forlogging[1]] = 0;
		environment [forlogging[2], forlogging[3]] = 0;
		for (int i = 0; i < 6; i++)
			forlogging [i] = rnd.Range (0, 16);
		environment [forlogging[4], forlogging[5]] = 4;
		Debug.LogFormat("[Cartiac Arrest #{0}] The ambulance has now been placed at ({1}, {2}).", moduleId, forlogging[4], forlogging[5]);
		environment [forlogging[0], forlogging[1]] = 1;
		environment [forlogging[2], forlogging[3]] = 2;
		Debug.LogFormat("[Cartiac Arrest #{0}] Now starting at ({1}, {2}) and ending at ({3}, {4}).", moduleId, forlogging[0], forlogging[1], forlogging[2], forlogging[3]);
		buttonColors [0] = colorValues[forlogging [2] / 4];
		buttonColors [1] = colorValues[forlogging [2] % 4];
		buttonColors [2] = colorValues[forlogging [3] / 4];
		buttonColors [3] = colorValues[forlogging [3] % 4];
		textscroll[5] = textscroll[forlogging[4]/4];
		textscroll[6] = textscroll[forlogging[4]%4];
		textscroll[8] = textscroll[forlogging[5]/4];
		textscroll[9] = textscroll[forlogging[5]%4];
		StartCoroutine (ColorButtons ());
		isplaying = false;
	}

    private IEnumerator ColorButtons()
    {
        for (int i = 0; i < 4; i++)
        {
			if (!colored) {
				StartCoroutine (ColorButton (buttonRenders [i], i, buttonRenders [i].material.color, !moduleSolved ? buttonColors [i] : 4));
			} else {
				StartCoroutine (ColorButton (buttonRenders [i], i, buttonRenders [i].material.color, 4));
			}
			if (!solving)
            	yield return new WaitForSeconds(.5f);
        }
		colored = !colored;
    }

    private IEnumerator ColorButton(Renderer button, int i, Color startColor, int endColorIndex)
    {
        var elapsed = 0f;
        var duration = .75f;
		if (solving)
			duration = .05f;
        var endColor = endColorIndex == 0 ? Color.magenta : endColorIndex == 1 ? Color.yellow : endColorIndex == 2 ? Color.cyan : endColorIndex == 3 ? Color.blue : Color.black;
        while (elapsed < duration)
        {
            button.material.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }
        button.material.color = endColor;
    }
}
