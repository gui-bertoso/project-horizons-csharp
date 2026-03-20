using Godot;
using System;

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
                sb.AppendLine($"[{dialogueNode.Name}]");
                sb.AppendLine(dialogueNode.ExportBody(this));
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

        string path = "res://addons/dialogue_editor/output/dialogue_preview.dat";

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(dat);

        previewText.Text = dat;

        GD.Print("dat salvo em: " + path);
    }

    private bool HasSelectedNode()
    {
        foreach (Node child in graph.GetChildren())
        {
            if (child is GraphNode graphNode && graphNode.Selected)
                return true;
        }

        RefreshPreview();

        return false;
    }

    private GraphNode GetFirstSelectedNode()
    {
        foreach (Node child in graph.GetChildren())
        {
            if (child is GraphNode graphNode && graphNode.Selected)
                return graphNode;
        }

        RefreshPreview();

        return null;
    }

    private bool HasStartNode()
    {
        foreach (Node child in graph.GetChildren())
        {
            if (child is DialogueNodeTemplate baseNode && baseNode.NodeType == "start")
                return true;
        }

        RefreshPreview();

        return false;
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
                DeleteSelectedNodes();

                RefreshPreview();
        }
    }

    private void OnConnectionRequest(StringName from, long fromSlot, StringName to, long toSlot)
    {
        graph.ConnectNode(from, (int)fromSlot, to, (int)toSlot);
        RefreshPreview();
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
            node.QueueFree();
        }

        RefreshPreview();
    }

    private void RemoveConnectionsFromNode(GraphNode node)
    {
        var connections = graph.GetConnectionList();

        foreach (Godot.Collections.Dictionary connection in connections)
        {
            StringName fromNode = (StringName)connection["from_node"];
            int fromPort = (int)(long)connection["from_port"];
            StringName toNode = (StringName)connection["to_node"];
            int toPort = (int)(long)connection["to_port"];

            if (fromNode == node.Name || toNode == node.Name)
                graph.DisconnectNode(fromNode, fromPort, toNode, toPort);
        }

        RefreshPreview();
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