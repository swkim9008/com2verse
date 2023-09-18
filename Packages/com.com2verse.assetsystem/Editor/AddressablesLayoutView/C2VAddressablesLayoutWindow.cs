/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesLayoutWindow.cs
* Developer:	tlghks1009
* Date:			2023-03-23 10:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Com2VerseEditor.AssetSystem
{
	public class C2VAddressablesLayoutWindow : EditorWindow
	{
		private readonly List<C2VAddressablesBaseLayoutView> _layoutViews = new();

		private Action _onGUIHandler;
		public event Action OnGUIHandler
		{
			add
			{
				_onGUIHandler -= value;
				_onGUIHandler += value;
			}
			remove => _onGUIHandler -= value;
		}

		private void OnEnable()
		{
			CreateView<C2VAddressablesAssetView>();
			CreateView<C2VAddressablesReferenceByView>();
			CreateView<C2VAddressablesReferenceToView>();

			ShowView(FindView<C2VAddressablesAssetView>());
		}

		public T CreateView<T>() where T : C2VAddressablesBaseLayoutView, new()
		{
			T instance = new T();
			instance.Window = this;

			_layoutViews.Add(instance);

			return instance;
		}


		public void ShowView<T>(T view) where T : C2VAddressablesBaseLayoutView
		{
			view.Show();
		}


		public T FindView<T>() where T : C2VAddressablesBaseLayoutView
		{
			foreach (var layoutView in _layoutViews)
			{
				if (layoutView is T view)
				{
					return view;
				}
			}

			return null;
		}

		private void OnGUI()
		{
			_onGUIHandler?.Invoke();
		}


		[MenuItem("Com2Verse/AssetSystem/Addressable Dependencies Viewer")]
		public static void OpenWindow()
		{
			GetWindow<C2VAddressablesLayoutWindow>("Addressable Dependencies Viewer");
		}
	}
}
