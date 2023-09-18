/*===============================================================
* Product:    Com2Verse
* File Name:  INotifyPropertyChanged.cs
* Developer:  tlghks1009
* Date:       2022-03-11 10:52
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public interface IPropertySynchronizer
	{
		void AddBinder(Binder binder);
		void RemoveBinder(Binder binder);
		void InvokePropertyValueChanged(string propertyName);
	}
}
