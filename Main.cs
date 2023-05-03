using Onvif.Core.Client.Common;
using Onvif.Core.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverControlApp
{
	public class MainClass
	{
		public static async Task Main(string[] args)
		{
			Console.WriteLine("JD");

			var account = new Account("192.168.1.35", "admin", "admin");
			var camera = Camera.Create(account, ex =>
			{
				// exception
			});
			await camera.Ptz.StopAsync(camera.Profile.token, true, true);

			while (true)
			{
				//move...
				//var vector1 = new PTZVector { PanTilt = new Vector2D { x = 1f, y = 0f } };
				//var speed1 = new PTZSpeed { PanTilt = new Vector2D { x = 0.1f, y = 0f } };
				//await camera.Ptz.AbsoluteMoveAsync(camera.Profile.token, vector1, speed1);

				//await camera.MoveAsync(MoveType.Absolute, vector1, speed1, 0);

				//zoom...
				//var vector2 = new PTZVector { Zoom = new Vector1D { x = 1f } };
				//var speed2 = new PTZSpeed { Zoom = new Vector1D { x = 1f } };
				//await camera.MoveAsync(MoveType.Absolute, vector2, speed2, 0);

				//await camera.Ptz.StopAsync(camera.Profile.token, true, true);
				//focus...
				//var focusMove = new FocusMove { Continuous = new ContinuousFocus() { Speed = 1f} };
				//await camera.FocusAsync(focusMove);
			}
		}


	}
}
