using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Cocos2D;

namespace WhatsInTheMountain
{
	public class DogGame : Game
	{
		readonly GraphicsDeviceManager graphics;

		public DogGame ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
			graphics.IsFullScreen = false;

			// Frame rate is 30 fps by default for Windows Phone.
			TargetElapsedTime = TimeSpan.FromTicks (333333 / 2);

			// Extend battery life under lock.
			//InactiveSleepTime = TimeSpan.FromSeconds(1);

			var application = new DogApplication (this, graphics);
			Components.Add (application);
		}

		private void ProcessBackClick ()
		{
			if (CCDirector.SharedDirector.CanPopScene) {
				CCDirector.SharedDirector.PopScene ();
			} else {
				Exit ();
			}
		}

		protected override void Update (GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
				ProcessBackClick ();
			}

			// TODO: Add your update logic here

			base.Update (gameTime);
		}
	}
}