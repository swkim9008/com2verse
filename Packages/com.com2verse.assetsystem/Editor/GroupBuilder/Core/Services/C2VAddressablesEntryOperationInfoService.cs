/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEntryOperationInfoService.cs
* Developer:	tlghks1009
* Date:			2023-03-07 11:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesEntryOperationInfoService : C2VAddressablesGroupBuilderServiceBase
	{
		private C2VAddressablesEditorAdapter _addressableEditorAdapter;

		public C2VAddressablesEntryOperationInfoService(IServicePack servicePack) : base(servicePack)
		{
			_addressableEditorAdapter = servicePack.GetAdapterPack().AddressablesEditorAdapter;
		}

		public void Apply(C2VAddressablesEntryOperationInfo operationInfo)
		{
			if (operationInfo.GroupRemoveInfo is {IsValid: true})
			{
				ApplyRemoveGroup(operationInfo.GroupRemoveInfo);
			}

			if (operationInfo.EntryRemoveInfo is {IsValid: true})
			{
				ApplyRemoveEntry(operationInfo.EntryRemoveInfo);
			}

			if (operationInfo.EntryCreateInfo is {IsValid: true})
			{
				ApplyCreateOrMoveEntry(operationInfo.EntryCreateInfo);
			}
		}


		public bool IsValid(C2VAddressablesEntryOperationInfo operationInfo)
		{
			bool processed = false;

			if (operationInfo.GroupRemoveInfo != null)
			{
				processed |= operationInfo.GroupRemoveInfo.IsValid = CanRemoveGroup(operationInfo.GroupRemoveInfo);
			}

			if (operationInfo.EntryRemoveInfo != null)
			{
				processed |= operationInfo.EntryRemoveInfo.IsValid = CanRemoveEntry(operationInfo.EntryRemoveInfo);
			}

			if (operationInfo.EntryCreateInfo != null)
			{
				processed |= operationInfo.EntryCreateInfo.IsValid = CanCreateOrMoveEntry(operationInfo);
			}

			return processed;
		}


		private void ApplyCreateOrMoveEntry(C2VAddressableEntryCreateInfo createInfo)
		{
			_addressableEditorAdapter.CreateOrMoveEntry(createInfo);
		}


		private void ApplyRemoveEntry(C2VAddressableEntryRemoveInfo removeInfo)
		{
			_addressableEditorAdapter.RemoveAssetEntry(removeInfo);
		}


		private void ApplyRemoveGroup(C2VAddressableGroupRemoveInfo removeGroupInfo)
		{
			_addressableEditorAdapter.RemoveGroup(removeGroupInfo.GroupName);
		}


		private bool CanCreateOrMoveEntry(C2VAddressablesEntryOperationInfo operationInfo)
		{
			return _addressableEditorAdapter.CanCreateOrMoveEntry(operationInfo);
		}


		private bool CanRemoveEntry(C2VAddressableEntryRemoveInfo removeInfo)
		{
			return _addressableEditorAdapter.CanRemoveAssetEntry(removeInfo.Guid);
		}

		private bool CanRemoveGroup(C2VAddressableGroupRemoveInfo removeGroupInfo)
		{
			return _addressableEditorAdapter.FindGroupByName(removeGroupInfo.GroupName) != null;
		}


		public override void Release()
		{
			_addressableEditorAdapter = null;
		}
	}
}
