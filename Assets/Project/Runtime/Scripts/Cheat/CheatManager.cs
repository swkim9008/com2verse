/*===============================================================
* Product:		Com2Verse
* File Name:	CheatManager.cs
* Developer:	jehyun
* Date:			2022-07-08 13:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Cheat
{
	public sealed class MetaverseCheatAttribute : Attribute
	{

		public MetaverseCheatAttribute(string name)
		{
			Name = name;
		}
		public string Name { get; } = string.Empty;
	}

	public class HelpTextAttribute : Attribute
	{

		public HelpTextAttribute(params string[] texts)
		{
			HelpTexts = texts;
		}
		public string[] HelpTexts { get; } = null;
	}

	public class CheatNode
	{
		public CheatNode Parent { get; set; } = null;
		public List<CheatNode> Children { get; set; } = new();
		public string NodeName { get; set; } = string.Empty;
		public CheatInfo CheatInfo { get; set; } = null;
		public bool IsRootNode => Parent == null;
		public bool IsLastNode => Children.Count == 0 && CheatInfo != null;

		public string NodePath()
		{
			var path = NodeName;
			var parentNode = Parent;
			while (parentNode != null)
			{
				path = $"{parentNode.NodeName}/{path}";
				parentNode = parentNode.Parent;
			}

			return path;
		}
	}

	public class CheatInfo
	{
		public MethodInfo MethodInfo { get; init; } = null;
		public string CheatName { get; init; } = string.Empty;
		public string[] HelpTexts { get; set; } = null;
	}

	public sealed class CheatManager : MonoSingleton<CheatManager>
	{

		private readonly SortedDictionary<string, CheatInfo> _cheatContainer = new SortedDictionary<string, CheatInfo>();
		private readonly Dictionary<string, List<string>> _cheatNameContainer = new Dictionary<string, List<string>>();
		private readonly string CHEAT_WINDOW_PREFAB = "UI_Cheat";
		public readonly string ROOT_NODE_NAME = "Cheat";
		private GUIView _cheatWindow = null;
		private CheatNode _rootNode = null;

		public CheatNode RootNode
		{
			get
			{
				if (_rootNode == null)
					_rootNode = new CheatNode() { NodeName = ROOT_NODE_NAME };

				return _rootNode;
			}
		}


#if UNITY_STANDALONE && ENABLE_CHEATING
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.F11))
				OpenCheat();
		}
#endif

		private void OpenCheat()
		{
			C2VDebug.LogWarning("Open Cheat");

			if (_cheatWindow != null)
			{
				if (_cheatWindow.VisibleState == GUIView.eVisibleState.OPENED)
					_cheatWindow.Hide();
				else
					_cheatWindow.Show();
			}
			else
				InitAllCheat();
		}

		private void InitAllCheat()
		{
			FindCheatFunction();

			foreach (KeyValuePair<string, CheatInfo> pair in _cheatContainer)
				MakeCheatNodes(pair.Key);

			SortNodeByName(RootNode);

			LoadCheatWindow();
		}

		private void MakeCheatNodes(string key)
		{
			var splits = key.Split(new char[] { '/' });
			var currentNode = RootNode;

			for (int i = 0; i < splits.Length; ++i)
			{
				var nodeName = splits[i];
				if (i == 0)
				{
					if (nodeName.Equals(ROOT_NODE_NAME))
						continue;
					else
					{
						C2VDebug.LogWarning("The first node in the cheat must be \"Cheat\". - " + key);
						break;
					}
				}

				currentNode = AddNode(currentNode, nodeName);
				// last node.
				if (i == splits.Length - 1)
				{
					var nodePath = currentNode.NodePath();
					if (!_cheatContainer.TryGetValue(nodePath, out var cheatInfo))
						C2VDebug.LogError($"Not found nodePath : {nodePath}");
					else
						currentNode.CheatInfo = cheatInfo;
				}
			}
		}

		private CheatNode AddNode(CheatNode currentNode, string childName)
		{
			var childNode = currentNode.Children.Find((node) => node.NodeName.Equals(childName));
			if (childNode == null)
				currentNode.Children.Add(childNode = new CheatNode() { Parent = currentNode, NodeName = childName });

			return childNode;
		}

		private void FindCheatFunction()
		{
			Assembly referencedAssembly = Assembly.GetExecutingAssembly();

			var types = referencedAssembly.GetTypes();

			foreach (var type in types)
			{
				var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				foreach (var method in methods)
				{
					var attrs = method.GetCustomAttributes(typeof(MetaverseCheatAttribute), true);
					if (attrs.Length == 0)
						continue;

					if (FilterParam(method))
						continue;

					var helpAttrs = method.GetCustomAttributes(typeof(HelpTextAttribute), true);

					for (int i = 0; attrs.Length > i; ++i)
					{
						CheatInfo cheat = new CheatInfo
						{
							MethodInfo = method,
							CheatName = (attrs[i] as MetaverseCheatAttribute)?.Name,
						};

						if (helpAttrs.Length > i)
							cheat.HelpTexts = (helpAttrs[i] as HelpTextAttribute).HelpTexts;

						if (_cheatContainer.ContainsKey(cheat.CheatName))
							C2VDebug.LogWarning("Duplicate Cheat[" + cheat.CheatName + "]");
						else
							_cheatContainer[cheat.CheatName] = cheat;
					}
				}
			}
		}

		private bool FilterParam(MethodInfo method)
		{
			var isFilter = false;
			var paramInfos = method.GetParameters();
			foreach (ParameterInfo param in paramInfos)
			{
				if (param.ParameterType != typeof(string))
				{
					isFilter = true;
					break;
				}
			}

			return isFilter;
		}

		private void SortNodeByName(CheatNode node)
		{
			if (node.Children.Count > 0)
			{
				node.Children.Sort((lNode, rNode) => lNode.NodeName.CompareTo(rNode.NodeName));
				foreach (var childNode in node.Children)
					SortNodeByName(childNode);
			}
		}

		private void LoadCheatWindow()
		{
			if (_cheatWindow != null)
			{
				_cheatWindow.Show();
				return;
			}

			if (_cheatWindow.IsUnityNull())
			{
				UIManager.Instance.CreatePopup("UI_Cheat", (cheatWindow) =>
				{
					_cheatWindow = cheatWindow;
					_cheatWindow.Show();
				}).Forget();
			}
		}

#if UNITY_EDITOR && ENABLE_CHEATING

		[UnityEditor.MenuItem("Com2Verse/Cheat/OpenCheat", false)]
		static void CheatOpenCheat()
		{
			CheatManager.Instance.OpenCheat();
		}

#endif
	}
}
