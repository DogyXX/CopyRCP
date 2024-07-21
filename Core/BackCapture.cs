﻿using Godot;
using RoverControlApp.MVVM.Model;
using System;
using System.Collections.Generic;

namespace RoverControlApp.Core
{
	internal class BackCapture
	{
		const string SAVE_FOLDER = "BackCapture";

		private readonly Queue<BackCaptureFrame> _history = new Queue<BackCaptureFrame>();

		/// <summary>
		/// How long frame remains in memory [ms]. 
		/// </summary>
		public long HistoryLength { get => LocalSettings.Singleton.General.BackCaptureLength; }
		/// <summary>
		/// Remove all frames from memory on save?
		/// </summary>
		public bool ClearOnSave { get; set; } = true;

		/// <summary>
		/// Niom niom niom
		/// </summary>
		public void FrameFeed(Image image)
		{
			if (HistoryLength <= 0)
				return;
			_history.Enqueue(BackCaptureFrame.FromCopy(image));
		}

		public void CleanUpHistory()
		{
			while (_history.Count > 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _history.Peek().Timestamp > HistoryLength)
			{
				_history.Dequeue().Frame.Dispose();
			}
		}

		public bool SaveHistory()
		{
			if (_history.Count == 0)
			{
				EventLogger.LogMessage("BackCapture", EventLogger.LogLevel.Error, "Save failed, no data in history!");
				return false;
			}

			string path = "user://" + SAVE_FOLDER;

			if (!DirAccess.DirExistsAbsolute(path))
			{
				var err = DirAccess.MakeDirAbsolute(path);
				if (err != Error.Ok)
				{
					EventLogger.LogMessage("BackCapture", EventLogger.LogLevel.Error, $"Creating folder \"{path}\" failed. ({err})");
					return false;
				}
			}

			path += "/" + (_history.Peek().Timestamp / 1000).ToString();

			if (!DirAccess.DirExistsAbsolute(path))
			{
				var err = DirAccess.MakeDirAbsolute(path);
				if (err != Error.Ok)
				{
					EventLogger.LogMessage("BackCapture", EventLogger.LogLevel.Error, $"Creating folder \"{path}\" failed. ({err})");
					return false;
				}
			}

			while (_history.Count > 0)
			{
				var singleFrame = _history.Dequeue();
				var err = singleFrame.Frame.SaveJpg($"{path}/{singleFrame.Timestamp}.jpg");
				if (err != Error.Ok)
					EventLogger.LogMessage("BackCapture", EventLogger.LogLevel.Error, $"Saving frame \"{singleFrame.Timestamp}\" failed. ({err})");
			}

			return true;
		}

		public class BackCaptureFrame
		{
			public long Timestamp { get; private set; }
			public Image Frame { get; private set; }

			public BackCaptureFrame(Image frame, long timestamp)
			{
				Timestamp = timestamp;
				Frame = frame;
			}

			public static BackCaptureFrame FromCopy(Image frame)
			{
				return FromCopy(frame, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
			}

			public static BackCaptureFrame FromCopy(Image frame, long timestamp)
			{
				Image frameCopy = new();
				frameCopy.CopyFrom(frame);
				return new BackCaptureFrame(frameCopy, timestamp);
			}
		}

	}
}
