using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class OpenPoseMissingJointSetter : MonoBehaviour
{
    OpenPoseGenerator openPoseGenerator = null;
    Animator animator = null;
    public Transform headTransform;

    private void Start()
    {
        openPoseGenerator = GetComponent<OpenPoseGenerator>();
        animator = GetComponent<Animator>();
        if (animator != null && animator.isHuman)
        {
            headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
            if (headTransform == null)
            {
                Debug.LogWarning("Head bone not found on humanoid avatar.");
            }
            
            if (openPoseGenerator != null)
            {
                openPoseGenerator.mechanimAvatarHasEyes = animator.GetBoneTransform(HumanBodyBones.LeftEye) != null && animator.GetBoneTransform(HumanBodyBones.RightEye) != null;
            }
            else
            {
                Debug.LogWarning("OpenPoseGenerator component not found on this GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("Animator with humanoid avatar not found on this GameObject.");
        }
    }

    void DrawGizmoSphere(Vector3 position, Color color, string label)
    {
        Gizmos.color = color;

        Gizmos.DrawWireSphere(position, 0.03f);
        Gizmos.DrawSphere(position, 0.015f);

        Vector3 labelPosition = position + Vector3.up * 0.05f;
        Handles.Label(labelPosition, label, new GUIStyle() { fontSize = 8, alignment = TextAnchor.MiddleCenter, normal = new GUIStyleState() { textColor = color } });
    }

    void OnDrawGizmos()
    {
        if (openPoseGenerator != null && animator != null)
        {
            Transform headTransform = animator.GetBoneTransform(HumanBodyBones.Head);

            if (headTransform != null)
            {
                DrawGizmoSphere(headTransform.position + headTransform.TransformVector(openPoseGenerator.NosePosition), Color.red, "Nose");
                DrawGizmoSphere(headTransform.position + headTransform.TransformVector(openPoseGenerator.EarLeftPosition), Color.green, "EarL");
                DrawGizmoSphere(headTransform.position + headTransform.TransformVector(openPoseGenerator.EarRightPosition), Color.yellow, "EarR");
                if (!openPoseGenerator.mechanimAvatarHasEyes)
                {
                    DrawGizmoSphere(headTransform.position + headTransform.TransformVector(openPoseGenerator.EyeLeftPosition), Color.cyan, "EyeL");
                    DrawGizmoSphere(headTransform.position + headTransform.TransformVector(openPoseGenerator.EyeRightPosition), Color.blue, "EyeR");
                }
            }
        }
    }

        public void AlignToSceneView()
    {
        if (headTransform != null)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Quaternion flippedRotation = headTransform.rotation * Quaternion.Euler(0, 180, 0);
                
                sceneView.LookAt(headTransform.position, flippedRotation, sceneView.size);
                SceneView.FocusWindowIfItsOpen<SceneView>();
            }
            else
            {
                Debug.LogWarning("No active SceneView found.");
            }
        }
        else
        {
            Debug.LogWarning("Target transform is not assigned.");
        }
    }
}
