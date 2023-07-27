﻿using System;
using Godot;
using RoverControlApp.MVVM.ViewModel;

namespace RoverControlApp.Core
{
	public abstract class RoverControllerPresets
	{
		public interface IRoverDriveController
		{
			public bool CalculateMoveVector(out MqttClasses.RoverControl roverControl);
		}

		public interface IRoverManipulatorController
		{
			public bool CalculateMoveVector(out MqttClasses.ManipulatorControl manipulatorControl);
		}

		public class ForzaLikeController : IRoverDriveController
		{
			public bool CalculateMoveVector(out MqttClasses.RoverControl roverControl)
			{
				roverControl = new MqttClasses.RoverControl();

				float velocity = Input.GetAxis("rover_move_backward", "rover_move_forward");
				velocity = Mathf.IsEqualApprox(velocity, 0f, 0.005f) ? 0 : velocity;

				float turn = Input.GetAxis("rover_move_left", "rover_move_right");
				turn = Mathf.IsEqualApprox(turn, 0f, Mathf.Max(0.1f, Convert.ToInt32(MainViewModel.Settings?.Settings?.JoyPadDeadzone))) ? 0 : turn;

				turn *= Mathf.Abs(velocity) / 2.5f; // Max turn angle: 36 deg.

				Vector2 vec = new Vector2(turn, velocity);
				float forcedX = Input.GetAxis("rover_rotate_left", "rover_rotate_right");
				if (!Mathf.IsEqualApprox(forcedX, 0f, 0.05f)) vec.X = forcedX / 2f;

				if (Input.IsActionPressed("camera_zoom_mod"))
				{
					vec.X /= 8f;
					vec.Y /= 8f;
				}

				var oldVelocity = new Vector2(Convert.ToSingle(MainViewModel.PressedKeys?.RoverMovement.ZRotAxis),
					Convert.ToSingle(MainViewModel.PressedKeys?.RoverMovement.XVelAxis));
				if (oldVelocity.IsEqualApprox(vec)) return false;


				roverControl.ZRotAxis = vec.X;
				roverControl.XVelAxis = vec.Y;
				return true;
			}
		}

		public class GoodOldGamesLikeController : IRoverDriveController
		{
			public bool CalculateMoveVector(out MqttClasses.RoverControl roverControl)
			{
				roverControl = new MqttClasses.RoverControl();

				var joyDeadZone = Convert.ToSingle(MainViewModel.Settings?.Settings?.JoyPadDeadzone);

				Vector2 velocity = Input.GetVector("rover_move_left", "rover_move_right", "rover_move_down",
					"rover_move_up", Mathf.Max(0.1f, joyDeadZone));
				// velocity = velocity.Clamp(new Vector2(-1f, -1f), new Vector2(1f, 1f));
				velocity.X = Mathf.IsEqualApprox(velocity.X, 0f, Mathf.Max(0.1f, joyDeadZone)) ? 0 : velocity.X;
				velocity.Y = Mathf.IsEqualApprox(velocity.Y, 0f, Mathf.Max(0.1f, joyDeadZone)) ? 0 : velocity.Y;
				if (Input.IsActionPressed("camera_zoom_mod"))
				{
					velocity.X /= 8f;
					velocity.Y /= 8f;
				}
				if (new Vector2(Convert.ToSingle(MainViewModel.PressedKeys?.RoverMovement.ZRotAxis),
					    Convert.ToSingle(MainViewModel.PressedKeys?.RoverMovement.XVelAxis))
				    .IsEqualApprox(velocity)) return false;


				roverControl.ZRotAxis = velocity.X;
				roverControl.XVelAxis = velocity.Y;
				return true;
			}
		}

		public class SingleAxisManipulatorController : IRoverManipulatorController
		{
			public bool CalculateMoveVector(out MqttClasses.ManipulatorControl manipulatorControl)
			{
				manipulatorControl = new();

				float velocity = Input.GetAxis("manipulator_speed_backward", "manipulator_speed_forward");

				manipulatorControl = new MqttClasses.ManipulatorControl()
				{
					Axis1 = Input.IsActionPressed("manipulator_axis_1") ? velocity : 0f,
					Axis2 = Input.IsActionPressed("manipulator_axis_2") ? velocity : 0f,
					Axis3 = Input.IsActionPressed("manipulator_axis_3") ? velocity : 0f,
					Axis4 = Input.IsActionPressed("manipulator_axis_4") ? velocity : 0f,
					Axis5 = Input.IsActionPressed("manipulator_axis_5") ? velocity : 0f,
					Gripper = Input.IsActionPressed("manipulator_gripper") ? velocity : 0f
				};

				return !manipulatorControl.Equals(MainViewModel.PressedKeys?.ManipulatorMovement);
			}
		}
	}
}