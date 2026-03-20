using Godot;
using System.Text;

public partial class DialogueNode : DialogueNodeTemplate
{
    private LineEdit textEdit;
    private LineEdit option1;
    private LineEdit option2;

    public override void _Ready()
    {
        SetupNode("dialogue", "dialogue");
        Title = "Dialogue Node";

        textEdit = GetNodeOrNull<LineEdit>("%TextEdit");
        option1 = GetNodeOrNull<LineEdit>("%Option1");
        option2 = GetNodeOrNull<LineEdit>("%Option2");

        if (textEdit == null)
            GD.PrintErr($"DialogueNode {Name}: %TextEdit nao encontrado.");

        if (option1 == null)
            GD.PrintErr($"DialogueNode {Name}: %Option1 nao encontrado.");

        if (option2 == null)
            GD.PrintErr($"DialogueNode {Name}: %Option2 nao encontrado.");

        ClearAllSlots();
        AddInput(0);
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

        string mainText = Escape(textEdit?.Text?.Trim() ?? "");
        string option1Text = Escape(option1?.Text?.Trim() ?? "");
        string option2Text = Escape(option2?.Text?.Trim() ?? "");

        string next1 = editor.GetConnectionFrom(Name, 1);
        string next2 = editor.GetConnectionFrom(Name, 2);

        bool hasNext1 = !string.IsNullOrEmpty(next1);
        bool hasNext2 = !string.IsNullOrEmpty(next2);

        sb.AppendLine("type=dialogue");
        sb.AppendLine($"text={mainText}");

        // 0 saídas
        if (!hasNext1 && !hasNext2)
            return sb.ToString().TrimEnd();

        // 1 saída só no slot 1
        if (hasNext1 && !hasNext2)
        {
            sb.AppendLine($"next={next1}");
            return sb.ToString().TrimEnd();
        }

        // 1 saída só no slot 2
        if (!hasNext1 && hasNext2)
        {
            sb.AppendLine($"next={next2}");
            return sb.ToString().TrimEnd();
        }

        // 2 saídas
        sb.AppendLine($"option_0_text={option1Text}");
        sb.AppendLine($"option_0_next={next1}");
        sb.AppendLine($"option_1_text={option2Text}");
        sb.AppendLine($"option_1_next={next2}");

        return sb.ToString().TrimEnd();
    }

    private string Escape(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }
}