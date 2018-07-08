/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"StaticObstacle.cs"
 * 
 *	This script is used to show and hide StaticObstacle prefabs in the Scene Manager
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * Used to show and hide StaticObstacle prefabs in SceneManager.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_static_obstacle.html")]
	#endif
	public class StaticObstacle : NavMeshSegment
	{
	}

}