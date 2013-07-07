using System;
using Microsoft.Xna.Framework;

namespace WhatsInTheMountain
{

	public unsafe struct TunnelLayer
	{
		const int corners = 8;
		fixed int textureID [corners];
		fixed int obstacleID [corners];
		fixed float cornerOffsets [corners * 3];

		public TunnelLayer (Random random, int texMin, int texMax, int obMin, int obMax, float obChance, float offsetScale)
		{
			fixed (int *buf = textureID) {
				for (int i = 0; i < corners; i++) {
					buf [i] = random.Next (texMin, texMax);
				}
			}

			fixed (int *buf = obstacleID) {
				int offset = random.Next (0, corners - 1);
				for (int i = 0; i < corners; i++) {
					int offsetIdx = (offset + i) % corners;
					//avoid runs of three or more adjacent obstacles, complicates collision handling
					if (offsetIdx > 1 && buf [(offsetIdx - 1) % corners] >= 0 && buf [(offsetIdx - 2) % corners] >= 0) {
						buf [offsetIdx] = -1;
						continue;
					}

					if (random.NextDouble () <= obChance)
					    buf [offsetIdx] = random.Next (obMin, obMax);
					else
					    buf [offsetIdx] = -1;
				}
			}

			fixed (float *p = cornerOffsets) {
				var p1 = p + corners * 3 - 1;
				do {
					*p1 = (float)(random.NextDouble () - 0.5) * offsetScale;
				} while (--p1 >= p);
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
