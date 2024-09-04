using Godot;
using RoverControlApp.Core.JSONConverters;
using System;
using System.Text.Json.Serialization;

namespace RoverControlApp.Core.Settings;

[JsonConverter(typeof(GeneralConverter))]
public partial class General : SettingBase, ICloneable
{

	public General()
	{
		_verboseDebug = false;
		_missionControlPosition = "20;30";
		_missionControlSize = "480;360";
		_backCaptureLength = 15000;
		_allowedAfkTime = 30;
	}

	public General(bool verboseDebug, string missionControlPosition, string missionControlSize, long backCaptureLength, int allowedAfkTime)
	{
		_verboseDebug = verboseDebug;
		_missionControlPosition = missionControlPosition;
		_missionControlSize = missionControlSize;
		_backCaptureLength = backCaptureLength;
		_allowedAfkTime = allowedAfkTime;
	}

	public object Clone()
	{
		return new General()
		{
			VerboseDebug = _verboseDebug,
			MissionControlPosition = _missionControlPosition,
			MissionControlSize = _missionControlSize,
			BackCaptureLength = _backCaptureLength,
			AllowedAfkTime = _allowedAfkTime
		};
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Check)]
	public bool VerboseDebug
	{
		get => _verboseDebug;
		set => EmitSignal_SettingChanged(ref _verboseDebug, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.String, formatData: @"-?[0-9]+;-?[0-9]+")]
	public string MissionControlPosition
	{
		get => _missionControlPosition;
		set => EmitSignal_SettingChanged(ref _missionControlPosition, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.String, formatData: @"-?[0-9]+;-?[0-9]+")]
	public string MissionControlSize
	{
		get => _missionControlSize;
		set => EmitSignal_SettingChanged(ref _missionControlSize, value);
	}

	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Range, formatData: "0;60000;100;f;l", customTooltip: "How long is history [ms]")]
	public long BackCaptureLength
	{
		get => _backCaptureLength;
		set => EmitSignal_SettingChanged(ref _backCaptureLength, value);
	}
	
	[SettingsManagerVisible(cellMode: TreeItem.TreeCellMode.Range, formatData: "0;180;1;f;i", customTooltip: "How long [secs] before you are moved to EStop")]
	public int AllowedAfkTime
	{
		get => _allowedAfkTime;
		set => EmitSignal_SettingChanged(ref _allowedAfkTime, value);
	}

	bool _verboseDebug;
	string _missionControlPosition;
	string _missionControlSize;
	long _backCaptureLength;
	int _allowedAfkTime;
}