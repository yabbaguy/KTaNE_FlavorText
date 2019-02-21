using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;

public class FlavorTextOption
{
    public String name;
    public int steam_id;
    public String module_id;
    public String text;
}

public class FlavorText : MonoBehaviour
{
    public TextAsset flavorTextJson;
    public Text textDisplay;
    public KMSelectable[] buttons;
    public KMBombInfo bombInfo;

    List<FlavorTextOption> textOptions;
    FlavorTextOption textOption;
    bool isActive = false;
    List<string> moduleNames;
    static int _moduleIdCounter = 1;
    int _moduleId = 0;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        moduleNames = bombInfo.GetModuleNames();
        for (int i = 0; i < textOptions.Count; i++)
        {
            for (int j = i + 1; j < textOptions.Count; j++)
            {
                if (textOptions[i].text == textOptions[j].text && moduleNames.Contains(textOptions[i].name))
                {
                    moduleNames.Add(textOptions[j].name);
                }
            }
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        isActive = true;
        Debug.LogFormat("[Flavor Text #{0}] It's on.", _moduleId);
        textOption = textOptions[UnityEngine.Random.Range(0, textOptions.Count)];
        textDisplay.text = textOption.text;
		if (textOption.text == "And here's the Countdown clock...")
		{
			Debug.LogFormat("[Flavor Text #{0}] It's looking for (Cruel) Countdown.", _moduleId);
		}
		else
		{
			Debug.LogFormat("[Flavor Text #{0}] It's looking for {1}.", _moduleId, textOption.name);
		}
        Debug.LogFormat("[Flavor Text #{0}] It said: {1}", _moduleId, textOption.text);
        Debug.LogFormat("[Flavor Text #{0}] Do you accept it?", _moduleId);
        for(int i = 0; i < buttons.Count(); i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }
    }

    void OnReactivate()
    {
        isActive = true;
        Debug.LogFormat("[Flavor Text #{0}] It's back on.", _moduleId);
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        textOption = textOptions[UnityEngine.Random.Range(0, textOptions.Count)];
        textDisplay.text = textOption.text;
		if (textOption.text == "And here's the Countdown clock...")
		{
			Debug.LogFormat("[Flavor Text #{0}] It's looking for (Cruel) Countdown.", _moduleId);
		}
		else
		{
			Debug.LogFormat("[Flavor Text #{0}] It's looking for {1}.", _moduleId, textOption.name);
		}
        Debug.LogFormat("[Flavor Text #{0}] It said: {1}", _moduleId, textOption.text);
        Debug.LogFormat("[Flavor Text #{0}] Do you accept it?", _moduleId);
    }
    
    void OnPress(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (isActive)
        {
            Debug.LogFormat("[Flavor Text #{0}] You chose {1}to accept.", _moduleId, (pressedButton == 0) ? "not " : "");
            if ((pressedButton > 0) == moduleNames.Contains(textOption.name))
            {
                Debug.LogFormat("[Flavor Text #{0}] Flavor Text was spared.", _moduleId);
                textDisplay.text = "";
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                GetComponent<KMBombModule>().HandlePass();
                isActive = false;
            }
            else
            {
                Debug.LogFormat("[Flavor Text #{0}] But it refused.", _moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                OnReactivate();
            }
        }
    }
}
