/*===============================================================
* Product:		Com2Verse
* File Name:	Binder_Synchronizer.cs
* Developer:	tlghks1009
* Date:			2022-11-24 18:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Reflection;
using Com2Verse.Logger;

namespace Com2Verse.UI
{
    public abstract partial class Binder
    {
#region UpdateProp
        protected void UpdateTargetProp()
        {
            if (!TryUpdateTargetProp(SourceOwnerOfOneWayTarget, SourcePropertyName))
            {
                UpdateTargetProp(GetValue(SourcePropertyInfoOfOneWayTarget, SourceOwnerOfOneWayTarget));
            }
        }

        protected void UpdateSourceProp()
        {
            if (!TryUpdateSourceProp(SourceOwnerOfOneWaySource, TargetPropertyName))
            {
                UpdateSourceProp(GetValue(SourcePropertyInfoOfOneWaySource, SourceOwnerOfOneWaySource));
            }
        }

        private bool TryUpdateTargetProp(object owner, string propertyName)
        {
            if (PropertyPathAccessors.TryGetIntValue(owner, propertyName, out var intValue))
            {
                UpdateTargetProp(intValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetFloatValue(owner, propertyName, out var floatValue))
            {
                UpdateTargetProp(floatValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetBoolValue(owner, propertyName, out var boolValue))
            {
                UpdateTargetProp(boolValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetDoubleValue(owner, propertyName, out var doubleValue))
            {
                UpdateTargetProp(doubleValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetDecimalValue(owner, propertyName, out var decimalValue))
            {
                UpdateTargetProp(decimalValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetLongValue(owner, propertyName, out var longValue))
            {
                UpdateTargetProp(longValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetObjectValue(owner, propertyName, out var objectValue))
            {
                UpdateTargetProp(objectValue);
                return true;
            }

            return false;
        }

        private bool TryUpdateSourceProp(object owner, string propertyName)
        {
            if (PropertyPathAccessors.TryGetIntValue(owner, propertyName, out var intValue))
            {
                UpdateSourceProp(intValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetFloatValue(owner, propertyName, out var floatValue))
            {
                UpdateSourceProp(floatValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetBoolValue(owner, propertyName, out var boolValue))
            {
                UpdateSourceProp(boolValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetLongValue(owner, propertyName, out var longValue))
            {
                UpdateSourceProp(longValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetDecimalValue(owner, propertyName, out var decimalValue))
            {
                UpdateSourceProp(decimalValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetDoubleValue(owner, propertyName, out var doubleValue))
            {
                UpdateSourceProp(doubleValue);
                return true;
            }

            if (PropertyPathAccessors.TryGetObjectValue(owner, propertyName, out var objectValue))
            {
                UpdateSourceProp(objectValue);
                return true;
            }

            return false;
        }

        private void UpdateTargetProp(object propValue)
        {
            if (!PropertyPathAccessors.TrySetValue(TargetOwnerOfOneWayTarget, TargetPropertyName, propValue))
            {
                SetValue(TargetPropertyInfoOfOneWayTarget, TargetOwnerOfOneWayTarget, propValue);
            }
        }

        private void UpdateTargetProp<T>(T propValue) where T : unmanaged, IConvertible
        {
            if (!PropertyPathAccessors.TrySetValue(TargetOwnerOfOneWayTarget, TargetPropertyName, propValue))
            {
                SetValue(TargetPropertyInfoOfOneWayTarget, TargetOwnerOfOneWayTarget, propValue);
            }
        }

        private void UpdateSourceProp(object propValue)
        {
            if (!PropertyPathAccessors.TrySetValue(TargetOwnerOfOneWaySource, SourcePropertyName, propValue))
            {
                SetValue(TargetPropertyInfoOfOneWaySource, TargetOwnerOfOneWaySource, propValue);
            }
        }

        private void UpdateSourceProp<T>(T propValue) where T : unmanaged, IConvertible
        {
            if (!PropertyPathAccessors.TrySetValue(TargetOwnerOfOneWaySource, SourcePropertyName, propValue))
            {
                SetValue(TargetPropertyInfoOfOneWaySource, TargetOwnerOfOneWaySource, propValue);
            }
        }
#endregion UpdateProp

#region Command
        protected void RaiseCommand(PropertyInfo propertyInfo, object owner, object additionalData)
        {
            if (propertyInfo == null)
            {
                C2VDebug.LogWarning($"[Binding Warning] {MVVMUtil.GetFullPathInHierarchy(this.transform)} - CommandHandler TargetProperty is empty.");
                return;
            }

            var commandHandler = propertyInfo.GetValue(owner) as ICommand;

            commandHandler?.Invoke(additionalData);
        }
#endregion Command

#region GetValue
        private object GetValue(PropertyInfo propertyInfo, object owner)
        {
            try
            {
                return propertyInfo?.GetValue(owner);
            }
            catch (Exception e)
            {
                C2VDebug.LogWarning(e.Message);
                return null;
            }
        }
#endregion GetValue

#region SetValue
        private void SetValue(PropertyInfo propertyInfo, object owner, object value)
        {
            try
            {
                propertyInfo?.SetValue(owner, value);
            }
            catch (Exception e)
            {
                C2VDebug.LogWarning($"{e.Message} \n " +
                                    $"Binding Exception. Path : {MVVMUtil.GetFullPathInHierarchy(this.transform)} \n" +
                                    $"PropertyType : {propertyInfo.PropertyType} Owner : {owner} Value : {value} \n"  +
                                    $"TargetPath : {_targetPath.propertyOwner} / {_targetPath.property} SourcePath : {_sourcePath.propertyOwner} / {_sourcePath.property}");
            }
        }

        private void SetValue<T>(PropertyInfo propertyInfo, object owner, T value) where T : unmanaged, IConvertible
        {
            try
            {
                propertyInfo?.SetValue(owner, value);
            }
            catch (Exception e)
            {
                C2VDebug.LogWarning($"{e.Message} \n " +
                                    $"Binding Exception. Path : {MVVMUtil.GetFullPathInHierarchy(this.transform)} \n" +
                                    $"PropertyType : {propertyInfo.PropertyType} Owner : {owner} Value : {value} \n"  +
                                    $"TargetPath : {_targetPath.propertyOwner} / {_targetPath.property} SourcePath : {_sourcePath.propertyOwner} / {_sourcePath.property}");
            }
        }
#endregion SetValue
    }
}
