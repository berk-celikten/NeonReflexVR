using UnityEngine;
using UnityEngine.XR;

namespace NeonReflexVR
{
    public sealed class ControllerHitHaptics : MonoBehaviour
    {
        [SerializeField] XRNode hand = XRNode.RightHand;
        [SerializeField, Range(0f, 0.3f)] float amplitude = 0.12f;
        [SerializeField, Range(0.01f, 0.2f)] float duration = 0.08f;
        public XRNode Hand => hand;
        public int PulseRequests { get; private set; }

        public bool Pulse()
        {
            PulseRequests++;
            if (hand != XRNode.LeftHand && hand != XRNode.RightHand) return false;
            return TrySendImpulse(InputDevices.GetDeviceAtXRNode(hand), amplitude, duration);
        }

        public static bool TrySendImpulse(InputDevice device, float amplitude, float duration)
        {
            if (!device.isValid || !device.TryGetHapticCapabilities(out var capabilities) ||
                !capabilities.supportsImpulse || capabilities.numChannels == 0) return false;
            return device.SendHapticImpulse(0, Mathf.Clamp(amplitude, 0f, 0.3f), Mathf.Clamp(duration, 0f, 0.2f));
        }
    }
}