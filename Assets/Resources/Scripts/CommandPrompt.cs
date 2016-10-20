﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MessageType = System.Int32;

public class CommandPrompt : MonoBehaviour {
	public GameObject inputGO;
	public GameObject outputGO;
	public GameObject atlasDebugGO;
	public GameObject atlasDebugImageGO;

	List<string> tokens;
	List<string> outputList = new List<string>();
	List<string> helpList = new List<string>();

	public const MessageType kNormal = 0;
	public const MessageType kHeading = 1;
	public const MessageType kListItem = 2;
	public const MessageType kWarning = 3;

	void Awake()
	{
		helpList.Add("atlas copy <from> <to> : copy subimage inside project atlas");
		helpList.Add("atlas show : show atlas image");
		helpList.Add("atlas hide: hide atlas image");
		helpList.Add("baseatlas show : show base atlas image");
		helpList.Add("baseatlas hide: hide base atlas image");
		helpList.Add("baseatlas copy <from> <to> : copy subimage inside base atlas");
		helpList.Add("baseatlas copytoproject <from> <to> : copy subimage from base atas to project atlas");
		helpList.Add("baseatlas copyfromproject <from> <to> : copy subimage from project atas to base atlas");
		helpList.Add("entitypainter currentindex : print current atlas index in entity painter");
		helpList.Add("close : close console");
		helpList.Add("clear : clear console");
		helpList.Add("project name : print name of current project");
		helpList.Add("project new <name> : Create a new project");
		helpList.Add("project load <name> : load project");
		helpList.Add("project save : save project");
		helpList.Add("project saveAs <name> : save a copy of the project");
		helpList.Add("project list [pattern] : list all project that conforms to pattern");
		helpList.Add("player entity ; print entity held by player");
		helpList.Add("player pos: print players position");
		helpList.Add("player move x z: move player on top of landscape at position");
		helpList.Add("entity indexlist <id> : print atlas indecies used by entity");
		helpList.Add("entity clearcache <id> : clear entity mesh cache");
		helpList.Add("notify entitychanged <id> : update listeners that entity changed");
	}

	void OnEnable()
	{
		InputField input = inputGO.GetComponent<InputField>();
		input.ActivateInputField();
		input.text = System.String.Empty;
	}

	bool hasNext()
	{
		return tokens.Count > 0;
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
		return int.Parse(nextToken());
	}

	public void log(string message, MessageType messageType = kNormal)
	{
		InputField output = outputGO.GetComponent<InputField>();
		string formattedMessage;

		if (messageType == kHeading)
			formattedMessage = "<b><color=#8080FF>" + message + "</color></b>";
		else if (messageType == kListItem)
			formattedMessage = "<i><color=#00FF00>" + message + "</color></i>";
		else if (messageType == kWarning)
			formattedMessage = "<color=#FF8080>" + message + "</color>";
		else
			formattedMessage = "<color=white>" + message + "</color>";;

		outputList.Insert(0, formattedMessage);

		const int maxLines = 50;
		if (outputList.Count >= maxLines)
			outputList.RemoveRange(maxLines, outputList.Count - maxLines);

		output.text = string.Join("\n", outputList.ToArray());
	}

