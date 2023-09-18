using UnityEngine;

namespace Com2Verse.PhysicsAssetSerialization
{
	[ExecuteInEditMode]
	public class LocationGizmo : MonoBehaviour
	{
#if UNITY_EDITOR
		private void Update()
		{
			if (transform.hideFlags != HideFlags.HideInHierarchy)
			{
				DestroyImmediate(gameObject);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position, 1);
		}
#endif
	}
}
