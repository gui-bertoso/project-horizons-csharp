using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

public class DalgeData
{
	public string Id = "";
	public string Type = "";
	public string Text = "";
	public Vector2 Position = Vector2.Zero;
	public Dictionary<int, string> OptionTextByIndex = new();
	public Dictionary<int, string> NextByPort = new();
}

public static class DalgeParser
{
	public static Dictionary<string, DalgeData> Load(string path)
	{
		var nodes = new Dictionary<string, DalgeData>();

		if (!FileAccess.FileExists(path))
		{
			GD.PrintErr($"arquivo não encontrado: {path}");
			return nodes;
		}

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

		DalgeData currentNode = null;

		while (!file.EofReached())
		{
			string rawLine = file.GetLine();
			string line = rawLine.Trim();

			if (string.IsNullOrWhiteSpace(line))
				continue;

			if (line.StartsWith("[") && line.EndsWith("]"))
			{
				string nodeId = line.Substring(1, line.Length - 2).Trim();

				currentNode = new DalgeData
				{
					Id = nodeId
				};

				nodes[nodeId] = currentNode;
				continue;
			}

			if (currentNode == null)
				continue;

			string[] split = line.Split('=', 2);
			if (split.Length != 2)
				continue;

			string key = split[0].Trim().ToLower();
			string value = split[1].Trim();

			switch (key)
			{
				case "type":
					currentNode.Type = value;
					break;

				case "text":
					currentNode.Text = Unescape(value);
					break;

				case "position":
				{
					string[] posSplit = value.Split(',', 2);

					if (posSplit.Length == 2 &&
						float.TryParse(posSplit[0].Trim(), CultureInfo.InvariantCulture, out float x) &&
						float.TryParse(posSplit[1].Trim(), CultureInfo.InvariantCulture, out float y))
					{
						currentNode.Position = new Vector2(x, y);
					}
					break;
				}

				default:
					if (TryParseOptionKey(key, "option_", "_text", out int optionTextIndex))
					{
						currentNode.OptionTextByIndex[optionTextIndex] = Unescape(value);
					}
					else if (TryParseOptionKey(key, "option_", "_next", out int optionNextIndex))
					{
						currentNode.NextByPort[optionNextIndex] = value;
					}
					else if (key.StartsWith("next_"))
					{
						string portText = key.Substring(5);

						if (int.TryParse(portText, out int port))
							currentNode.NextByPort[port] = value;
					}
					else if (key == "next")
					{
						// compatibilidade com formato antigo
						currentNode.NextByPort[0] = value;
					}
					break;
			}
		}

		return nodes;
	}

	private static string Unescape(string text)
	{
		return text
			.Replace("\\n", "\n")
			.Replace("\\\\", "\\");
	}

	private static bool TryParseOptionKey(string key, string prefix, string suffix, out int index)
	{
		index = -1;
		if (!key.StartsWith(prefix) || !key.EndsWith(suffix))
			return false;

		string numberText = key.Substring(prefix.Length, key.Length - prefix.Length - suffix.Length);
		return int.TryParse(numberText, out index);
	}
}

[Tool]
public partial class DialogueEditor : Control
{
	private const int MenuAddDialogueNode = 0;
	private const int MenuAddSwitchNode = 1;
	private const int MenuAddStartNode = 2;
	private const int MenuDeleteSelected = 3;
	private const int MenuDisconnectSelected = 4;
	private const int MenuAddEndNode = 5;
	private const int MenuAddRandomizeNode = 6;
	private const int FileMenuNew = 100;
	private const int FileMenuOpen = 101;
	private const int FileMenuSave = 102;
	private const int FileMenuSaveAs = 103;
	private const int LoadBatchSize = 40;
	private const string DefaultDialoguePath = "res://addons/dialogue_editor/output/dialogue_preview.dalge";

	private bool isCreatingFromConnection = false;
	private StringName pendingFromNode;
	private int pendingFromSlot;
	private bool pendingFromOutput = true;

	private GraphEdit graph;
	private PopupMenu popupMenu;

	private PackedScene dialogueNodeScene;
	private PackedScene startNodeScene;
	private PackedScene switchNodeScene;

	private Vector2 popupSpawnPosition;

