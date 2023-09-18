// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderBaseViewModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오전 11:25
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.Builder;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse
{
	public sealed class BuilderBaseViewModel : ViewModelBase
	{
		public string SpaceName => SpaceManager.InstanceExists ? SpaceManager.Instance.SpaceID : string.Empty;
		public CommandHandler CommandSave { get; }
		public CommandHandler CommandExit { get; }
		public CommandHandler CommandToggleInventory { get; }
		public CommandHandler CommandCloseInventory { get; }
		public CommandHandler CommandUndo { get; }
		public CommandHandler CommandRedo { get; }
		public CommandHandler CommandToggleCullWall { get; }
		private List<InventoryItemViewModel> _inventoryObjectItemCollection = new List<InventoryItemViewModel>();
		private List<InventoryItemViewModel> _inventoryTextureItemCollection = new List<InventoryItemViewModel>();

		private eInventoryItemCategory _currentInventoryCategory = eInventoryItemCategory.NONE;
		public eInventoryItemCategory CurrentInventoryCategory
		{
			get => _currentInventoryCategory;
			set
			{
				if (_currentInventoryCategory != value)
				{
					_currentInventoryCategory = value;
					CurrentCollection.Reset();

					var targetCollection = value switch
					{
						eInventoryItemCategory.OBJECT  => _inventoryObjectItemCollection,
						eInventoryItemCategory.TEXTURE => _inventoryTextureItemCollection,
						_ => null
					};
					
					CurrentCollection.AddRange(targetCollection);
				}
			}
		}

		public bool OnTabGroupObject
		{
			get => false;
			set
			{
				if (value) CurrentInventoryCategory = eInventoryItemCategory.OBJECT;
			}
		}
		public bool OnTabGroupTexture
		{
			get => false;
			set
			{
				if (value) CurrentInventoryCategory = eInventoryItemCategory.TEXTURE;
			}
		}

		public Collection<InventoryItemViewModel> CurrentCollection { get; set; } = new Collection<InventoryItemViewModel>();

		public bool CanRedo => ActionSystem.Instance.CanRedo;
		public bool CanUndo => ActionSystem.Instance.CanUndo;
		public bool CullWall => SpaceManager.Instance.CullWall;

		public override void OnInitialize()
		{
			base.OnInitialize();
			ActionSystem.Instance.ActionPerformed += RefreshActionItems;
		}

		private void RefreshActionItems()
		{
			InvokePropertyValueChanged(nameof(CanRedo), CanRedo);
			InvokePropertyValueChanged(nameof(CanUndo), CanUndo);
		}
		
		public GameObject ParentScrollView
		{
			get { return _parentScrollView.gameObject; }
			set { _parentScrollView = value.GetComponent<ScrollRect>(); }
		}

		public GameObject CanvasRoot
		{
			get { return _canvasRoot.gameObject; }
			set { _canvasRoot = (RectTransform)value.transform; }
		}

		private ScrollRect _parentScrollView;
		private RectTransform _canvasRoot;
		
		public GameObject InventoryObject { get; set; }

		public void AddItemsToInventory(Dictionary<eInventoryItemCategory, Dictionary<long, BuilderInventoryItem>> itemsDictionary)
		{
			foreach (var kv in itemsDictionary)
			{
				var targetCollection = kv.Key switch
				{
					eInventoryItemCategory.OBJECT  => _inventoryObjectItemCollection,
					eInventoryItemCategory.TEXTURE => _inventoryTextureItemCollection,
					_ => null
				};
				foreach (var item in kv.Value)
				{
					targetCollection.Add(new InventoryItemViewModel()
					{
						Data = item.Value,
						CanvasRoot = _canvasRoot,
						ParentScrollView = _parentScrollView
					});
				}
			}

			CurrentInventoryCategory = eInventoryItemCategory.OBJECT;
		}

		public BuilderBaseViewModel()
		{
			CommandSave = new CommandHandler(OnClickSave);
			CommandExit = new CommandHandler(OnClickExit);
			CommandToggleInventory = new CommandHandler(OnToggleInventory);
			CommandCloseInventory = new CommandHandler(OnCloseInventory);
			CommandRedo = new CommandHandler(OnClickRedo);
			CommandUndo = new CommandHandler(OnClickUndo);
			CommandToggleCullWall = new CommandHandler(OnClickToggleWall);
		}

		private void OnClickToggleWall()
		{
			SpaceManager.Instance.CullWall = !SpaceManager.Instance.CullWall;
			InvokePropertyValueChanged(nameof(CullWall), CullWall);
		}

		private void OnClickUndo()
		{
			ActionSystem.Instance.Undo();
		}

		private void OnClickRedo()
		{
			ActionSystem.Instance.Redo();
		}

		private void OnToggleInventory()
		{
			InventoryObject.SetActive(!InventoryObject.activeSelf);
		}

		private void OnCloseInventory()
		{
			InventoryObject.SetActive(false);
		}

		private void OnClickSave()
		{
			UIManager.Instance.ShowPopupYesNo("저장하시겠습니까?", "저장하면 다시는 돌이킬 수 없습니다.", view =>
			{
				string json = BuilderSerializer.SerializeSpace();
				string builderDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "C2VBuilder");
				if (!System.IO.Directory.Exists(builderDataPath))
				{
					System.IO.Directory.CreateDirectory(builderDataPath);
				}
				File.WriteAllText(System.IO.Path.Combine(builderDataPath, $"{SpaceManager.Instance.SpaceID}{BuilderManager.TempDataExtension}"), json);
			});
		}

		private void OnClickExit()
		{
			UIManager.Instance.ShowPopupYesNo("종료하시겠습니까?", "종료하면 저장되지 않은 사항이 폐기됩니다.", view => { });
		}

		public static void Show()
		{
			UIManager.Instance.CreatePopup("UI_BuilderBase", view =>
			{
				view.Show();
				var viewModel = view.ViewModelContainer.GetViewModel<BuilderBaseViewModel>();
				viewModel.AddItemsToInventory(BuilderAssetManager.Instance.ItemListMap);
				SpaceManager.Instance.ActiveCamera(true);
			}).Forget();
		}
	}
}
