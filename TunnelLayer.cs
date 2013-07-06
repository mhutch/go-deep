using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WhatsInTheMountain
{

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
