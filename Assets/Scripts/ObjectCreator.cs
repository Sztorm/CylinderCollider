using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sztorm.CylinderCollider.Demo
{
    public class ObjectCreator : MonoBehaviour
    {
        [SerializeField]
        private Transform[] objects;

        [SerializeField]
        private int objectIndex;

        [SerializeField]
        private Vector3 objectCountDimensions;

        [SerializeField]
        private Vector3 origin;

        [SerializeField]
        private Vector3 distanceBetweenObjects;

        [SerializeField]
        private Vector3 objectRotation;

        private void OnValidate()
        {
            bool isObjectArrayNull = objects == null;

            if (!isObjectArrayNull)
            {
                if (objects.Length == 0)
                {
                    objects = new Transform[1];
                }
            }
            int maxIndex = isObjectArrayNull ? 0 : objects.Length - 1;

            objectIndex = Mathf.Clamp(objectIndex, 0, maxIndex);
        }

        private void Awake()
        {
            Quaternion rotation = Quaternion.Euler(objectRotation);

            for (int x = 0; x < objectCountDimensions.x; x++)
            {
                for (int y = 0; y < objectCountDimensions.y; y++)
                {
                    for (int z = 0; z < objectCountDimensions.z; z++)
                    {
                        var localPosition = new Vector3(
                            x * distanceBetweenObjects.x,
                            y * distanceBetweenObjects.y,
                            z * distanceBetweenObjects.z);
                        var position = origin + localPosition;

                        Instantiate(objects[objectIndex], position, rotation);
                    }
                }
            }
        }
    }
}