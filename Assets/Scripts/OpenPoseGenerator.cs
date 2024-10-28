using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using NaughtyAttributes;
[RequireComponent(typeof(OpenPoseMissingJointSetter))]
[RequireComponent(typeof(Animator))]
public class OpenPoseGenerator : MonoBehaviour
{
    private GameObject Nose;
    private readonly GameObject Neck;

    private readonly GameObject ShoulderLeft;
    private readonly GameObject ElbowLeft;
    private readonly GameObject WristLeft;

    private readonly GameObject ShoulderRight;
    private readonly GameObject ElbowRight;
    private readonly GameObject WristRight;

    private readonly GameObject HipLeft;
    private readonly GameObject KneeLeft;
    private readonly GameObject AnkleLeft;

    private readonly GameObject HipRight;
    private readonly GameObject KneeRight;
    private readonly GameObject AnkleRight;

    [Tooltip("Sets to true automatically if the humanoid avatar has eye bones in its skeleton. Set to false to manually position eye bones.")]
    public bool mechanimAvatarHasEyes;

    private GameObject EyeLeft;
    private GameObject EarLeft;
    private GameObject EyeRight;
    private GameObject EarRight;

    public Vector3 NosePosition = new Vector3(0.0f, 0.05f, 0.15f);
    public Vector3 EarLeftPosition = new Vector3(-0.1f, 0.1f, 0.0f);
    public Vector3 EarRightPosition = new Vector3(0.1f, 0.1f, 0.0f);
    [HideIf("mechanimAvatarHasEyes")]
    public Vector3 EyeLeftPosition = new Vector3(-0.05f, 0.1f, 0.12f);
    [HideIf("mechanimAvatarHasEyes")]
    public Vector3 EyeRightPosition = new Vector3(0.05f, 0.1f, 0.12f);

    [Tooltip("Line renderer to draw the skeleton lines.")]
    public LineRenderer skeletonLine;
    [Tooltip("Object to draw the skeleton joints.")]
    public GameObject jointPrefab;

    private GameObject[] bones;
    private LineRenderer[] lines;

    private int openPoseLayer;

    public enum JointType : int
    {
        Nose = 0,
        Neck = 1,
        ShoulderRight = 2,
        ElbowRight = 3,
        WristRight = 4,
        ShoulderLeft = 5,
        ElbowLeft = 6,
        WristLeft = 7,
        HipRight = 8,
        KneeRight = 9,
        AnkleRight = 10,
        HipLeft = 11,
        KneeLeft = 12,
        AnkleLeft = 13,
        EyeRight = 14,
        EyeLeft = 15,
        EarRight = 16,
        EarLeft = 17,
        Count = 18
    }

    [System.Serializable]
    public class JointData
    {
        public JointType ParentJoint;
        public string BoneColorHex;
        public string JointColorHex;
    }

