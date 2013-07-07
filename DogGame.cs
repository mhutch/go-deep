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

		Tunnel tunnel;
		Cinematic intro;

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

			intro = new Cinematic (this, null, Color.Black) {
				{ "intro1", 1f },
				{ "intro2", 1f },
				{ "intro3", 1f },
				{ "intro4", 1f },
				{ 1f },
			};

			tunnel = new Tunnel (this);
			tunnel.Enabled = false;

			Components.Add (tunnel);
			Components.Add (intro);
		}

		protected override void Update (GameTime gameTime)
		{
			if (intro.Enabled && intro.Ended) {
				intro.Enabled = false;
				tunnel.Enabled = true;
			}

			base.Update (gameTime);
		}
	}
}