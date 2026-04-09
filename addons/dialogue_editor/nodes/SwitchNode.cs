using Godot;
using System.Text;

[Tool]
public partial class SwitchNode : DialogueNodeTemplate
{
    private LineEdit option1;
    private LineEdit option2;
    private LineEdit option3;

    public override void _Ready()
    {
        SetupNode("switch", "switch");
        Title = "Switch Node";

        option1 = GetNodeOrNull<LineEdit>("%Option1");
        option2 = GetNodeOrNull<LineEdit>("%Option2");
        option3 = GetNodeOrNull<LineEdit>("%Option3");

        ClearAllSlots();
        AddOutput(0);
        AddOutput(2);
        AddOutput(4);
        AddInput(7);
    }

    public override bool HasInput() => true;
    public override bool HasOutput() => true;
    public override int GetPrimaryInputSlot() => 7;
    public override int GetPrimaryOutputSlot() => 0;

    public override int VisualOutputSlotToPort(int visualSlot)
    {
        return visualSlot switch
        {
            0 => 0,
            2 => 1,
            4 => 2,
            _ => 0
        };
    }

    public override int PortToVisualOutputSlot(int port)
    {
        return port switch
        {
            0 => 0,
            1 => 2,
            2 => 4,
            _ => 0
        };
    }

    public override string ExportBody(DialogueEditor editor)
    {
        var sb = new StringBuilder();

        sb.AppendLine("type=switch");

        sb.AppendLine($"option_0_text={EscapeText(option1?.Text?.Trim() ?? "")}");
        sb.AppendLine($"option_0_next={editor.GetFirstConnectionFrom(Name, 0)}");

        sb.AppendLine($"option_1_text={EscapeText(option2?.Text?.Trim() ?? "")}");
        sb.AppendLine($"option_1_next={editor.GetFirstConnectionFrom(Name, 2)}");

        sb.AppendLine($"option_2_text={EscapeText(option3?.Text?.Trim() ?? "")}");
        sb.AppendLine($"option_2_next={editor.GetFirstConnectionFrom(Name, 4)}");

        return sb.ToString().TrimEnd();
    }

    public void SetOptionText(int index, string text)
    {
        switch (index)
        {
            case 0:
                if (option1 != null)
                    option1.Text = text;
                break;
            case 1:
                if (option2 != null)
                    option2.Text = text;
                break;
            case 2:
                if (option3 != null)
                    option3.Text = text;
                break;
        }
    }
}
