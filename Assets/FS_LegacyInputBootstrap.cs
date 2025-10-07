// Assets/FS/Editor/FS_LegacyInputBootstrap.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FS.Tools
{
    [InitializeOnLoad]
    public static class FS_LegacyInputBootstrap
    {
        static FS_LegacyInputBootstrap()
        {
            // Run once on editor load to ensure essentials exist.
            EnsureInputs();
        }

        [MenuItem("Tools/FS/Legacy Input/Ensure Required Axes")]
        public static void EnsureInputs()
        {
            var inputManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
            if (inputManagerAssets == null || inputManagerAssets.Length == 0)
            {
                Debug.LogError("[FS Input] Could not find ProjectSettings/InputManager.asset");
                return;
            }

            var im = new SerializedObject(inputManagerAssets[0]);
            var axesProp = im.FindProperty("m_Axes");
            if (axesProp == null || !axesProp.isArray)
            {
                Debug.LogError("[FS Input] InputManager.asset does not contain m_Axes array.");
                return;
            }

            void AddAxisIfMissing(Axis axis)
            {
                if (!AxisExists(axesProp, axis.name))
                {
                    AddAxis(axesProp, axis);
                    Debug.Log($"[FS Input] Added legacy Input axis/button '{axis.name}'.");
                }
            }

            // --- Core missing bindings your project references ---

            // Camera look (mouse by default). Change via menu to gamepad mapping if desired.
            AddAxisIfMissing(new Axis {
                name = "Analog X",
                gravity = 0, dead = 0, sensitivity = 0.1f,
                type = AxisType.MouseMovement, axis = 0, // Mouse X
                snap = false, invert = false
            });
            AddAxisIfMissing(new Axis {
                name = "Analog Y",
                gravity = 0, dead = 0, sensitivity = 0.1f,
                type = AxisType.MouseMovement, axis = 1, // Mouse Y
                snap = false, invert = false
            });

            // Sprint (button-like)
            AddAxisIfMissing(new Axis {
                name = "Sprint",
                positiveButton = "left shift",
                altPositiveButton = "joystick button 8", // adjust to your controller if needed
                gravity = 1000f, dead = 0.001f, sensitivity = 1000f,
                type = AxisType.KeyOrMouseButton, axis = 0, snap = false, invert = false
            });

            // Fly (button-like)
            AddAxisIfMissing(new Axis {
                name = "Fly",
                positiveButton = "f",
                altPositiveButton = "joystick button 4", // Y/Triangle common
                gravity = 1000f, dead = 0.001f, sensitivity = 1000f,
                type = AxisType.KeyOrMouseButton, axis = 0, snap = false, invert = false
            });

            // Aim (button-like axis so GetAxisRaw returns 0/1 promptly)
            AddAxisIfMissing(new Axis {
                name = "Aim",
                positiveButton = "mouse 1",              // Right mouse
                altPositiveButton = "joystick button 6", // LT (varies by controller)
                gravity = 1000f, dead = 0.001f, sensitivity = 1000f,
                type = AxisType.KeyOrMouseButton, axis = 0, snap = false, invert = false
            });

            im.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        // -------- Convenience: quickly switch Analog axes to mouse or gamepad --------

        [MenuItem("Tools/FS/Legacy Input/Analog Axes → Mouse X/Y")]
        public static void SetAnalogToMouse()
        {
            if (!TryOpenAxes(out var im, out var axesProp)) return;
            SetAxisTypeAndIndex(axesProp, "Analog X", AxisType.MouseMovement, 0); // Mouse X
            SetAxisTypeAndIndex(axesProp, "Analog Y", AxisType.MouseMovement, 1); // Mouse Y
            im.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[FS Input] Set 'Analog X/Y' to Mouse movement (X/Y).");
        }

        // Common gamepad mapping: Right Stick X = axis 3, Right Stick Y = axis 4.
        // (On some pads Y may be 5; adjust if needed.)
        [MenuItem("Tools/FS/Legacy Input/Analog Axes → Gamepad Right Stick")]
        public static void SetAnalogToGamepad()
        {
            if (!TryOpenAxes(out var im, out var axesProp)) return;
            SetAxisTypeAndIndex(axesProp, "Analog X", AxisType.JoystickAxis, 3); // Right Stick X
            SetAxisTypeAndIndex(axesProp, "Analog Y", AxisType.JoystickAxis, 4); // Right Stick Y (try 5 if needed)
            im.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[FS Input] Set 'Analog X/Y' to Gamepad Right Stick (3/4). Change Y to 5 if your pad uses that.");
        }

        // ------------------------- helpers -------------------------

        enum AxisType { KeyOrMouseButton = 0, MouseMovement = 1, JoystickAxis = 2 }

        struct Axis
        {
            public string name;
            public string descriptiveName;
            public string descriptiveNegativeName;
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;
            public float gravity;
            public float dead;
            public float sensitivity;
            public bool snap;
            public bool invert;
            public AxisType type;
            public int axis;
            public int joyNum;
        }

        static bool TryOpenAxes(out SerializedObject im, out SerializedProperty axesProp)
        {
            im = null; axesProp = null;
            var inputManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
            if (inputManagerAssets == null || inputManagerAssets.Length == 0)
            {
                Debug.LogError("[FS Input] Could not find ProjectSettings/InputManager.asset");
                return false;
            }
            im = new SerializedObject(inputManagerAssets[0]);
            axesProp = im.FindProperty("m_Axes");
            if (axesProp == null || !axesProp.isArray)
            {
                Debug.LogError("[FS Input] InputManager.asset does not contain m_Axes array.");
                return false;
            }
            return true;
        }

        static bool AxisExists(SerializedProperty axesArray, string axisName)
        {
            for (int i = 0; i < axesArray.arraySize; i++)
            {
                var axis = axesArray.GetArrayElementAtIndex(i);
                var nameProp = axis.FindPropertyRelative("m_Name");
                if (nameProp != null && nameProp.stringValue == axisName) return true;
            }
            return false;
        }

        static SerializedProperty FindAxis(SerializedProperty axesArray, string axisName)
        {
            for (int i = 0; i < axesArray.arraySize; i++)
            {
                var axis = axesArray.GetArrayElementAtIndex(i);
                var nameProp = axis.FindPropertyRelative("m_Name");
                if (nameProp != null && nameProp.stringValue == axisName) return axis;
            }
            return null;
        }

        static void SetAxisTypeAndIndex(SerializedProperty axesArray, string axisName, AxisType type, int axisIndex)
        {
            var axisProp = FindAxis(axesArray, axisName);
            if (axisProp == null)
            {
                Debug.LogWarning($"[FS Input] Axis '{axisName}' not found; creating it.");
                AddAxis(axesArray, new Axis { name = axisName, type = type, axis = axisIndex, sensitivity = 0.1f });
                return;
            }
            axisProp.FindPropertyRelative("type").intValue = (int)type;
            axisProp.FindPropertyRelative("axis").intValue = axisIndex;
            // Reasonable defaults for look axes
            axisProp.FindPropertyRelative("gravity").floatValue = 0f;
            axisProp.FindPropertyRelative("dead").floatValue = 0f;
            if (axisProp.FindPropertyRelative("sensitivity").floatValue <= 0f)
                axisProp.FindPropertyRelative("sensitivity").floatValue = 0.1f;
            axisProp.FindPropertyRelative("snap").boolValue = false;
        }

        static void AddAxis(SerializedProperty axesArray, Axis axis)
        {
            axesArray.arraySize++;
            var axisProp = axesArray.GetArrayElementAtIndex(axesArray.arraySize - 1);

            axisProp.FindPropertyRelative("m_Name").stringValue = axis.name ?? "";
            axisProp.FindPropertyRelative("descriptiveName").stringValue = axis.descriptiveName ?? "";
            axisProp.FindPropertyRelative("descriptiveNegativeName").stringValue = axis.descriptiveNegativeName ?? "";
            axisProp.FindPropertyRelative("negativeButton").stringValue = axis.negativeButton ?? "";
            axisProp.FindPropertyRelative("positiveButton").stringValue = axis.positiveButton ?? "";
            axisProp.FindPropertyRelative("altNegativeButton").stringValue = axis.altNegativeButton ?? "";
            axisProp.FindPropertyRelative("altPositiveButton").stringValue = axis.altPositiveButton ?? "";
            axisProp.FindPropertyRelative("gravity").floatValue = axis.gravity;
            axisProp.FindPropertyRelative("dead").floatValue = axis.dead;
            axisProp.FindPropertyRelative("sensitivity").floatValue = axis.sensitivity == 0 ? 0.1f : axis.sensitivity;
            axisProp.FindPropertyRelative("snap").boolValue = axis.snap;
            axisProp.FindPropertyRelative("invert").boolValue = axis.invert;
            axisProp.FindPropertyRelative("type").intValue = (int)axis.type;
            axisProp.FindPropertyRelative("axis").intValue = axis.axis;
            axisProp.FindPropertyRelative("joyNum").intValue = axis.joyNum;
        }
    }
}
#endif
