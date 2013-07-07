using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace WhatsInTheMountain
{
	public class Tunnel : DrawableGameComponent
	{
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

		float tunnelOffset, dogAnimationOffset;
		TunnelLayer [] layers = new TunnelLayer [tunnelDepth];
		int layerOffset;

		float dogDistance = -5;

		int playerRotation;
		float playerRotationRemaining;
		bool bouncing;

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

			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			wallTextures.Add (Game.Content.Load<Texture2D> ("walls\\dirt2"));

			dogTexture = Game.Content.Load<Texture2D> ("sprites\\dog_run");
			obstacleTextures.Add (Game.Content.Load<Texture2D> ("obst\\rocks"));

			sfxHitBoulder = Game.Content.Load<SoundEffect> ("sfx\\hit-boulder.m4a");
			sfxLose = Game.Content.Load<SoundEffect> ("sfx\\lose.m4a");
			sfxPsychedelic = Game.Content.Load<SoundEffect> ("sfx\\psychedelic.m4a");
			sfxWin = Game.Content.Load<SoundEffect> ("sfx\\win.m4a");
			sfxRotate = Game.Content.Load<SoundEffect> ("sfx\\rotate.m4a");
			sfxSpeedUp = Game.Content.Load<SoundEffect> ("sfx\\speed-up.m4a");
			sfxSlowDown = Game.Content.Load<SoundEffect> ("sfx\\slow-down.m4a");

			music.Add (Game.Content.Load<SoundEffect> ("music\\song001.m4a"));
			music.Add (Game.Content.Load<SoundEffect> ("music\\song002.m4a"));
			music.Add (Game.Content.Load<SoundEffect> ("music\\song003.m4a"));

			for (int i = 0; i < layers.Length; i++) {
				layers [i] = GenerateLayer ();
			}

			base.LoadContent ();
		}

		TunnelLayer GenerateLayer ()
		{
			return new TunnelLayer (random, 0, wallTextures.Count, 0, 1, 0.30f, 0.1f);
		}

		public override void Update (GameTime gameTime)
		{
			if (!Enabled)
				return;

			//check for rotation commands
			KeyboardState ks = Keyboard.GetState ();
			if (playerRotationRemaining == 0f) {
				if (ks.IsKeyDown (Keys.Right)) {
					Rotate (-1f);
				} else if (ks.IsKeyDown (Keys.Left)) {
					Rotate (1f);
				}
			}

			if (playingMusic == null || playingMusic.State == SoundState.Stopped) {
				var m = music [random.Next (0, music.Count)];
				playingMusic = m.CreateInstance ();
				playingMusic.Play ();
			}

			float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

			//simple collision detection, bounce off rocks
			if (playerRotationRemaining == 0f) {
				if (layers [layerOffset].GetObstacleID ((playerRotation + 3) % 8) >= 0) {
					//prefer to rotate away from adjacent obstacles if possible
					bool obstacleOnLeft = (layers [layerOffset].GetObstacleID ((playerRotation + 4) % 8) >= 0);
					bool obstacleOnRight = (layers [layerOffset].GetObstacleID ((playerRotation + 2) % 8) >= 0);
					if ((!obstacleOnLeft && !obstacleOnRight) || (obstacleOnLeft && obstacleOnRight)) {
						playerRotationRemaining = random.NextDouble () > 0.5? -1f : 1f;
					} else if (obstacleOnLeft) {
						playerRotationRemaining = -1f;
					} else {
						playerRotationRemaining = 1f;
					}
					bouncing = true;
					ReplaceSoundInstance (sfxHitBoulder, ref sfxInstanceHitBoulder).Play ();
				}
			}

			//update tunnel offset and distance of dog
			var tunnelOffsetChange = speed * elapsedSeconds;

			if (bouncing) {
				dogDistance -= tunnelOffsetChange * dogCatchupSpeed;
				tunnelOffset = tunnelOffset + tunnelOffsetChange * -bounceBackSpeed;
			} else {
				dogDistance += tunnelOffsetChange * dogCatchupSpeed;
				tunnelOffset = tunnelOffset + tunnelOffsetChange;
			}

			if (tunnelOffset < 0) {
				tunnelOffset = 0;
			}

			//cygle the layer offset if necessary, and fill in new layers
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
				Console.WriteLine ("YOU WIN");
			} else if (dogDistance < loseDistance) {
				Console.WriteLine ("YOU LOSE");
			}

			base.Update (gameTime);
		}

		void Rotate (float s)
		{
			playerRotationRemaining = s;

			ReplaceSoundInstance (sfxRotate, ref sfxInstanceRotate);
			sfxInstanceRotate.Volume = 0.5f;
			sfxInstanceRotate.Pan = -s;
			sfxInstanceRotate.Play ();
		}

		public override void Draw (GameTime gameTime)
		{
			if (!Enabled)
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

				bool dogInLayer = i == 0 && d >= dogDistance && (d + layerDepth) < dogDistance;

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
