using Godot;
using System.Collections.Generic;

public class DalgeRuntimeNode
{
    public string Id = "";
    public string Type = "";
    public string Text = "";

    public string Option0Text = "";
    public string Option1Text = "";

    public Dictionary<int, string> NextByPort = new();
}

public static class DalgeRuntimeParser
{
    public static Dictionary<string, DalgeRuntimeNode> Load(string path)
    {
        var nodes = new Dictionary<string, DalgeRuntimeNode>();

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"arquivo não encontrado: {path}");
            return nodes;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        DalgeRuntimeNode currentNode = null;

        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                string id = line.Substring(1, line.Length - 2).Trim();
                currentNode = new DalgeRuntimeNode { Id = id };
                nodes[id] = currentNode;
                continue;
            }

            if (currentNode == null)
                continue;

            string[] split = line.Split('=', 2);
            if (split.Length != 2)
                continue;

            string key = split[0].Trim().ToLower();
            string value = Unescape(split[1].Trim());

            switch (key)
            {
                case "type":
                    currentNode.Type = value;
                    break;

                case "text":
                    currentNode.Text = value;
                    break;

                case "option_0_text":
                    currentNode.Option0Text = value;
                    break;

                case "option_0_next":
                    currentNode.NextByPort[0] = value;
                    break;

                case "option_1_text":
                    currentNode.Option1Text = value;
                    break;

                case "option_1_next":
                    currentNode.NextByPort[1] = value;
                    break;

                case "next":
                    currentNode.NextByPort[0] = value;
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

public partial class DialogueComponent : Control
{
    [Export] public string DialogueFilePath = "res://Dialogues/DummyDialogues.dalge";
    [Export] public float LetterDelay = 0.03f;

    private RichTextLabel _dialogueText;
    private Button _option1Button;
    private Button _option2Button;

    private Dictionary<string, DalgeRuntimeNode> _nodes = new();
    private DalgeRuntimeNode _currentNode;

    private bool _isTyping = false;
    private string _fullText = "";
    private int _typingVersion = 0;

    public override void _Ready()
    {
        _dialogueText = GetNodeOrNull<RichTextLabel>("%DialogueText");
        _option1Button = GetNodeOrNull<Button>("%Option1Button");
        _option2Button = GetNodeOrNull<Button>("%Option2Button");

        if (_dialogueText == null)
            GD.PrintErr("DialogueComponent: %DialogueText nao encontrado.");

        if (_option1Button == null)
            GD.PrintErr("DialogueComponent: %Option1Button nao encontrado.");

        if (_option2Button == null)
            GD.PrintErr("DialogueComponent: %Option2Button nao encontrado.");

        if (_option1Button != null)
            _option1Button.Pressed += OnOption1Pressed;

        if (_option2Button != null)
            _option2Button.Pressed += OnOption2Pressed;

        Hide();
    }

    public void StartDialogue(string path = "")
    {
        string finalPath = string.IsNullOrWhiteSpace(path) ? DialogueFilePath : path;

        if (string.IsNullOrWhiteSpace(finalPath))
        {
            GD.PrintErr("DialogueComponent: nenhum caminho de diálogo foi definido.");
            return;
        }

        _nodes = DalgeRuntimeParser.Load(finalPath);

        if (_nodes.Count == 0)
        {
            GD.PrintErr("DialogueComponent: nenhum node carregado.");
            return;
        }

        if (!_nodes.TryGetValue("start", out var startNode))
        {
            GD.PrintErr("DialogueComponent: node 'start' não encontrado.");
            return;
        }

        _currentNode = startNode;
        AdvanceFromStart();
        Show();
        RefreshUI();
    }

    public void StopDialogue()
    {
        _typingVersion++;
        _isTyping = false;
        _currentNode = null;
        Hide();
    }

    private void AdvanceFromStart()
    {
        if (_currentNode == null)
            return;

        if (_currentNode.Type != "start")
            return;

        if (_currentNode.NextByPort.TryGetValue(0, out string nextId))
        {
            if (_nodes.TryGetValue(nextId, out var nextNode))
                _currentNode = nextNode;
            else
                _currentNode = null;
        }
        else
        {
            _currentNode = null;
        }
    }

    private void RefreshUI()
    {
        if (_currentNode == null)
        {
            StopDialogue();
            return;
        }

        if (_currentNode.Type == "end")
        {
            StopDialogue();
            return;
        }

        if (_dialogueText != null)
            TypeText(_currentNode.Text);

        bool hasOption0Text = !string.IsNullOrWhiteSpace(_currentNode.Option0Text);
        bool hasOption1Text = !string.IsNullOrWhiteSpace(_currentNode.Option1Text);

        bool hasNext0 = _currentNode.NextByPort.ContainsKey(0) && !string.IsNullOrWhiteSpace(_currentNode.NextByPort[0]);
        bool hasNext1 = _currentNode.NextByPort.ContainsKey(1) && !string.IsNullOrWhiteSpace(_currentNode.NextByPort[1]);

        if (hasOption0Text || hasOption1Text)
        {
            if (_option1Button != null)
            {
                _option1Button.Visible = hasOption0Text;
                _option1Button.Text = _currentNode.Option0Text;
            }

            if (_option2Button != null)
            {
                _option2Button.Visible = hasOption1Text;
                _option2Button.Text = _currentNode.Option1Text;
            }

            return;
        }

        if (_option1Button != null)
        {
            _option1Button.Visible = hasNext0;
            _option1Button.Text = "continuar";
        }

        if (_option2Button != null)
            _option2Button.Visible = false;
    }

    private async void TypeText(string text)
    {
        if (_dialogueText == null)
            return;

        _typingVersion++;
        int myVersion = _typingVersion;

        _isTyping = true;
        _fullText = text;
        _dialogueText.Text = "";

        foreach (char c in text)
        {
            if (!_isTyping || myVersion != _typingVersion)
                return;

            _dialogueText.Text += c;
            await ToSignal(GetTree().CreateTimer(LetterDelay), "timeout");
        }

        if (myVersion != _typingVersion)
            return;

        _dialogueText.Text = text;
        _isTyping = false;
    }

    private void SkipTyping()
    {
        if (!_isTyping)
            return;

        _typingVersion++;
        _isTyping = false;

        if (_dialogueText != null)
            _dialogueText.Text = _fullText;
    }

    private void OnOption1Pressed()
    {
        if (_isTyping)
        {
            SkipTyping();
            return;
        }

        if (_currentNode == null)
            return;

        GoToNext(0);
    }

    private void OnOption2Pressed()
    {
        if (_isTyping)
        {
            SkipTyping();
            return;
        }

        if (_currentNode == null)
            return;

        GoToNext(1);
    }

    private void GoToNext(int port)
    {
        if (_currentNode == null)
            return;

        if (_currentNode.NextByPort.TryGetValue(port, out string nextId))
        {
            if (_nodes.TryGetValue(nextId, out var nextNode))
                _currentNode = nextNode;
            else
                _currentNode = null;
        }
        else
        {
            _currentNode = null;
        }

        RefreshUI();
    }
}