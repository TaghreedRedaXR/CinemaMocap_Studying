using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core.Collada
{
    /// <summary>
    /// A Utility class for reading Rig Data from a COLLADA file, as well as saving animation data to an existing COLLADA.
    /// </summary>
    public class ColladaUtility
    {
        #region Strings
        private const string node = "node";
        private const string id_label = "id";
        private const string type = "type";
        private const string JOINT = "JOINT";
        #endregion

        /// <summary>
        /// Provides rig data associated with a given COLLADA file.
        /// </summary>
        /// <param name="FilePath">The filepath of the COLLADA to be opened.</param>
        /// <returns>The ColladaRigData</returns>
        public static ColladaRigData ReadRigData(string FilePath)
        {
            ColladaRigData rigData = new ColladaRigData();

            XmlDocument colladaDocument = new XmlDocument();
            colladaDocument.Load(FilePath);

            XmlNodeList visualSceneNodes = colladaDocument.GetElementsByTagName("visual_scene");
            if (visualSceneNodes.Count > 0)
            {
                // We will just use the first one we find.
                XmlNode visualScene = visualSceneNodes.Item(0);

                // Add nodes to the rig recursively.
                addJointsToRig(rigData, visualScene, null);
            }

            return rigData;
        }

        private static void addJointsToRig(ColladaRigData rig, XmlNode xmlNode, ColladaJointData parentJoint)
        {
            // Find the next child joint node.
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                bool isJoint = false;
                string id = string.Empty;

                // Look at the attributes to check if this node is a joint
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name == id_label)
                    {
                        id = attribute.Value;
                    }
                    if (attribute.Name == type && attribute.Value == JOINT)
                    {
                        isJoint = true;
                    }
                }

                if (isJoint) // found a child joint. Create the data, get the parent, add it to the rig
                {
                    ColladaJointData jointData = getColladaJointFromNode(node, id);
                    
                    Quaternion lhsRotation = QuaternionHelper.RHStoLHS(jointData.RotationVector);
                    jointData.LHSTransformationMatrix =
                        Matrix4x4.TRS(new Vector3(-jointData.Translation.x, jointData.Translation.y, jointData.Translation.z),
                        lhsRotation, Vector3.one);

                    // Get parent joint
                    if (parentJoint != null)
                    {
                        jointData.LHSWorldTransformationMatrix = parentJoint.LHSWorldTransformationMatrix * jointData.LHSTransformationMatrix;

                        rig.Add(id, parentJoint.Id, jointData);
                    }
                    else
                    {
                        rig.Add(id, jointData);
                    }

                    //  look for children joints.
                    addJointsToRig(rig, node, jointData);
                }
            }
        }

        private static ColladaJointData getColladaJointFromNode(XmlNode xmlNode, string id)
        {
            ColladaJointData jointData = new ColladaJointData(id);

            Vector3 translation = Vector3.zero;
            float xRotation = 0f;
            float yRotation = 0f;
            float zRotation = 0f;

            foreach (XmlNode child in xmlNode.ChildNodes)
            {
                foreach (XmlAttribute attribute in child.Attributes)
                {
                    if (attribute.Name == "sid" && attribute.Value == "translate")
                    {
                        string[] values = child.InnerText.Split(' ');
                        for (int i = 0; i < values.Length; i++)
                        {
                            translation[i] = float.Parse(values[i]);
                        }
                    }
                    else if (attribute.Name == "sid" && (attribute.Value == "rotateZ" || attribute.Value == "jointOrientZ"))
                    {
                        string[] values = child.InnerText.Split(' ');
                        zRotation = float.Parse(values[3]);
                    }
                    else if (attribute.Name == "sid" && (attribute.Value == "rotateY" || attribute.Value == "jointOrientY"))
                    {
                        string[] values = child.InnerText.Split(' ');
                        yRotation = float.Parse(values[3]);
                    }
                    else if (attribute.Name == "sid" && (attribute.Value == "rotateX" || attribute.Value == "jointOrientX"))
                    {
                        string[] values = child.InnerText.Split(' ');
                        xRotation = float.Parse(values[3]);
                    }
                }
            }
            jointData.Translation = translation;
            jointData.RotationVector = new Vector3(xRotation, yRotation, zRotation);
            jointData.Rotation = Quaternion.Euler(jointData.RotationVector);

            return jointData;
        }

        /// <summary>
        /// Save the animation Data
        /// </summary>
        public static void SaveAnimationData(ColladaAnimationData data, string SourceFilePath, string DestFilePath, bool isMatrix)
        {
            if (isMatrix)
            {
                SaveMatrixAnimationData(data, SourceFilePath, DestFilePath);
            }
            else
            {
                SaveComponentAnimationData(data, SourceFilePath, DestFilePath);
            }
        }

        public static void SaveComponentAnimationData(ColladaAnimationData data, string SourceFilePath, string DestFilePath)
        {
            int totalFrameCount = data.frameTimelapse.Count;
            XmlDocument colladaDocument = new XmlDocument();
            colladaDocument.Load(SourceFilePath);

            if (colladaDocument.DocumentElement == null)
            {
                // TODO: Return if document not found, gracefully.
                return;
            }

            XmlNode root = colladaDocument.DocumentElement;
            XmlNode library_animation = colladaDocument.CreateElement("library_animations");
            root.AppendChild(library_animation);

            foreach (string jointId in data.joints)
            {
                for (int j = 0; j < 6; j++)
                {
                    string values = data.jointTranslateX[jointId];
                    string prefix = "translate.X";
                    if (j == 1)
                    {
                        prefix = "translate.Y";
                        values = data.jointTranslateY[jointId];
                    }
                    else if (j == 2)
                    {
                        prefix = "translate.Z";
                        values = data.jointTranslateZ[jointId];
                    }
                    else if (j == 3)
                    {
                        prefix = "rotateX.ANGLE";
                        values = data.jointRotateX[jointId];
                    }
                    else if (j == 4)
                    {
                        prefix = "rotateY.ANGLE";
                        values = data.jointRotateY[jointId];
                    }
                    else if (j == 5)
                    {
                        prefix = "rotateZ.ANGLE";
                        values = data.jointRotateZ[jointId];
                    }
                    XmlNode animationChild = colladaDocument.CreateElement("animation");
                    library_animation.AppendChild(animationChild);

                    // Add input source node
                    XmlNode sourceInput = createComponentInputSourceElement(colladaDocument, jointId, totalFrameCount, data.frameTimelapse, prefix);
                    animationChild.AppendChild(sourceInput);

                    // Add output source node
                    XmlNode sourceOutput = createComponentOutputSourceElement(colladaDocument, jointId, totalFrameCount, values, prefix);
                    animationChild.AppendChild(sourceOutput);

                    // Add interp source node
                    XmlNode sourceInterp = createComponentInterpolationsSourceElement(colladaDocument, jointId, totalFrameCount, "LINEAR", prefix);
                    animationChild.AppendChild(sourceInterp);

                    // Add intan node?
                    // Add out-tan node?

                    // Add sampler node
                    XmlNode sampler = createComponentSamplerElement(colladaDocument, jointId, prefix);
                    animationChild.AppendChild(sampler);

                    // Add channel node
                    XmlNode channel = createComponentChannelElement(colladaDocument, jointId, prefix);
                    animationChild.AppendChild(channel);
                }
            }

            colladaDocument.Save(DestFilePath);
        }

        public static void SaveMatrixAnimationData(ColladaAnimationData data, string SourceFilePath, string DestFilePath)
        {
            int totalFrameCount = data.frameTimelapse.Count;

            XmlDocument colladaDocument = new XmlDocument();
            colladaDocument.Load(SourceFilePath);

            if (colladaDocument.DocumentElement == null)
            {
                // TODO: Return if document not found, gracefully.
                return;
            }

            XmlNode root = colladaDocument.DocumentElement;
            XmlNode library_animation = colladaDocument.CreateElement("library_animations");
            root.AppendChild(library_animation);

            for (int i = 0; i < data.joints.Count; i++)
            {
                XmlNode animationParent = colladaDocument.CreateElement("animation");
                library_animation.AppendChild(animationParent);

                XmlAttribute animationParentIdAttribute = colladaDocument.CreateAttribute("id");
                animationParentIdAttribute.Value = string.Format("{0}-anim", data.joints[i]);
                animationParent.Attributes.Append(animationParentIdAttribute);
                XmlAttribute animationParentNameAttribute = colladaDocument.CreateAttribute("name");
                animationParentNameAttribute.Value = data.joints[i];
                animationParent.Attributes.Append(animationParentNameAttribute);

                XmlNode animationChild = colladaDocument.CreateElement("animation");
                animationParent.AppendChild(animationChild);

                // Add input source node
                XmlNode sourceInput = createInputSourceElement(colladaDocument, data.joints[i], totalFrameCount, data.frameTimelapse);
                animationChild.AppendChild(sourceInput);

                // Add output source node
                XmlNode sourceOutput = createOutputSourceElement(colladaDocument, data.joints[i], totalFrameCount, data.jointValues[data.joints[i]]);
                animationChild.AppendChild(sourceOutput);

                // Add interp source node
                XmlNode sourceInterp = createInterpolationsSourceElement(colladaDocument, data.joints[i], totalFrameCount, "LINEAR");
                animationChild.AppendChild(sourceInterp);

                // Add sampler node
                XmlNode sampler = createSamplerElement(colladaDocument, data.joints[i]);
                animationChild.AppendChild(sampler);

                // Add channel node
                XmlNode channel = createChannelElement(colladaDocument, data.joints[i]);
                animationChild.AppendChild(channel);
            }

            colladaDocument.Save(DestFilePath);
        }

        #region Private

        /// <summary>
        /// Create the Channel Element for an animation element in a COLLADA file
        /// </summary>
        /// <param name="document">the XmlDocument representing the COLLADA file</param>
        /// <param name="joint">The joint to create a channel element for</param>
        /// <returns>The Channel XmlNode</returns>
        private static XmlNode createChannelElement(XmlDocument document, string joint)
        {
            XmlNode channel = document.CreateElement("channel");
            XmlAttribute channelSourceAttribute = document.CreateAttribute("source");
            channelSourceAttribute.Value = string.Format("#{0}-Matrix-animation-transform", joint);
            channel.Attributes.Append(channelSourceAttribute);
            XmlAttribute channelTargetAttribute = document.CreateAttribute("target");
            channelTargetAttribute.Value = string.Format("{0}/matrix", joint);
            channel.Attributes.Append(channelTargetAttribute);

            return channel;
        }

        /// <summary>
        /// Create the Sampler Element for an animation element in a COLLADA file
        /// </summary>
        /// <param name="document">the XmlDocument representing the COLLADA file</param>
        /// <param name="joint">The joint to create a sampler element for</param>
        /// <returns>The Sampler XmlNode</returns>
        private static XmlNode createSamplerElement(XmlDocument document, string joint)
        {
            // Add sampler node
            XmlNode sampler = document.CreateElement("sampler");
            XmlAttribute samplerIdAttribute = document.CreateAttribute("id");
            samplerIdAttribute.Value = string.Format("{0}-Matrix-animation-transform", joint);
            sampler.Attributes.Append(samplerIdAttribute);

            // Add input nodes
            XmlNode samplerInputInput = document.CreateElement("input");
            XmlAttribute samplerInputInputSemanticAttribute = document.CreateAttribute("semantic");
            samplerInputInputSemanticAttribute.Value = "INPUT";
            samplerInputInput.Attributes.Append(samplerInputInputSemanticAttribute);
            XmlAttribute samplerInputInputSourceAttribute = document.CreateAttribute("source");
            samplerInputInputSourceAttribute.Value = string.Format("#{0}-Matrix-animation-input", joint);
            samplerInputInput.Attributes.Append(samplerInputInputSourceAttribute);
            sampler.AppendChild(samplerInputInput);

            XmlNode samplerOutputInput = document.CreateElement("input");
            XmlAttribute samplerOutputInputSemanticAttribute = document.CreateAttribute("semantic");
            samplerOutputInputSemanticAttribute.Value = "OUTPUT";
            samplerOutputInput.Attributes.Append(samplerOutputInputSemanticAttribute);
            XmlAttribute samplerOuputInputSourceAttribute = document.CreateAttribute("source");
            samplerOuputInputSourceAttribute.Value = string.Format("#{0}-Matrix-animation-output-transform", joint);
            samplerOutputInput.Attributes.Append(samplerOuputInputSourceAttribute);
            sampler.AppendChild(samplerOutputInput);

            XmlNode samplerInterpInput = document.CreateElement("input");
            XmlAttribute samplerInterpInputSemanticAttribute = document.CreateAttribute("semantic");
            samplerInterpInputSemanticAttribute.Value = "INTERPOLATION";
            samplerInterpInput.Attributes.Append(samplerInterpInputSemanticAttribute);
            XmlAttribute samplerInterpInputSourceAttribute = document.CreateAttribute("source");
            samplerInterpInputSourceAttribute.Value = string.Format("#{0}-Interpolations", joint);
            samplerInterpInput.Attributes.Append(samplerInterpInputSourceAttribute);
            sampler.AppendChild(samplerInterpInput);

            return sampler;
        }

        /// <summary>
        /// Create the Interpolations Source Element for an animation element in a COLLADA file
        /// </summary>
        /// <param name="document">the XmlDocument representing the COLLADA file</param>
        /// <param name="joint">The joint to create a interpolation source element for</param>
        /// <param name="totalFrameCount">The total frame count for the animation</param>
        /// <param name="InterpMethod">The Interpolation method between keyframes. use LINEAR or BEZIER</param>
        /// <returns>The Source node for interpolations</returns>
        private static XmlNode createInterpolationsSourceElement(XmlDocument document, string joint, int totalFrameCount, string InterpMethod)
        {
            XmlNode sourceInterp = document.CreateElement("source");
            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-Interpolations", joint);
            sourceInterp.Attributes.Append(sourceIdAttribute);

            // Create name array element
            XmlNode nameArray = document.CreateElement("Name_array");
            XmlAttribute nameArrayIdAttribute = document.CreateAttribute("id");
            nameArrayIdAttribute.Value = string.Format("{0}-Interpolations-array", joint);
            nameArray.Attributes.Append(nameArrayIdAttribute);
            XmlAttribute nameArrayCountAttribute = document.CreateAttribute("count");
            nameArrayCountAttribute.Value = totalFrameCount.ToString();
            nameArray.Attributes.Append(nameArrayCountAttribute);

            string nameArrayTextNode = string.Empty;
            for (int i = 0; i < totalFrameCount; i++)
            {
                nameArrayTextNode = string.Format("{0} {1}", nameArrayTextNode, InterpMethod);
            }
            nameArray.AppendChild(document.CreateTextNode(nameArrayTextNode));

            sourceInterp.AppendChild(nameArray);

            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            sourceInterp.AppendChild(techniqueCommon);

            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");
            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-Interpolations-array", joint);
            accessor.Attributes.Append(accessorSourceAttribute);
            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            techniqueCommon.AppendChild(accessor);

            // Create param element
            XmlNode param = document.CreateElement("param");
            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "name";
            param.Attributes.Append(paramTypeAttribute);
            accessor.AppendChild(param);

            return sourceInterp;
        }

        /// <summary>
        /// Create the Input Source Element for an animation element in a COLLADA file.
        /// </summary>
        /// <param name="document">the XmlDocument representing the COLLADA file</param>
        /// <param name="joint">The joint to create an input source element for</param>
        /// <param name="totalFrameCount">The total frame count for the animation</param>
        /// <param name="frameValues">The time between frame captures</param>
        /// <returns>The source node for input</returns>
        private static XmlNode createInputSourceElement(XmlDocument document, string joint, int totalFrameCount, List<float> frameValues)
        {
            XmlNode source = document.CreateElement("source");

            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-Matrix-animation-input", joint);
            source.Attributes.Append(sourceIdAttribute);

            // Create Float array element
            XmlNode floatArray = document.CreateElement("float_array");

            XmlAttribute floatArrayIdAttribute = document.CreateAttribute("id");
            floatArrayIdAttribute.Value = string.Format("{0}-Matrix-animation-input-array", joint);
            floatArray.Attributes.Append(floatArrayIdAttribute);

            XmlAttribute floatArrayCountAttribute = document.CreateAttribute("count");
            floatArrayCountAttribute.Value = totalFrameCount.ToString();
            floatArray.Attributes.Append(floatArrayCountAttribute);

            string floatArrayTextNode = string.Empty;
            for (int i = 0; i < frameValues.Count; i++)
            {
                floatArrayTextNode = string.Format("{0} {1}", floatArrayTextNode, frameValues[i].ToString());
            }
            floatArray.AppendChild(document.CreateTextNode(floatArrayTextNode));

            source.AppendChild(floatArray);


            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            source.AppendChild(techniqueCommon);


            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");

            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-Matrix-animation-input-array", joint);
            accessor.Attributes.Append(accessorSourceAttribute);

            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            techniqueCommon.AppendChild(accessor);


            // Create param element
            XmlNode param = document.CreateElement("param");

            XmlAttribute paramNameAttribute = document.CreateAttribute("name");
            paramNameAttribute.Value = "TIME";
            param.Attributes.Append(paramNameAttribute);

            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "float";
            param.Attributes.Append(paramTypeAttribute);

            accessor.AppendChild(param);

            return source;
        }

        /// <summary>
        /// Create the Input Source Element for an animation element in a COLLADA file.
        /// </summary>
        /// <param name="document">the XmlDocument representing the COLLADA file</param>
        /// <param name="joint">The joint to create an input source element for</param>
        /// <param name="totalFrameCount">The total frame count for the animation</param>
        /// <param name="frameRate">The frame rate of the animation</param>
        /// <returns>The source node for input</returns>
        private static XmlNode createInputSourceElement(XmlDocument document, string joint, int totalFrameCount, float frameRate)
        {
            XmlNode source = document.CreateElement("source");

            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-Matrix-animation-input", joint);
            source.Attributes.Append(sourceIdAttribute);

            // Create Float array element
            XmlNode floatArray = document.CreateElement("float_array");

            XmlAttribute floatArrayIdAttribute = document.CreateAttribute("id");
            floatArrayIdAttribute.Value = string.Format("{0}-Matrix-animation-input-array", joint);
            floatArray.Attributes.Append(floatArrayIdAttribute);

            XmlAttribute floatArrayCountAttribute = document.CreateAttribute("count");
            floatArrayCountAttribute.Value = totalFrameCount.ToString();
            floatArray.Attributes.Append(floatArrayCountAttribute);

            string floatArrayTextNode = string.Empty;
            float frameRateValue = 0f;
            for (int i = 0; i < totalFrameCount; i++)
            {
                floatArrayTextNode = string.Format("{0} {1}", floatArrayTextNode, frameRateValue.ToString());
                frameRateValue += (1 / frameRate);
            }
            floatArray.AppendChild(document.CreateTextNode(floatArrayTextNode));

            source.AppendChild(floatArray);


            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            source.AppendChild(techniqueCommon);


            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");

            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-Matrix-animation-input-array", joint);
            accessor.Attributes.Append(accessorSourceAttribute);

            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            techniqueCommon.AppendChild(accessor);


            // Create param element
            XmlNode param = document.CreateElement("param");

            XmlAttribute paramNameAttribute = document.CreateAttribute("name");
            paramNameAttribute.Value = "TIME";
            param.Attributes.Append(paramNameAttribute);

            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "float";
            param.Attributes.Append(paramTypeAttribute);

            accessor.AppendChild(param);

            return source;
        }

        /// <summary>
        /// Create the Output Source Element for an animation element in a COLLADA file.
        /// </summary>
        /// <param name="document">the XmlDocument representing the COLLADA file</param>
        /// <param name="joint">The joint to create an output source element for</param>
        /// <param name="totalFrameCount">The total frame count for the animation</param>
        /// <param name="floatArrayValues">The float4x4 values for the joint transformations</param>
        /// <returns>The source node for output</returns>
        private static XmlNode createOutputSourceElement(XmlDocument document, string joint, int totalFrameCount, string floatArrayValues)
        {
            XmlNode source = document.CreateElement("source");
            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-Matrix-animation-output-transform", joint);
            source.Attributes.Append(sourceIdAttribute);


            // Create Float array element
            XmlNode floatArray = document.CreateElement("float_array");

            XmlAttribute floatArrayIdAttribute = document.CreateAttribute("id");
            floatArrayIdAttribute.Value = string.Format("{0}-Matrix-animation-output-transform-array", joint);
            floatArray.Attributes.Append(floatArrayIdAttribute);

            XmlAttribute floatArrayCountAttribute = document.CreateAttribute("count");
            floatArrayCountAttribute.Value = (totalFrameCount * 16).ToString();
            floatArray.Attributes.Append(floatArrayCountAttribute);

            floatArray.AppendChild(document.CreateTextNode(floatArrayValues));

            source.AppendChild(floatArray);


            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            source.AppendChild(techniqueCommon);


            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");

            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-Matrix-animation-output-transform-array", joint);
            accessor.Attributes.Append(accessorSourceAttribute);

            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            XmlAttribute accessorStrideAttribute = document.CreateAttribute("stride");
            accessorStrideAttribute.Value = "16";
            accessor.Attributes.Append(accessorStrideAttribute);

            techniqueCommon.AppendChild(accessor);


            // Create param element
            XmlNode param = document.CreateElement("param");

            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "float4x4";
            param.Attributes.Append(paramTypeAttribute);

            accessor.AppendChild(param);

            return source;
        }

        private static XmlNode createComponentInputSourceElement(XmlDocument document, string joint, int totalFrameCount, List<float> frameValues, string prefix)
        {
            XmlNode source = document.CreateElement("source");

            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-{1}-input", joint, prefix);
            source.Attributes.Append(sourceIdAttribute);

            // Create Float array element
            XmlNode floatArray = document.CreateElement("float_array");

            XmlAttribute floatArrayIdAttribute = document.CreateAttribute("id");
            floatArrayIdAttribute.Value = string.Format("{0}-{1}-input-array", joint, prefix);
            floatArray.Attributes.Append(floatArrayIdAttribute);

            XmlAttribute floatArrayCountAttribute = document.CreateAttribute("count");
            floatArrayCountAttribute.Value = totalFrameCount.ToString();
            floatArray.Attributes.Append(floatArrayCountAttribute);

            string floatArrayTextNode = string.Empty;
            for (int i = 0; i < frameValues.Count; i++)
            {
                floatArrayTextNode = string.Format("{0} {1}", floatArrayTextNode, frameValues[i].ToString());
            }
            floatArray.AppendChild(document.CreateTextNode(floatArrayTextNode));

            source.AppendChild(floatArray);


            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            source.AppendChild(techniqueCommon);


            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");

            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-{1}-input-array", joint, prefix);
            accessor.Attributes.Append(accessorSourceAttribute);

            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            XmlAttribute accessorStrideAttribute = document.CreateAttribute("stride");
            accessorStrideAttribute.Value = "1";
            accessor.Attributes.Append(accessorStrideAttribute);

            techniqueCommon.AppendChild(accessor);


            // Create param element
            XmlNode param = document.CreateElement("param");

            XmlAttribute paramNameAttribute = document.CreateAttribute("name");
            paramNameAttribute.Value = "TIME";
            param.Attributes.Append(paramNameAttribute);

            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "float";
            param.Attributes.Append(paramTypeAttribute);

            accessor.AppendChild(param);

            return source;
        }

        private static XmlNode createComponentOutputSourceElement(XmlDocument document, string joint, int totalFrameCount, string floatArrayValues, string prefix)
        {
            XmlNode source = document.CreateElement("source");
            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-{1}-output", joint, prefix);
            source.Attributes.Append(sourceIdAttribute);


            // Create Float array element
            XmlNode floatArray = document.CreateElement("float_array");

            XmlAttribute floatArrayIdAttribute = document.CreateAttribute("id");
            floatArrayIdAttribute.Value = string.Format("{0}-{1}-output-array", joint, prefix);
            floatArray.Attributes.Append(floatArrayIdAttribute);

            XmlAttribute floatArrayCountAttribute = document.CreateAttribute("count");
            floatArrayCountAttribute.Value = totalFrameCount.ToString();
            floatArray.Attributes.Append(floatArrayCountAttribute);

            floatArray.AppendChild(document.CreateTextNode(floatArrayValues));

            source.AppendChild(floatArray);

            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            source.AppendChild(techniqueCommon);


            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");

            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-{1}-output-array", joint, prefix);
            accessor.Attributes.Append(accessorSourceAttribute);

            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            XmlAttribute accessorStrideAttribute = document.CreateAttribute("stride");
            accessorStrideAttribute.Value = "1";
            accessor.Attributes.Append(accessorStrideAttribute);

            techniqueCommon.AppendChild(accessor);


            // Create param element
            XmlNode param = document.CreateElement("param");

            XmlAttribute paramNameAttribute = document.CreateAttribute("name");
            paramNameAttribute.Value = "ANGLE";
            param.Attributes.Append(paramNameAttribute);

            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "float";
            param.Attributes.Append(paramTypeAttribute);

            accessor.AppendChild(param);

            return source;
        }

        private static XmlNode createComponentInterpolationsSourceElement(XmlDocument document, string joint, int totalFrameCount, string InterpMethod, string prefix)
        {
            XmlNode sourceInterp = document.CreateElement("source");
            XmlAttribute sourceIdAttribute = document.CreateAttribute("id");
            sourceIdAttribute.Value = string.Format("{0}-{1}-interpolation", joint, prefix);
            sourceInterp.Attributes.Append(sourceIdAttribute);

            // Create name array element
            XmlNode nameArray = document.CreateElement("Name_array");
            XmlAttribute nameArrayIdAttribute = document.CreateAttribute("id");
            nameArrayIdAttribute.Value = string.Format("{0}-{1}-interpolation-array", joint, prefix);
            nameArray.Attributes.Append(nameArrayIdAttribute);
            XmlAttribute nameArrayCountAttribute = document.CreateAttribute("count");
            nameArrayCountAttribute.Value = totalFrameCount.ToString();
            nameArray.Attributes.Append(nameArrayCountAttribute);

            string nameArrayTextNode = string.Empty;
            for (int i = 0; i < totalFrameCount; i++)
            {
                nameArrayTextNode = string.Format("{0} {1}", nameArrayTextNode, InterpMethod);
            }
            nameArray.AppendChild(document.CreateTextNode(nameArrayTextNode));

            sourceInterp.AppendChild(nameArray);

            // Create technique common element
            XmlNode techniqueCommon = document.CreateElement("technique_common");
            sourceInterp.AppendChild(techniqueCommon);

            // Create accessor element
            XmlNode accessor = document.CreateElement("accessor");
            XmlAttribute accessorSourceAttribute = document.CreateAttribute("source");
            accessorSourceAttribute.Value = string.Format("#{0}-{1}-interpolation-array", joint, prefix);
            accessor.Attributes.Append(accessorSourceAttribute);
            XmlAttribute accessorCountAttribute = document.CreateAttribute("count");
            accessorCountAttribute.Value = totalFrameCount.ToString();
            accessor.Attributes.Append(accessorCountAttribute);

            XmlAttribute accessorStrideAttribute = document.CreateAttribute("stride");
            accessorStrideAttribute.Value = "1";
            accessor.Attributes.Append(accessorStrideAttribute);

            techniqueCommon.AppendChild(accessor);

            // Create param element
            XmlNode param = document.CreateElement("param");
            XmlAttribute paramTypeAttribute = document.CreateAttribute("type");
            paramTypeAttribute.Value = "name";
            param.Attributes.Append(paramTypeAttribute);
            accessor.AppendChild(param);

            return sourceInterp;
        }

        private static XmlNode createComponentSamplerElement(XmlDocument document, string joint, string prefix)
        {
            // Add sampler node
            XmlNode sampler = document.CreateElement("sampler");
            XmlAttribute samplerIdAttribute = document.CreateAttribute("id");
            samplerIdAttribute.Value = string.Format("{0}-{1}", joint, prefix);
            sampler.Attributes.Append(samplerIdAttribute);

            // Add input nodes
            XmlNode samplerInputInput = document.CreateElement("input");
            XmlAttribute samplerInputInputSemanticAttribute = document.CreateAttribute("semantic");
            samplerInputInputSemanticAttribute.Value = "INPUT";
            samplerInputInput.Attributes.Append(samplerInputInputSemanticAttribute);
            XmlAttribute samplerInputInputSourceAttribute = document.CreateAttribute("source");
            samplerInputInputSourceAttribute.Value = string.Format("#{0}-{1}-input", joint, prefix);
            samplerInputInput.Attributes.Append(samplerInputInputSourceAttribute);
            sampler.AppendChild(samplerInputInput);

            XmlNode samplerOutputInput = document.CreateElement("input");
            XmlAttribute samplerOutputInputSemanticAttribute = document.CreateAttribute("semantic");
            samplerOutputInputSemanticAttribute.Value = "OUTPUT";
            samplerOutputInput.Attributes.Append(samplerOutputInputSemanticAttribute);
            XmlAttribute samplerOuputInputSourceAttribute = document.CreateAttribute("source");
            samplerOuputInputSourceAttribute.Value = string.Format("#{0}-{1}-output", joint, prefix);
            samplerOutputInput.Attributes.Append(samplerOuputInputSourceAttribute);
            sampler.AppendChild(samplerOutputInput);

            XmlNode samplerInterpInput = document.CreateElement("input");
            XmlAttribute samplerInterpInputSemanticAttribute = document.CreateAttribute("semantic");
            samplerInterpInputSemanticAttribute.Value = "INTERPOLATION";
            samplerInterpInput.Attributes.Append(samplerInterpInputSemanticAttribute);
            XmlAttribute samplerInterpInputSourceAttribute = document.CreateAttribute("source");
            samplerInterpInputSourceAttribute.Value = string.Format("#{0}-{1}-interpolation", joint, prefix);
            samplerInterpInput.Attributes.Append(samplerInterpInputSourceAttribute);
            sampler.AppendChild(samplerInterpInput);

            return sampler;
        }

        private static XmlNode createComponentChannelElement(XmlDocument document, string joint, string prefix)
        {
            XmlNode channel = document.CreateElement("channel");
            XmlAttribute channelSourceAttribute = document.CreateAttribute("source");
            channelSourceAttribute.Value = string.Format("#{0}-{1}", joint, prefix);
            channel.Attributes.Append(channelSourceAttribute);
            XmlAttribute channelTargetAttribute = document.CreateAttribute("target");
            channelTargetAttribute.Value = string.Format("{0}/{1}", joint, prefix);
            channel.Attributes.Append(channelTargetAttribute);

            return channel;
        }

        #endregion
    }
}