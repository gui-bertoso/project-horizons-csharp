using Godot;
using Godot.Collections;
using projecthorizonscs.Autoload;
using System;

namespace projecthorizonscs.Interface.SettingsMenu;

public partial class SettingsMenu
{
	private VBoxContainer _keybindsSection;
	private Label _keybindStatusLabel;
	private readonly Dictionary<string, Button> _keybindButtons = new();
	private string _pendingRebindAction = "";
	private int _pendingRebindSlot = -1;
	
	
	

	public override void _UnhandledInput(InputEvent @event)
	{
		if (string.IsNullOrWhiteSpace(_pendingRebindAction) || _pendingRebindSlot < 0)
			return;

		if (InputBindingsManager.TrySetBinding(DataManager.I.GameDataDictionary, _pendingRebindAction, _pendingRebindSlot, @event, out string errorMessage))
		{
			string action = _pendingRebindAction;
			int slot = _pendingRebindSlot;

			_pendingRebindAction = "";
			_pendingRebindSlot = -1;
			RefreshKeybindingButtons();
			_keybindStatusLabel.Text = $"{InputBindingsManager.GetActionLabel(action)} ({GetSlotLabel(slot)}) updated.";
			DataManager.I.SaveGameData();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (!string.IsNullOrWhiteSpace(errorMessage))
			_keybindStatusLabel.Text = errorMessage;
	}

	private void BuildKeybindingSection()
	{
		_keybindsSection = new VBoxContainer();
		_keybindsSection.AddThemeConstantOverride("separation", 8);

		var title = new Label { Text = "Controls" };
		_keybindsSection.AddChild(title);
		_keybindsSection.AddChild(new HSeparator());

		foreach (string action in InputBindingsManager.BindableActions)
		{
			var row = new HBoxContainer();
			var label = new Label
			{
				Text = InputBindingsManager.GetActionLabel(action),
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};

			Button primaryButton = CreateBindButton(action, 0);
			Button secondaryButton = CreateBindButton(action, 1);

			row.AddChild(label);
			row.AddChild(primaryButton);
			row.AddChild(secondaryButton);
			_keybindsSection.AddChild(row);
			_keybindButtons[BuildButtonKey(action, 0)] = primaryButton;
			_keybindButtons[BuildButtonKey(action, 1)] = secondaryButton;
		}

		var resetRow = new HBoxContainer();
		var resetButton = new Button
		{
			Text = "Reset Keybinds",
			CustomMinimumSize = new Vector2(180, 0)
		};
		resetButton.Pressed += ResetKeybinds;
		resetRow.AddChild(resetButton);
		_keybindsSection.AddChild(resetRow);

		_keybindStatusLabel = new Label
		{
			Text = "Primary and secondary bindings are available. Duplicate inputs are blocked.",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		_keybindsSection.AddChild(_keybindStatusLabel);

		_rootScrollContent.AddChild(new HSeparator());
		_rootScrollContent.AddChild(_keybindsSection);
	}

	private Button CreateBindButton(string action, int slot)
	{
		var button = new Button
		{
			Text = "Unbound",
			CustomMinimumSize = new Vector2(160, 0)
		};
		button.Pressed += () => StartRebind(action, slot);
		return button;
	}

	private void StartRebind(string action, int slot)
	{
		_pendingRebindAction = action;
		_pendingRebindSlot = slot;
		_keybindStatusLabel.Text = $"Press an input for {InputBindingsManager.GetActionLabel(action)} ({GetSlotLabel(slot)}).";
		RefreshKeybindingButtons();
	}

	private void ResetKeybinds()
	{
		_pendingRebindAction = "";
		_pendingRebindSlot = -1;
		InputBindingsManager.ResetBindingsToDefault(DataManager.I.GameDataDictionary);
		DataManager.I.SaveGameData();
		RefreshKeybindingButtons();
		_keybindStatusLabel.Text = "Keybinds reset to default.";
	}

	private void RefreshKeybindingButtons()
	{
		foreach (var pair in _keybindButtons)
		{
			(string action, int slot) = ParseButtonKey(pair.Key);
			pair.Value.Text = action == _pendingRebindAction && slot == _pendingRebindSlot
				? "Press input..."
				: $"{GetSlotLabel(slot)}: {InputBindingsManager.GetBindingText(DataManager.I.GameDataDictionary, action, slot)}";
		}
	}

	private static string GetSlotLabel(int slot)
	{
		return slot == 0 ? "Primary" : "Secondary";
	}

	private static string BuildButtonKey(string action, int slot)
	{
		return $"{action}:{slot}";
	}

	private static (string action, int slot) ParseButtonKey(string key)
	{
		string[] parts = key.Split(':');
		return (parts[0], int.Parse(parts[1]));
	}
}
