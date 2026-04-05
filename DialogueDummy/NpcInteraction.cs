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
        _playerInside = true;
    }

    private void OnBodyExited(Node body)
    {
        _playerInside = false;

        if (_dialogue != null && _dialogue.Visible)
            _dialogue.StopDialogue();
    }
}