    private static readonly Dictionary<JointType, JointData> jointData = new()
    {
        { JointType.Nose, new JointData { ParentJoint = JointType.Nose, BoneColorHex = "#000099", JointColorHex = "#FF0000" } },
        { JointType.Neck, new JointData { ParentJoint = JointType.Nose, BoneColorHex = "#000099", JointColorHex = "#FF5500" } },
        { JointType.EyeRight, new JointData { ParentJoint = JointType.Nose, BoneColorHex = "#330099", JointColorHex = "#AA00FF" } },
        { JointType.EarRight, new JointData { ParentJoint = JointType.EyeRight, BoneColorHex = "#660099", JointColorHex = "#FF00AA" } },
        { JointType.EyeLeft, new JointData { ParentJoint = JointType.Nose, BoneColorHex = "#990099", JointColorHex = "#FF00FF" } },
        { JointType.EarLeft, new JointData { ParentJoint = JointType.EyeLeft, BoneColorHex = "#990066", JointColorHex = "#FF0055" } },
        { JointType.ShoulderRight, new JointData { ParentJoint = JointType.Neck, BoneColorHex = "#990000", JointColorHex = "#FFAA00" } },
        { JointType.ElbowRight, new JointData { ParentJoint = JointType.ShoulderRight, BoneColorHex = "#996600", JointColorHex = "#FFFF00" } },
        { JointType.WristRight, new JointData { ParentJoint = JointType.ElbowRight, BoneColorHex = "#999900", JointColorHex = "#AAFF00" } },
        { JointType.ShoulderLeft, new JointData { ParentJoint = JointType.Neck, BoneColorHex = "#993300", JointColorHex = "#55FF00" } },
        { JointType.ElbowLeft, new JointData { ParentJoint = JointType.ShoulderLeft, BoneColorHex = "#669900", JointColorHex = "#00FF00" } },
        { JointType.WristLeft, new JointData { ParentJoint = JointType.ElbowLeft, BoneColorHex = "#339900", JointColorHex = "#00FF55" } },
        { JointType.HipRight, new JointData { ParentJoint = JointType.Neck, BoneColorHex = "#009900", JointColorHex = "#00FFAA" } },
        { JointType.KneeRight, new JointData { ParentJoint = JointType.HipRight, BoneColorHex = "#009933", JointColorHex = "#00FFFF" } },
        { JointType.AnkleRight, new JointData { ParentJoint = JointType.KneeRight, BoneColorHex = "#009966", JointColorHex = "#00AAFF" } },
        { JointType.HipLeft, new JointData { ParentJoint = JointType.Neck, BoneColorHex = "#009999", JointColorHex = "#0055FF" } },
        { JointType.KneeLeft, new JointData { ParentJoint = JointType.HipLeft, BoneColorHex = "#006699", JointColorHex = "#0000FF" } },
        { JointType.AnkleLeft, new JointData { ParentJoint = JointType.KneeLeft, BoneColorHex = "#003399", JointColorHex = "#5500FF" } }
    };

    private static readonly Dictionary<HumanBodyBones, JointType> dictHumanBone = new()
    {
        { HumanBodyBones.Neck, JointType.Neck },
        { HumanBodyBones.RightEye, JointType.EyeRight },
        { HumanBodyBones.LeftEye, JointType.EyeLeft },
        { HumanBodyBones.RightUpperArm, JointType.ShoulderRight },
        { HumanBodyBones.RightLowerArm, JointType.ElbowRight },
        { HumanBodyBones.RightHand, JointType.WristRight },
        { HumanBodyBones.LeftUpperArm, JointType.ShoulderLeft },
        { HumanBodyBones.LeftLowerArm, JointType.ElbowLeft },
        { HumanBodyBones.LeftHand, JointType.WristLeft },
        { HumanBodyBones.RightUpperLeg, JointType.HipRight },
        { HumanBodyBones.RightLowerLeg, JointType.KneeRight },
        { HumanBodyBones.RightFoot, JointType.AnkleRight },
        { HumanBodyBones.LeftUpperLeg, JointType.HipLeft },
        { HumanBodyBones.LeftLowerLeg, JointType.KneeLeft },
        { HumanBodyBones.LeftFoot, JointType.AnkleLeft },
    };

