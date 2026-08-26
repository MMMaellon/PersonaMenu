
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.PersonaMenu
{
    [RequireComponent(typeof(Animator))]
    public class PersonaMenu : UdonSharpBehaviour
    {
        private readonly int VisibleHash = Animator.StringToHash("visible");
        public Animator animator;
        public VRC_Pickup pickup;
        public bool startVisible = true;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif
        VRCPlayerApi localPlayer;
        void Start()
        {
            localPlayer = Networking.LocalPlayer;
            if (despawnDistance > 0 || !localPlayer.IsUserInVR())
            {
                SendCustomEventDelayedFrames(nameof(Loop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            }
            if (startVisible)
            {
                ToggleOnMenu();
            }
            else
            {
                ToggleOffMenu();
            }
        }

        public void Loop()
        {
            SendCustomEventDelayedFrames(nameof(Loop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleMenu();
            }
            else if (MenuVisible && Vector3.ProjectOnPlane(pickup.transform.position - localPlayer.GetPosition(), Vector3.up).magnitude > despawnDistance)
            {
                ToggleOffMenu();
            }
        }

        float lastSecondNoInput = -1001;
        public float doubleClickDuration = 0.5f;
        public float doubleClickDistance = 0.2f;
        Vector3 headPos;
        public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (!localPlayer.IsUserInVR())
            {
                return;
            }
            if (value)
            {
                headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                float distance;
                if (args.handType == VRC.Udon.Common.HandType.LEFT)
                {
                    distance = Vector3.Distance(headPos, localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position);

                }
                else
                {
                    distance = Vector3.Distance(headPos, localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
                }
                if (distance <= doubleClickDistance)
                {
                    if (Time.timeSinceLevelLoad - lastSecondNoInput > doubleClickDuration)
                    {
                        lastSecondNoInput = Time.timeSinceLevelLoad;
                    }
                    else
                    {
                        ToggleMenu();
                    }
                }
            }
        }

        public bool MenuVisible
        {
            get
            {
                return animator.GetBool(VisibleHash);
            }
        }

        public void ToggleMenu()
        {
            if (MenuVisible)
            {
                ToggleOffMenu();
            }
            else
            {
                ToggleOnMenu();
            }
        }

        VRCPlayerApi.TrackingData localHeadTracking;
        public float spawnDistance = 0.5f;
        [Header("Set despawn distance to 0 or negative to disable")]
        public float despawnDistance = 3f;
        public float spawnHeight = -0.1f;
        public void ToggleOnMenu()
        {
            localHeadTracking = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.SetPositionAndRotation(localHeadTracking.position + localHeadTracking.rotation * new Vector3(0, spawnHeight, spawnDistance), localHeadTracking.rotation);
            animator.SetBool(VisibleHash, true);
            pickup.Drop();
            pickup.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void ToggleOffMenu()
        {
            pickup.Drop();
            animator.SetBool(VisibleHash, false);
        }
    }
}

