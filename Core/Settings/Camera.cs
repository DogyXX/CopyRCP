﻿using Godot;
using RoverControlApp.Core.JSONConverters;
using System;
using System.Text.Json.Serialization;

namespace RoverControlApp.Core.Settings;

[JsonConverter(typeof(CameraConverter))]
public partial class Camera : SettingBase, ICloneable
{

	public Camera()
	{
		_connectionSettings = new();
		_streamPathHD = "http://pendelcam.kip.uni-heidelberg.de/mjpg/video.mjpg";
		_streamPathSD = "http://158.58.130.148/mjpg/video.mjpg";
		_inverseAxis = false;
		_enableRtspStream = true;
		_enablePtzControl = true;
		_ptzRequestFrequency = 2.69;
		_dontRefresh = false;
	}

	public Camera(CameraConnection connectionSettings, string streamPathHD, string streamPathSD, bool inverseAxis, bool enableRtspStream, bool enablePtzControl, double ptzRequestFrequency, bool dontRefresh)
	{
		_connectionSettings = connectionSettings;
		_streamPathHD = streamPathHD;
		_streamPathSD = streamPathSD;
		_inverseAxis = inverseAxis;
		_enableRtspStream = enableRtspStream;
		_enablePtzControl = enablePtzControl;
		_ptzRequestFrequency = ptzRequestFrequency;
		_dontRefresh = dontRefresh;
	}

	public object Clone()
	{
		return new Camera()
		{
			ConnectionSettings = _connectionSettings,
			StreamPathHD = _streamPathHD,
			StreamPathSD = _streamPathSD,
			InverseAxis  = _inverseAxis,
			EnableRtspStream  = _enableRtspStream,
			EnablePtzControl  = _enablePtzControl,
			PtzRequestFrequency  = _ptzRequestFrequency,
			DontRefresh = _dontRefresh
		};
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Custom, immutableSection: true)]
	public CameraConnection ConnectionSettings
	{
		get => _connectionSettings;
		set => EmitSignal_SectionChanged(ref _connectionSettings, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.String)]
	public string StreamPathHD
	{
		get => _streamPathHD;
		set => EmitSignal_SettingChanged(ref _streamPathHD,value);
	}
	
	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.String)]
	public string StreamPathSD
	{
		get => _streamPathSD;
		set => EmitSignal_SettingChanged(ref _streamPathSD,value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Check)]
	public bool InverseAxis
	{
		get => _inverseAxis;
		set => EmitSignal_SettingChanged(ref _inverseAxis,value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Check)]
	public bool EnableRtspStream
	{
		get => _enableRtspStream;
		set => EmitSignal_SettingChanged(ref _enableRtspStream, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Check)]
	public bool EnablePtzControl
	{
		get => _enablePtzControl;
		set => EmitSignal_SettingChanged(ref _enablePtzControl, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Range, formatData: "1;4;0.01;f;d")]
	public double PtzRequestFrequency
	{
		get => _ptzRequestFrequency;
		set => EmitSignal_SettingChanged(ref _ptzRequestFrequency, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Check)]
	public bool DontRefresh
	{
		get => _dontRefresh;
		set => EmitSignal_SettingChanged(ref _dontRefresh, value);
	}
	CameraConnection _connectionSettings;
	string _streamPathHD;
	string _streamPathSD;
	bool _inverseAxis;
	bool _enableRtspStream;
	bool _enablePtzControl;
	double _ptzRequestFrequency;
	bool _dontRefresh;
}


