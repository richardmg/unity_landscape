using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LineType = System.Int32;

public class CommandPrompt : MonoBehaviour {
	public GameObject inputGO;
	public GameObject outputGO;
	List<string> tokens;

	const LineType kNormal = 0;
	const LineType kHeading = 0;
	const LineType kListItem = 0;

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

	public void log(string message, LineType lineType = kNormal)
	{
		InputField output = outputGO.GetComponent<InputField>();
		string formattedMessage;
		if (lineType == kHeading)
			formattedMessage = "<b>" + message + "</b>";
		else if (lineType == kListItem)
			formattedMessage = "<i>" + message + "</i>";
		else
			formattedMessage = message;

		output.text = formattedMessage + "\n" + output.text.Substring(0, Mathf.Min(output.text.Length, 500));
		print(output.text);
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
			} else if (token == "load") {
				token = nextString();
				Root.instance.projectManager.loadProject(token);
				accepted = true;
			} else if (token == "new") {
				token = nextString();
				Root.instance.projectManager.createProject(token);
				accepted = true;
			} else if (token == "list") {
				token = nextString();
				string[] paths = Root.instance.projectManager.listProjects(token == "" ? "*" : token);
				foreach (string path in paths)
					log(path, kListItem);
				log("Projects", kHeading);
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
			log("atlas [[copyback|copy] [from] [to]]", kListItem);
			log("paint [index]", kListItem);
			log("project [load <name>] | [save [name]] | new | list [pattern]", kListItem);
			log("close", kListItem);
			log("clear", kListItem);
			log("Help", kHeading);
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