	private CodeEdit previewText;
	private Button refreshPreviewButton;
	private Button saveDatButton;
	private Button loadDatButton;
	private Button browseLoadButton;
	private Button browseSaveButton;
	private LineEdit filePathEdit;
	private Label statusLabel;
	private MenuButton fileMenuButton;
	private PopupMenu fileMenuPopup;
	private FileDialog openFileDialog;
	private FileDialog saveFileDialog;

	private PackedScene endNodeScene;
	private PackedScene randomizeNodeScene;
	private bool _isRefreshingPreview;
	private bool _isBulkLoading;
	private string _currentFilePath = DefaultDialoguePath;

	public override void _Ready()
	{
		graph = GetNodeOrNull<GraphEdit>("%GraphEdit")
			?? GetNodeOrNull<GraphEdit>("Root/MainSplit/LeftPanel/LeftMargin/GraphEdit")
			?? GetNodeOrNull<GraphEdit>("LeftPanel/GraphEdit");
		popupMenu = GetNode<PopupMenu>("PopupMenu");
		previewText = GetNode<CodeEdit>("%PreviewText");
		refreshPreviewButton = GetNode<Button>("%RefreshPreviewButton");
		saveDatButton = GetNode<Button>("%SaveDatButton");
		loadDatButton = GetNode<Button>("%LoadDatButton");
		browseLoadButton = GetNode<Button>("%BrowseLoadButton");
		browseSaveButton = GetNode<Button>("%BrowseSaveButton");
		filePathEdit = GetNode<LineEdit>("%FilePathEdit");
		statusLabel = GetNode<Label>("%StatusLabel");
		fileMenuButton = GetNode<MenuButton>("%FileMenuButton");
		openFileDialog = GetNode<FileDialog>("%OpenFileDialog");
		saveFileDialog = GetNode<FileDialog>("%SaveFileDialog");

		if (graph == null)
		{
			GD.PrintErr("DialogueEditor: GraphEdit nao encontrado.");
			return;
		}

		refreshPreviewButton.Pressed += RefreshPreview;
		saveDatButton.Pressed += SaveDat;
		loadDatButton.Pressed += LoadCurrentPath;
		browseLoadButton.Pressed += OpenLoadDialog;
		browseSaveButton.Pressed += OpenSaveDialog;
		filePathEdit.TextSubmitted += OnFilePathSubmitted;

		dialogueNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/DialogueNode.tscn");
		startNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/StartNode.tscn");
		switchNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/SwitchNode.tscn");
		endNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/EndNode.tscn");
		randomizeNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/RandomizeNode.tscn");

		graph.ConnectionRequest += OnConnectionRequest;
		graph.DisconnectionRequest += OnDisconnectionRequest;
		graph.ConnectionToEmpty += OnConnectionToEmpty;
		graph.ConnectionFromEmpty += OnConnectionFromEmpty;
		graph.GuiInput += OnGraphGuiInput;

		popupMenu.IdPressed += OnPopupMenuIdPressed;
		fileMenuPopup = fileMenuButton.GetPopup();
		fileMenuPopup.IdPressed += OnFileMenuPressed;
		openFileDialog.FileSelected += OnOpenFileSelected;
		saveFileDialog.FileSelected += OnSaveFileSelected;

		SetupPopupMenu();
		SetupFileMenu();
		SetCurrentFilePath(DefaultDialoguePath, false);

		if (!HasStartNode())
			CreateStartNode();

		if (!HasEndNode())
			CreateEndNode();

		RefreshPreview();
	}

	public string GetFirstConnectionFrom(StringName nodeName, int slot)
	{
		var connections = graph.GetConnectionList();

		foreach (Godot.Collections.Dictionary connection in connections)
		{
			StringName fromNode = (StringName)connection["from_node"];
			int fromPort = (int)(long)connection["from_port"];
			StringName toNode = (StringName)connection["to_node"];

			if (fromNode == nodeName && fromPort == slot)
				return toNode.ToString();
		}

		return "";
	}

	private bool HasEndNode()
	{
		foreach (Node child in graph.GetChildren())
		{
			if (child is DialogueNodeTemplate baseNode && baseNode.NodeType == "end")
				return true;
		}

		return false;
	}

	public void CreateEndNode()
	{
		CreateEndNodeAt(new Vector2(900, 200));
	}

	public GraphNode CreateEndNodeAt(Vector2 pos)
	{
		if (HasEndNode())
			return null;

		var node = endNodeScene.Instantiate<GraphNode>();
		graph.AddChild(node);
		node.PositionOffset = pos;
		return node;
	}

