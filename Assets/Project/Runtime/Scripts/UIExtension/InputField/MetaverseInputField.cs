/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseInputField.cs
* Developer:	wlemon
* Date:			2023-08-11 11:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using TMPro;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	public sealed class MetaverseInputField : TMP_InputField
	{
		public override Selectable FindSelectableOnUp()
		{
			if (navigation.mode != Navigation.Mode.Explicit)
			{
				return base.FindSelectableOnUp();
			}

			var foundSelectable = base.FindSelectableOnUp();
			while (foundSelectable != null)
			{
				if (foundSelectable.gameObject.activeInHierarchy) break;
				foundSelectable = foundSelectable.FindSelectableOnUp();
			}

			return foundSelectable;
		}

		public override Selectable FindSelectableOnDown()
		{
			if (navigation.mode != Navigation.Mode.Explicit)
			{
				return base.FindSelectableOnDown();
			}

			var foundSelectable = base.FindSelectableOnDown();
			while (foundSelectable != null)
			{
				if (foundSelectable.gameObject.activeInHierarchy) break;
				foundSelectable = foundSelectable.FindSelectableOnDown();
			}

			return foundSelectable;
		}

		public override Selectable FindSelectableOnLeft()
		{
			if (navigation.mode != Navigation.Mode.Explicit)
			{
				return base.FindSelectableOnLeft();
			}

			var foundSelectable = base.FindSelectableOnLeft();
			while (foundSelectable != null)
			{
				if (foundSelectable.gameObject.activeInHierarchy) break;
				foundSelectable = foundSelectable.FindSelectableOnLeft();
			}

			return foundSelectable;
		}

		public override Selectable FindSelectableOnRight()
		{
			if (navigation.mode != Navigation.Mode.Explicit)
			{
				return base.FindSelectableOnRight();
			}

			var foundSelectable = base.FindSelectableOnRight();
			while (foundSelectable != null)
			{
				if (foundSelectable.gameObject.activeInHierarchy) break;
				foundSelectable = foundSelectable.FindSelectableOnRight();
			}

			return foundSelectable;
		}
	}
}
