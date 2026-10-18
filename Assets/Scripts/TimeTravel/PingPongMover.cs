using UnityEngine;

namespace TimeTravel
{
    public class PingPongMover : MonoBehaviour
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public float speed = 2.0f;
        public bool isMoving = true;

        private float progress = 0f;
        private bool movingToB = true;

        private void Update()
        {
            if (!isMoving) return;
            float dist = Vector3.Distance(pointA, pointB);
            if (dist <= 0.001f) return;

            float step = (speed / dist) * Time.deltaTime;
            if (movingToB)
            {
                progress += step;
                if (progress >= 1f)
                {
                    progress = 1f;
                    movingToB = false;
                }
            }
            else
            {
                progress -= step;
                if (progress <= 0f)
                {
                    progress = 0f;
                    movingToB = true;
                }
            }

            transform.position = Vector3.Lerp(pointA, pointB, progress);
        }
    }
}
