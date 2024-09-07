﻿using Godot;
using System;
using System.Collections.Generic;
using System.ServiceModel;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

namespace RoverControlApp.Core
{
	public abstract class MqttClasses
	{
		public enum ControlMode
		{
			EStop = 0,
			Rover = 1,
			Manipulator = 2,
			Autonomy = 3
		}
		public enum KinematicMode
		{
			Compatibility = 0,
			Ackermann = 1,
			Crab = 2
		}
		public enum MissionStatus
		{
			Created = 0,
			Starting = 1,
			Started = 2,
			Stopping = 3,
			Stopped = 4,
			Interrupted = 5
		}
		public enum PointType
		{
			Landmark = 0,
			Obstacle = 1,
			RemovePoint = 2,
			CreatePoly = 3,
			AddPointToPoly = 4,
			RemovePoly = 5,
		}
		public enum PhotoType
		{
			None = 0,
			Generic = 1,
			Spheric = 2
		}

		public enum SamplerDirection
		{
			Stop = 0,
			Down = 1,
			Up = 2
		}

		public enum DrillState
		{
			Stopped = 0,
			Left = 1,
			Right = 2,
			FastLeft = 3,
			FastRight = 4
		}

		public class RoverStatus
		{
			public CommunicationState CommunicationState { get; set; } = CommunicationState.Closed;
			public bool PadConnected { get; set; }
			public ControlMode ControlMode { get; set; } = ControlMode.EStop;
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public class RoverControl
		{
			public double Vel { get; set; }
			public double XAxis { get; set; }
			public double YAxis { get; set; }
			public KinematicMode Mode { get; set; } = KinematicMode.Compatibility;
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public class ManipulatorControl
		{
			public float Axis1 { get; set; } = 0;
			public float Axis2 { get; set; } = 0;
			public float Axis3 { get; set; } = 0;
			public float Axis4 { get; set; } = 0;
			public float Axis5 { get; set; } = 0;
			public float Axis6 { get; set; } = 0;
			public float Gripper { get; set; } = 0;
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			public override bool Equals(object? obj)
			{
				if (obj is not ManipulatorControl manipObj) return false;
				bool isEqual = true;
				isEqual &= Mathf.IsEqualApprox(Axis1, manipObj.Axis1, 0.005f);
				isEqual &= Mathf.IsEqualApprox(Axis2, manipObj.Axis2, 0.005f);
				isEqual &= Mathf.IsEqualApprox(Axis3, manipObj.Axis3, 0.005f);
				isEqual &= Mathf.IsEqualApprox(Axis4, manipObj.Axis4, 0.005f);
				isEqual &= Mathf.IsEqualApprox(Axis5, manipObj.Axis5, 0.005f);
				isEqual &= Mathf.IsEqualApprox(Axis6, manipObj.Axis6, 0.005f);
				isEqual &= Mathf.IsEqualApprox(Gripper, manipObj.Gripper, 0.005f);
				return isEqual;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Axis1, Axis2, Axis3, Axis4, Axis5, Axis6, Gripper);
			}
		}

		public class SamplerControl
		{
			public SamplerDirection DrillCommand { get; set; } = SamplerDirection.Stop; //todo change to movement
			public SamplerDirection PlatformCommand { get; set; } = SamplerDirection.Stop; //todo change to movement
			public DrillState DrillState { get; set; } = DrillState.Stopped; //todo change to drillaction
			public bool isContainerExtended { get; set; } = false; //todo change to extendContainer
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}
		
		public class RoverContainer
		{
			public float Axis1 { get; set; } = 0f;
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public class RoverFeedback
		{
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			public bool EStop { get; set; } = true;
			public string Status { get; set; } = string.Empty;
		}
		public class RoverMissionStatus
		{
			public MissionStatus MissionStatus { get; set; } = MissionStatus.Stopped;
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public class RoverSetPoint
		{
			public PointType PointType { get; set; }
			public string? Target { get; set; }
			public string? Description { get; set; }
			public PhotoType PhotoType { get; set; }
			public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}
		public class ActiveKmlObjects
		{
			public List<string> area { get; set; } = [];
			public List<string> poi { get; set; } = [];
			public long? Timestamp { get; set; }
		}
		public class AngularVelocity
		{
			public double x { get; set; }
			public double y { get; set; }
			public double z { get; set; }
		}

		public class LinearAcceleration
		{
			public double x { get; set; }
			public double y { get; set; }
			public double z { get; set; }
		}

		public class Orientation
		{
			public double x { get; set; }
			public double y { get; set; }
			public double z { get; set; }
			public double w { get; set; }
		}

		public class ZedImuData
		{
			public List<double> orientation_covariance { get; set; }
			public List<double> angular_velocity_covariance { get; set; }
			public List<double> linear_acceleration_covariance { get; set; }
			public AngularVelocity angular_velocity { get; set; }
			public LinearAcceleration linear_acceleration { get; set; }
			public Orientation orientation { get; set; }
			public long Timestamp { get; set; }
		}
	}
}