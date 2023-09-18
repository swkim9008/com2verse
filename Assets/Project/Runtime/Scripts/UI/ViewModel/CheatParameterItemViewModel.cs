/*===============================================================
* Product:		Com2Verse
* File Name:	CheatParameterItemViewModel.cs
* Developer:	jehyun
* Date:			2022-07-11 11:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	[ViewModelGroup("Cheat")]
	public sealed class CheatParameterItemViewModel : ViewModelBase
	{
		private string _parameterDescription;
		private string _parameterName;
		private string _parameterValue;

		public string ParameterName
		{
			get => _parameterName;
			set
			{
				_parameterName = value;
				base.InvokePropertyValueChanged(nameof(ParameterName), value);
			}
		}

		/// <summary>
		/// Placeholder에 해당됨.
		/// </summary>
		public string ParameterDescription
		{
			get => _parameterDescription;
			set
			{
				_parameterDescription = value;
				base.InvokePropertyValueChanged(nameof(ParameterDescription), value);
			}
		}

		public string ParameterValue
		{
			get => _parameterValue;
			set
			{
				_parameterValue = value;
				base.InvokePropertyValueChanged(nameof(ParameterValue), value);
			}
		}
	}
}
