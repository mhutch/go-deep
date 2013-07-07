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

			intro = new Cinematic (this, "music\\cinematic.m4a", Color.Black) {
				{ "cine\\intro1", 3.5f },
				{ "cine\\intro2", 4.5f },
				{ "cine\\intro3", 3.5f },
				{ "cine\\intro4", 6f },
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