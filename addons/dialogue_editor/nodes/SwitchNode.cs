using Godot;
using System.Text;

public partial class SwitchNode : DialogueNodeTemplate
{
    private LineEdit option1;
    private LineEdit option2;
    private LineEdit option3;

    public override void _Ready()
    {
        NodeType = "switch";

        option1 = GetNode<LineEdit>("VBoxContainer/Option1");
        option2 = GetNode<LineEdit>("VBoxContainer/Option2");
        option3 = GetNode<LineEdit>("VBoxContainer/Option3");

        AddInput(3);
        AddOutput(0);
        AddOutput(1);
        AddOutput(2);
    }

    public override bool HasInput() => true;
    public override bool HasOutput() => true;
    public override int GetPrimaryInputSlot() => 0;
    public override int GetPrimaryOutputSlot() => 1;

    public override string ExportBody(DialogueEditor editor)
    {
        var sb = new StringBuilder();

        sb.AppendLine("type=switch");

        sb.AppendLine($"option_0_text={option1.Text}");
        sb.AppendLine($"option_0_next={editor.GetFirstConnectionFrom(Name, 1)}");

        sb.AppendLine($"option_1_text={option2.Text}");
        sb.AppendLine($"option_1_next={editor.GetFirstConnectionFrom(Name, 2)}");

        sb.AppendLine($"option_2_text={option3.Text}");
        sb.AppendLine($"option_2_next={editor.GetFirstConnectionFrom(Name, 3)}");

        return sb.ToString().TrimEnd();
    }
}