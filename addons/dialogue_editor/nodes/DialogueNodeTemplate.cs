using Godot;
using System;
using System.Xml;

public partial class DialogueNodeTemplate : GraphNode
{
    public string NodeType = "base";
    public string NodeId = "";

    public virtual void SetupNode(string type, string prefix)
    {
        if (!string.IsNullOrEmpty(NodeId))
            return;

        NodeType = type;
        NodeId = $"{prefix}_{GD.Randi()}";
        Name = NodeId;
    }

    public virtual int GetPrimaryInputSlot() => 0;
    public virtual int GetPrimaryOutputSlot() => 0;
    public virtual bool HasInput() => true;
    public virtual bool HasOutput() => true;
    public virtual string ExportBody(DialogueEditor editor) => "";

    protected void ClearAllSlots(int maxSlots = 10)
    {
        for (int i = 0; i < maxSlots; i++)
            SetSlot(i, false, 0, Colors.White, false, 0, Colors.White);
    }

	public void Setup(string id, string type)
	{
		NodeId = id;
		NodeType = type;

		Name = id;
		Title = id;
	}

	protected void AddInput(int slot)
	{
		SetSlot(
			slot,
			true, 0, Colors.White,
			false, 0, Colors.White
		);
	}

	protected void AddOutput(int slot)
	{
		SetSlot(
			slot,
			false, 0, Colors.White,
			true, 0, Colors.White
		);
	}
}