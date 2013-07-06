using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Diagnostics;

namespace WhatsInTheMountain
{
	class Program : NSApplicationDelegate 
	{
		DogGame game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			#if DEBUG
			Debug.Listeners.Add (new TextWriterTraceListener (System.Console.Out));
			#endif

			game = new DogGame ();
			game.Run ();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}

		static void Main (string[] args)
		{
			NSApplication.Init ();

			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new Program ();
				NSApplication.Main(args);
			}

		}
	}
}