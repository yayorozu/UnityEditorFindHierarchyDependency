using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Yorozu.EditorTool.FindHierarchyDependency
{
	[Serializable]
	internal class Dependency
	{
		[SerializeField]
		private Object _src;
		[SerializeField]
		private List<DependencyComponent> _hits;

		internal Object Src => _src;

		internal Dependency(Object src)
		{
			_src = src;
			_hits = new List<DependencyComponent>();
		}

		internal void Add(Component dest, FieldInfo fieldInfo)
		{
			_hits.Add(new DependencyComponent(dest, fieldInfo));
		}

		internal void OnGUI()
		{
			if (_src == null)
				return;

			EditorGUILayout.LabelField(_src.GetType().Name, EditorStyles.boldLabel);

			foreach (var component in _hits)
			{
				using (new EditorGUILayout.VerticalScope("box"))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.PrefixLabel("GameObject");
						EditorGUILayout.ObjectField(component.Dest.gameObject, typeof(GameObject));
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.PrefixLabel("Component");
						EditorGUILayout.ObjectField(
							component.Script == null ? component.Dest : component.Script,
							typeof(Object));
					}

					EditorGUILayout.LabelField("FieldName", component.FieldName);
				}
			}
		}
	}

	[Serializable]
	internal class DependencyComponent
	{
        /// <summary>
        /// オブジェクトを参照しているコンポーネント
        /// </summary>
        [SerializeField]
		internal Component Dest;
        /// <summary>
        /// オブジェクトを利用しているフィールド
        /// </summary>
        [SerializeField]
		internal string FieldName;

        [SerializeField]
        internal UnityEngine.Object Script;

		internal DependencyComponent(Component dest, FieldInfo fieldInfo)
		{
			Dest = dest;
			FieldName = fieldInfo.Name;

			var finds = AssetDatabase.FindAssets($"{dest.GetType().Name} t:Script");
			if (finds.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(finds.First());
				Script = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			}
		}
	}
}
