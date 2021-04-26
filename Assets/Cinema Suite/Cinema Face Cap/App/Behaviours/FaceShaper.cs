using System.Collections.Generic;
using UnityEngine;

namespace CinemaSuite.CinemaFaceCap.App.Behaviours
{
    /// <summary>
    /// A behaviour that allows for updating blend shape weights.
    /// </summary>
    [ExecuteInEditMode]
    public class FaceShaper : MonoBehaviour
    {
        public class BlendShapeWeight
        {
            public string key;
            //public int index;
            public float value;
        }

        private Transform orientationTarget;
        private string orientationPath = string.Empty;
        private Quaternion defaultLocalOrientation;

        private SkinnedMeshRenderer skinMeshRenderer;
        private string skinMeshPath = string.Empty;

        private SkinnedMeshRenderer SkinMeshRenderer
        {
            get
            {
                if (skinMeshRenderer != null)
                {
                    return skinMeshRenderer;
                }
                else
                {
                    SetSkinnedMeshRendererPath(skinMeshPath);
                    return skinMeshRenderer;
                }
            }
        }

        private Transform OrientationTarget
        {
            get
            {
                if (orientationTarget != null)
                {
                    return orientationTarget;
                }
                else
                {
                    SetOrientationNodePath(orientationPath);
                    return orientationTarget;
                }
            }
        }

        /// <summary>
        /// Set the path of the node that contains the SkinnedMeshRenderer component.
        /// </summary>
        /// <param name="path">A path delimited by '.' (example: spine.neck.head.face)</param>
        public void SetSkinnedMeshRendererPath(string path)
        {
            skinMeshPath = path;
            if (string.IsNullOrEmpty(path))
            {
                skinMeshRenderer = transform.GetComponent<SkinnedMeshRenderer>();
                return;
            }

            var children = path.Split('.');
            Transform temp = this.transform;

            foreach (var child in children)
            {
                temp = temp.Find(child);
            }

            skinMeshRenderer = temp.GetComponent<SkinnedMeshRenderer>();
        }

        /// <summary>
        /// Set the path of the node you wish to update orientation on.
        /// </summary>
        /// <param name="path">A path delimited by '.' (example: "spine.neck.head")</param>
        public void SetOrientationNodePath(string path)
        {
            orientationPath = path;
            if (string.IsNullOrEmpty(path))
            {
                orientationTarget = this.transform;
                return;
            }

            var children = path.Split('.');
            Transform temp = this.transform;

            foreach (var child in children)
            {
                temp = temp.Find(child);
            }

            defaultLocalOrientation = temp.localRotation;
            orientationTarget = temp;
        }

        /// <summary>
        /// Set blend shape weights and rotation
        /// </summary>
        /// <param name="rotation">The rotation to apply.</param>
        /// <param name="weights">The list of weights to update.</param>
        public void SetValues(Quaternion rotation, List<BlendShapeWeight> weights)
        {
            OrientationTarget.localRotation = defaultLocalOrientation;
            OrientationTarget.rotation = rotation * OrientationTarget.rotation;
            foreach (var weight in weights)
            {
                int index = SkinMeshRenderer.sharedMesh.GetBlendShapeIndex(weight.key);
                SkinMeshRenderer.SetBlendShapeWeight(index, weight.value);
            }
        }
    }
}
