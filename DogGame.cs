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
		Cinematic intro, lose, win, title;

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

			title = new Cinematic (this, "music\\title.m4a", Color.Black, true) {
				{ "cine\\titlescreen", 0f },
			};

			//18s long
			intro = new Cinematic (this, "music\\cinematic.m4a", Color.Black) {
				{ "cine\\intro1", 3.5f },
				{ "cine\\intro2", 4.5f },
				{ "cine\\intro3", 2.5f },
				{ "cine\\intro4", 7.5f },
				{ 0.0f },
			};
			intro.Enabled = false;

			//12s long
			win = new Cinematic (this, "music\\win.m4a", Color.Black) {
				{ "cine\\ending1", 3f },
				{ "cine\\ending2", 3f },
				{ "cine\\ending3", 3f },
				{ "cine\\ending4", 3f },
				{ 0.0f },
			};
			win.Enabled = false;

			//17s long
			lose = new Cinematic (this, "music\\lose.m4a", Color.Black) {
				{ "cine\\gameover", 17.0f },
			};
			lose.Enabled = false;

			tunnel = new Tunnel (this);
			tunnel.Enabled = false;

			Components.Add (title);
			Components.Add (tunnel);
			Components.Add (intro);
			Components.Add (win);
			Components.Add (lose);
		}

		protected override void Update (GameTime gameTime)
		{
			if (intro.Enabled) {
				if (intro.Ended) {
					intro.Enabled = false;
					tunnel.Reset ();
					tunnel.Enabled = true;
				}
			} else if (win.Enabled) {
				if (win.Ended) {
					win.Enabled = false;
					title.Reset ();
					title.Enabled = true;
				}
			} else if (lose.Enabled) {
				if (lose.Ended) {
					lose.Enabled = false;
					title.Reset ();
					title.Enabled = true;
				}
			} else if (tunnel.Enabled) {
				if (tunnel.Ended) {
					tunnel.Enabled = false;
					if (tunnel.Won) {
						win.Reset ();
						win.Enabled = true;
					} else {
						lose.Reset ();
						lose.Enabled = true;
					}
				}
			} else if (title.Enabled) {
				if (title.Ended) {
					title.Enabled = false;
					intro.Reset ();
					intro.Enabled = true;
				}
			}

			base.Update (gameTime);
		}
	}
}