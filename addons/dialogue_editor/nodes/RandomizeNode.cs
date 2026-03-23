using Godot;
using System.Text;

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
    public override int GetPrimaryOutputSlot() => 1;

    public override string ExportBody(DialogueEditor editor)
    {
        var sb = new StringBuilder();

        sb.AppendLine("type=randomize");

        int index = 0;

        for (int slot = 1; slot <= 3; slot++)
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