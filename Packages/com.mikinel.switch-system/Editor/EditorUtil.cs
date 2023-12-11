using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using mikinel.vrc.SwitchSystem.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.Udon;
using Object = UnityEngine.Object;

namespace mikinel.vrc.SwitchSystem
{
    public static class EditorUtil
    {
        public static readonly List<string> onOffStateList = new List<string> {"Off", "On"};
        
        public static void SetVariable(MonoBehaviour self, Type type, string symbolName, object value)
        {
            const BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var field = type.GetField(symbolName, flag);
            field.SetValue(self, value);
        }

        /// <summary>
        /// objectを管理するReorderableListを作成する
        /// </summary>
        public static ReorderableList SinglePropertyReorderableList<T>(
            SerializedObject serializedObject,
            SerializedProperty property,
            string headerLabel
        ) where T : Object
        {
            return new ReorderableList(
                serializedObject,
                property
            )
            {
                elementHeightCallback = _ => 25,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, headerLabel);
                    DropArea<T>(rect, droppedObject =>
                    {
                        property.arraySize++;
                        var index = property.arraySize - 1;
                        property.GetArrayElementAtIndex(index).objectReferenceValue = droppedObject;
                    });
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var propertyFieldRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 2);
                    EditorGUI.PropertyField(propertyFieldRect, property.GetArrayElementAtIndex(index));
                },
                onAddCallback = x => { property.arraySize++; },
                onRemoveCallback = x =>
                {
                    if (property.GetArrayElementAtIndex(x.index).objectReferenceValue != null)
                    {
                        property.DeleteArrayElementAtIndex(x.index);
                        property.DeleteArrayElementAtIndex(x.index);
                    }
                    else
                    {
                        property.DeleteArrayElementAtIndex(x.index);
                    }
                },
                onReorderCallbackWithDetails = (x, oldIndex, newIndex) =>
                {
                    //NOTE : MoveArrayElementが正常に動作しないので、別の方法で行う
                    (property.GetArrayElementAtIndex(newIndex).isExpanded, property.GetArrayElementAtIndex(oldIndex).isExpanded)
                        = (property.GetArrayElementAtIndex(oldIndex).isExpanded, property.GetArrayElementAtIndex(newIndex).isExpanded);
                }
            };
        }
        
        /// <summary>
        /// SwitchBase用のReorderableListを作成する
        /// LabelにはsyncModeを表示する
        /// </summary>
        public static ReorderableList SwitchBaseReorderableList(
            SerializedObject serializedObject,
            SerializedProperty property,
            string headerLabel
        )
        {
            return new ReorderableList(
                serializedObject,
                property
            )
            {
                elementHeightCallback = _ => 25,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, headerLabel);
                    DropArea<OnOffSwitch>(rect, droppedObject =>
                    {
                        property.arraySize++;
                        var index = property.arraySize - 1;
                        property.GetArrayElementAtIndex(index).objectReferenceValue = droppedObject;
                    });
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var propertyFieldRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 2);
                    
                    var switchBase = property.GetArrayElementAtIndex(index).objectReferenceValue as SwitchBase;
                    var labelText = " ";
                    if (switchBase != null)
                    {
                        if (switchBase.EnableLinkMode)
                        {
                            labelText = $"Copycat switch";
                        }
                        else
                        {
                            switch (switchBase.SyncedSyncMode)
                            {
                                case SwitchBase.SYNC_MODE_LOCAL:
                                    labelText = $"SyncMode : Local";
                                    break;
                                case SwitchBase.SYNC_MODE_GLOBAL:
                                    labelText = $"SyncMode : Global";
                                    break;
                                default:
                                    Debug.LogError($"Unknown syncMode: {switchBase.SyncedSyncMode}");
                                    break;
                            }   
                        }
                    }
                    
                    EditorGUI.PropertyField(propertyFieldRect, property.GetArrayElementAtIndex(index), new GUIContent(labelText));
                },
                onAddCallback = x => { property.arraySize++; },
                onRemoveCallback = x =>
                {
                    if (property.GetArrayElementAtIndex(x.index).objectReferenceValue != null)
                    {
                        property.DeleteArrayElementAtIndex(x.index);
                        property.DeleteArrayElementAtIndex(x.index);
                    }
                    else
                    {
                        property.DeleteArrayElementAtIndex(x.index);
                    }
                },
                onReorderCallbackWithDetails = (x, oldIndex, newIndex) =>
                {
                    //NOTE : MoveArrayElementが正常に動作しないので、別の方法で行う
                    (property.GetArrayElementAtIndex(newIndex).isExpanded, property.GetArrayElementAtIndex(oldIndex).isExpanded)
                        = (property.GetArrayElementAtIndex(oldIndex).isExpanded, property.GetArrayElementAtIndex(newIndex).isExpanded);
                }
            };
        }

        /// <summary>
        /// state, objectのペアを管理するReorderableListを作成する
        /// </summary>
        public static ReorderableList StateAndPropertyReorderableList<T>(
            SerializedObject serializedObject,
            SerializedProperty statesProperty,
            SerializedProperty objectsProperty,
            string headerLabel,
            string elementLabel,
            bool onOffStateMode = false
        ) where T : Object
        {
            objectsProperty.arraySize = statesProperty.arraySize;
            
            return new ReorderableList(
                serializedObject,
                statesProperty
            )
            {
                elementHeightCallback = _ => 50,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, headerLabel);
                    DropArea<T>(rect, droppedObject =>
                    {
                        statesProperty.arraySize++;
                        objectsProperty.arraySize++;
                        
                        var index = statesProperty.arraySize - 1;
                        
                        statesProperty.GetArrayElementAtIndex(index).intValue = 0;
                        objectsProperty.GetArrayElementAtIndex(index).objectReferenceValue = droppedObject;
                    });
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();

                    var labelWidth = EditorGUIUtility.labelWidth * 0.8f;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, rect.height / 2 - 2), "State");
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + rect.height / 2, labelWidth, rect.height / 2 - 2), elementLabel);

                    var width = rect.width - labelWidth;
                    
                    var enableStateFieldRect = new Rect(rect.x + labelWidth, rect.y + 2, width, rect.height / 2 - 4);
                    var enableState = onOffStateMode 
                        ? EditorGUI.Popup(enableStateFieldRect, statesProperty.GetArrayElementAtIndex(index).intValue, onOffStateList.ToArray())
                        : EditorGUI.IntField(enableStateFieldRect, statesProperty.GetArrayElementAtIndex(index).intValue);
                    
                    var enableObject = EditorGUI.ObjectField(
                        new Rect(rect.x + rect.width - width, rect.y + rect.height / 2, width, rect.height / 2 - 2),
                        objectsProperty.GetArrayElementAtIndex(index).objectReferenceValue,
                        typeof(T), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        statesProperty.GetArrayElementAtIndex(index).intValue = enableState;
                        objectsProperty.GetArrayElementAtIndex(index).objectReferenceValue = enableObject;
                    }
                },
                onAddCallback = x =>
                {
                    statesProperty.arraySize++;
                    objectsProperty.arraySize = statesProperty.arraySize;
                },
                onRemoveCallback = x =>
                {
                    statesProperty.DeleteArrayElementAtIndex(x.index);
                    
                    if (objectsProperty.GetArrayElementAtIndex(x.index).objectReferenceValue != null)
                    {
                        objectsProperty.DeleteArrayElementAtIndex(x.index);
                        objectsProperty.DeleteArrayElementAtIndex(x.index);
                    }
                    else
                    {
                        objectsProperty.DeleteArrayElementAtIndex(x.index);
                    }
                },
                onReorderCallbackWithDetails = (x, oldIndex, newIndex) =>
                {
                    //move
                    objectsProperty.MoveArrayElement(oldIndex, newIndex);
                    
                    //NOTE : statesPropertyのみMoveArrayElementが正常に動作しないので、別の方法で行う
                    (statesProperty.GetArrayElementAtIndex(newIndex).isExpanded, statesProperty.GetArrayElementAtIndex(oldIndex).isExpanded)
                        = (statesProperty.GetArrayElementAtIndex(oldIndex).isExpanded, statesProperty.GetArrayElementAtIndex(newIndex).isExpanded);
                }
            };
        }
        
        /// <summary>
        /// udonCustomEvents用のReorderableListを作成する
        /// </summary>
        /// <param name="enableAnyStatesProperty">statesに関係せずに発火するか</param>
        /// <param name="statesProperty">発火するState</param>
        /// <param name="targetUdonBehavioursProperty">CustomEventの実行先Udon</param>
        /// <param name="udonEventNamesProperty">メソッド名</param>
        public static ReorderableList StateUdonCustomEventsReorderableList(
            SerializedObject serializedObject,
            SerializedProperty enableAnyStatesProperty,
            SerializedProperty statesProperty,
            SerializedProperty targetUdonBehavioursProperty,
            SerializedProperty udonEventNamesProperty,
            string headerLabel,
            bool onOffStateMode = false
        )
        {
            enableAnyStatesProperty.arraySize = statesProperty.arraySize;
            targetUdonBehavioursProperty.arraySize = statesProperty.arraySize;
            udonEventNamesProperty.arraySize = statesProperty.arraySize;
            
            return new ReorderableList(
                serializedObject,
                statesProperty
            )
            {
                elementHeightCallback = _ => 100,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, headerLabel);
                    DropArea<UdonBehaviour>(rect, droppedObject =>
                    {
                        statesProperty.arraySize++;
                        enableAnyStatesProperty.arraySize++;
                        targetUdonBehavioursProperty.arraySize++;
                        udonEventNamesProperty.arraySize++;
                        
                        var index = statesProperty.arraySize - 1;
                        
                        statesProperty.GetArrayElementAtIndex(index).intValue = 0;
                        enableAnyStatesProperty.GetArrayElementAtIndex(index).boolValue = false;
                        targetUdonBehavioursProperty.GetArrayElementAtIndex(index).objectReferenceValue = droppedObject;
                        udonEventNamesProperty.GetArrayElementAtIndex(index).stringValue = "";
                    });
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
        
                    var labelWidth = EditorGUIUtility.labelWidth * 0.8f;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + rect.height / 4, labelWidth, rect.height / 4 - 2), "State");
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, rect.height / 4 - 2), "AnyStates");
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + rect.height / 4 * 2, labelWidth, rect.height / 4 - 2), "TargetUdon");
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + rect.height / 4 * 3, labelWidth, rect.height / 4 - 2), "EventName");

                    var width = rect.width - labelWidth;
                    var enableAnyState = EditorGUI.Toggle(new Rect(rect.x + labelWidth, rect.y, width, rect.height / 4 - 4),
                        enableAnyStatesProperty.GetArrayElementAtIndex(index).boolValue);
                    
                    //if enableAnyState is true, disable state field
                    EditorGUI.BeginDisabledGroup(enableAnyState);

                    var enableStateFieldRect = new Rect(rect.x + labelWidth, rect.y + rect.height / 4, width, rect.height / 4 - 3);
                    var enableState = onOffStateMode 
                        ? EditorGUI.Popup(enableStateFieldRect, statesProperty.GetArrayElementAtIndex(index).intValue, onOffStateList.ToArray())
                        : EditorGUI.IntField(enableStateFieldRect, statesProperty.GetArrayElementAtIndex(index).intValue);
                    
                    EditorGUI.EndDisabledGroup();

                    var targetUdon = EditorGUI.ObjectField(
                        new Rect(rect.x + rect.width - width, rect.y + rect.height / 4 * 2, width, rect.height / 4 - 2),
                        targetUdonBehavioursProperty.GetArrayElementAtIndex(index).objectReferenceValue,
                        typeof(UdonBehaviour), true);
                    
                    var eventName = EditorGUI.TextField(
                        new Rect(rect.x + rect.width - width, rect.y + rect.height / 4 * 3, width, rect.height / 4 - 2),
                        udonEventNamesProperty.GetArrayElementAtIndex(index).stringValue);
        
                    if (EditorGUI.EndChangeCheck())
                    {
                        statesProperty.GetArrayElementAtIndex(index).intValue = enableState;
                        enableAnyStatesProperty.GetArrayElementAtIndex(index).boolValue = enableAnyState;
                        targetUdonBehavioursProperty.GetArrayElementAtIndex(index).objectReferenceValue = targetUdon;
                        udonEventNamesProperty.GetArrayElementAtIndex(index).stringValue = eventName;
                    }
                },
                onAddCallback = x =>
                {
                    statesProperty.arraySize++;
                    enableAnyStatesProperty.arraySize = statesProperty.arraySize;
                    targetUdonBehavioursProperty.arraySize = statesProperty.arraySize;
                    udonEventNamesProperty.arraySize = statesProperty.arraySize;
                },
                onRemoveCallback = x =>
                {
                    statesProperty.DeleteArrayElementAtIndex(x.index);
                    enableAnyStatesProperty.DeleteArrayElementAtIndex(x.index);
                    udonEventNamesProperty.DeleteArrayElementAtIndex(x.index);

                    //NOTE : DeleteArrayElementAtIndexはpropertyが参照型でnullではないとき、
                    //1回実行するだけではnullになるだけで、要素が削除されないので、もう一度実行する
                    if (targetUdonBehavioursProperty.GetArrayElementAtIndex(x.index).objectReferenceValue != null)
                    {
                        targetUdonBehavioursProperty.DeleteArrayElementAtIndex(x.index);
                        targetUdonBehavioursProperty.DeleteArrayElementAtIndex(x.index);
                    }
                    else
                    {
                        targetUdonBehavioursProperty.DeleteArrayElementAtIndex(x.index);
                    }
                },
                onReorderCallbackWithDetails = (x, oldIndex, newIndex) =>
                {
                    //move
                    enableAnyStatesProperty.MoveArrayElement(oldIndex, newIndex);
                    targetUdonBehavioursProperty.MoveArrayElement(oldIndex, newIndex);
                    udonEventNamesProperty.MoveArrayElement(oldIndex, newIndex);
                    
                    //NOTE : statesPropertyのみMoveArrayElementが正常に動作しないので、別の方法で行う
                    (statesProperty.GetArrayElementAtIndex(newIndex).isExpanded, statesProperty.GetArrayElementAtIndex(oldIndex).isExpanded)
                        = (statesProperty.GetArrayElementAtIndex(oldIndex).isExpanded, statesProperty.GetArrayElementAtIndex(newIndex).isExpanded);
                }
            };
        }
        
        /// <summary>
        /// DropAreaを作成する
        /// </summary>
        public static void DropArea<T>(Rect dropArea, Action<T>onDrop) where T : Object
        {
            var evt = Event.current;
            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                    {
                        return;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject.GetType() != typeof(GameObject))
                            {
                                continue;
                            }
                                        
                            var gameObject = draggedObject as GameObject;
                            if (typeof(T) == typeof(GameObject))
                            {
                                //GameObject
                                onDrop(gameObject as T);
                            }
                            else
                            {
                                //Component
                                if (!gameObject.TryGetComponent(out T component))
                                {
                                    continue;
                                }

                                onDrop(component);
                            }
                        }
                    }
                    break;
            }
        }
        
        #region ProximitySettings

        /// <summary>
        /// 現在設定されているProximityを取得する
        /// </summary>
        public static float GetProximity(this SwitchBase self)
        {
            var udonBehaviour = self.GetComponent<UdonBehaviour>();
            return udonBehaviour.proximity;
        }
        
        /// <summary>
        /// Proximityの変更を行う
        /// </summary>
        public static void SetProximity(this SwitchBase self, float proximity)
        {
            if(proximity < 0f)
                proximity = 0f;
            
            //undo
            var udonBehaviour = self.GetComponent<UdonBehaviour>();
            Undo.RecordObject(udonBehaviour, "Change proximity");
            udonBehaviour.proximity = proximity;
            
            //set dirty
            EditorUtility.SetDirty(udonBehaviour);
        }

        #endregion
        
        /// <summary>
        /// Placeholderを置き換える
        /// </summary>
        public static string ReplacePlaceholder(string text, string[] replaceTexts)
        {
            //place holder sample : {1}, {2}, {3}
            
            var result = text;
            for (var i = 0; i < replaceTexts.Length; i++)
            {
                result = result.Replace($"{{{i}}}", replaceTexts[i]);
            }
            
            return result;
        }
        
        public static bool GetLanguageDataSet(string path, string language, out LanguageDataSet languageDataSet)
        {
            var languageDataSetNames = AssetDatabase.FindAssets("t:LanguageDataSet", new[] {path});
            
            //LanguageDataSetのScriptableObjectを全て取得する
            var languageDataSetList = languageDataSetNames
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<LanguageDataSet>)
                .ToList();

            languageDataSet = languageDataSetList.FirstOrDefault(x => x.languageCode == language);
            if (languageDataSet != null)
            {
                return true;
            }
            
            languageDataSet = new LanguageDataSet();
            return false;

        }
    }
}