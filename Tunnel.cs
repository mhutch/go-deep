using System;
using System.Runtime.InteropServices;

namespace WhatsInTheMountain
{
	public class Tunnel
	{
		//float playerPosition, dogPosition;

		TunnelSegment[] segments = new TunnelSegment[128];
		int lastGenerated;

		void GenerateNextSegment ()
		{
			var seg = new TunnelSegment ();

			segments [lastGenerated] = seg;
			lastGenerated = (lastGenerated + 1 + 128) % 128;
		}
	}

	public unsafe struct TunnelSegment
	{
		public float XOffset, YOffset;
		public fixed int textureID [8];
		public fixed int obstacleID [8]; 
	}
}
