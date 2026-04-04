using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class WaypointPath : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new();

        public int Count => waypoints.Count;

        public Vector3 GetPosition(int index)
        {
            if (index < 0 || index >= waypoints.Count)
            {
                return transform.position;
            }

            return waypoints[index].position;
        }

        public bool TryGetPosition(int index, out Vector3 position)
        {
            if (index < 0 || index >= waypoints.Count)
            {
                position = default;
                return false;
            }

            position = waypoints[index].position;
            return true;
        }

        public void SetWaypoints(List<Transform> points)
        {
            waypoints = points;
        }

        private void OnDrawGizmos()
        {
            if (waypoints.Count < 2)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (var i = 0; i < waypoints.Count - 1; i++)
            {
                var from = waypoints[i];
                var to = waypoints[i + 1];
                if (from != null && to != null)
                {
                    Gizmos.DrawLine(from.position, to.position);
                }
            }
        }
    }
}