    void Start()
    {
        // Create the "OpenPose" layer if it doesn't exist
        openPoseLayer = LayerMask.NameToLayer("OpenPose");
        if (openPoseLayer == -1)
        {
            openPoseLayer = CreateLayer("OpenPose");
        }

        // Store bones in a list for easier access
        bones = new GameObject[]
        {
            Nose,
            Neck,
            ShoulderRight,
            ElbowRight,
            WristRight,
            ShoulderLeft,
            ElbowLeft,
            WristLeft,
            HipRight,
            KneeRight,
            AnkleRight,
            HipLeft,
            KneeLeft,
            AnkleLeft,
            EyeRight,
            EyeLeft,
            EarRight,
            EarLeft
        };

        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.avatar != null && animator.avatar.isHuman)
        {
            HumanBodyBones[] humanBones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));

            foreach (HumanBodyBones bone in humanBones)
            {
                if (bone != HumanBodyBones.LastBone)
                {
                    Transform boneTransform = animator.GetBoneTransform(bone);
                    if (boneTransform != null)
                    {
                        // Instantiate the bone if it is not already instantiated
                        if (bone == HumanBodyBones.Head)
                        {
                            Nose = new GameObject("Nose");
                            Nose.transform.parent = boneTransform;
                            Nose.transform.localPosition = NosePosition;
                            bones[(int)JointType.Nose] = Nose;

                            EarLeft = new GameObject("EarLeft");
                            EarLeft.transform.parent = boneTransform;
                            EarLeft.transform.localPosition = EarLeftPosition;
                            bones[(int)JointType.EarLeft] = EarLeft;

                            EarRight = new GameObject("EarRight");
                            EarRight.transform.parent = boneTransform;
                            EarRight.transform.localPosition = EarRightPosition;
                            bones[(int)JointType.EarRight] = EarRight;

                            if (!mechanimAvatarHasEyes)
                            {
                                EyeLeft = new GameObject("EyeLeft");
                                EyeLeft.transform.parent = boneTransform;
                                bones[(int)JointType.EyeLeft] = EyeLeft;

                                EyeRight = new GameObject("EyeRight");
                                EyeRight.transform.parent = boneTransform;
                                bones[(int)JointType.EyeRight] = EyeRight;
                            }
                        }

                        // Map the bone to the corresponding joint if it exists in dictHumanBone
                        if (dictHumanBone.TryGetValue(bone, out JointType joint))
                        {
                            bones[(int)joint] = boneTransform.gameObject;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Animator or Avatar is not set or Avatar is not humanoid.");
        }

        // Array holding the skeleton lines
        lines = new LineRenderer[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) continue;

            bones[i].SetActive(true);
            bones[i].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            bones[i].layer = openPoseLayer;

            if (lines[i] != null)
            {
                lines[i].gameObject.SetActive(false);
            }

            if (jointPrefab != null)
            {
                GameObject joint = Instantiate(jointPrefab);
                joint.transform.parent = bones[i].transform;
                joint.transform.localPosition = Vector3.zero;
                joint.name = ((JointType)i).ToString() + " Joint";
                joint.layer = openPoseLayer;

                // Set the color of the joint based on JointColorHex in jointData
                string colorHex = jointData[(JointType)i].JointColorHex;
                Color color = ColorUtility.TryParseHtmlString(colorHex, out color) ? color : Color.white;
                joint.GetComponent<Renderer>().sharedMaterial.color = color;
            }
        }

        PositionFakeJoints();
    }

    void PositionFakeJoints()
    {
        // Set the position of the fake joints
        if (Nose != null)
        {
            Nose.transform.localPosition = NosePosition;
        }

        if (EarLeft != null)
        {
            EarLeft.transform.localPosition = EarLeftPosition;
        }

        if (EarRight != null)
        {
            EarRight.transform.localPosition = EarRightPosition;
        }

        if (mechanimAvatarHasEyes) return;

        if (EyeLeft != null)
        {
            EyeLeft.transform.localPosition = EyeLeftPosition;
        }

        if (EyeRight != null)
        {
            EyeRight.transform.localPosition = EyeRightPosition;
        }
    }

    void Update()
    {
        // Update the local positions of the bones
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                JointType jointType = (JointType)i;
                JointData data = jointData[jointType];

                int parentJointIndex = (int)data.ParentJoint;
                if (lines[i] == null && skeletonLine != null)
                {
                    lines[i] = Instantiate(skeletonLine);
                    lines[i].transform.parent = transform;
                    lines[i].gameObject.layer = openPoseLayer;
                }

                if (lines[i] != null && parentJointIndex < bones.Length)
                {
                    lines[i].gameObject.SetActive(true);
                    Vector3 posJoint = bones[i].transform.position;
                    Vector3 posParent = bones[parentJointIndex]?.transform.position ?? posJoint;

                    lines[i].SetPosition(0, posParent);
                    lines[i].SetPosition(2, posJoint);
                    lines[i].SetPosition(1, (posParent + posJoint) / 2);

                    Color color = ColorUtility.TryParseHtmlString(data.BoneColorHex, out color) ? color : Color.white;
                    lines[i].startColor = color;
                    lines[i].endColor = color;
                }
            }
        }
    }

    int CreateLayer(string layerName)
    {
        SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(layer.stringValue))
            {
                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }

        Debug.LogError("Maximum number of layers reached. Could not create new layer.");
        return -1;
    }
}
