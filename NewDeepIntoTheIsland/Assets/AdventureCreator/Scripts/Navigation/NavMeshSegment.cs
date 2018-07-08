/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"NavMeshSegment.cs"
 * 
 *	This script is used for the NavMeshSegment prefab, which defines
 *	the area to be baked by the Unity Navigation window.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * Controls a navigation area used by Unity Navigation-based pathfinding method.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_nav_mesh_segment.html")]
	#endif
	[AddComponentMenu("Adventure Creator/Navigation/NavMesh Segment")]
	public class NavMeshSegment : NavMeshBase
	{

		private void Awake ()
		{
			BaseAwake ();

			if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.UnityNavigation)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer) == -1)
				{
					ACDebug.LogWarning ("No 'NavMesh' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.navMeshLayer);
				}
			}
		}

	}

}