	public string GetConnectionFrom(StringName nodeName, int slot)
	{
		var connections = graph.GetConnectionList();

		foreach (Godot.Collections.Dictionary connection in connections)
		{
			StringName fromNode = (StringName)connection["from_node"];
			int fromPort = (int)(long)connection["from_port"];
			StringName toNode = (StringName)connection["to_node"];

			if (fromNode == nodeName && fromPort == slot)
				return toNode.ToString();
		}

		return "";
	}

	public bool HasConnectionFrom(StringName nodeName, int slot)
	{
		return !string.IsNullOrEmpty(GetConnectionFrom(nodeName, slot));
	}

	public string BuildDat()
	{
		var sb = new System.Text.StringBuilder();

		foreach (Node child in graph.GetChildren())
		{
			if (child is DialogueNodeTemplate dialogueNode)
			{
				Vector2 pos = dialogueNode.PositionOffset;

				sb.AppendLine($"[{dialogueNode.Name}]");
				sb.AppendLine($"position={(int)pos.X},{(int)pos.Y}");
				sb.AppendLine(dialogueNode.ExportBody(this).TrimEnd());
				sb.AppendLine();
			}
		}

		return sb.ToString().TrimEnd();
	}

	private void RefreshPreview()
	{
		if (_isRefreshingPreview || _isBulkLoading)
			return;

		_isRefreshingPreview = true;
		previewText.Text = BuildDat();
		_isRefreshingPreview = false;
		UpdateStatus($"Preview atualizado: {GetFileNameFromPath(_currentFilePath)}");
	}

	private void SaveDat()
	{
		SaveDatToPath(GetValidatedPathInput());
	}

	private void SaveDatToPath(string path)
	{
		string dat = BuildDat();
		if (string.IsNullOrWhiteSpace(path))
			path = DefaultDialoguePath;

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		file.StoreString(dat);

		SetCurrentFilePath(path);
		previewText.Text = dat;
		UpdateStatus("Salvo em: " + path);
	}
	
