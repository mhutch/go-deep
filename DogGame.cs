using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WhatsInTheMountain
{
	public class DogGame : Game
	{
		readonly GraphicsDeviceManager graphics;

		public DogGame ()
		{
			const int preferredWidth = 1024;
			const int preferredHeight = 768;

			Content.RootDirectory = "Content";

			graphics = new GraphicsDeviceManager (this) {
				PreferredBackBufferWidth = preferredWidth,
				PreferredBackBufferHeight = preferredHeight
			};
			graphics.IsFullScreen = false;

			Components.Add (new Tunnel (this));
		}

		protected override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
		}
	}
}