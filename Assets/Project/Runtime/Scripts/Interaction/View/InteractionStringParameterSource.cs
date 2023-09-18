// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	InteractionStringParameterSource.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-27 오후 5:21
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.UI;

namespace Com2Verse.Interaction
{
	public class InteractionStringParameterSource
	{
		public InteractionStringParameterSource(int length)
		{
			_parameters = new string[length];
		}
		
		public int Length => _parameters.Length;
		private string[] _parameters;
		
		public InteractionUIViewModel InteractionViewModel { get; set; }

		private void RefreshView()
		{
			InteractionViewModel?.Refresh();
		}
		
		public string GetParameter(int index) => _parameters[index];

		public void SetParameter(int index, string value)
		{
			_parameters[index] = value;
			RefreshView();
		}
	}
}
