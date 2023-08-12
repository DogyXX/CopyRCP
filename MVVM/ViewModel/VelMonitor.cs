using Godot;
using MQTTnet;
using MQTTnet.Internal;
using OpenCvSharp;
using RoverControlApp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoverControlApp.MVVM.ViewModel;
public partial class VelMonitor : Panel
{
	const int ITEMS = 6;

	[ExportGroup("Settings")]
	[Export]
	string headStr = "ID ";
	[ExportGroup("Settings")]
	[Export]
	string angvelStr = "AngVel: ";
	string steerangStr = "SteerAng: ";

	[ExportGroup("Settings")]
	[Export]
	float SliderMaxVal = 5;
	[ExportGroup("Settings")]
	[Export]
	float SliderMinVal = -5;

	[ExportGroup("NodePaths")]
	[Export]
	NodePath[] headLabs_NodePaths = new NodePath[6];
	[ExportGroup("NodePaths")]
	[Export]
	NodePath[] dataLabs_NodePaths = new NodePath[6];
	[ExportGroup("NodePaths")]
	[Export]
	NodePath[] sliders_NodePaths = new NodePath[6];

	Dictionary<int, int> idSettings = new()
	{
		{ 1, 1 },
		{ 2, 0 },
		{ 3, 3 },
		{ 4, 2 },
		{ 5, 5 },
		{ 6, 4 },
	};

	Label[] headLabs;
	Label[] dataLabs;
	SliderController[] sliderControllers;

	private bool LenCheck()
	{
		return headLabs_NodePaths.Length == 6 && dataLabs_NodePaths.Length == 6 && sliders_NodePaths.Length == 6 && idSettings.Count == 6;
	}

	public override void _Ready()
	{
		if (!LenCheck())
			throw new Exception("Array lenght missmath!");

		headLabs = new Label[ITEMS];
		dataLabs = new Label[ITEMS];
		sliderControllers = new SliderController[ITEMS];
		for (int i = 0; i < ITEMS; i++)
		{
			headLabs[i] = GetNode<Label>(headLabs_NodePaths[i]);
			dataLabs[i] = GetNode<Label>(dataLabs_NodePaths[i]);

			var keyOfValue = idSettings.First(kvp => kvp.Value == i).Key;

			headLabs[i].Text = headStr + keyOfValue.ToString();
			dataLabs[i].Text = angvelStr + "N/A";

			sliderControllers[i] = GetNode<SliderController>(sliders_NodePaths[i]);
			sliderControllers[i].InputMinValue(SliderMinVal);
			sliderControllers[i].InputMaxValue(SliderMaxVal);
		}
	}

	struct SingleWheel
	{
		public UInt32 id;
		public float angleVelocity;
		public float steerAngle;

		public SingleWheel(uint id, float angleVelocity, float steerAngle)
		{
			this.id = id;
			this.angleVelocity = angleVelocity;
			this.steerAngle = steerAngle;
		}

		public static unsafe SingleWheel FromBytes(byte* rawdata)
		{
			byte* idPtr = &rawdata[0];
			byte* angleVelocityPtr = &rawdata[4];
			byte* steerAnglePtr = &rawdata[8];

			return new SingleWheel(*(UInt32*)idPtr, *(float*)angleVelocityPtr, *(float*)steerAnglePtr);
		}
	}

	public Task MqttSubscriber(string subTopic, MqttApplicationMessage? msg)
	{

		if (MainViewModel.Settings?.Settings?.Mqtt.TopicWheelFeedback is null || subTopic != MainViewModel.Settings?.Settings?.Mqtt.TopicWheelFeedback)
			return Task.CompletedTask;

		if (msg is null || msg.PayloadSegment.Count == 0)
		{
			MainViewModel.EventLogger?.LogMessage($"VelMonitor WARNING: Empty payload!");
			return Task.CompletedTask;
		}

		//base64
		//var ka = System.Text.Encoding.ASCII.GetString(msg.PayloadSegment);
		//var iat = System.Convert.FromBase64String(ka);

		try
		{
			CallDeferred(MethodName.UpdateVisual, msg.PayloadSegment.Array);
		}
		catch (Exception e)
		{
			MainViewModel.EventLogger?.LogMessage($"VelMonitor ERROR: Well.. Something went wrong");
			MainViewModel.EventLogger?.LogMessage($"VelMonitor ERROR: {e.Message}");
		}
		return Task.CompletedTask;
	}

	public unsafe void UpdateVisual(byte[]? rawdata)
	{
		if (rawdata is null)
			throw new ArgumentNullException("rawdata is null");

		if (rawdata.Length != 76)
		{
			throw new ArgumentException("rawdata.Length mismatch (!= 76)");
		}

		if (!LenCheck())
			throw new Exception("Internal array lenght missmath!");

		fixed (byte* rawdataPtr = &rawdata[0])
			for (int offset = 4; offset < 76; offset += 12)
			{
				var wheelData = SingleWheel.FromBytes(&rawdataPtr[offset]);

				var localIdx = idSettings[(int)wheelData.id];
				dataLabs[localIdx].Text = angvelStr + $"{wheelData.angleVelocity}";
				sliderControllers[localIdx].InputValue(wheelData.angleVelocity);
			}
	}
}
