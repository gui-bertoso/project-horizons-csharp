using Godot;

public partial class EndNode : DialogueNodeTemplate
{
    public override void _Ready()
    {
        NodeType = "end";
        NodeId = "end";
        Name = "end";
        Title = "End Dialogue";

        ClearAllSlots();
        AddInput(0);
    }

    public override bool HasInput() => true;
    public override bool HasOutput() => false;
    public override int GetPrimaryInputSlot() => 0;

    public override string ExportBody(DialogueEditor editor)
    {
        return "type=end";
    }
}