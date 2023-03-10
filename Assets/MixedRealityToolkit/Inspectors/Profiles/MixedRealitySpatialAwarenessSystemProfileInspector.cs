// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information. 

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine;
using UnityEditor;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace Microsoft.MixedReality.Toolkit.SpatialAwareness.Editor
{
    [CustomEditor(typeof(MixedRealitySpatialAwarenessSystemProfile))]
    public class MixedRealitySpatialAwarenessSystemProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static readonly GUIContent AddObserverContent = new GUIContent("+ Add Spatial Observer", "Add Spatial Observer");
        private static readonly GUIContent RemoveObserverContent = new GUIContent("-", "Remove Spatial Observer");

        private static readonly GUIContent ComponentTypeContent = new GUIContent("Type");
        private static readonly GUIContent RuntimePlatformContent = new GUIContent("Platform(s)");
        private static readonly GUIContent ObserverProfileContent = new GUIContent("Profile");

        private static bool showObservers = true;
        private SerializedProperty observerConfigurations;

        private bool[] observerFoldouts;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!MixedRealityInspectorUtility.CheckMixedRealityConfigured(false))
            {
                return;
            }

            observerConfigurations = serializedObject.FindProperty("observerConfigurations");

            observerFoldouts = new bool[observerConfigurations.arraySize];
        }

        public override void OnInspectorGUI()
        {
            RenderMixedRealityToolkitLogo();
            if (!MixedRealityInspectorUtility.CheckMixedRealityConfigured())
            {
                return;
            }

            if (DrawBacktrackProfileButton("Back to Configuration Profile", MixedRealityToolkit.Instance.ActiveProfile))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spatial Awareness System Profile", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The Spatial Awareness System profile allows developers to configure cross-platform environmental awareness.", MessageType.Info);

            CheckProfileLock(target);

            serializedObject.Update();

            EditorGUILayout.Space();
            showObservers = EditorGUILayout.Foldout(showObservers, "Observers", true);
            if (showObservers)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    RenderList(observerConfigurations);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderList(SerializedProperty list)
        {
            EditorGUILayout.Space();

            bool changed = false;

            using (new EditorGUILayout.VerticalScope())
            {
                if (GUILayout.Button(AddObserverContent, EditorStyles.miniButton))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                    SerializedProperty observer = list.GetArrayElementAtIndex(list.arraySize - 1);

                    SerializedProperty observerName = observer.FindPropertyRelative("componentName");
                    observerName.stringValue = $"New spatial observer {list.arraySize - 1}";

                    SerializedProperty runtimePlatform = observer.FindPropertyRelative("runtimePlatform");
                    runtimePlatform.intValue = -1;

                    SerializedProperty configurationProfile = observer.FindPropertyRelative("observerProfile");
                    configurationProfile.objectReferenceValue = null;

                    serializedObject.ApplyModifiedProperties();

                    SystemType observerType = ((MixedRealitySpatialAwarenessSystemProfile)serializedObject.targetObject).ObserverConfigurations[list.arraySize - 1].ComponentType;
                    observerType.Type = null;

                    observerFoldouts = new bool[list.arraySize];
                    return;
                }

                GUILayout.Space(12f);

                if (list == null || list.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("The Mixed Reality Spatial Awareness System requires one or more observers.", MessageType.Warning);
                    return;
                }

                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty observer = list.GetArrayElementAtIndex(i);
                    SerializedProperty observerName = observer.FindPropertyRelative("componentName");
                    SerializedProperty observerType = observer.FindPropertyRelative("componentType");
                    SerializedProperty observerProfile = observer.FindPropertyRelative("observerProfile");
                    SerializedProperty runtimePlatform = observer.FindPropertyRelative("runtimePlatform");

                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            observerFoldouts[i] = EditorGUILayout.Foldout(observerFoldouts[i], observerName.stringValue, true);

                            if (GUILayout.Button(RemoveObserverContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                            {
                                list.DeleteArrayElementAtIndex(i);
                                serializedObject.ApplyModifiedProperties();
                                changed = true;
                                break;
                            }

                        }

                        if (observerFoldouts[i] || RenderAsSubProfile)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(observerType, ComponentTypeContent);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    serializedObject.ApplyModifiedProperties();
                                    System.Type type = ((MixedRealitySpatialAwarenessSystemProfile)serializedObject.targetObject).ObserverConfigurations[i].ComponentType.Type;
                                    ApplyObserverConfiguration(type, observerName, observerProfile, runtimePlatform);
                                    break;
                                }

                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(runtimePlatform, RuntimePlatformContent);
                                changed |= EditorGUI.EndChangeCheck();

                                System.Type serviceType = null;
                                if (observerProfile.objectReferenceValue != null)
                                {
                                    serviceType = (target as MixedRealitySpatialAwarenessSystemProfile).ObserverConfigurations[i].ComponentType;
                                }

                                changed |= RenderProfile(observerProfile, ObserverProfileContent, true, serviceType);
                            }

                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }

                if (changed)
                {
                    EditorApplication.delayCall += () => MixedRealityToolkit.Instance.ResetConfiguration(MixedRealityToolkit.Instance.ActiveProfile);
                }
            }
        }
        private void ApplyObserverConfiguration(
            System.Type type, 
            SerializedProperty observerName,
            SerializedProperty configurationProfile,
            SerializedProperty runtimePlatform)
        {
            if (type != null)
            {
                MixedRealityDataProviderAttribute observerAttribute = MixedRealityDataProviderAttribute.Find(type) as MixedRealityDataProviderAttribute;
                if (observerAttribute != null)
                {
                    observerName.stringValue = !string.IsNullOrWhiteSpace(observerAttribute.Name) ? observerAttribute.Name : type.Name;
                    configurationProfile.objectReferenceValue = observerAttribute.DefaultProfile;
                    runtimePlatform.intValue = (int)observerAttribute.RuntimePlatforms;
                }
                else
                {
                    observerName.stringValue = type.Name;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}