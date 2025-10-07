using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class FS_PlayerFlyController : MonoBehaviour
{
    [Header("Flight: Toggle")]
    [SerializeField] bool flightEnabled = false;
    [SerializeField] KeyCode toggleFlyKey = KeyCode.V;

    [Header("Flight: Movement")]
    [SerializeField] float flySpeed = 8f;
    [SerializeField, Range(0.01f, 30f)] float flyAcceleration = 12f;
    [SerializeField] float sprintMultiplier = 2.5f;
    [SerializeField] float slowMultiplier = 0.35f;
    [SerializeField] float verticalBoost = 1.0f;

    [Header("Flight: Vertical Keys")]
    [SerializeField] KeyCode ascendKey = KeyCode.Space;
    [SerializeField] KeyCode descendKey = KeyCode.LeftControl;

    [Header("Flight: Legacy Axes")]
    [SerializeField] string axisHorizontal = "Horizontal";
    [SerializeField] string axisVertical = "Vertical";

    [Header("Flight: Frame of Reference")]
    [SerializeField] bool moveRelativeToCamera = true;

    [Header("CharacterController Tweaks")]
    [SerializeField] bool raiseSlopeLimit = true;
    [SerializeField] bool zeroStepOffset = true;
    [SerializeField] bool disableDetectCollisions = false;

    [Header("Disable While Flying")]
    [SerializeField] MonoBehaviour[] disableTheseWhileFlying;

    [Header("Animator")]
    [Tooltip("Name of the bool parameter in Animator to indicate flying.")]
    [SerializeField] string flyBoolName = "Fly";

    // Internals
    CharacterController _cc;
    Animator _anim;
    float _ccDefaultSlope, _ccDefaultStep;
    bool _ccDefaultDetectCollisions;
    bool _animDefaultRootMotion;
    Vector3 _flyVelocity;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();

        _ccDefaultSlope = _cc.slopeLimit;
        _ccDefaultStep = _cc.stepOffset;
        _ccDefaultDetectCollisions = _cc.detectCollisions;

        if (_anim) _animDefaultRootMotion = _anim.applyRootMotion;
    }

    void OnEnable() { ApplyFlightState(flightEnabled, true); }
    void OnDisable() { ApplyFlightState(false, true); }

    void Update()
    {
        // Toggle
        if (Input.GetKeyDown(toggleFlyKey))
        {
            flightEnabled = !flightEnabled;
            ApplyFlightState(flightEnabled, false);
        }

        // Adjust speed with mouse wheel
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float factor = 1f + Mathf.Sign(scroll) * 0.15f;
            flySpeed = Mathf.Clamp(flySpeed * factor, 0.5f, 200f);
        }

        if (!flightEnabled) return;

        // Build input
        float h = SafeGetAxis(axisHorizontal, "Horizontal");
        float v = SafeGetAxis(axisVertical, "Vertical");

        Transform cam = Camera.main ? Camera.main.transform : null;
        Vector3 frameFwd, frameRight;

        if (moveRelativeToCamera && cam != null)
        {
            frameFwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            if (frameFwd.sqrMagnitude < 1e-4f) frameFwd = transform.forward;
            frameRight = cam.right.normalized;
        }
        else
        {
            frameFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            frameRight = transform.right.normalized;
        }

        Vector3 move = frameFwd * v + frameRight * h;

        float up = 0f;
        if (Input.GetKey(ascendKey) || Input.GetKey(KeyCode.E)) up += 1f;
        if (Input.GetKey(descendKey) || Input.GetKey(KeyCode.Q)) up -= 1f;
        move += Vector3.up * up;

        if (move.sqrMagnitude > 1f) move.Normalize();

        float speed = flySpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= sprintMultiplier;
        if (Input.GetKey(KeyCode.LeftAlt)) speed *= slowMultiplier;
        if (Mathf.Abs(up) > 0.1f && Input.GetKey(KeyCode.LeftShift)) speed *= (1f + verticalBoost);

        Vector3 targetVel = move * speed;

        float lerpT = 1f - Mathf.Exp(-flyAcceleration * Time.unscaledDeltaTime);
        _flyVelocity = Vector3.Lerp(_flyVelocity, targetVel, lerpT);

        _cc.Move(_flyVelocity * Time.unscaledDeltaTime);

        // Rotate to camera yaw
        if (moveRelativeToCamera && cam != null)
        {
            Vector3 yawForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            if (yawForward.sqrMagnitude > 1e-4f)
            {
                Quaternion targetRot = Quaternion.LookRotation(yawForward, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.unscaledDeltaTime);
            }
        }
    }

    void ApplyFlightState(bool enable, bool forceRestore)
    {
        if (enable)
        {
            if (raiseSlopeLimit) _cc.slopeLimit = 90f;
            if (zeroStepOffset) _cc.stepOffset = 0f;
            if (disableDetectCollisions) _cc.detectCollisions = false;

            if (_anim)
            {
                _anim.applyRootMotion = false;
                if (!string.IsNullOrEmpty(flyBoolName))
                    _anim.SetBool(flyBoolName, true);
            }

            if (disableTheseWhileFlying != null)
                foreach (var mb in disableTheseWhileFlying) if (mb) mb.enabled = false;
        }
        else
        {
            _cc.slopeLimit = _ccDefaultSlope;
            _cc.stepOffset = _ccDefaultStep;
            _cc.detectCollisions = _ccDefaultDetectCollisions;

            if (_anim)
            {
                _anim.applyRootMotion = _animDefaultRootMotion;
                if (!string.IsNullOrEmpty(flyBoolName))
                    _anim.SetBool(flyBoolName, false);
            }

            if (disableTheseWhileFlying != null)
                foreach (var mb in disableTheseWhileFlying) if (mb) mb.enabled = true;

            if (forceRestore) _flyVelocity = Vector3.zero;
        }
    }

    static float SafeGetAxis(string custom, string fallback)
    {
        string axis = string.IsNullOrWhiteSpace(custom) ? fallback : custom;
        return Input.GetAxis(axis);
    }
}
