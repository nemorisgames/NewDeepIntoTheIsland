/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"NavMeshBase.cs"
 * 
 *	A base class for NavigationMesh and NavMeshSegment
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A base class for NavigationMesh and NavMeshSegment, which control scene objects used by pathfinding algorithms.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_nav_mesh_base.html")]
	#endif
	public class NavMeshBase : MonoBehaviour
	{

		/** Disables the Renderer when the game begins */
		public bool disableRenderer = true;

		private Collider _collider;
		private MeshRenderer _meshRenderer;
		private MeshCollider _meshCollider;
		private MeshFilter _meshFilter;

		#if UNITY_5 || UNITY_2017_1_OR_NEWER
		/** If True, then Physics collisions with this GameObject's Collider will be disabled (Unity 5 only) */
		public bool ignoreCollisions = true;
		#endif


		protected void BaseAwake ()
		{
			_collider = GetComponent <Collider>();
			_meshRenderer = GetComponent <MeshRenderer>();
			_meshCollider = GetComponent <MeshCollider>();
			_meshFilter = GetComponent <MeshFilter>();

			if (disableRenderer)
			{
				Hide ();
			}

			#if !(UNITY_5 || UNITY_2017_1_OR_NEWER)
			if (_collider != null)
			{
				_collider.isTrigger = true;
			}
			#endif
		}


		private void OnEnable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void Start ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void OnDisable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Unregister (this);
		}


		/**
		 * Disables the Renderer component.
		 */
		public void Hide ()
		{
			#if UNITY_EDITOR
			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent <MeshRenderer>();
			}
			#endif

			if (_meshRenderer != null)
			{
				_meshRenderer.enabled = false;
			}
		}


		/**
		 * Enables the Renderer component.
		 * If the GameObject has both a MeshFilter and a MeshCollider, then the MeshColliders's mesh will be used by the MeshFilter.
		 */
		public void Show ()
		{
			#if UNITY_EDITOR
			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent <MeshRenderer>();
			}
			#endif

			if (_meshRenderer != null)
			{
				_meshRenderer.enabled = true;

				if (_meshFilter != null && _meshCollider != null && _meshCollider.sharedMesh)
				{
					_meshFilter.mesh = _meshCollider.sharedMesh;
				}
			}
		}


		/**
		 * Calls Physics.IgnoreCollision on all appropriate Collider combinations (Unity 5 only).
		 */
		public void IgnoreNavMeshCollisions (Collider[] allColliders = null)
		{
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			if (ignoreCollisions)
			{
				if (allColliders == null)
				{
					allColliders = FindObjectsOfType (typeof(Collider)) as Collider[];
				}

				if (_collider != null && _collider.enabled && _collider.gameObject.activeInHierarchy)
				{
					foreach (Collider otherCollider in allColliders)
					{
						if (_collider != otherCollider && !_collider.isTrigger && !otherCollider.isTrigger && otherCollider.enabled && otherCollider.gameObject.activeInHierarchy && !(_collider is TerrainCollider))
						{
							Physics.IgnoreCollision (_collider, otherCollider);
						}
					}
				}
			}
			#endif
		}


		/** The attached Collider component */
		public Collider Collider
		{
			get
			{
				return _collider;
			}
		}

	}

}
