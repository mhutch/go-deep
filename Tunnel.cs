using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WhatsInTheMountain
{
	public class Tunnel : DrawableGameComponent
	{
		const int tunnelDepth = 10;
		const float layerDepth = -1, layerRadius = 1;

		float speed = 1.2f; //units / s
		float fov = 70; //degrees
		float distanceAboveFloor = 0.20f;

		BasicEffect basicEffect;
		List<Texture2D> wallTextures = new List<Texture2D> ();
		Texture2D dogTexture;

		Matrix view, projection;
		Vector3 cameraPosition;
		VertexPositionTexture[] quadVertices = new VertexPositionTexture[4];

		Vector3[] unitOctagon = ComputeUnitOctagon ();
		short[] clockwiseQuadIndices = { 0, 1, 2, 2, 1, 3 };

		Random random = new Random ();

		float tunnelOffset, dogAnimationOffset;
		TunnelLayer [] layers = new TunnelLayer [tunnelDepth];
		int layerOffset;

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
				FogStart = 0,
				FogEnd = layers.Length - 1,
				FogEnabled = true,
			};

			GraphicsDevice.BlendState = BlendState.NonPremultiplied;

			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			const int earthTextures = 2;
			for (int i = 0; i < 2; i++) {
				wallTextures.Add (Game.Content.Load<Texture2D> ("earth" + i));
			}
			dogTexture = Game.Content.Load<Texture2D> ("dog_run");

			for (int i = 0; i < layers.Length; i++) {
				layers [i] = GenerateLayer ();
			}

			base.LoadContent ();
		}

		TunnelLayer GenerateLayer ()
		{
			return new TunnelLayer (random, 0, wallTextures.Count, 0, 1, 0.0001f);
		}

		public override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
		}

		public override void Draw (GameTime gameTime)
		{
			GraphicsDevice.Clear (Color.Black);

			tunnelOffset = (tunnelOffset + speed * (float)gameTime.ElapsedGameTime.TotalSeconds);

			if (tunnelOffset > 1f) {
				float floor = (float) Math.Floor (tunnelOffset);
				tunnelOffset = tunnelOffset - floor;
				layerOffset = (layerOffset + (int)floor) % layers.Length;

				for (int i = 1; i <= (int)floor; i++) {
					var offsetIndex = (layerOffset - i + layers.Length) % layers.Length;
					layers [offsetIndex] = GenerateLayer ();
				}
			}

			for (int i = layers.Length - 1; i >= 0; i--) {
				var offsetIndex = (layerOffset + i + layers.Length) % layers.Length;
				var layer = layers [offsetIndex];
				RenderLayer (i, layer);
			}

			int dogFrameCount = 10;
			float dogAnimationLength = 0.5f; //seconds
			dogAnimationOffset = (dogAnimationOffset + (float)gameTime.ElapsedGameTime.TotalSeconds / dogAnimationLength * speed) %1f;
			int dogFrame = (int) (dogAnimationOffset * (float)dogFrameCount);

			float dogDistance = -5;
			RenderAnimatedFlatQuad (new Vector3 (0, - (1f - distanceAboveFloor), dogDistance), dogTexture, 0.5f, 0.5f, dogFrameCount, dogFrame);

			base.Draw (gameTime);
		}

		void RenderLayer (int depthIndex, TunnelLayer layer)
		{
			var d = depthIndex * layerDepth + tunnelOffset;
			for (int j = 0; j < 8; j++) {
				basicEffect.Texture = wallTextures [layer.GetTextureID (j)];
				FillOctagonSectionVertices (quadVertices, d, d + layerDepth, layerRadius, j);
				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
					pass.Apply ();
					GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture> (
						PrimitiveType.TriangleList,
						quadVertices, 0, 4,
						clockwiseQuadIndices, 0, 2);
				}
			}
		}

		void RenderFlatQuad (Vector3 origin, Texture2D texture, float width, float height)
		{
			FillQuadVertices (quadVertices, origin, Vector3.Backward, Vector3.Up, width, height);
			basicEffect.Texture = texture;
			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
				pass.Apply ();
				GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture> (
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
				GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture> (
					PrimitiveType.TriangleList,
					quadVertices, 0, 4,
					clockwiseQuadIndices, 0, 2);
			}
		}

		void FillQuadVertices (
			VertexPositionTexture[] vertices, Vector3 origin, Vector3 normal, Vector3 up,
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

		void FillOctagonSectionVertices (VertexPositionTexture[] vertices, float startDepth, float endDepth, float radius, int index)
		{
			int iNext = (index + 1 + 8) % 8;

			Vector3 outerDepth = new Vector3 (0, 0, startDepth);
			Vector3 innerDepth = new Vector3 (0, 0, endDepth);
			Vector3 startXY = unitOctagon [index] * radius;
			Vector3 endXY = unitOctagon [iNext] * radius;

			Vector3 outerStart = startXY + outerDepth;
			Vector3 outerEnd = endXY + outerDepth;
			Vector3 innerStart = startXY + innerDepth;
			Vector3 innerEnd   = endXY + innerDepth;

			Vector3 normal = Vector3.Cross (outerEnd - outerStart, innerEnd - outerStart);
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
		}

		void PrintVertexCoords ()
		{
			foreach (var coord in quadVertices) {
				Console.Write ("{0}, ", coord.Position);
			}
			Console.WriteLine ();
		}
	}

	public unsafe struct TunnelLayer
	{
		const int corners = 8;
		fixed int textureID [corners];
		fixed int obstacleID [corners];
		fixed float cornerOffsets [corners * 3];

		public TunnelLayer (Random random, int texMin, int texMax, int obMin, int obMax, float offsetScale)
		{
			fixed (int *buf = textureID) {
				for (int i = 0; i < corners; i++) {
					buf [i] = random.Next (texMin, texMax);
				}
			}

			fixed (int *buf = obstacleID) {
				for (int i = 0; i < corners; i++) {
					buf [i] = random.Next (obMin, obMax);
				}
			}

			fixed (int *buf = cornerOffsets) {
				for (int i = 0; i < corners; i++) {
			//		buf [i] = random.Next (texMax, texMax);
				}
			}
		}

		public unsafe int GetTextureID (int corner)
		{
			fixed (int *p = textureID) {
				return p [corner];
			}
		}

		public unsafe int GetObstacleID (int corner)
		{
			fixed (int *p = obstacleID) {
				return p [corner];
			}
		}

		public unsafe Vector3 GetCornerOffset (int corner)
		{
			fixed (float *p = cornerOffsets) {
				var p1 = p + corner * 3;
				return new Vector3 (*(p1), *(p1 + 1), *(p1 + 2));
			}
		}
	}
}
