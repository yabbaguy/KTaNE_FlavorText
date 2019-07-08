using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

public class FlavorTextCruel : MonoBehaviour
{
    public TextAsset flavorTextJson;
    public Text textDisplay;
    public KMSelectable[] buttons;
    public MeshRenderer[] leds;
    public KMBombInfo bombInfo;

    public Material off;
    public Material green;
    public Material red;
    
    List<FlavorTextOption> textOptions;
    FlavorTextOption textOption;
    bool isActive = false;
    int[] buttonNumbers;
    bool[] buttonStates;
    List<int> moduleIds;
    int stage = 0;
    int maxStageAmount = 3;
    static int _moduleIdCounter = 1;
    int _moduleId = 0;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        stage = 0;
        isActive = true;
        Debug.LogFormat("[Flavor Text EX #{0}] It's on.", _moduleId);
        for(int i = 0; i < buttons.Count(); i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }
        Randomize();
    }

    void OnReactivate()
    {
        isActive = true;
        Debug.LogFormat("[Flavor Text EX #{0}] It's back on.", _moduleId);
        Randomize();
    }
    
    void Randomize()
    {
        for (int i = 0; i < 4; i++)
        {
            leds[i].material = off;
        }
        buttonStates = new[] {false, false, false, false};
        buttonNumbers = new[] {0, 0, 0, 0};
        buttonNumbers[0] = UnityEngine.Random.Range(0, 10);
        buttonNumbers[1] = UnityEngine.Random.Range(0, 10);
        do
        {
            buttonNumbers[1] = UnityEngine.Random.Range(0, 10);
        } while (buttonNumbers[1] == buttonNumbers[0]);
        buttonNumbers[2] = UnityEngine.Random.Range(0, 10);
        do
        {
            buttonNumbers[2] = UnityEngine.Random.Range(0, 10);
        } while (buttonNumbers[2] == buttonNumbers[0] || buttonNumbers[2] == buttonNumbers[1]);
        buttonNumbers[3] = UnityEngine.Random.Range(0, 10);
        do
        {
            buttonNumbers[3] = UnityEngine.Random.Range(0, 10);
        } while (buttonNumbers[3] == buttonNumbers[0] || buttonNumbers[3] == buttonNumbers[1] || buttonNumbers[3] == buttonNumbers[2]);
        string choice = "";
        for(int i = 0; i < buttons.Length; i++)
        {
            string label = buttonNumbers[i].ToString();
            choice += label;
            TextMesh buttonText = buttons[i].GetComponentInChildren<TextMesh>();
            buttonText.text = label;
        }
        
        textOption = textOptions[UnityEngine.Random.Range(0, textOptions.Count)];
        textDisplay.text = textOption.text;
        moduleIds = new List<int>();
        for (int i = 0; i < textOptions.Count; i++)
        {
            if (textOptions[i].text == textOption.text && !moduleIds.Contains(textOptions[i].steam_id))
            {
                moduleIds.Add(textOptions[i].steam_id);
            }
        }
        
		if (textOption.text == "And here's the Countdown clock...")
		{
			Debug.LogFormat("[Flavor Text #{0}] It's looking for (Cruel) Countdown.", _moduleId);
		}
		else
		{
			Debug.LogFormat("[Flavor Text #{0}] It's looking for {1}.", _moduleId, textOption.name);
		}
		
        Debug.LogFormat("[Flavor Text EX #{0}] It said: {1}", _moduleId, textOption.text);
        Debug.LogFormat("[Flavor Text EX #{0}] It offered you a choice. ({1})", _moduleId, choice);
    }
    
    void OnPress(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (isActive)
        {
            Debug.LogFormat("[Flavor Text EX #{0}] You chose {1}.", _moduleId, buttonNumbers[pressedButton]);
            bool buttonIsCorrect = !buttonStates[pressedButton];
            List<int> moduleIdsCopy = new List<int>(moduleIds);
            foreach (int id in moduleIds) {
                string steamId = id.ToString();
                for (int i = 0; i < buttonNumbers.Count(); i++)
                {
                    if (!buttonStates[i] && (steamId.IndexOf(buttonNumbers[pressedButton].ToString()) > steamId.IndexOf(buttonNumbers[i].ToString()) || steamId.IndexOf(buttonNumbers[pressedButton].ToString()) < 0) && steamId.IndexOf(buttonNumbers[i].ToString()) >= 0 && textOption.steam_id > 0)
                    {
                        moduleIdsCopy.Remove(id);
                        break;
                    }
                }
            }
            moduleIds = moduleIdsCopy;
            if (moduleIds.Count() == 0)
            {
                buttonIsCorrect = false;
            }
            if (buttonIsCorrect)
            {
                buttonStates[pressedButton] = true;
                leds[pressedButton].material = green;
                Debug.LogFormat("[Flavor Text EX #{0}] It accepted your choice.", _moduleId);
                if (buttonStates[0] && buttonStates[1] && buttonStates[2] && buttonStates[3])
                {
                    stage++;
                    Debug.LogFormat("[Flavor Text EX #{0}] It became more content.", _moduleId);
                    if (stage == maxStageAmount)
                    {
                        Debug.LogFormat("[Flavor Text EX #{0}] Flavor Text EX was spared.", _moduleId);
                        textDisplay.text = "";
                        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                        GetComponent<KMBombModule>().HandlePass();
                        isActive = false;
                    }
                    else
                    {
                        Randomize();
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Flavor Text EX #{0}] It refused your choice.", _moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                StartCoroutine(RedLights());
            }
        }
    }
    
    IEnumerator RedLights()
    {
        isActive = false;
        for (int i = 0; i < 4; i++)
        {
            leds[i].material = red;
        }
        yield return new WaitForSeconds(1f);
        stage = 0;
        OnReactivate();
    }

    //twitch plays
    private bool cmdIsValid1(string cmd)
    {
        char[] valids = { '1','2','3','4' };
        if((cmd.Length >= 1) && (cmd.Length <= 4))
        {
            foreach(char c in cmd)
            {
                if (!valids.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool cmdIsValid2(string cmd)
    {
        int[] valids = { buttonNumbers[0], buttonNumbers[1], buttonNumbers[2], buttonNumbers[3] };
        if ((cmd.Length >= 1) && (cmd.Length <= 4))
        {
            foreach (char c in cmd)
            {
                int test = (int)Char.GetNumericValue(c);
                if (!valids.Contains(test))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} position/pos/p 1234 [Presses the buttons from top to bottom] | !{0} label/lab/l 6805 [Presses the buttons labelled '6','8','0', then '5']";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*position\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*pos\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*p\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 2)
            {
                if (cmdIsValid1(parameters[1]))
                {
                    yield return null;
                    foreach (char c in parameters[1])
                    {
                        if (c.Equals('1'))
                        {
                            yield return new[] { buttons[0] };
                        }
                        else if (c.Equals('2'))
                        {
                            yield return new[] { buttons[1] };
                        }
                        else if (c.Equals('3'))
                        {
                            yield return new[] { buttons[2] };
                        }
                        else if (c.Equals('4'))
                        {
                            yield return new[] { buttons[3] };
                        }
                        yield return new WaitForSeconds(.1f);
                    }
                }
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*label\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*lab\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*l\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 2)
            {
                if (cmdIsValid2(parameters[1]))
                {
                    yield return null;
                    foreach (char c in parameters[1])
                    {
                        if (c.Equals('1'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(1));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('2'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(2));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('3'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(3));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('4'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(4));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('5'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(5));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('6'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(6));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('7'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(7));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('8'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(8));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('9'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(9));
                            yield return new[] { buttons[index] };
                        }
                        else if (c.Equals('0'))
                        {
                            int index = Array.FindIndex(buttonNumbers, x => x.Equals(0));
                            yield return new[] { buttons[index] };
                        }
                        yield return new WaitForSeconds(.1f);
                    }
                }
            }  
            yield break;
        }
    }
}
