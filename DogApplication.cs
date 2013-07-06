using Microsoft.Xna.Framework;
using Cocos2D;

namespace WhatsInTheMountain
{
	public class DogApplication : CCApplication
	{
		public DogApplication (Game game, GraphicsDeviceManager graphics)
		: base(game, graphics)
		{
			s_pSharedApplication = this;

			int preferredWidth = 1024;
			int preferredHeight = 768;

			graphics.PreferredBackBufferWidth = preferredWidth;
			graphics.PreferredBackBufferHeight = preferredHeight;

			CCDrawManager.InitializeDisplay (
				game, 
				graphics, 
				DisplayOrientation.LandscapeRight | DisplayOrientation.LandscapeLeft);

			graphics.PreferMultiSampling = false;
		}

		public override bool ApplicationDidFinishLaunching ()
		{
			//initialize director
			CCDirector pDirector = CCDirector.SharedDirector;
			pDirector.SetOpenGlView ();

			// 2D projection
			pDirector.Projection = CCDirectorProjection.Projection2D;

			//CCScene pScene = IntroLayer.Scene;

			//pDirector.RunWithScene (pScene);
			return true;
		}

		public override void ApplicationDidEnterBackground ()
		{
			CCDirector.SharedDirector.Pause ();

			//CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic = true;
		}

		public override void ApplicationWillEnterForeground ()
		{
			CCDirector.SharedDirector.Resume ();

			//CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic = false;
		}
	}
}