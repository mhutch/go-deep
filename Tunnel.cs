using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace GoDeep
{
	public class Tunnel : DrawableGameComponent
	{
		const float musicVolume = 0.4f;
		const float sfxVolume = 1.0f;

		const int bottomSegment = 3;
		const int tunnelDepth = 20;
		const float layerDepth = -1f, layerRadius = 1;
		const float distanceAboveFloor = 0.20f;
		const float dogDistanceAboveFloor = 0.15f;
		const float speed = 3f; //units / s
		const float rotationSpeed = 0.5f; // seconds / segment
		const float fov = 70; //degrees
		const float dogCatchupSpeed = 0.04f;

		const float bounceBackSpeed = 0.2f;
		const float bounceBackRotationSpeed = 1f;

		const float winDistance = -1.5f;
		const float loseDistance = -10f;

		BasicEffect basicEffect;
		List<Texture2D> wallTextures = new List<Texture2D> ();
		Texture2D dogTexture;
		List<Texture2D> obstacleTextures = new List<Texture2D> ();
		Vector3 lightColor = new Vector3 (1.0f, 1.0f, 1.0f);

		SoundEffect sfxHitBoulder, sfxLose, sfxPsychedelic, sfxWin, sfxRotate, sfxSpeedUp, sfxSlowDown;
		SoundEffectInstance sfxInstanceRotate, sfxInstanceHitBoulder;

		List<SoundEffect> music = new List<SoundEffect> ();
		SoundEffectInstance playingMusic;

		Matrix view, projection;
		Vector3 cameraPosition;
		VertexPositionNormalTexture[] quadVertices = new VertexPositionNormalTexture[4];

		Vector3[] unitOctagon = ComputeUnitOctagon ();
		short[] clockwiseQuadIndices = { 0, 1, 2, 2, 1, 3 };

		Random random = new Random ();

		TunnelLayer [] layers = new TunnelLayer [tunnelDepth];

		float tunnelOffset, dogAnimationOffset;
		int layerOffset;
		float dogDistance;
		int playerRotation;
		float playerRotationRemaining;
		bool bouncing;
		bool startedPlaying;

		TimeSpan startTime;

		public void Reset ()
		{
			ResetValues ();
			RegenerateTunnel ();
		}

		void ResetValues ()
		{
			dogDistance = -5;
			playerRotation = 0;
			playerRotationRemaining = 0f;
			bouncing = false;
			Ended = false;
			Won = false;
			tunnelOffset = 0;
			dogAnimationOffset = 0;
			basicEffect.World = Matrix.Identity;
			startedPlaying = false;
		}

		public Tunnel (Game game) : base (game)
		{
		}

		public override void Initialize ()
		{
			float aspectRatio = 4f / 3f;
			cameraPosition = new Vector3 (0, -(1f - distanceAboveFloor), 0);
			view = Matrix.CreateLookAt (cameraPosition, cameraPosition + 0.001f * Vector3.Forward, Vector3.Up);
			projection = Matrix.CreatePerspectiveFieldOfView (
				MathHelper.ToRadians (fov),
				aspectRatio,
				0.001f,
				tunnelDepth);

			basicEffect = new BasicEffect (GraphicsDevice) {
				World = Matrix.Identity,
				View = view,
				Projection = projection,
				TextureEnabled = true,
				FogColor = Color.Black.ToVector3 (),
				FogStart = layers.Length / 2,
				FogEnd = layers.Length - 1,
				FogEnabled = true,
			};

			basicEffect.LightingEnabled = true;
			basicEffect.DirectionalLight0.DiffuseColor = lightColor;
			basicEffect.DirectionalLight0.Enabled = true;

			GraphicsDevice.BlendState = BlendState.NonPremultiplied;

			ResetValues ();

			base.Initialize ();
		}

		public new DogGame Game { get { return (DogGame) base.Game; } }

		protected override void LoadContent ()
		{
			wallTextures.Add (Game.Content.Load<Texture2D> ("walls\\dirt2"));
			wallTextures.Add (Game.Content.Load<Texture2D> ("walls\\dirt4"));
			wallTextures.Add (Game.Content.Load<Texture2D> ("walls\\dirt5"));
			wallTextures.Add (Game.Content.Load<Texture2D> ("walls\\dirt6"));

			dogTexture = Game.Content.Load<Texture2D> ("sprites\\dog_run");
			obstacleTextures.Add (Game.Content.Load<Texture2D> ("obst\\rocks"));

			sfxHitBoulder = Game.LoadAudio ("sfx\\hit-boulder");
			sfxLose = Game.LoadAudio ("sfx\\lose");
			sfxPsychedelic = Game.LoadAudio ("sfx\\psychedelic");
			sfxWin = Game.LoadAudio ("sfx\\win");
			sfxRotate = Game.LoadAudio ("sfx\\rotate");
			sfxSpeedUp = Game.LoadAudio ("sfx\\speed-up");
			sfxSlowDown = Game.LoadAudio ("sfx\\slow-down");

			music.Add (Game.LoadAudio ("music\\song001"));
			music.Add (Game.LoadAudio ("music\\song002"));
			music.Add (Game.LoadAudio ("music\\song003"));

			RegenerateTunnel ();

			base.LoadContent ();
		}

		void RegenerateTunnel ()
		{
			for (int i = 0; i < layers.Length; i++) {
				layers [i] = GenerateLayer ();
			}
		}

		TunnelLayer GenerateLayer ()
		{
			var tl = new TunnelLayer ();
			tl.Randomize (random, 0, wallTextures.Count, 0, 1, 0.30f, 0.1f);
			return tl;
		}

		public bool Ended { get; private set; }

		public bool Won { get; private set; }

		void End (bool won)
		{
			playingMusic.Stop ();
			Ended = true;
			Won = won;
		}

		public override void Update (GameTime gameTime)
		{
			if (!Enabled || Ended)
				return;

			if (!startedPlaying) {
				startedPlaying = true;
				startTime = gameTime.TotalGameTime;
			}

			var totalPlayedSeconds = (gameTime.TotalGameTime - startTime).TotalSeconds;

			//check for rotation commands
			KeyboardState ks = Keyboard.GetState ();
			if (playerRotationRemaining == 0f) {
				if (ks.IsKeyDown (Keys.Right)) {
					Rotate (-1f);
				} else if (ks.IsKeyDown (Keys.Left)) {
					Rotate (1f);
				} else if (ks.IsKeyDown (Keys.W)) {
					End (true);
				} else if (ks.IsKeyDown (Keys.L)) {
					End (false);
				} else if (ks.IsKeyDown (Keys.Escape) && totalPlayedSeconds > 1f) {
					End (false);
				}
			}

			if (Ended)
				return;

			if (playingMusic == null || playingMusic.State == SoundState.Stopped) {
				var m = music [random.Next (0, music.Count)];
				playingMusic = m.CreateInstance ();
				playingMusic.Volume = musicVolume;
				playingMusic.Play ();
			}

			float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

			//simple collision detection, bounce off rocks
			if (playerRotationRemaining == 0f) {
				if (layers [layerOffset].GetObstacleID ((playerRotation + bottomSegment) % 8) >= 0) {
					//prefer to rotate away from adjacent obstacles if possible
					bool obstacleOnLeft = (layers [layerOffset].GetObstacleID ((playerRotation + bottomSegment + 1) % 8) >= 0);
					bool obstacleOnRight = (layers [layerOffset].GetObstacleID ((playerRotation + bottomSegment - 1) % 8) >= 0);
					if ((!obstacleOnLeft && !obstacleOnRight) || (obstacleOnLeft && obstacleOnRight)) {
						playerRotationRemaining = random.NextDouble () > 0.5? -1f : 1f;
					} else if (obstacleOnLeft) {
						playerRotationRemaining = -1f;
					} else {
						playerRotationRemaining = 1f;
					}
					bouncing = true;
					ReplaceSoundInstance (sfxHitBoulder, ref sfxInstanceHitBoulder);
					sfxInstanceHitBoulder.Volume = sfxVolume * 2f;
					sfxInstanceHitBoulder.Play ();
				}
			}

			//update tunnel offset and distance of dog
			var tunnelOffsetChange = speed * elapsedSeconds;

			if (bouncing) {
				dogDistance -= tunnelOffsetChange * dogCatchupSpeed + tunnelOffsetChange * bounceBackSpeed;
				tunnelOffset = tunnelOffset + tunnelOffsetChange * -bounceBackSpeed;
			} else {
				dogDistance += tunnelOffsetChange * dogCatchupSpeed;
				tunnelOffset = tunnelOffset + tunnelOffsetChange;
			}

			if (tunnelOffset < 0) {
				tunnelOffset = 0;
			}

			//cycle the layer offset if necessary, and fill in new layers
			if (tunnelOffset > 1f) {
				float floor = (float) Math.Floor (tunnelOffset);
				tunnelOffset = tunnelOffset - floor;
				layerOffset = (layerOffset + (int)floor) % layers.Length;

				for (int i = 1; i <= (int)floor; i++) {
					var offsetIndex = (layerOffset - i + layers.Length) % layers.Length;
					layers [offsetIndex] = GenerateLayer ();
				}
			}

			//compute rotation updates
			if (playerRotationRemaining != 0f) {
				float direction = playerRotationRemaining > 0 ? 1f : -1f;

				playerRotationRemaining -= direction * elapsedSeconds / (bouncing? bounceBackRotationSpeed : rotationSpeed);

				float rot;

				if (playerRotationRemaining * direction < 0) {
					playerRotationRemaining = 0f;
					bouncing = false;
					playerRotation += (int)direction;
					rot = playerRotation;
				} else {
					rot = playerRotation + (direction - playerRotationRemaining);
				}

				basicEffect.World = Matrix.CreateRotationZ ((float)(Math.PI * rot / 4f));// *  Matrix.CreateScale (1f, 1f, 3f);
			}

			//detect win/loss
			if (dogDistance > winDistance) {
				End (true);
			} else if (dogDistance < loseDistance) {
				End (false);
			}

			base.Update (gameTime);
		}

		void Rotate (float s)
		{
			playerRotationRemaining = s;

			ReplaceSoundInstance (sfxRotate, ref sfxInstanceRotate);
			sfxInstanceRotate.Volume = sfxVolume * 0.6f;
			sfxInstanceRotate.Pan = -s;
			sfxInstanceRotate.Play ();
		}

		public override void Draw (GameTime gameTime)
		{
			if (!Enabled || Ended)
				return;

			GraphicsDevice.Clear (Color.Black);

			for (int i = layers.Length - 1; i >= 0; i--) {
				var offsetIndex = (layerOffset + i + layers.Length) % layers.Length;
				var previousOffsetIndex = (layerOffset + i + 1 + layers.Length) % layers.Length;
				var layer = layers [offsetIndex];
				var previousLayer = layers [previousOffsetIndex];
				RenderLayer (gameTime, i, layer, previousLayer);
			}

			base.Draw (gameTime);
		}

		void RenderDog (GameTime gameTime)
		{
			int dogFrameCount = 10;
			float dogAnimationLength = 0.5f;
			//seconds
			dogAnimationOffset = (dogAnimationOffset + (float)gameTime.ElapsedGameTime.TotalSeconds / dogAnimationLength * speed) % 1f;
			int dogFrame = (int)(dogAnimationOffset * (float)dogFrameCount);
			UpdateLight (Vector3.Forward, dogDistance);
			RenderAnimatedFlatQuad (new Vector3 (0, -(1f - dogDistanceAboveFloor), dogDistance), dogTexture, 0.5f, 0.5f, dogFrameCount, dogFrame);
		}

		void RenderLayer (GameTime gameTime, int depthIndex, TunnelLayer layer, TunnelLayer previousLayer)
		{
			var d = (depthIndex - tunnelOffset) * layerDepth;

			for (int i = 0; i < 8; i++) {
				FillOctagonSectionVertices (quadVertices, d, d + layerDepth, layerRadius, i, layer, previousLayer);
				var lightDirection = quadVertices [0].Position;
				lightDirection.Normalize ();
				UpdateLight (Vector3.Transform (lightDirection, basicEffect.World), d);
				basicEffect.Texture = wallTextures [layer.GetTextureID (i)];

				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
					pass.Apply ();
					GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture> (
						PrimitiveType.TriangleList,
						quadVertices, 0, 4,
						clockwiseQuadIndices, 0, 2);
				}

				bool dogInLayer = i == bottomSegment && d >= dogDistance && (d + layerDepth) < dogDistance;

				var ob = layer.GetObstacleID (i);
				if (ob < 0 && !dogInLayer) {
					continue;
				}

				// obstacles's origin is average of the section it's on
				Vector3 obstacleOrigin = new Vector3 ();
				for (int j = 0; j < quadVertices.Length; j++) {
					obstacleOrigin += quadVertices [j].Position;
				}
				obstacleOrigin /= quadVertices.Length;

				const float obstacleSize = 0.4f;

				//lifted off it a little
				obstacleOrigin += quadVertices [0].Normal * obstacleSize / 1.5f;

				if (dogInLayer && dogDistance < obstacleOrigin.Z) {
					RenderDog (gameTime);
				}

				if (ob >= 0) {
					UpdateLight (Vector3.Forward, d);
					FillQuadVertices (quadVertices, obstacleOrigin, Vector3.Backward, quadVertices [0].Normal, obstacleSize, obstacleSize);
					basicEffect.Texture = obstacleTextures [ob];
					foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
						pass.Apply ();
						GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture> (
							PrimitiveType.TriangleList,
							quadVertices, 0, 4,
							clockwiseQuadIndices, 0, 2);
					}
				}

				if (dogInLayer && dogDistance >= obstacleOrigin.Z) {
					RenderDog (gameTime);
				}
			}
		}

		void UpdateLight (Vector3 direction, float distance)
		{
			//user a softer fall-off than 1/r^2
			basicEffect.DirectionalLight0.Direction = direction;
			basicEffect.DirectionalLight0.DiffuseColor = lightColor / Math.Max (1,  0.3f * Math.Abs (distance));
		}

		void RenderFlatQuad (Vector3 origin, Texture2D texture, float width, float height)
		{
			FillQuadVertices (quadVertices, origin, Vector3.Backward, Vector3.Up, width, height);
			basicEffect.Texture = texture;
			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
				pass.Apply ();
				GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture> (
					PrimitiveType.TriangleList,
					quadVertices, 0, 4,
					clockwiseQuadIndices, 0, 2);
			}
		}

		void RenderAnimatedFlatQuad (Vector3 origin, Texture2D texture, float width, float height, int frameCount, int frame)
		{
			float frameWidth = 1f / frameCount;
			float xTextureStart = frameWidth * frame;
			float xTextureEnd = xTextureStart + frameWidth;

			basicEffect.Texture = texture;
			FillQuadVertices (quadVertices, origin, Vector3.Backward, Vector3.Up, width, height, xTextureStart, xTextureEnd);
			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
				pass.Apply ();
				GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture> (
					PrimitiveType.TriangleList,
					quadVertices, 0, 4,
					clockwiseQuadIndices, 0, 2);
			}
		}

		public static void FillQuadVertices (
			VertexPositionNormalTexture[] vertices, Vector3 origin, Vector3 normal, Vector3 up,
			float width, float height,
			float xTextureStart = 0f, float xTextureEnd = 1f)
		{
			// Calculate the quad corners
			var left = Vector3.Cross (normal, up);
			Vector3 uppercenter = (up * height / 2) + origin;
			var upperLeft = uppercenter + (left * width / 2);
			var upperRight = uppercenter - (left * width / 2);
			var lowerLeft = upperLeft - (up * height);
			var lowerRight = upperRight - (up * height);

			Vector2 textureUpperLeft = new Vector2 (xTextureStart, 0.0f);
			Vector2 textureUpperRight = new Vector2 (xTextureEnd, 0.0f);
			Vector2 textureLowerLeft = new Vector2 (xTextureStart, 1.0f);
			Vector2 textureLowerRight = new Vector2 (xTextureEnd, 1.0f);

			for (int i = 0; i < vertices.Length; i++) {
				vertices [i].Normal = normal;
			}

			// Set the position and texture coordinate for each
			// vertex
			vertices[0].Position = lowerLeft;
			vertices[0].TextureCoordinate = textureLowerLeft;
			vertices[1].Position = upperLeft;
			vertices[1].TextureCoordinate = textureUpperLeft;
			vertices[2].Position = lowerRight;
			vertices[2].TextureCoordinate = textureLowerRight;
			vertices[3].Position = upperRight;
			vertices[3].TextureCoordinate = textureUpperRight;
		}

		static Vector3[] ComputeUnitOctagon ()
		{
			var coords = new Vector3[8];

			float sinHalfAngle = (float) Math.Sin (Math.PI / 8);

			coords[0] = new Vector3 ( sinHalfAngle,  1, 0);
			coords[1] = new Vector3 ( 1,  sinHalfAngle, 0);
			coords[2] = new Vector3 ( 1, -sinHalfAngle, 0);
			coords[3] = new Vector3 ( sinHalfAngle, -1, 0);
			coords[4] = new Vector3 (-sinHalfAngle, -1, 0);
			coords[5] = new Vector3 (-1, -sinHalfAngle, 0);
			coords[6] = new Vector3 (-1,  sinHalfAngle, 0);
			coords[7] = new Vector3 (-sinHalfAngle,  1, 0);

			return coords;
		}

		void FillOctagonSectionVertices (
			VertexPositionNormalTexture[] vertices,
			float startDepth, float endDepth, float radius, int index,
			TunnelLayer layer, TunnelLayer previousLayer)
		{
			int iNext = (index + 1 + 8) % 8;

			Vector3 outerDepth = new Vector3 (0, 0, startDepth);
			Vector3 innerDepth = new Vector3 (0, 0, endDepth);
			Vector3 startXY = unitOctagon [index] * radius;
			Vector3 endXY = unitOctagon [iNext] * radius;

			Vector3 outerStart = startXY + outerDepth + layer.GetCornerOffset (index);
			Vector3 outerEnd = endXY + outerDepth + layer.GetCornerOffset (iNext);
			Vector3 innerStart = startXY + innerDepth + previousLayer.GetCornerOffset (index);
			Vector3 innerEnd   = endXY + innerDepth + previousLayer.GetCornerOffset (iNext);

			Vector3 normal = Vector3.Cross (startXY - endXY, -Vector3.UnitZ);
			normal.Normalize ();

			// Fill in texture coordinates to display full texture
			// on quad
			Vector2 textureUpperLeft = new Vector2 (0.0f, 0.0f);
			Vector2 textureUpperRight = new Vector2 (1.0f, 0.0f);
			Vector2 textureLowerLeft = new Vector2 (0.0f, 1.0f);
			Vector2 textureLowerRight = new Vector2 (1.0f, 1.0f);

			// Set the position and texture coordinate for each
			// vertex
			vertices [0].Position = innerStart;
			vertices [0].TextureCoordinate = textureLowerLeft;
			vertices [1].Position = outerStart;
			vertices [1].TextureCoordinate = textureUpperLeft;
			vertices [2].Position = innerEnd;
			vertices [2].TextureCoordinate = textureLowerRight;
			vertices [3].Position = outerEnd;
			vertices [3].TextureCoordinate = textureUpperRight;

			for (int i = 0; i < vertices.Length; i++) {
				vertices [i].Normal = normal;
			}
		}

		void PrintVertexCoords ()
		{
			foreach (var coord in quadVertices) {
				Console.Write ("{0}, ", coord.Position);
			}
			Console.WriteLine ();
		}

		SoundEffectInstance ReplaceSoundInstance (SoundEffect effect, ref SoundEffectInstance instance)
		{
			if (instance != null && instance.State == SoundState.Playing) {
				instance.Stop ();
				instance.Dispose ();
			}
			instance = effect.CreateInstance ();
			return instance;
		}
	}
}
