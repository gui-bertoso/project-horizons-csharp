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
        GD.Print("Can talk");
        if (!_playerInside)
            return;
        GD.Print("Can2 talk");

        if (Input.IsActionJustPressed("interact"))
        {
            GD.Print("init talk");
            if (_dialogue != null && !_dialogue.Visible)
                _dialogue.StartDialogue(DialoguePath);
            GD.Print("here talk");
        }
        GD.Print("not talk");
    }

    private void OnBodyEntered(Node body)
    {
        _playerInside = true;
        GD.Print("player entrou na área");
    }

    private void OnBodyExited(Node body)
    {
        _playerInside = false;

        if (_dialogue != null && _dialogue.Visible)
            _dialogue.StopDialogue();

        GD.Print("player saiu da área");
    }
}