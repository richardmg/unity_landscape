using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MessageType = System.Int32;

public class CommandPrompt : MonoBehaviour {
	public GameObject inputGO;
	public GameObject outputGO;
	List<string> tokens;
	List<string> outputList = new List<string>();

	public const MessageType kNormal = 0;
	public const MessageType kHeading = 1;
	public const MessageType kListItem = 2;
	public const MessageType kWarning = 3;

	void OnEnable()
	{
		InputField input = inputGO.GetComponent<InputField>();
		input.ActivateInputField();
		input.text = System.String.Empty;
	}

	string nextToken()
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

	public void log(string message, MessageType messageType = kNormal)
	{
		InputField output = outputGO.GetComponent<InputField>();
		string formattedMessage;

		if (messageType == kHeading)
			formattedMessage = "<b><color=blue>" + message + "</color></b>";
		else if (messageType == kListItem)
			formattedMessage = "<i><color=green>" + message + "</color></i>";
		else if (messageType == kWarning)
			formattedMessage = "<color=red>" + message + "</color>";
		else
			formattedMessage = message;

		outputList.Insert(0, formattedMessage);

		const int maxLines = 50;
		if (outputList.Count >= maxLines)
			outputList.RemoveRange(maxLines, outputList.Count - maxLines);

		output.text = string.Join("\n", outputList.ToArray());
	}

	public void onInputChanged(InputField inputField)
	{
		tokens = new List<string>(inputField.text.Split(new char[]{' '}));
		bool accepted = false;

		string token = nextToken();

		if (token == "atlas") {
			token = nextToken();
			if (token == "copyback") {
				log("copyback index x to base atlas index y");
				accepted = true;
			}
		} else if (token == "clear") {
			InputField output = outputGO.GetComponent<InputField>();
			outputList.Clear();
			output.text = "";
			accepted = true;
		} else if (token == "project") {
			token = nextToken();
			if (token == "save") {
				Root.instance.projectManager.currentProject.save();
				accepted = true;
			} else if (token == "load") {
				token = nextToken();
				Root.instance.projectManager.loadProject(token);
				accepted = true;
			} else if (token == "new") {
				token = nextToken();
				Root.instance.projectManager.createProject(token);
				accepted = true;
			} else if (token == "list") {
				token = nextToken();
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
			token = nextToken();
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
