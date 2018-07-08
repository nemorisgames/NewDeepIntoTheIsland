using UnityEngine;

namespace AC
{

	public static class ACDebug
	{

		private static string hr = "\n\n -> AC debug logger";


		public static void Log (object message, UnityEngine.Object context = null)
		{
			if (CanDisplay (true))
			{
				Debug.Log (message + hr, context);
			}
		}


		public static void LogWarning (object message, UnityEngine.Object context = null)
		{
			if (CanDisplay ())
			{
				Debug.LogWarning (message + hr, context);
			}
		}


		public static void LogError (object message, UnityEngine.Object context = null)
		{
			if (CanDisplay ())
			{
				Debug.LogError (message + hr, context);
			}
		}


		private static bool CanDisplay (bool isInfo = false)
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return true;
			}
			#endif
			if (KickStarter.settingsManager != null)
			{
				switch (KickStarter.settingsManager.showDebugLogs)
				{
				case ShowDebugLogs.Always :
					return true;

				case ShowDebugLogs.Never :
					return false;

				case ShowDebugLogs.OnlyWarningsOrErrors :
					if (!isInfo)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

	}

}