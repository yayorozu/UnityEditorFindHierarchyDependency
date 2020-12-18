using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yorozu.EditorTool.FindHierarchyDependency
{
	internal class FindDependencyResultWindow : EditorWindow
	{
		internal static void ShowWindow(GameObject gameObject, List<Dependency> results)
		{
			var window = GetWindow<FindDependencyResultWindow>();
			window.titleContent = new GUIContent("HierarchyFindResult");
			window.SetResult(gameObject, results);
			window.Show();
		}

		private void SetResult(GameObject gameObject, List<Dependency> results)
		{
			_gameObject = gameObject;
			_results = results;
		}

		[SerializeField]
		private List<Dependency> _results;
		[SerializeField]
		private GameObject _gameObject;

		private Vector2 _scrollPosition;

		private void OnGUI()
		{
			if (_gameObject == null)
			{
				EditorGUILayout.HelpBox("GameObject Reference missing", MessageType.Warning);
				return;
			}
			
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel("Dependency Find Object");
				EditorGUILayout.ObjectField(_gameObject, typeof(GameObject));
			}

			GUILayout.Space(10);

			using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
			{
				_scrollPosition = scroll.scrollPosition;
				foreach (var result in _results)
				{
					result.OnGUI();
				}
			}
		}
	}
}
