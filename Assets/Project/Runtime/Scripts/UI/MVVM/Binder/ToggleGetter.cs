/*===============================================================
* Product:		Com2Verse
* File Name:	ToggleGetter.cs
* Developer:	tlghks1009
* Date:			2022-07-13 19:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] Toggle Getter")]
	[RequireComponent(typeof(MetaverseToggle))]
	public sealed class ToggleGetter : Binder
	{
		public override void Bind()
		{
			base.Bind();

			OneWayToSource();
		}


		protected override void OneWayToSource()
		{
			base.OneWayToSource();

			Subscribe(eBindingMode.ONE_WAY_TO_SOURCE);

			UpdateSourceProp();
		}


		protected override void InitializeSource()
		{
			SourceOwnerOfOneWaySource = _targetPath.component;
			SourcePropertyInfoOfOneWaySource = _targetPath.component.GetType().GetProperty(TargetPropertyName!);

			TargetOwnerOfOneWaySource = SourceViewModel;
			TargetPropertyInfoOfOneWaySource = SourceViewModel.GetType().GetProperty(SourcePropertyName!);

			_eventPath.property = "onValueChanged";
		}
	}
}
