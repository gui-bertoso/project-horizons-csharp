using Godot;
using System.Text;

[Tool]
public partial class RandomizeNode : DialogueNodeTemplate
{
    public override void _Ready()
    {
        SetupNode("randomize", "randomize");
        Title = "Randomize Node";

        ClearAllSlots();
        AddInput(0);
        AddOutput(2);
        AddOutput(3);
        AddOutput(4);
    }

    public override bool HasInput() => true;
    public override bool HasOutput() => true;
    public override int GetPrimaryInputSlot() => 0;
    public override int GetPrimaryOutputSlot() => 2;

    public override int VisualOutputSlotToPort(int visualSlot)
    {
        return visualSlot switch
        {
            2 => 0,
            3 => 1,
            4 => 2,
            _ => 0
        };
    }

    public override int PortToVisualOutputSlot(int port)
    {
        return port switch
        {
            0 => 2,
            1 => 3,
            2 => 4,
            _ => 2
        };
    }

    public override string ExportBody(DialogueEditor editor)
    {
        var sb = new StringBuilder();

        sb.AppendLine("type=randomize");

        int index = 0;

        for (int slot = 2; slot <= 4; slot++)
        {
            string next = editor.GetConnectionFrom(Name, slot);

            if (!string.IsNullOrEmpty(next))
            {
                sb.AppendLine($"option_{index}_next={next}");
                index++;
            }
        }

        return sb.ToString().TrimEnd();
    }
}
