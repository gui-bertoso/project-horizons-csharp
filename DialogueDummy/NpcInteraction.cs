using Godot;

public partial class NpcInteraction : Area2D
{
    [Export] public string DialoguePath = "res://Dialogues/DummyDialogues.dalge";
    [Export] public NodePath DialogueComponentPath;

    private DialogueComponent _dialogue;
    private bool _playerInside = false;

    public override void _Ready()
    {
        _dialogue = GetNodeOrNull<DialogueComponent>(DialogueComponentPath);

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        if (!_playerInside)
            return;

        if (Input.IsActionJustPressed("interact"))
        {
            if (_dialogue != null && !_dialogue.Visible)
                _dialogue.StartDialogue(DialoguePath);
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (!body.IsInGroup("player"))
            return;

        _playerInside = true;
        GD.Print("player entrou na área");
    }

    private void OnBodyExited(Node body)
    {
        if (!body.IsInGroup("player"))
            return;

        _playerInside = false;

        if (_dialogue != null && _dialogue.Visible)
            _dialogue.StopDialogue();

        GD.Print("player saiu da área");
    }
}