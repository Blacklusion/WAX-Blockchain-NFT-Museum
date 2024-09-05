using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShadowProfile
{
    //  Namespace Properties ------------------------------

    //  Class Attributes ----------------------------------

    /// <summary>
    /// Replace with comments...
    /// </summary>
    public class DefineSymbolsSetter : EditorWindow
    {
        //  Events ----------------------------------------


        //  Properties ------------------------------------


        //  Fields ----------------------------------------
        private const string MENU_ITEM_NAME01 = "Tools/[MyProject]/Define symbols setter";

        //  Unity Methods ---------------------------------
        private void OnGUI()
        {
#if !RELEASE
            if (GUILayout.Button("Turn debug off", GUILayout.Height(80.0f)))
            {
                TurnDebugOff();
            }
#else
            if (GUILayout.Button("Turn debug on", GUILayout.Height(80.0f)))
            {
                TurnDebugOn();
            }
#endif
        }

        //  Methods ---------------------------------------
        [MenuItem(MENU_ITEM_NAME01)]
        public static void ShowWindow()
        {
            GetWindow<DefineSymbolsSetter>("Define Symbols Setter");
        }

        private void TurnDebugOff()
        {
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone,
                Consts.RELEASE_DEFINE_SYMBOL);
            Close();
        }

        private void TurnDebugOn()
        {
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone, out var defines);
            var excepted = new string[] { Consts.RELEASE_DEFINE_SYMBOL };
            var newDefines = defines.Except(excepted).ToArray();

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
            Close();
        }

        //  Event Handlers --------------------------------
    }
}