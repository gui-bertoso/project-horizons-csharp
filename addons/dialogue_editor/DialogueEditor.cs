using Godot;
using System;
using System.Collections.Generic;

public class DalgeData
{
    public string Id = "";
    public string Type = "";
    public string Text = "";
    public Vector2 Position = Vector2.Zero;

    public string Option0Text = "";
    public string Option1Text = "";

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
                        float.TryParse(posSplit[0].Trim(), out float x) &&
                        float.TryParse(posSplit[1].Trim(), out float y))
                    {
                        currentNode.Position = new Vector2(x, y);
                    }
                    break;
                }
                
                case "option_0_text":
                    currentNode.Option0Text = Unescape(value);
                    break;

                case "option_0_next":
                    currentNode.NextByPort[0] = value; // porta interna da saída visual 1
                    break;

                case "option_1_text":
                    currentNode.Option1Text = Unescape(value);
                    break;

                case "option_1_next":
                    currentNode.NextByPort[1] = value; // porta interna da saída visual 2
                    break;

                default:
                    if (key.StartsWith("next_"))
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
}

public partial class DialogueEditor : Control
{
    private const int MenuAddDialogueNode = 0;
    private const int MenuAddSwitchNode = 1;
    private const int MenuAddStartNode = 2;
    private const int MenuDeleteSelected = 3;
    private const int MenuDisconnectSelected = 4;
    private const int MenuAddEndNode = 5;
    private const int MenuAddRandomizeNode = 6;

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

    private PackedScene endNodeScene;
    private PackedScene randomizeNodeScene;

    public override void _Ready()
    {
        graph = GetNode<GraphEdit>("LeftPanel/GraphEdit");
        popupMenu = GetNode<PopupMenu>("PopupMenu");
        previewText = GetNode<CodeEdit>("RightPanel/PreviewText");
        refreshPreviewButton = GetNode<Button>("RightPanel/HBoxContainer/RefreshPreviewButton");
        saveDatButton = GetNode<Button>("RightPanel/HBoxContainer/SaveDatButton");

        refreshPreviewButton.Pressed += RefreshPreview;
        saveDatButton.Pressed += SaveDat;

        dialogueNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/DialogueNode.tscn");
        startNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/StartNode.tscn");
        switchNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/SwitchNode.tscn");
        endNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/EndNode.tscn");
        randomizeNodeScene = GD.Load<PackedScene>("res://addons/dialogue_editor/nodes/RandomizeNode.tscn");

        var loadDatButton = GetNode<Button>("RightPanel/HBoxContainer/LoadDatButton");
        loadDatButton.Pressed += LoadPreviewDat;

        graph.ConnectionRequest += OnConnectionRequest;
        graph.DisconnectionRequest += OnDisconnectionRequest;
        graph.ConnectionToEmpty += OnConnectionToEmpty;
        graph.ConnectionFromEmpty += OnConnectionFromEmpty;
        graph.GuiInput += OnGraphGuiInput;

        popupMenu.IdPressed += OnPopupMenuIdPressed;

        SetupPopupMenu();

        if (!HasStartNode())
            CreateStartNode();

        if (!HasEndNode())
            CreateEndNode();
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
        previewText.Text = BuildDat();
    }

    private void SaveDat()
    {
        string dat = BuildDat();

        string path = "res://addons/dialogue_editor/output/dialogue_preview.dalge";

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(dat);

        previewText.Text = dat;

        GD.Print("dat salvo em: " + path);
    }
    
    public void LoadDat(string path)
    {
        var data = DalgeParser.Load(path);

        ClearGraph();

        var createdNodes = new Dictionary<string, DialogueNodeTemplate>();

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
        }

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

                int toPort = toNode.GetPrimaryInputSlot();

                if (!graph.IsNodeConnected(fromNode.Name, fromPort, toNode.Name, toPort))
                    graph.ConnectNode(fromNode.Name, fromPort, toNode.Name, toPort);
            }
        }

        RefreshPreview();
        GD.Print($"dalge carregado: {path}");
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
        string path = "res://addons/dialogue_editor/output/dialogue_preview.dalge";
        LoadDat(path);
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
                    dialogueNode.SetOption1Text(data.Option0Text);
                    dialogueNode.SetOption2Text(data.Option1Text);
                }
                break;

            case "start":
                break;

            case "end":
                break;

            case "switch":
                break;

            case "randomize":
                break;
        }
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
}