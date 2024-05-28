using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
//using OpenCvSharp;
using Emgu.CV;
using RoverControlApp.MVVM.ViewModel;
using RoverControlApp.Core;
using System.Collections.Specialized;

namespace RoverControlApp.MVVM.Model
{
	public class RtspStreamClient : IDisposable
	{
		public VideoCapture? Capture { get; private set; }
		private Image? _latestImage;
		private Mat? m;

		public event Action? OnFrameReceived;

		public Image LatestImage
		{
			get
			{
				NewFrameSaved = false;
				return _latestImage;
			}
			private set
			{
				NewFrameSaved = true;
				_latestImage = value;
			}
		}

		private volatile Stopwatch _generalPurposeStopwatch;
		public double ElapsedSecondsOnCurrentState => _generalPurposeStopwatch.Elapsed.TotalSeconds;

		public volatile bool NewFrameSaved;

		private Thread? _rtspThread;

		private string _ip;
		private int _port;
		private string _protocol;
		private string _login;
		private string _password;
		private string _pathToStreamHQ;
		private string _pathToStreamLQ;
		private int _id;

		public CommunicationState State
		{
			get => _state;
			private set
			{
				MainViewModel.EventLogger.LogMessage($"RTSP: CommunicationState update: {value}");
				_state = value;
			}
		}

		private volatile CommunicationState _state = CommunicationState.Closed;

		private CancellationTokenSource _cts;

		public RtspStreamClient(int id, string login, string password, string pathToStreamHQ, string pathToStreamLQ, string ip, string protocol = "rtsp", int port = 554)
		{
			this._ip = ip;
			this._port = port;
			this._protocol = protocol;
			this._login = login;
			this._password = password;
			this._pathToStreamHQ = pathToStreamHQ;
			this._pathToStreamLQ = pathToStreamLQ;
			this._id = id;
			_generalPurposeStopwatch = Stopwatch.StartNew();
			_cts = new CancellationTokenSource();
			_rtspThread = new Thread(ThreadWork) { IsBackground = true, Name = "RtspStream_Thread" + _id, Priority = ThreadPriority.BelowNormal };
			_rtspThread.Start();
		}

		private void ThreadWork()
		{
			MainViewModel.EventLogger.LogMessage("RTSP: Thread started");
			while (!_cts.IsCancellationRequested)
			{
				DoWork();
			}
			State = CommunicationState.Closing;
			DoWork();
		}

		public void Dispose()
		{
			MainViewModel.EventLogger.LogMessage("RTSP: Dispose called... Closing client");
			_cts.Cancel();
			_rtspThread?.Join(1000);
			_cts.Dispose();
			_rtspThread = null;
		}

		private void EndCapture()
		{
			Capture?.Release();
			Capture?.Dispose();
			Capture = null;
			m?.Dispose();
		}

		private void CreateCapture()
		{
			//OnFrameReceived?.Invoke();
			
			if (Capture != null) EndCapture();
			State = CommunicationState.Created;
			var task = Task.Run(() => Capture = new VideoCapture($"{_protocol}://{_login}:{_password}@{_ip}:{_port}{_pathToStreamHQ}"));
			//var task = Task.Run(() => Capture = new VideoCapture($"http://158.58.130.148:80/mjpg/video.mjpg"));

			m = new Mat();
			_generalPurposeStopwatch.Restart();
			State = CommunicationState.Opening;
			if (!task.Wait(TimeSpan.FromSeconds(15)) || Capture == null || !Capture.IsOpened)// || !Capture.IsOpened())
			{
				MainViewModel.EventLogger?.LogMessage($"RTSP: Connecting to camera failed after {(int)_generalPurposeStopwatch.Elapsed.TotalSeconds}s");
				State = CommunicationState.Faulted;
				EndCapture();
				return;
			}

			MainViewModel.EventLogger?.LogMessage($"RTSP: Connecting to camera succeeded in {(int)_generalPurposeStopwatch.Elapsed.TotalSeconds}s");

			Capture?.Set(Emgu.CV.CvEnum.CapProp.XiTimeout, 5000);
			//Capture?.Set(VideoCaptureProperties.XI_Timeout, 5000);
			
			Capture?.Set(Emgu.CV.CvEnum.CapProp.Buffersize, 0);
			//Capture?.Set(VideoCaptureProperties.BufferSize, 0);
			
			if(Capture != null) Capture.ExceptionMode = false; 
			//Capture?.SetExceptionMode(false);

			State = CommunicationState.Opened;
		}

		private void DoWork()
		{
			switch (State)
			{
				case CommunicationState.Created:
					State = CommunicationState.Closing;
					break;
				case CommunicationState.Opening:
					State = CommunicationState.Closing;
					break;
				case CommunicationState.Opened:

					_generalPurposeStopwatch.Restart();
					var ret = TryGrabImage();

					if (!ret || _generalPurposeStopwatch.Elapsed.TotalSeconds > 5)
					{
						MainViewModel.EventLogger?.LogMessage($"RTSP: Camera connection lost ;( Grabbing a frame took {(int)_generalPurposeStopwatch.Elapsed.TotalSeconds}s");
						State = CommunicationState.Faulted;
						EndCapture();
						return;
					}
					break;
				case CommunicationState.Closing:
					EndCapture();
					State = CommunicationState.Closed;
					break;
				case CommunicationState.Closed:
					if (!_cts.IsCancellationRequested) CreateCapture();
					break;
				case CommunicationState.Faulted:
					_generalPurposeStopwatch.Restart();
					Thread.Sleep(TimeSpan.FromSeconds(10));
					State = CommunicationState.Closed;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private System.Threading.Mutex _grabFrameMutex = new();

		public void LockGrabbingFrames()
		{
			_grabFrameMutex.WaitOne();
		}
		public void UnLockGrabbingFrames()
		{
			_grabFrameMutex.ReleaseMutex();
		}


		private byte[]? _arr;

		private bool TryGrabImage()
		{
			if (Capture == null || m == null) return false;

			try
			{
				if (!Capture.Grab()) return false;
				if (!Capture.Retrieve(m)) return false;
			}
			catch (Exception e)
			{
				MainViewModel.EventLogger?.LogMessage(e.ToString());
				return false;
			}


			CvInvoke.CvtColor(m, m, Emgu.CV.CvEnum.ColorConversion.Bgr2Rgb);
			//Cv2.CvtColor(m, m, ColorConversionCodes.BGR2RGB);

			if (_arr?.Length != m.Total * m.NumberOfChannels)
				_arr = new byte[m.Total * m.NumberOfChannels];
			//if (_arr?.Length != m.Total() * m.Channels())
			//	_arr = new byte[m.Total() * m.Channels()];

			Marshal.Copy(m.DataPointer, _arr, 0, (int)m.Total * m.NumberOfChannels);
			//Marshal.Copy(m.Data, _arr, 0, (int)m.Total() * m.Channels());

			LockGrabbingFrames();
			if (LatestImage?.GetWidth() != m.Width && LatestImage?.GetHeight() != m.Height)
				LatestImage = Image.CreateFromData(m.Width, m.Height, false, Image.Format.Rgb8, _arr);
			else
				LatestImage.SetData(m.Width, m.Height, false, Image.Format.Rgb8, _arr);
			NewFrameSaved = true;
			UnLockGrabbingFrames();

			if (MainViewModel.Settings.Settings.VerboseDebug)
				MainViewModel.EventLogger.LogMessage($"RTSP: Frame received in: {_generalPurposeStopwatch.ElapsedMilliseconds}ms");

			OnFrameReceived?.Invoke();

			return true;
		}


	}
}