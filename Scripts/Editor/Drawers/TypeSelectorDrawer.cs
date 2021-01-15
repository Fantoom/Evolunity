using System;
using System.Linq;
using System.Reflection;
using Evolutex.Evolunity.Attributes;
using UnityEditor;
using UnityEngine;

namespace Evolutex.Evolunity.Editor.Drawers
{
    /// <summary>
    /// Displays a custom inspector type picker dropdown.
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    public class TypeSelectorDrawer : PropertyDrawer
    {
        /// <summary>
        /// A PickerWindow for a specified type.
        /// </summary>
        public class PickerWindow : PickerWindow<Type, PickerWindow> { }

        /// <summary>
        /// The type for the picker inspector.
        /// </summary>
        protected Type type;
        
        /// <summary>
        /// The GetTooltipAttribute method returns the [Tooltip(")] attribute for the given FieldInfo.
        /// </summary>
        /// <param name="fieldInfo">The FieldInfo to get the tooltip attribute from.</param>
        /// <returns>A TooltipAttribute linked to the given FieldInfo.</returns>
        public static TooltipAttribute GetTooltipAttribute(FieldInfo fieldInfo)
        {
            return (TooltipAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TooltipAttribute));
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.Log("VAR");
            
            label.tooltip = GetTooltipAttribute(fieldInfo)?.tooltip ?? string.Empty;

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                
                SerializedProperty assemblyQualifiedTypeNameProperty = property.FindPropertyRelative("assemblyQualifiedTypeName");
                int? index = property.TryGetIndex();
                label.text = index == null ? label.text : $"Element {index}";
                Rect buttonPosition = EditorGUI.PrefixLabel(position, label);

                if (type?.AssemblyQualifiedName != assemblyQualifiedTypeNameProperty.stringValue)
                {
                    type = Type.GetType(assemblyQualifiedTypeNameProperty.stringValue);
                }
                // Debug.Log(type.Name);


                if (!GUI.Button(buttonPosition, new GUIContent(type?.Name, type?.FullName)))
                {
                    Debug.Log("VAR");
                    return;
                }

                Rect creatorRect = new Rect
                {
                    min = GUIUtility.GUIToScreenPoint(position.min),
                    max = GUIUtility.GUIToScreenPoint(position.max)
                };
                
                Type baseType = ((TypeSelectorAttribute)attribute).baseType;
                PickerWindow.Show(
                    creatorRect,
                    AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(
                            possibleComponentType =>
                            {
                                AddComponentMenu addComponentMenuAttribute = possibleComponentType
                                    .GetCustomAttributes<AddComponentMenu>(true)
                                    .FirstOrDefault();
                                return baseType.IsAssignableFrom(possibleComponentType)
                                    && !possibleComponentType.IsAbstract
                                    && !possibleComponentType.IsNestedPrivate
                                    && (addComponentMenuAttribute == null
                                        || !string.IsNullOrWhiteSpace(addComponentMenuAttribute.componentMenu));
                            })
                        .OrderBy(componentType => componentType.Name),
                    selectedType =>
                    {
                        assemblyQualifiedTypeNameProperty.stringValue = selectedType.AssemblyQualifiedName;
                        property.serializedObject.ApplyModifiedProperties();
                    },
                    searchedType => searchedType.Name,
                    drawnType => new GUIContent(
                        ObjectNames.NicifyVariableName(drawnType.Name),
                        AssetPreview.GetMiniTypeThumbnail(drawnType),
                        drawnType.FullName));
            }
        }
    }
}