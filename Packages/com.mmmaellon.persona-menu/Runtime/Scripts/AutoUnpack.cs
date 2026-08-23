using UnityEngine;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MMMaellon.PersonaMenu
{
    [ExecuteAlways]
    public class AutoUnpack : MonoBehaviour
    {
        void OnValidate()
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null || PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    return;
                }
                var status = PrefabUtility.GetPrefabInstanceStatus(gameObject);
                if (status == PrefabInstanceStatus.Connected)
                {
                    PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
                DestroyImmediate(this);
            };
        }
    }
}
#endif