	public void onInputChanged(InputField inputField)
	{
		string commandString = inputField.text;
		tokens = new List<string>(commandString.Split(new char[]{' '}));
		bool accepted = false;

		string token = nextToken();

		if (token == "atlas") {
			token = nextToken();
			if (token == "copy") {
				int srcIndex = nextInt();
				int destIndex = nextInt();
				Root.instance.atlasManager.copySubImageFromProjectToProject(srcIndex, destIndex);
				log("copy atlas sub image " + srcIndex + " to " + destIndex);
				accepted = true;
			} else if (token == "show") {
				atlasDebugImageGO.GetComponent<RawImage>().texture = Root.instance.voxelMaterialExact.mainTexture;
				atlasDebugGO.SetActive(true);
				accepted = true;
			} else if (token == "hide") {
				atlasDebugGO.SetActive(false);
				accepted = true;
			}
		} else if (token == "baseatlas") {
			token = nextToken();
			if (token == "show") {
				atlasDebugImageGO.GetComponent<RawImage>().texture = Root.instance.textureAtlas;
				atlasDebugGO.SetActive(true);
				accepted = true;
			} else if (token == "hide") {
				atlasDebugGO.SetActive(false);
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
				token = nextToken();
				if (token != "")
					Root.instance.projectManager.currentProject.saveAs(token);
				else
					Root.instance.projectManager.currentProject.save();
				accepted = true;
			} else if (token == "load") {
				token = nextToken();
				if (token != "") {
					Root.instance.projectManager.loadProject(token);
					accepted = true;
				}
			} else if (token == "new") {
				token = nextToken();
				if (token != "") {
					Root.instance.projectManager.createProject(token);
					accepted = true;
				}
			} else if (token == "list") {
				token = nextToken();
				string[] paths = Root.instance.projectManager.listProjects(token == "" ? "*" : token);
				foreach (string path in paths)
					log(path, kListItem);
				log("Projects", kHeading);
				accepted = true;
			} else if (token == "name") {
				log("Name of current project: " + Root.instance.projectManager.currentProject.name);
				accepted = true;
			}
		} else if (token == "close") {
			Root.instance.uiManager.toggleCommandPromptUI(false);
			accepted = true;
		} else if (token == "entitypainter") {
			token = nextToken();
			if (token == "currentindex") {
				log("Current entity painter index: " + Root.instance.uiManager.entityPainter.currentAtlasIndex());
				accepted = true;
			}
		} else if (token == "player") {
			token = nextToken();
			if (token == "pos") {
				log("Player position: " + Root.instance.player.transform.position);
				accepted = true;
			} else if (token == "move") {
				int x = nextInt();
				int z = nextInt();
				Vector3 pos = new Vector3(x, 0, z);
				pos.y = Root.instance.landscapeManager.sampleHeight(pos) + 1;
				Root.instance.player.transform.position = pos;
				log("Moved player to position: " + Root.instance.player.transform.position);
				accepted = true;
			} else if (token == "entity") {
				log("Player holds entity: " + Root.instance.player.currentEntityClass.id);
				accepted = true;
			}
		} else if (token == "entity") {
			token = nextToken();
			if (token == "indexlist") {
				int id = nextInt();
				EntityClass entityClass = Root.instance.entityManager.getEntity(id);
				List<int> list = entityClass.atlasIndexList();
				string s = "Entity index list: ";
				foreach (int i in list)
					s += i + ", ";
				log(s);
				accepted = true;
			} else if (token == "clearcache") {
				int id = nextInt();
				EntityClass entityClass = Root.instance.entityManager.getEntity(id);
				entityClass.markDirty(EntityClass.DirtyFlags.Mesh);
				Root.instance.notificationManager.notifyEntityClassChanged(entityClass);
				log("Cleard mesh cache (and sendt entity changed notification) for entity: " + id);
				accepted = true;
			}
		} else if (token == "notify") {
			token = nextToken();
			if (token == "entitychanged") {
				int id = nextInt();
				EntityClass entityClass = Root.instance.entityManager.getEntity(id);
				Root.instance.notificationManager.notifyEntityClassChanged(entityClass);
				log("Sent entitychanged notification for entity: " + id);
				accepted = true;
			}
		}

		if (accepted) {
			inputField.text = "";
		} else {
			foreach (string helpString in helpList) {
				if (helpString.StartsWith(commandString))
					log(helpString, kListItem);
			}
			log("Help", kHeading);
		}

		inputField.ActivateInputField();
		UnityEditor.EditorApplication.delayCall += ()=> {inputField.MoveTextEnd(false); };
	}
}
