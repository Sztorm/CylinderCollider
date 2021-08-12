using System;
using UnityEngine;

namespace Sztorm.CylinderCollider
{
    /// <summary>
    /// Class representing an approximation of a cylinder collider using compound primitive 
    /// colliders. <see cref="CylinderCollider"/> components are only mutable in inspector in 
    /// editor mode.
    /// </summary>
    [AddComponentMenu("Physics/CylinderCollider")]
    [DisallowMultipleComponent]
    public sealed class CylinderCollider : MonoBehaviour
    {
        private const string ColliderHolderNamePrefix = "ColliderHolder_ffzTiMRQ5y_";
        private const int MinSides = 6;
        private const int MaxSides = 72;
        private const float StraightAngle = 180F;
        private static readonly Color GizmoColor = new Color32(r: 51, g: 204, b: 25, a: 255);

        [SerializeField]
        private bool isTrigger;

        [SerializeField]
        private PhysicMaterial material;

        [SerializeField]
        private Vector3 center;

        [Range(MinSides, MaxSides)]
        [SerializeField]
        private int sides = MinSides;

        [Min(0F)]
        [SerializeField]
        private float radius = 1;

        [Min(0F)]
        [SerializeField]
        private float height = 1;

        [SerializeField]
        private ColliderDirection Direction = ColliderDirection.YAxis;

        private Vector3 TransformYAxisVector(Vector3 value)
        {
            switch (Direction)
            {
                case ColliderDirection.XAxis:
                    return new Vector3(value.y, value.x, value.z);
                case ColliderDirection.YAxis:
                    return value;
                case ColliderDirection.ZAxis:
                    return new Vector3(value.x, value.z, value.y);
            }
            return value;
        }

        private Vector3 BoxColliderSize
        {
            get
            {
                float halfDegreePerCollider = StraightAngle / PrimitiveColliderCount * 0.5F;
                float diameter = radius * 2F;
                var size = new Vector3(
                    x: Math.Abs(diameter * Mathf.Tan(halfDegreePerCollider * Mathf.Deg2Rad)),
                    y: height,
                    z: diameter);

                return TransformYAxisVector(size);
            }
        }

        private Vector3 BoundsSize
        {
            get
            {
                float diameter = radius * 2F;
                var size = new Vector3(diameter, height, diameter);

                return TransformYAxisVector(size);
            }
        }

        public Vector3 DirectionVector => TransformYAxisVector(new Vector3(0F, 1F, 0F));

        public int PrimitiveColliderCount => sides / 2;

        public Bounds Bounds => new Bounds(center, BoundsSize);

        public bool IsTrigger => isTrigger;

        public PhysicMaterial Material => material;

        public Vector3 Center => center;

        public int Sides => sides;

        public float Radius => radius;

        public float Height => height;

        void OnDrawGizmosSelected()
        {
            int length = PrimitiveColliderCount;
            float degreePerCollider = StraightAngle / length;
            Vector3 size = BoxColliderSize;
            Vector3 direction = DirectionVector;
            Quaternion rotation = Quaternion.AngleAxis(degreePerCollider, direction);
            Matrix4x4 translationMatrix = Matrix4x4.Translate(center);
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotation);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.matrix *= translationMatrix;
            Gizmos.color = GizmoColor;

            for (int i = 0; i < length; i++)
            {
                Gizmos.matrix *= rotationMatrix;
                Gizmos.DrawWireCube(new Vector3(), size);
            }
        }

        private void Reset()
        {
            if(TryGetComponent(out MeshFilter meshFilter))
            {
                Bounds meshBounds = meshFilter.sharedMesh.bounds;
                Vector3 meshSize = meshBounds.size;
                height = meshSize.y;
                radius = (meshSize.x + meshSize.z) * 0.25F;
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }
            sides &= 0b_1111111_11111111_11111111_11111110;    
        }

        private bool IsValid(out int holderCount)
        {
            holderCount = 0;

            if (Application.isPlaying)
            {
                return true;
            }  
            Transform colliderHolder = transform.Find(ColliderHolderNamePrefix + holderCount);

            while (colliderHolder != null)
            {
                holderCount++;
                colliderHolder = transform.Find(ColliderHolderNamePrefix + holderCount);
            }
            bool isInvalidHolderCount = holderCount != PrimitiveColliderCount;

            if (isInvalidHolderCount || colliderHolder == null)
            {
                return false;
            }
            if (colliderHolder.TryGetComponent(out BoxCollider boxCollider))
            {
                bool isholderCenterChanged = colliderHolder.localPosition != center;
                bool isholderSizeChanged = boxCollider.bounds.size != BoundsSize;
                bool isTriggerNotEqual = boxCollider.isTrigger != this.isTrigger;
                bool isMaterialNotEqual = boxCollider.sharedMaterial != material;

                return !(isholderCenterChanged || isholderSizeChanged ||
                    isTriggerNotEqual || isMaterialNotEqual);
            }
            return true;
        }

        private void DestroyInvalidColliders(int holderCount)
        {
            for (int i = 0; i < holderCount; i++)
            {
                DestroyImmediate(transform.Find(ColliderHolderNamePrefix + i).gameObject);
            }
        }

        private void GenerateColliders()
        {
            int holderCount;

            if (IsValid(out holderCount))
            {
                return;
            }
            DestroyInvalidColliders(holderCount);

            int length = PrimitiveColliderCount;
            float degreePerCollider = StraightAngle / length;
            Vector3 size = BoxColliderSize;
            Vector3 direction = DirectionVector;

            for (int i = 0; i < length; i++)
            {
                GameObject colliderHolder = new GameObject(ColliderHolderNamePrefix + i);
                BoxCollider collider = colliderHolder.AddComponent<BoxCollider>();
                Transform holderTransform = colliderHolder.transform;
                holderTransform.parent = transform;
                holderTransform.localPosition = center;
                holderTransform.localRotation = Quaternion.AngleAxis(degreePerCollider * i, direction);
                collider.isTrigger = IsTrigger;
                collider.sharedMaterial = material;
                collider.size = size;
                colliderHolder.hideFlags = HideFlags.NotEditable;
            }
        }
    }
}