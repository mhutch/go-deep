using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WhatsInTheMountain
{
	public class Tunnel : DrawableGameComponent
	{
		const int tunnelDepth = 10;

		BasicEffect basicEffect;
		List<Texture2D> wallTextures = new List<Texture2D> ();
		TunnelLayer [] layers = new TunnelLayer [tunnelDepth];
		Matrix view, projection;
		Vector3 cameraPosition;
		VertexPositionTexture[] quadVertices = new VertexPositionTexture[4];

		Vector3[] unitOctagon = ComputeUnitOctagon ();
		short[] clockwiseQuadIndices = { 0, 1, 2, 2, 1, 3 };

		float tunnelOffset;

		public Tunnel (Game game) : base (game)
		{
		}

		public override void Initialize ()
		{
			float aspectRatio = 4f / 3f;
			cameraPosition = new Vector3 (0, -0.5f, 0);
			view = Matrix.CreateLookAt (cameraPosition, new Vector3 (0, -0.5f, -2), Vector3.Up);
			projection = Matrix.CreatePerspectiveFieldOfView (
				MathHelper.ToRadians (90),
				aspectRatio,
				1,
				500);

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
			//basicEffect.EnableDefaultLighting ();

			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			const int earthTextures = 2;
			for (int i = 0; i < 2; i++) {
				wallTextures.Add (Game.Content.Load<Texture2D> ("earth" + i.ToString ()));
			}

			base.LoadContent ();
		}

		public override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
		}

		public override void Draw (GameTime gameTime)
		{
			GraphicsDevice.Clear (Color.Black);

			const float depth = -1, radius = 1;

			float speed = 1.2f; //units / s
			tunnelOffset = (tunnelOffset + speed * (float)gameTime.ElapsedGameTime.TotalSeconds) %1f;

			for (int i = layers.Length - 1; i >= 0; i--) {
				var layer = layers [i];
				for (int j = 0; j < 8; j++) {
					basicEffect.Texture = wallTextures [layer.GetTextureID (j)];
					var d = i * depth + tunnelOffset;
					FillOctagonSectionVertices (quadVertices, d, d + depth, radius, j);
					foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
						pass.Apply ();
						GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture> (PrimitiveType.TriangleList, quadVertices, 0, 4, clockwiseQuadIndices, 0, 2);
					}
				}
			}

			//DOG
			float dogDistance = -5;
			RenderFlatQuad (new Vector3 (0, -0.75f, dogDistance), wallTextures[0], 0.5f, 0.5f);

			base.Draw (gameTime);
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

		void FillQuadVertices (VertexPositionTexture[] vertices, Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
		{
			// Calculate the quad corners
			var left = Vector3.Cross (normal, up);
			Vector3 uppercenter = (up * height / 2) + origin;
			var upperLeft = uppercenter + (left * width / 2);
			var upperRight = uppercenter - (left * width / 2);
			var lowerLeft = upperLeft - (up * height);
			var lowerRight = upperRight - (up * height);

			// Fill in texture coordinates to display full texture
			// on quad
			Vector2 textureUpperLeft = new Vector2 (0.0f, 0.0f);
			Vector2 textureUpperRight = new Vector2 (1.0f, 0.0f);
			Vector2 textureLowerLeft = new Vector2 (0.0f, 1.0f);
			Vector2 textureLowerRight = new Vector2 (1.0f, 1.0f);

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
			vertices[0].Position = innerStart;
			vertices[0].TextureCoordinate = textureLowerLeft;
			vertices[1].Position = outerStart;
			vertices[1].TextureCoordinate = textureUpperLeft;
			vertices[2].Position = innerEnd;
			vertices[2].TextureCoordinate = textureLowerRight;
			vertices[3].Position = outerEnd;
			vertices[3].TextureCoordinate = textureUpperRight;
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
		fixed int textureID [8];
		fixed int obstacleID [8];

		public unsafe int GetTextureID (int layer)
		{
			fixed (int *buf = textureID) {
				return buf [layer];
			}
		}

		public unsafe int GetObstacleID (int layer)
		{
			fixed (int *buf = obstacleID) {
				return buf [layer];
			}
		}
	}
}
