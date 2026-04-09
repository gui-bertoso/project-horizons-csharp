using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public static class InputBindingsManager
{
	public const int MaxBindingsPerAction = 2;
	private const string InputBindingsKey = "Settings.InputBindings";

	public static readonly string[] BindableActions =
	[
		"move_up",
		"move_down",
		"move_left",
		"move_right",
		"action",
		"dash",
		"collect",
		"consume",
		"interact",
		"pause"
	];

	private static readonly Dictionary<string, string> ActionLabels = new()
	{
		{ "move_up", "Move Up" },
		{ "move_down", "Move Down" },
		{ "move_left", "Move Left" },
		{ "move_right", "Move Right" },
		{ "action", "Attack / Action" },
		{ "dash", "Dash" },
		{ "collect", "Collect" },
		{ "consume", "Consume" },
		{ "interact", "Interact" },
		{ "pause", "Pause" }
	};

	public static void EnsureBindings(Dictionary<string, Variant> gameData)
	{
		var defaults = CreateDefaultBindingsVariant();
		Dictionary mergedBindings = defaults.Duplicate(true);

		if (gameData.TryGetValue(InputBindingsKey, out Variant bindingsVariant) &&
			bindingsVariant.VariantType == Variant.Type.Dictionary)
		{
			Dictionary storedBindings = bindingsVariant.AsGodotDictionary();
			foreach (string action in BindableActions)
			{
				if (!storedBindings.ContainsKey(action))
					continue;

				mergedBindings[action] = SanitizeEventArray(storedBindings[action]);
			}
		}

		gameData[InputBindingsKey] = mergedBindings;
		ApplyStoredBindings(gameData);
	}

	public static void ResetBindingsToDefault(Dictionary<string, Variant> gameData)
	{
		gameData[InputBindingsKey] = CreateDefaultBindingsVariant();
		ApplyStoredBindings(gameData);
	}

	public static bool TrySetBinding(Dictionary<string, Variant> gameData, string action, int slot, InputEvent inputEvent, out string errorMessage)
	{
		errorMessage = "";

		if (!IsBindableAction(action) || inputEvent == null || slot < 0 || slot >= MaxBindingsPerAction)
		{
			errorMessage = "Invalid binding request.";
			return false;
		}

		inputEvent = NormalizeBindableEvent(inputEvent);
		if (inputEvent == null)
		{
			errorMessage = "This input type is not supported.";
			return false;
		}

		if (TryFindConflict(gameData, action, inputEvent, out string conflictAction))
		{
			errorMessage = $"{FormatInputEvent(inputEvent)} is already used by {GetActionLabel(conflictAction)}.";
			return false;
		}

		Dictionary bindings = GetBindingsDictionary(gameData);
		Array<Dictionary> eventArray = GetBindingArray(bindings, action);
		while (eventArray.Count < MaxBindingsPerAction)
			eventArray.Add(new Dictionary());

		eventArray[slot] = SerializeInputEvent(inputEvent);
		bindings[action] = eventArray;
		gameData[InputBindingsKey] = bindings;
		ApplyStoredBindings(gameData);
		return true;
	}

	public static void ApplyStoredBindings(Dictionary<string, Variant> gameData)
	{
		Dictionary bindings = GetBindingsDictionary(gameData);

		foreach (string action in BindableActions)
		{
			if (!InputMap.HasAction(action))
				InputMap.AddAction(action);

			InputMap.ActionEraseEvents(action);

			Array<Dictionary> eventArray = GetBindingArray(bindings, action);
			foreach (Dictionary eventData in eventArray)
			{
				InputEvent inputEvent = DeserializeInputEvent(eventData);
				if (inputEvent != null)
					InputMap.ActionAddEvent(action, inputEvent);
			}
		}
	}

	public static string GetActionLabel(string action)
	{
		return ActionLabels.TryGetValue(action, out string label) ? label : action;
	}

	public static string GetBindingText(Dictionary<string, Variant> gameData, string action, int slot)
	{
		Dictionary bindings = GetBindingsDictionary(gameData);
		Array<Dictionary> eventArray = GetBindingArray(bindings, action);
		if (slot < 0 || slot >= eventArray.Count)
			return "Unbound";

		InputEvent inputEvent = DeserializeInputEvent(eventArray[slot]);
		return inputEvent == null ? "Unbound" : FormatInputEvent(inputEvent);
	}

	public static string FormatInputEvent(InputEvent inputEvent)
	{
		return inputEvent switch
		{
			InputEventKey keyEvent => OS.GetKeycodeString(keyEvent.Keycode),
			InputEventMouseButton mouseEvent => $"Mouse {mouseEvent.ButtonIndex}",
			InputEventJoypadButton joypadButton => $"Pad Button {joypadButton.ButtonIndex}",
			_ => "Unknown"
		};
	}

	private static bool IsBindableAction(string action)
	{
		foreach (string bindableAction in BindableActions)
		{
			if (bindableAction == action)
				return true;
		}

		return false;
	}

	private static bool TryFindConflict(Dictionary<string, Variant> gameData, string actionToIgnore, InputEvent inputEvent, out string conflictAction)
	{
		conflictAction = "";
		Dictionary bindings = GetBindingsDictionary(gameData);

		foreach (string action in BindableActions)
		{
			if (action == actionToIgnore)
				continue;

			Array<Dictionary> eventArray = GetBindingArray(bindings, action);
			foreach (Dictionary eventData in eventArray)
			{
				InputEvent storedEvent = DeserializeInputEvent(eventData);
				if (storedEvent != null && AreEquivalent(storedEvent, inputEvent))
				{
					conflictAction = action;
					return true;
				}
			}
		}

		return false;
	}

	private static bool AreEquivalent(InputEvent left, InputEvent right)
	{
		if (left == null || right == null || left.GetType() != right.GetType())
			return false;

		return left switch
		{
			InputEventKey leftKey when right is InputEventKey rightKey => leftKey.Keycode == rightKey.Keycode,
			InputEventMouseButton leftMouse when right is InputEventMouseButton rightMouse => leftMouse.ButtonIndex == rightMouse.ButtonIndex,
			InputEventJoypadButton leftJoypad when right is InputEventJoypadButton rightJoypad => leftJoypad.ButtonIndex == rightJoypad.ButtonIndex,
			_ => false
		};
	}

	private static Dictionary CreateDefaultBindingsVariant()
	{
		return new Dictionary
		{
			{ "move_up", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.W)), SerializeInputEvent(CreateKeyEvent(Key.Up)) } },
			{ "move_down", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.S)), SerializeInputEvent(CreateKeyEvent(Key.Down)) } },
			{ "move_left", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.A)), SerializeInputEvent(CreateKeyEvent(Key.Left)) } },
			{ "move_right", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.D)), SerializeInputEvent(CreateKeyEvent(Key.Right)) } },
			{ "action", new Array<Dictionary> { SerializeInputEvent(CreateMouseButtonEvent(MouseButton.Left)), SerializeInputEvent(CreateKeyEvent(Key.X)) } },
			{ "dash", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.Space)), SerializeInputEvent(CreateKeyEvent(Key.Shift)) } },
			{ "collect", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.E)), SerializeInputEvent(CreateKeyEvent(Key.C)) } },
			{ "consume", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.G)), SerializeInputEvent(CreateKeyEvent(Key.Q)) } },
			{ "interact", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.F)), SerializeInputEvent(CreateKeyEvent(Key.Enter)) } },
			{ "pause", new Array<Dictionary> { SerializeInputEvent(CreateKeyEvent(Key.Escape)), SerializeInputEvent(CreateKeyEvent(Key.P)) } }
		};
	}

	private static Dictionary GetBindingsDictionary(Dictionary<string, Variant> gameData)
	{
		if (!gameData.TryGetValue(InputBindingsKey, out Variant bindingsVariant) ||
			bindingsVariant.VariantType != Variant.Type.Dictionary)
		{
			return CreateDefaultBindingsVariant();
		}

		return bindingsVariant.AsGodotDictionary();
	}

	private static Array<Dictionary> GetBindingArray(Dictionary bindings, string action)
	{
		if (!bindings.ContainsKey(action))
			return CreateEmptyBindingArray();

		return SanitizeEventArray(bindings[action]);
	}

	private static Array<Dictionary> SanitizeEventArray(Variant variant)
	{
		var sanitized = CreateEmptyBindingArray();
		Array sourceArray = variant.VariantType == Variant.Type.Array ? variant.AsGodotArray() : new Array();

		for (int i = 0; i < MaxBindingsPerAction && i < sourceArray.Count; i++)
		{
			if (sourceArray[i].VariantType == Variant.Type.Dictionary)
				sanitized[i] = sourceArray[i].AsGodotDictionary();
		}

		return sanitized;
	}

	private static Array<Dictionary> CreateEmptyBindingArray()
	{
		return new Array<Dictionary> { new Dictionary(), new Dictionary() };
	}

	private static InputEvent NormalizeBindableEvent(InputEvent inputEvent)
	{
		return inputEvent switch
		{
			InputEventKey keyEvent when keyEvent.Pressed && !keyEvent.Echo => CreateKeyEvent(keyEvent.Keycode),
			InputEventMouseButton mouseEvent when mouseEvent.Pressed => CreateMouseButtonEvent(mouseEvent.ButtonIndex),
			InputEventJoypadButton joypadButton when joypadButton.Pressed => CreateJoypadButtonEvent(joypadButton.ButtonIndex),
			_ => null
		};
	}

	private static Dictionary SerializeInputEvent(InputEvent inputEvent)
	{
		var dict = new Dictionary();

		switch (inputEvent)
		{
			case InputEventKey keyEvent:
				dict["type"] = "key";
				dict["keycode"] = (int)keyEvent.Keycode;
				break;
			case InputEventMouseButton mouseEvent:
				dict["type"] = "mouse_button";
				dict["button_index"] = (int)mouseEvent.ButtonIndex;
				break;
			case InputEventJoypadButton joypadButton:
				dict["type"] = "joypad_button";
				dict["button_index"] = (int)joypadButton.ButtonIndex;
				break;
		}

		return dict;
	}

	private static InputEvent DeserializeInputEvent(Dictionary eventData)
	{
		if (!eventData.ContainsKey("type"))
			return null;

		string type = eventData["type"].AsString();
		return type switch
		{
			"key" => CreateKeyEvent((Key)eventData["keycode"].AsInt32()),
			"mouse_button" => CreateMouseButtonEvent((MouseButton)eventData["button_index"].AsInt32()),
			"joypad_button" => CreateJoypadButtonEvent((JoyButton)eventData["button_index"].AsInt32()),
			_ => null
		};
	}

	private static InputEventKey CreateKeyEvent(Key key)
	{
		return new InputEventKey
		{
			Keycode = key,
			PhysicalKeycode = key
		};
	}

	private static InputEventMouseButton CreateMouseButtonEvent(MouseButton button)
	{
		return new InputEventMouseButton
		{
			ButtonIndex = button
		};
	}

	private static InputEventJoypadButton CreateJoypadButtonEvent(JoyButton button)
	{
		return new InputEventJoypadButton
		{
			ButtonIndex = button
		};
	}
}