	public async void LoadDat(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return;

		var data = DalgeParser.Load(path);
		if (data.Count == 0 && !FileAccess.FileExists(path))
			return;

		_isBulkLoading = true;
		graph.Visible = false;
		graph.MouseFilter = MouseFilterEnum.Ignore;
		UpdateStatus($"Carregando {GetFileNameFromPath(path)}...");

		try
		{
			ClearGraph();

			var createdNodes = new Dictionary<string, DialogueNodeTemplate>();
			int processedNodes = 0;

			foreach (var pair in data)
			{
				DalgeData nodeData = pair.Value;

				if (string.IsNullOrWhiteSpace(nodeData.Type))
					continue;

				DialogueNodeTemplate node = CreateNodeFromType(nodeData.Type);
				if (node == null)
					continue;

				graph.AddChild(node);

				node.Name = nodeData.Id;
				node.NodeId = nodeData.Id;
				node.NodeType = nodeData.Type;
				node.PositionOffset = nodeData.Position;

				ApplyNodeData(node, nodeData);
				createdNodes[nodeData.Id] = node;

				processedNodes++;
				if (processedNodes % LoadBatchSize == 0)
				{
					UpdateStatus($"Carregando nodes... {processedNodes}/{data.Count}");
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
			}

			int processedConnections = 0;
			foreach (var pair in data)
			{
				DalgeData nodeData = pair.Value;

				if (!createdNodes.TryGetValue(nodeData.Id, out var fromNode))
					continue;

				foreach (var connectionPair in nodeData.NextByPort)
				{
					int fromPort = connectionPair.Key;
					string targetId = connectionPair.Value;

					if (string.IsNullOrWhiteSpace(targetId))
						continue;

					if (!createdNodes.TryGetValue(targetId, out var toNode))
						continue;

					int visualFromPort = fromNode.PortToVisualOutputSlot(fromPort);
					int toPort = toNode.GetPrimaryInputSlot();

					if (!graph.IsNodeConnected(fromNode.Name, visualFromPort, toNode.Name, toPort))
						graph.ConnectNode(fromNode.Name, visualFromPort, toNode.Name, toPort);

					processedConnections++;
					if (processedConnections % LoadBatchSize == 0)
					{
						UpdateStatus($"Conectando nodes... {processedConnections}");
						await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
					}
				}
			}

			SetCurrentFilePath(path);
			_isBulkLoading = false;
			RefreshPreview();
			UpdateStatus($"Carregado: {path}");
		}
		finally
		{
			graph.MouseFilter = MouseFilterEnum.Pass;
			graph.Visible = true;
			_isBulkLoading = false;
		}
	}

	private void ClearGraph()
	{
		var connections = new List<Godot.Collections.Dictionary>();

		foreach (Godot.Collections.Dictionary connection in graph.GetConnectionList())
			connections.Add(connection);

		foreach (var connection in connections)
		{
			StringName fromNode = (StringName)connection["from_node"];
			int fromPort = (int)(long)connection["from_port"];
			StringName toNode = (StringName)connection["to_node"];
			int toPort = (int)(long)connection["to_port"];

			graph.DisconnectNode(fromNode, fromPort, toNode, toPort);
		}

		var nodesToRemove = new List<DialogueNodeTemplate>();

		foreach (Node child in graph.GetChildren())
		{
			if (child is DialogueNodeTemplate dialogueNode)
				nodesToRemove.Add(dialogueNode);
		}

		foreach (var node in nodesToRemove)
		{
			if (IsInstanceValid(node) && node.GetParent() == graph)
				graph.RemoveChild(node);

			node.QueueFree();
		}
	}

	private DialogueNodeTemplate CreateNodeFromType(string type)
	{
		switch (type)
		{
			case "start":
				return startNodeScene.Instantiate<DialogueNodeTemplate>();

			case "dialogue":
				return dialogueNodeScene.Instantiate<DialogueNodeTemplate>();

			case "switch":
				return switchNodeScene.Instantiate<DialogueNodeTemplate>();

			case "randomize":
				return randomizeNodeScene.Instantiate<DialogueNodeTemplate>();

			case "end":
				return endNodeScene.Instantiate<DialogueNodeTemplate>();

			default:
				GD.PrintErr($"tipo de node desconhecido: {type}");
				return null;
		}
	}

	private bool HasSelectedNode()
	{
		foreach (Node child in graph.GetChildren())
		{
			if (child is GraphNode graphNode && graphNode.Selected)
				return true;
		}

		return false;
	}

	private GraphNode GetFirstSelectedNode()
	{
		foreach (Node child in graph.GetChildren())
		{
			if (child is GraphNode graphNode && graphNode.Selected)
				return graphNode;
		}

		return null;
	}

	private bool HasStartNode()
	{
		foreach (Node child in graph.GetChildren())
		{
			if (child is DialogueNodeTemplate baseNode && baseNode.NodeType == "start")
				return true;
		}

		return false;
	}

	public Dictionary<int, string> GetConnectionsFrom(StringName nodeName)
	{
		var result = new Dictionary<int, string>();

		foreach (Godot.Collections.Dictionary connection in graph.GetConnectionList())
		{
			StringName fromNode = (StringName)connection["from_node"];
			int fromPort = (int)(long)connection["from_port"];
			StringName toNode = (StringName)connection["to_node"];

			if (fromNode == nodeName)
				result[fromPort] = toNode.ToString();
		}

		return result;
	}

	private void SetupPopupMenu()
	{
		popupMenu.Clear();

		bool hasSelectedNode = HasSelectedNode();

		if (hasSelectedNode)
		{
			popupMenu.AddItem("delete selected node", MenuDeleteSelected);
			popupMenu.AddItem("disconnect selected node", MenuDisconnectSelected);
			popupMenu.AddSeparator();
		}

		popupMenu.AddItem("add dialogue node", MenuAddDialogueNode);
		popupMenu.AddItem("add switch node", MenuAddSwitchNode);
		popupMenu.AddItem("add randomize node", MenuAddRandomizeNode);

		if (!HasStartNode())
		{
			popupMenu.AddSeparator();
			popupMenu.AddItem("add start node", MenuAddStartNode);
		}
		if (!HasEndNode())
		{
			popupMenu.AddSeparator();
			popupMenu.AddItem("add end node", MenuAddEndNode);
		}

		RefreshPreview();
	}

	private void SetupFileMenu()
	{
		fileMenuPopup.Clear();
		fileMenuPopup.AddItem("New Graph", FileMenuNew);
		fileMenuPopup.AddSeparator();
		fileMenuPopup.AddItem("Open .dalge...", FileMenuOpen);
		fileMenuPopup.AddItem("Save", FileMenuSave);
		fileMenuPopup.AddItem("Save As...", FileMenuSaveAs);
	}

	public GraphNode CreateRandomizeNode(Vector2 pos)
	{
		var node = randomizeNodeScene.Instantiate<GraphNode>();
		graph.AddChild(node);
		node.PositionOffset = pos;
		return node;
	}

	private void SetupCompatiblePopupMenu(bool fromOutput)
	{
		popupMenu.Clear();

		if (fromOutput)
		{
			// arrastou de um output -> precisa de nodes com input
			popupMenu.AddItem("add dialogue node", MenuAddDialogueNode);
			popupMenu.AddItem("add switch node", MenuAddSwitchNode);
		}
		else
		{
			// arrastou de um input -> precisa de nodes com output
			popupMenu.AddItem("add dialogue node", MenuAddDialogueNode);
			popupMenu.AddItem("add switch node", MenuAddSwitchNode);

			if (!HasStartNode())
			{
				popupMenu.AddSeparator();
				popupMenu.AddItem("add start node", MenuAddStartNode);
			}
		}

		RefreshPreview();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Delete)
			{
				DeleteSelectedNodes();
				RefreshPreview();
			}
		}
	}

	private void OnConnectionRequest(StringName fromNodeName, long fromPort, StringName toNodeName, long toPort)
	{
		GD.Print($"CONNECT REQUEST: {fromNodeName}:{fromPort} -> {toNodeName}:{toPort}");

		var fromNode = graph.GetNodeOrNull<DialogueNodeTemplate>(fromNodeName.ToString());
		var toNode = graph.GetNodeOrNull<DialogueNodeTemplate>(toNodeName.ToString());

		if (fromNode == null || toNode == null)
		{
			GD.PrintErr("fromNode ou toNode veio null");
			return;
		}

		if (!CanConnectNodes(fromNode, toNode))
		{
			GD.PrintErr($"conexão bloqueada: {fromNode.Name} -> {toNode.Name}");
			return;
		}

		if (!graph.IsNodeConnected(fromNodeName, (int)fromPort, toNodeName, (int)toPort))
		{
			graph.ConnectNode(fromNodeName, (int)fromPort, toNodeName, (int)toPort);
			GD.Print($"CONNECTED: {fromNodeName}:{fromPort} -> {toNodeName}:{toPort}");
			RefreshPreview();
		}
		else
		{
			GD.Print("já estava conectado");
		}
	}

	private void OnDisconnectionRequest(StringName from, long fromSlot, StringName to, long toSlot)
	{
		graph.DisconnectNode(from, (int)fromSlot, to, (int)toSlot);

		RefreshPreview();
	}

	private void OnConnectionToEmpty(StringName fromNode, long fromSlot, Vector2 releasePosition)
	{
		isCreatingFromConnection = true;
		pendingFromNode = fromNode;
		pendingFromSlot = (int)fromSlot;
		pendingFromOutput = true;

		popupSpawnPosition = graph.ScrollOffset + releasePosition;
		popupMenu.Position = (Vector2I)GetViewport().GetMousePosition();

		SetupCompatiblePopupMenu(true);
		popupMenu.Popup();

		RefreshPreview();
	}

	private void OnConnectionFromEmpty(StringName toNode, long toSlot, Vector2 releasePosition)
	{
		isCreatingFromConnection = true;
		pendingFromNode = toNode;
		pendingFromSlot = (int)toSlot;
		pendingFromOutput = false;

		popupSpawnPosition = graph.ScrollOffset + releasePosition;
		popupMenu.Position = (Vector2I)GetViewport().GetMousePosition();

		SetupCompatiblePopupMenu(false);
		popupMenu.Popup();

		RefreshPreview();
	}

	private void OnGraphGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
			{
				popupSpawnPosition = graph.ScrollOffset + graph.GetLocalMousePosition();
				popupMenu.Position = (Vector2I)mouseButton.GlobalPosition;

				SetupPopupMenu();
				popupMenu.Popup();
			}
		}

		RefreshPreview();
	}

	private void OnFileMenuPressed(long id)
	{
		switch ((int)id)
		{
			case FileMenuNew:
				CreateFreshGraph();
				break;
			case FileMenuOpen:
				OpenLoadDialog();
				break;
			case FileMenuSave:
				SaveDat();
				break;
			case FileMenuSaveAs:
				OpenSaveDialog();
				break;
		}
	}

	private void OnPopupMenuIdPressed(long id)
	{
		GraphNode createdNode = null;

		switch ((int)id)
		{
			case MenuAddDialogueNode:
				createdNode = CreateDialogueNode(popupSpawnPosition);
				break;

			case MenuAddSwitchNode:
				createdNode = CreateSwitchNode(popupSpawnPosition);
				break;

			case MenuAddStartNode:
				createdNode = CreateStartNodeAt(popupSpawnPosition);
				break;

			case MenuDeleteSelected:
				DeleteSelectedNodes();
				break;

			case MenuDisconnectSelected:
				DisconnectSelectedNode();
				break;
			
			case MenuAddRandomizeNode:
				createdNode = CreateRandomizeNode(popupSpawnPosition);
				break;

			case MenuAddEndNode:
				createdNode = CreateEndNodeAt(popupSpawnPosition);
				break;
		}

		if (isCreatingFromConnection && createdNode != null)
			ConnectPendingNode(createdNode);

		isCreatingFromConnection = false;

		RefreshPreview();
	}

	private void ConnectPendingNode(GraphNode createdNode)
	{
		if (createdNode is not DialogueNodeTemplate nodeTemplate)
			return;

		if (pendingFromOutput)
		{
			if (!nodeTemplate.HasInput())
				return;

			var fromNode = graph.GetNodeOrNull<DialogueNodeTemplate>(pendingFromNode.ToString());
			var toNode = nodeTemplate;

			if (fromNode == null || toNode == null)
				return;

			if (!CanConnectNodes(fromNode, toNode))
				return;

			graph.ConnectNode(
				pendingFromNode,
				pendingFromSlot,
				createdNode.Name,
				nodeTemplate.GetPrimaryInputSlot()
			);
		}
		else
		{
			if (!nodeTemplate.HasOutput())
				return;

			var fromNode = nodeTemplate;
			var toNode = graph.GetNodeOrNull<DialogueNodeTemplate>(pendingFromNode.ToString());

			if (fromNode == null || toNode == null)
				return;

			if (!CanConnectNodes(fromNode, toNode))
				return;

			graph.ConnectNode(
				createdNode.Name,
				nodeTemplate.GetPrimaryOutputSlot(),
				pendingFromNode,
				pendingFromSlot
			);
		}

		RefreshPreview();
	}

	private void DisconnectSelectedNode()
	{
		GraphNode selectedNode = GetFirstSelectedNode();

		if (selectedNode == null)
			return;

		if (selectedNode is DialogueNodeTemplate baseNode && baseNode.NodeType == "start")
			return;

		RemoveConnectionsFromNode(selectedNode);

		RefreshPreview();
	}

	private void LoadPreviewDat()
	{
		LoadDat(DefaultDialoguePath);
	}

	private bool HasIncomingConnection(StringName nodeName)
	{
		foreach (Godot.Collections.Dictionary connection in graph.GetConnectionList())
		{
			StringName toNode = (StringName)connection["to_node"];

			if (toNode == nodeName)
				return true;
		}

		return false;
	}

	private bool CanConnectNodes(DialogueNodeTemplate fromNode, DialogueNodeTemplate toNode)
	{
		if (fromNode == null || toNode == null)
			return false;

		if (fromNode == toNode)
			return false;

		if (toNode.NodeType == "start")
			return false;

		if (fromNode.NodeType == "end")
			return false;

		return true;
	}

	private void DeleteSelectedNodes()
	{
		var nodesToDelete = new Godot.Collections.Array<GraphNode>();

		foreach (Node child in graph.GetChildren())
		{
			if (child is GraphNode graphNode && graphNode.Selected)
			{
				if (child is DialogueNodeTemplate baseNode && baseNode.NodeType == "start")
					continue;

				nodesToDelete.Add(graphNode);
			}
		}

		foreach (GraphNode node in nodesToDelete)
		{
			RemoveConnectionsFromNode(node);

			if (node.GetParent() == graph)
				graph.RemoveChild(node);

			node.QueueFree();
		}

		RefreshPreview();
	}

	private void RemoveConnectionsFromNode(GraphNode node)
	{
		var connections = new List<Godot.Collections.Dictionary>();

		foreach (Godot.Collections.Dictionary connection in graph.GetConnectionList())
			connections.Add(connection);

		foreach (var connection in connections)
		{
			StringName fromNode = (StringName)connection["from_node"];
			int fromPort = (int)(long)connection["from_port"];
			StringName toNode = (StringName)connection["to_node"];
			int toPort = (int)(long)connection["to_port"];

			if (fromNode == node.Name || toNode == node.Name)
				graph.DisconnectNode(fromNode, fromPort, toNode, toPort);
		}
	}

	private void ApplyNodeData(DialogueNodeTemplate node, DalgeData data)
	{
		switch (data.Type)
		{
			case "dialogue":
				if (node is DialogueNode dialogueNode)
				{
					dialogueNode.SetText(data.Text);
					dialogueNode.SetOption1Text(GetOptionText(data, 0));
					dialogueNode.SetOption2Text(GetOptionText(data, 1));
				}
				break;

			case "start":
				break;

			case "end":
				break;

			case "switch":
				if (node is SwitchNode switchNode)
				{
					for (int i = 0; i < 3; i++)
						switchNode.SetOptionText(i, GetOptionText(data, i));
				}
				break;

			case "randomize":
				break;
		}
	}

	private void CreateFreshGraph()
	{
		ClearGraph();
		CreateStartNodeAt(new Vector2(200, 200));
		CreateEndNodeAt(new Vector2(900, 200));
		RefreshPreview();
		UpdateStatus("Novo grafo criado.");
	}

	public void CreateStartNode()
	{
		CreateStartNodeAt(new Vector2(200, 200));

		RefreshPreview();
	}

	public GraphNode CreateStartNodeAt(Vector2 pos)
	{
		if (HasStartNode())
			return null;

		var node = startNodeScene.Instantiate<GraphNode>();
		graph.AddChild(node);
		node.PositionOffset = pos;

		RefreshPreview();
		return node;
	}

	public GraphNode CreateDialogueNode(Vector2 pos)
	{
		var node = dialogueNodeScene.Instantiate<GraphNode>();
		graph.AddChild(node);
		node.PositionOffset = pos;

		RefreshPreview();
		return node;
	}

	public GraphNode CreateSwitchNode(Vector2 pos)
	{
		var node = switchNodeScene.Instantiate<GraphNode>();
		graph.AddChild(node);
		node.PositionOffset = pos;

		RefreshPreview();
		return node;
	}

	private static string GetOptionText(DalgeData data, int index)
	{
		return data.OptionTextByIndex.TryGetValue(index, out var text) ? text : string.Empty;
	}

	private void OpenLoadDialog()
	{
		openFileDialog.CurrentPath = GetValidatedPathInput();
		openFileDialog.PopupCenteredRatio();
	}

	private void OpenSaveDialog()
	{
		saveFileDialog.CurrentPath = GetValidatedPathInput();
		saveFileDialog.PopupCenteredRatio();
	}

	private void OnOpenFileSelected(string path)
	{
		LoadDat(path);
	}

	private void OnSaveFileSelected(string path)
	{
		SaveDatToPath(path);
	}

	private void OnFilePathSubmitted(string newText)
	{
		SetCurrentFilePath(newText);
		UpdateStatus($"Path selecionado: {_currentFilePath}");
	}

	private void LoadCurrentPath()
	{
		LoadDat(GetValidatedPathInput());
	}

	private void SetCurrentFilePath(string path, bool updateStatus = true)
	{
		_currentFilePath = string.IsNullOrWhiteSpace(path) ? DefaultDialoguePath : path;
		if (filePathEdit != null)
			filePathEdit.Text = _currentFilePath;

		if (updateStatus)
			UpdateStatus($"Path selecionado: {_currentFilePath}");
	}

	private string GetValidatedPathInput()
	{
		var rawPath = filePathEdit?.Text?.Trim();
		return string.IsNullOrWhiteSpace(rawPath) ? _currentFilePath : rawPath;
	}

	private void UpdateStatus(string message)
	{
		if (statusLabel != null)
			statusLabel.Text = message;
	}

	private static string GetFileNameFromPath(string path)
	{
		return string.IsNullOrWhiteSpace(path) ? "untitled.dalge" : path.GetFile();
	}
}
