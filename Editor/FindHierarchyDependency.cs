using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Yorozu.EditorTool.FindHierarchyDependency
{
	internal static class FindHierarchyDependency
	{
		[MenuItem("GameObject/Find Dependency", false, Int32.MinValue)]
		private static void FindDepend()
		{
			var obj = Selection.activeGameObject;

			if (obj == null)
				return;

			var hits = Find(obj);
			if (hits.Count <= 0)
			{
				EditorUtility.DisplayDialog(
					"Search Result",
					"Not Found Object Dependency",
					"OK"
				);
				return;
			}

			FindDependencyResultWindow.ShowWindow(obj, hits);
		}

		private static BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		/// <summary>
		/// 対象のオブジェクトもしくはオブジェクトのComponent を利用しているものを検索
		/// </summary>
		private static List<Dependency> Find(GameObject target)
		{
			var hits = new List<Dependency>();

			Object[] finds = target.GetComponents<Component>();
			ArrayUtility.Add(ref finds, target);

			var sceneObjects = GetSceneObjects(target, true);
			for (var i = 0; i < sceneObjects.Count; i++)
			{
				EditorUtility.DisplayProgressBar("Search Dependency", $"Progress {i + 1} / {sceneObjects.Count}", i + 1 / (float) sceneObjects.Count);
				foreach (var c in sceneObjects[i].GetComponents<Component>())
				{
					var type = c.GetType();
					foreach (var f in type.GetFields(Flags))
					{
						if (!f.IsPublic && !f.IsDefined(typeof(SerializeField)))
							continue;

						if (!f.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
							continue;

						foreach (var find in finds)
						{
							if (f.GetValue(c) != find)
								continue;

							var index = hits.FindIndex(h => h.Src == find);
							if (index < 0)
							{
								hits.Add(new Dependency(find));
								index = hits.Count - 1;
							}

							hits[index].Add(c, f);
						}
					}
				}
			}

			EditorUtility.ClearProgressBar();

			return hits;
		}

		/// <summary>
		/// 全シーンのオブジェクトを取得する
		/// </summary>
		private static List<GameObject> GetSceneObjects(GameObject target, bool isSameScene)
		{
			var objs = new List<GameObject>(100);
			var count = SceneManager.sceneCount;
			for (var i = 0; i < count; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (isSameScene && target.scene != scene)
					continue;

				foreach (var obj in scene.GetRootGameObjects())
					FindRecursive(ref objs, obj);
			}

			return objs;
		}

		/// <summary>
		/// 再帰的にオブジェクトを取得
		/// </summary>
		private static void FindRecursive(ref List<GameObject> list, GameObject root)
		{
			list.Add(root);
			foreach (Transform child in root.transform)
				FindRecursive(ref list, child.gameObject);
		}
	}
}
