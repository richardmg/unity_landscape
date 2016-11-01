using UnityEngine;
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
	List<string> commandHistory = new List<string>();
	int commandHistoryIndex = 0;

	public const MessageType kNormal = 0;
	public const MessageType kHeading = 1;
	public const MessageType kListItem = 2;
	public const MessageType kWarning = 3;

	void Awake()
	{
		helpList.Add("atlas copyindex [from] [to] : copy subimage inside project atlas");
		helpList.Add("atlas show : show atlas image");
		helpList.Add("atlas hide: hide atlas image");
		helpList.Add("atlas saveasbase: copy project atlas to base atlas, and save base atlas");
		helpList.Add("atlas saveindextopng [index] [name]: save subimage as png in project folder");
		helpList.Add("baseatlas show : show base atlas image");
		helpList.Add("baseatlas hide: hide base atlas image");
		helpList.Add("baseatlas copyindex [from] [to] : copy subimage inside base atlas");
		helpList.Add("baseatlas copyindextoproject [from] [to] : copy subimage from base atas to project atlas");
		helpList.Add("baseatlas copyindexfromproject [from] [to] : copy subimage from project atas to base atlas");
		helpList.Add("baseatlas copyatlasfromproject : copy project atlas to base atlas");
		helpList.Add("baseatlas copyatlastoproject : copy base atlas to project atlas");
		helpList.Add("baseatlas save : save base atlas image back to common resource folder");
		helpList.Add("painter index : print current atlas index in entity painter");
		helpList.Add("painter setindex : set current atlas index in entity painter");
		helpList.Add("painter save: save modifications back to texure atlas");
		helpList.Add("project name : print name of current project");
		helpList.Add("project new [name] : Create a new project");
		helpList.Add("project load [name] : load project");
		helpList.Add("project save : save project");
		helpList.Add("project saveAs [name] : save a copy of the project");
		helpList.Add("project list [pattern] : list all project that conforms to pattern");
		helpList.Add("entity indexlist [id] : print atlas indecies used by entity");
		helpList.Add("entity clearcache [id] : clear entity mesh cache");
		helpList.Add("entity vertexcount [id] [lod]: print the enity class' vertex count for the given lod");
		helpList.Add("entity classcount : print number of entity classes");
		helpList.Add("entity name [id]: print the name of entity class");
		helpList.Add("entity prefab [id]: print the name of the prefab the entity is based on");
		helpList.Add("entity new [prefab]: create a new entity class based on the given prefab");
		helpList.Add("notify entitychanged [id] : update listeners that entity changed");
		helpList.Add("player entity : print entity held by player");
		helpList.Add("player pos: print players position");
		helpList.Add("player move [x] [z]: move player on top of landscape at position");
		helpList.Add("close : close console");
		helpList.Add("clear : clear console");
		helpList.Add("help [keyword] : show help");
	}

	void OnEnable()
	{
		InputField input = inputGO.GetComponent<InputField>();
		input.ActivateInputField();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab)) {
			InputField inputField = inputGO.GetComponent<InputField>();
			string textBefore = inputField.text;
			string completed = System.String.Empty;

			if (textBefore.StartsWith("help")) {
				string str = textBefore.Substring(5);
				completed = "help " + stripNonCommands(autocomplete(str, helpList));
			} else {
				completed = stripNonCommands(autocomplete(textBefore, helpList));
			}

			if (completed.Length > 0 && completed != textBefore) {
				inputField.text = completed;
				inputField.MoveTextEnd(false);
			} else {
				printHelp(inputField.text);
			}
		} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (commandHistory.Count == 0)
				return;
			InputField inputField = inputGO.GetComponent<InputField>();
			inputField.text = commandHistory[commandHistoryIndex];
			inputField.MoveTextEnd(false);
			commandHistoryIndex--;
			if (commandHistoryIndex < 0)
				commandHistoryIndex = 0;
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (commandHistory.Count == 0)
				return;
			InputField inputField = inputGO.GetComponent<InputField>();
			commandHistoryIndex++;
			if (commandHistoryIndex >= commandHistory.Count) {
				commandHistoryIndex = commandHistory.Count - 1;
				inputField.text = System.String.Empty;
			} else {
				inputField.text = commandHistory[commandHistoryIndex];
			}
			inputField.MoveTextEnd(false);
		}
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

	public void performCommand(string commandString)
	{
		tokens = new List<string>(commandString.Split(new char[]{' '}));
		InputField inputField = inputGO.GetComponent<InputField>();
		bool accepted = false;

		string token = nextToken();

		if (token == "atlas") {
			token = nextToken();
			if (token == "copyindex") {
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
			} else if (token == "saveasbase") {
				Root.instance.atlasManager.copyAtlasProjectToBase();
				Root.instance.atlasManager.saveBaseAtlasTexture();
				accepted = true;
			} else if (token == "saveindextopng") {
				int index = nextInt();
				string name = nextToken();
				Root.instance.atlasManager.saveIndexToPNG(index, name);
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
			} else if (token == "copyindex") {
				int srcIndex = nextInt();
				int destIndex = nextInt();
				Root.instance.atlasManager.copySubImageFromBaseToBase(srcIndex, destIndex);
				accepted = true;
			} else if (token == "copyindextoproject") {
				int srcIndex = nextInt();
				int destIndex = nextInt();
				Root.instance.atlasManager.copySubImageFromBaseToProject(srcIndex, destIndex);
				accepted = true;
			} else if (token == "copyindexfromproject") {
				int srcIndex = nextInt();
				int destIndex = nextInt();
				Root.instance.atlasManager.copySubImageFromProjectToBase(srcIndex, destIndex);
				accepted = true;
			} else if (token == "copyatlastoproject") {
				Root.instance.atlasManager.copyAtlasBaseToProject();
				accepted = true;
			} else if (token == "copyatlasfromproject") {
				Root.instance.atlasManager.copyAtlasProjectToBase();
				accepted = true;
			} else if (token == "save") {
				Root.instance.atlasManager.saveBaseAtlasTexture();
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
			} else if (token == "saveAs") {
				token = nextToken();
				Root.instance.projectManager.currentProject.saveAs(token);
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
			Root.instance.uiManager.showFirstPersonUI();
			accepted = true;
		} else if (token == "help") {
			printHelp(nextToken());
			accepted = true;
		} else if (token == "painter") {
			token = nextToken();
			if (token == "index") {
				log("Current entity painter index: " + Root.instance.uiManager.entityPainter.m_currentAtlasIndex);
				accepted = true;
			} else if (token == "setindex") {
				int atlasIndex = nextInt();
				UIManager uiMgr = Root.instance.uiManager;
				uiMgr.entityPainter.setEntityClass(null);
				uiMgr.entityPainter.setAtlasIndex(atlasIndex);
				uiMgr.uiPaintEditorGO.pushDialog();
				log("Set index in entity painter to: " + atlasIndex);
				accepted = true;
			} else if (token == "save") {
				Root.instance.uiManager.entityPainter.saveChanges();
				log("Saved changes in entity painter. Atlas index: " + Root.instance.uiManager.entityPainter.m_currentAtlasIndex);
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
				log("Player holds entity: " + Root.instance.player.currentEntityClass().id);
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
			} else if (token == "name") {
				int id = nextInt();
				EntityClass entityClass = Root.instance.entityManager.getEntity(id);
				string name = entityClass.entityName;
				log("Name of entity class: " + name);
				accepted = true;
			} else if (token == "prefab") {
				int id = nextInt();
				EntityClass entityClass = Root.instance.entityManager.getEntity(id);
				string name = entityClass.prefabName;
				log("Name of entity class prefab: " + name);
				accepted = true;
			} else if (token == "classcount") {
				int count = Root.instance.entityManager.allEntityClasses.Count;
				log("Number of entity classes: " + count);
				accepted = true;
			} else if (token == "vertexcount") {
				int id = nextInt();
				int lod = nextInt();
				EntityClass entityClass = Root.instance.entityManager.getEntity(id);
				entityClass.getMesh(lod);
				log("Vertex count: " + entityClass.getVertexCount(lod));
				accepted = true;
			} else if (token == "new") {
				string prefabName = nextToken();
				EntityClass entityClass = new EntityClass(prefabName);
				log("Created new entity class with id: " + entityClass.id);
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
			const int maxLines = 10;
			commandHistory.Add(commandString);
			if (commandHistory.Count > maxLines)
				commandHistory.RemoveAt(0);
			commandHistoryIndex = commandHistory.Count - 1;
			inputField.text = "";
		}

		inputField.ActivateInputField();
		UnityEditor.EditorApplication.delayCall += ()=> {inputField.MoveTextEnd(false); };
	}

	public void onInputChanged(InputField inputField)
	{
		performCommand(inputField.text);
	}

	string autocomplete(string input, List<string> strings)
	{
		List<string> relevantStringList = new List<string>();
		foreach (string s in strings) {
			if (s.StartsWith(input))
				relevantStringList.Add(s);
		}

		if (relevantStringList.Count == 0)
			return System.String.Empty;
		if (relevantStringList.Count == 1)
			return relevantStringList[0];

		string shortestString = relevantStringList[0];
		for (int i = 1; i < relevantStringList.Count; ++i) {
			if (relevantStringList[i].Length < shortestString.Length)
				shortestString = relevantStringList[i];
		}

		char[] charArray = shortestString.ToCharArray();
		for (int i = 0; i < charArray.Length; ++i) {
			foreach (string s in relevantStringList) {
				if (s.ToCharArray()[i] != charArray[i])
					return s.Substring(0, i);
			}
		}

		return shortestString;
	}

	public string stripNonCommands(string helpDesc)
	{
		int index1 = helpDesc.IndexOf('[');
		if (index1 == -1)
			index1 = helpDesc.Length;
		int index2 = helpDesc.IndexOf(':');
		if (index2 == -1)
			index2 = helpDesc.Length;
		return helpDesc.Substring(0, Mathf.Min(index1, index2));	
	}

	void printHelp(string startsWithString)
	{
		if (startsWithString.Length == 0) {
			HashSet<string> helpIndex = new HashSet<string>();
			foreach (string helpString in helpList)
				helpIndex.Add(helpString.Split(new char[]{' '})[0]);
			foreach (string helpString in helpIndex)
				log(helpString, kListItem);
			log("Help index", kHeading);
		} else {
			foreach (string helpString in helpList) {
				if (helpString.StartsWith(startsWithString))
					log(helpString, kListItem);
			}
			log("Help", kHeading);
		}
	}
}
