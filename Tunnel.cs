using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WhatsInTheMountain
{
	public class Tunnel : DrawableGameComponent
	{
		BasicEffect basicEffect;
		List<Texture2D> wallTextures = new List<Texture2D> ();
		TunnelLayer [] layers = new TunnelLayer [32];
		Matrix view, projection;
		Vector3 cameraPosition;
		short[] clockwiseQuadIndexes = { 0, 1, 2, 2, 1, 3 };

		public Tunnel (Game game) : base (game)
		{
		}

		public override void Initialize ()
		{
			cameraPosition = new Vector3 (0, 0, 2);
			view = Matrix.CreateLookAt (cameraPosition, Vector3.Zero, Vector3.Up);
			projection = Matrix.CreatePerspectiveFieldOfView (
				MathHelper.ToRadians (70),
				GraphicsDevice.DisplayMode.AspectRatio,
				1,
				500);

			basicEffect = new BasicEffect (GraphicsDevice) {
				World = Matrix.Identity,
				View = view,
				Projection = projection,
				TextureEnabled = true
			};
			basicEffect.EnableDefaultLighting ();

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
			GraphicsDevice.Clear (Color.CornflowerBlue);
			var vertices = new VertexPositionNormalTexture[4];

			foreach (var layer in layers) {
				//foreach (var l in layer.
				FillQuadVertices (vertices, Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1);
				basicEffect.Texture = wallTextures [1];

				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
					pass.Apply ();

					GraphicsDevice.DrawUserIndexedPrimitives <VertexPositionNormalTexture> (
						PrimitiveType.TriangleList,
						vertices, 0, 4,
						clockwiseQuadIndexes, 0, 2);
				}
			}

			base.Draw (gameTime);
		}

		void FillQuadVertices (VertexPositionNormalTexture[] vertices, Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
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
			Vector2 textureUpperLeft = new Vector2( 0.0f, 0.0f );
			Vector2 textureUpperRight = new Vector2( 1.0f, 0.0f );
			Vector2 textureLowerLeft = new Vector2( 0.0f, 1.0f );
			Vector2 textureLowerRight = new Vector2( 1.0f, 1.0f );

			// Provide a normal for each vertex
			for (int i = 0; i < vertices.Length; i++) {
				vertices[i].Normal = normal;
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
	}

	public unsafe struct TunnelLayer
	{
		public fixed int TextureID [8];
		public fixed int ObstacleID [8];
	}
}
