using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Behaviours
{
    /// <summary>
    /// A behaviour that loads game objects in a Unity scene as joints
    /// which can then be used to pose the humanoid.
    /// </summary>
    [ExecuteInEditMode]
    public class HumanoidPoser : MonoBehaviour
    {
        private Vector3 startingPosition;
        private Dictionary<string, GameObject> Joints = new Dictionary<string, GameObject>();

        /// <summary>
        /// Initialize the posing behaviour by sending it a list of the joint names.
        /// It will search the hierarchy and find all the child joints.
        /// </summary>
        /// <param name="jointNames">The names of the joints.</param>
        public void Initialize(List<string> jointNames)
        {
            Stack<GameObject> nodes = new Stack<GameObject>();
            nodes.Push(this.gameObject);

            while (nodes.Count > 0)
            {
                GameObject currentNode = nodes.Pop();
                for (int i = jointNames.Count - 1; i >= 0; i--)
                {
                    string name = jointNames[i];
                    Transform transform = currentNode.transform.Find(name);

                    if (transform != null)
                    {
                        Joints.Add(name, transform.gameObject);
                        nodes.Push(transform.gameObject);
                        jointNames.RemoveAt(i);
                    }
                }
            }
        }

        public void SetWorldPosition(Vector3 position)
        {
            if (startingPosition == Vector3.zero)
            {
                startingPosition = position - this.transform.position;
            }
            this.transform.position = position - startingPosition;
        }

        /// <summary>
        /// Set the rotations of each joint.
        /// </summary>
        /// <param name="rotations">A dictionary that maps joint names to rotations.</param>
        public void SetRotations(Dictionary<string, Quaternion> rotations)
        {
            foreach (KeyValuePair<string, Quaternion> pair in rotations)
            {
                if (Joints.ContainsKey(pair.Key))
                {
                    GameObject joint = Joints[pair.Key];
                    joint.transform.localRotation = pair.Value;
                }
            }
        }
    }
}