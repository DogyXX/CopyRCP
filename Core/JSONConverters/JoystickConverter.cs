﻿using RoverControlApp.Core.Settings;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoverControlApp.Core.JSONConverters;

public class JoystickConverter : JsonConverter<Joystick>
{
	private static readonly Joystick Default = new();

	public override Joystick Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected start of an object.");

		int? roverDriveController = null;
		bool? toggleableKinematics = null;
		float? deadzone = null;
		bool? vibrateOnModeChange = null;

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				break;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected property name.");

			string propertyName = reader.GetString()!;
			reader.Read();

			switch (propertyName)
			{
				case nameof(Joystick.RoverDriveController):
					roverDriveController = reader.GetInt32();
					break;
				case nameof(Joystick.ToggleableKinematics):
					toggleableKinematics = reader.GetBoolean();
					break;
				case nameof(Joystick.Deadzone):
					deadzone = reader.GetSingle();
					break;
				case nameof(Joystick.VibrateOnModeChange):
					vibrateOnModeChange = reader.GetBoolean();
					break;
				default:
					reader.Skip();
					break;
			}
		}

		return new Joystick
		(
			roverDriveController ?? Default.RoverDriveController,
			toggleableKinematics ?? Default.ToggleableKinematics,
			deadzone ?? Default.Deadzone,
			vibrateOnModeChange ?? Default.VibrateOnModeChange
		);
	}

	public override void Write(Utf8JsonWriter writer, Joystick value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteNumber(nameof(Joystick.RoverDriveController), value.RoverDriveController);
		writer.WriteBoolean(nameof(Joystick.ToggleableKinematics), value.ToggleableKinematics);
		writer.WriteNumber(nameof(Joystick.Deadzone), value.Deadzone);
		writer.WriteBoolean(nameof(Joystick.VibrateOnModeChange), value.VibrateOnModeChange);
		writer.WriteEndObject();
	}
}