using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace GoDeep
{
	public class Cinematic : DrawableGameComponent, IEnumerable<Cinematic.Frame>
	{
		List<Frame> frames = new List<Frame> ();
		string musicName;
		SoundEffect music;
		BasicEffect basicEffect;

		bool startedPlaying;
		TimeSpan startTime;
		Color backgroundColor;
		SoundEffectInstance musicInstance;
		int frameIndex;
		bool loopLastFrame, loopMusic;

		VertexPositionNormalTexture[] quadVertices = new VertexPositionNormalTexture[4];
		short[] clockwiseQuadIndices = { 0, 1, 2, 2, 1, 3 };

		public Cinematic (Game game, string musicName, Color backgroundColor, bool loopLastFrame = false, bool loopMusic = false) : base (game)
		{
			this.musicName = musicName;
			this.backgroundColor = backgroundColor;
			this.loopLastFrame = loopLastFrame;
			this.loopMusic = loopMusic;
		}

		public void Add (float duration)
		{
			Add (null, duration);
		}

		public void Add (string imageName, float duration)
		{
			frames.Add (new Frame {
				ImageName = imageName,
				Duration = duration,
			});
		}

		public bool Ended { get; private set; }
		
		public bool Escaped { get; private set; }

		public void Reset ()
		{
			startedPlaying = Ended = Escaped = false;
		}

		public new DogGame Game { get { return (DogGame) base.Game; } }

		public override void Initialize ()
		{
			var fov = 90;
			float aspectRatio = 4f / 3f;

			var view = Matrix.CreateLookAt (Vector3.Zero, Vector3.Forward, Vector3.Up);
			var projection = Matrix.CreatePerspectiveFieldOfView (
				MathHelper.ToRadians (fov),
				aspectRatio,
				0.001f,
				1f);

			basicEffect = new BasicEffect (GraphicsDevice) {
				World = Matrix.Identity,
				View = view,
				Projection = projection,
				TextureEnabled = true
			};

			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			if (musicName != null) {
				music = Game.LoadAudio (musicName);
			}
			foreach (var frame in frames) {
				if (frame.ImageName != null) {
					frame.Image = Game.Content.Load<Texture2D> (frame.ImageName);
				}
			}

			base.LoadContent ();
		}

		public override void Update (GameTime gameTime)
		{
			if (!Enabled)
				return;

			if (!startedPlaying) {
				startedPlaying = true;
				startTime = gameTime.TotalGameTime;
				if (music != null) {
					musicInstance = music.CreateInstance ();
					musicInstance.Volume = 0.7f;
					if (loopMusic)
						musicInstance.IsLooped = true;
					musicInstance.Play ();
				}
			}

			var elapsedSeconds = (gameTime.TotalGameTime - startTime).TotalSeconds;
			frameIndex = -1;

			KeyboardState ks = Keyboard.GetState ();
			if ((ks.IsKeyDown (Keys.Space) || ks.IsKeyDown (Keys.Escape)) && elapsedSeconds > 0.5f) {
				Ended = true;
				Escaped = ks.IsKeyDown (Keys.Escape);
				musicInstance.Stop ();
			}

			float end = 0;
			for (int i = 0; i < frames.Count; i++) {
				float start = end;
				end += frames [i].Duration;
				if (elapsedSeconds >= start && elapsedSeconds < end) {
					frameIndex = i;
					break;
				}
			}

			if (frameIndex == -1) {
				if (loopLastFrame) {
					frameIndex = frames.Count - 1;
				} else if (musicInstance.State == SoundState.Stopped) {
					Ended = true;
				}
			}

			base.Update (gameTime);
		}

		public override void Draw (GameTime gameTime)
		{
			if (!Enabled)
				return;

			GraphicsDevice.Clear (backgroundColor);
			if (!Ended && frameIndex >= 0) {
				var image = frames [frameIndex].Image;
				if (image != null) {
					float maxH = 2f;
					float maxW = maxH * 4 / 3;
					float h = maxH;
					var w = maxH * ((float) image.Width / (float) image.Height);
					if (w > maxW) {
						w = maxW;
						h = maxW * ((float) image.Height / (float) image.Width);
					}
					RenderFlatQuad (Vector3.Forward, image, w, h);
				}
			}

			base.Draw (gameTime);
		}

		void RenderFlatQuad (Vector3 origin, Texture2D texture, float width, float height)
		{
			Tunnel.FillQuadVertices (quadVertices, origin, Vector3.Backward, Vector3.Up, width, height);
			basicEffect.Texture = texture;
			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
				pass.Apply ();
				GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture> (
					PrimitiveType.TriangleList,
					quadVertices, 0, 4,
					clockwiseQuadIndices, 0, 2);
			}
		}

		public IEnumerator<Frame> GetEnumerator ()
		{
			return frames.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return frames.GetEnumerator ();
		}

		public class Frame
		{
			public string ImageName { get; set; }
			public Texture2D Image { get; set; }
			public float Duration { get; set; }
		}
	}
}