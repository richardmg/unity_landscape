using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CommandPrompt : MonoBehaviour {
	public GameObject inputGO;
	public GameObject outputGO;
	List<string> tokens;

	void OnEnable()
	{
		inputGO.GetComponent<InputField>().ActivateInputField();
	}

	string nextString()
	{
		if (tokens.Count == 0)
			return "";
		string token = tokens[0];
		tokens.RemoveAt(0);
		return token;
	}

	int nextInt()
	{
//		string token = nextString();
		return 0;
	}

	public void log(string message)
	{
		InputField output = outputGO.GetComponent<InputField>();
		output.text = message + "\n" + output.text.Substring(0, Mathf.Min(output.text.Length, 500));
	}

	public void onInputChanged(InputField inputField)
	{
		tokens = new List<string>(inputField.text.Split(new char[]{' '}));
		bool accepted = false;

		string token = nextString();

		if (token == "atlas") {
			token = nextString();
			if (token == "copyback") {
				log("copyback index x to base atlas index y");
				accepted = true;
			}
		} else if (token == "clear") {
			InputField output = outputGO.GetComponent<InputField>();
			output.text = "";
			accepted = true;
		} else if (token == "project") {
			token = nextString();
			if (token == "save") {
				Root.instance.projectManager.currentProject.save();
				accepted = true;
			}
		} else if (token == "close") {
			Root.instance.uiManager.toggleCommandPromptUI(false);
			accepted = true;
		} else if (token == "paint") {
			token = nextString();
			if (token == "index") {
				log("Current paint index: " + Root.instance.uiManager.entityPainter.currentAtlasIndex());
				accepted = true;
			}
		}

		if (accepted) {
			inputField.text = "";
		} else {
			log("atlas [copyback|copy] [from] [to]");
			log("paint [index]");
			log("project [save]");
			log("close");
			log("clear");
			log("-- help --");
		}

		inputField.ActivateInputField();
		UnityEditor.EditorApplication.delayCall += ()=> {
//			inputField.selectionAnchorPosition = 0;
//			inputField.selectionFocusPosition = 0;
//			inputField.caretPosition = inputField.text.Length;
			inputField.MoveTextEnd(false);
		};
	}
}
