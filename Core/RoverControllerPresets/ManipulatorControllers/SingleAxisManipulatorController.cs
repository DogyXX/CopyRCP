﻿using System;
using Godot;
using static RoverControlApp.Core.MqttClasses;

namespace RoverControlApp.Core.RoverControllerPresets.ManipulatorControllers;

public class SingleAxisManipulatorController : IRoverManipulatorController
{
	public ManipulatorControl CalculateMoveVector()
	{
		float velocity = Input.GetAxis("manipulator_speed_backward", "manipulator_speed_forward");
		if (Mathf.Abs(velocity) < LocalSettings.Singleton.Joystick.Deadzone) 
			velocity = 0f;

		ManipulatorControl manipulatorControl = null;
		if (Input.IsActionPressed("manipulator_axis_5") && Input.IsActionPressed("manipulator_axis_6"))
		{
			manipulatorControl = new()
			{
				Axis1 = 0f,
				Axis2 = 0f,
				Axis3 = 0f,
				Axis4 = 0f,
				Axis5 = 0f,
				Axis6 = 0f,

				Gripper = velocity
			};
		}
		else
			manipulatorControl = new()
			{
				Axis1 = Input.IsActionPressed("manipulator_axis_1") ? velocity : 0f,
				Axis2 = Input.IsActionPressed("manipulator_axis_2") ? velocity : 0f,
				Axis3 = Input.IsActionPressed("manipulator_axis_3") ? velocity : 0f,
				Axis4 = Input.IsActionPressed("manipulator_axis_4") ? velocity : 0f,
				Axis5 = Input.IsActionPressed("manipulator_axis_5") ? velocity : 0f,
				Axis6 = Input.IsActionPressed("manipulator_axis_6") ? velocity : 0f,

				Gripper = Input.IsActionPressed("manipulator_gripper") ? velocity : 0f
			};

		return manipulatorControl;
	}
}