/*===============================================================
* Product:		Com2Verse
* File Name:	LabelSearchResultViewModel.cs
* Developer:	jhkim
* Date:			2022-10-07 12:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	public sealed class LabelSearchResultViewModel : BaseSearchResultViewModel
	{
#region Variables
		private string _label;
		public MemberIdType ID { get; set; }
#endregion // Variables

#region Properties
		public string Label
		{
			get => _label;
			set
			{
				_label = value;
				InvokePropertyValueChanged(nameof(Label), value);
			}
		}
#endregion // Properties
	}
}
