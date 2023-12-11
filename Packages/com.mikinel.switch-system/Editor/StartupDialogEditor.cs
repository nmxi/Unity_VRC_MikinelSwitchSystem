using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem.Editor
{
    public class StartupDialogEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _startupDialogUxml;
    }
}