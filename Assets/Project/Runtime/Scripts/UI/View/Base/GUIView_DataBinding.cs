/*===============================================================
* Product:		Com2Verse
* File Name:	NewBehaviourScript.cs
* Developer:	tlghks1009
* Date:			2022-07-25 17:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
    public partial class GUIView : IBindingContainer
    {
        public ViewModelContainer ViewModelContainer { get; } = new();

        private readonly List<Binder> _binders = new();

        public Transform GetTransform() => this.transform;

        public void Bind()
        {
            if (_binders.Count == 0)
            {
                FindBinderList();
            }

            ViewModelContainer.InitializeViewModel();

            foreach (var binder in _binders)
            {
                RedDotBinder redDotBinder = binder as RedDotBinder;
                if (redDotBinder.IsReferenceNull() == false)
                {
                    var redDotViewModel = new RedDotViewModel(new RedDotData(redDotBinder._badgeType, 0));
                    RedDotManager.Instance.CreateCollectionItem(redDotBinder.gameObject, redDotViewModel);
                }
                else
                {
                    binder.SetViewModelContainer(ViewModelContainer, _allowDuplicate);
                    binder.Bind();
                }
            }
        }

        public void Unbind()
        {
            if (_alwaysBinded)
            {
                if (_binders.Count == 0)
                    Bind();
            }

            if (!_alwaysBinded && _binders.Count != 0)
            {
                ForceUnbind();
            }
        }

        public void ForceUnbind()
        {
            if (_binders.Count == 0)
                return;

            foreach (var binder in _binders)
            {
                if (binder.IsUnityNull()) continue;
                binder.Unbind();
            }
        }

        private void FindBinderList()
        {
            var bindingContainers = GetComponentsInChildren<IBindingContainer>(true);
            var bindersOfChildren = GetComponentsInChildren<Binder>(true);

            var skipBinderList = new List<Binder>();
            if (bindingContainers.Length != 0)
            {
                foreach (var bindingContainer in bindingContainers)
                {
                    if (bindingContainer == (IBindingContainer)this)
                        continue;

                    var childBinderListOfBindingContainer = bindingContainer.GetTransform().GetComponentsInChildren<Binder>(true);
                    foreach (var binder in childBinderListOfBindingContainer)
                    {
                        if (bindingContainer.GetTransform() == binder.transform)
                        {
                            continue;
                        }

                        skipBinderList.Add(binder);
                    }
                }
            }

            // 레드닷 차일드들은 제거한다
            GetRedDotBinderChild(bindersOfChildren, ref skipBinderList);

            foreach (var binder in bindersOfChildren)
            {
                if (skipBinderList.Contains(binder))
                    continue;

                _binders.Add(binder);
            }
        }

        private void GetRedDotBinderChild(Binder[] bindersOfChildren, ref List<Binder> skipBinderList)
        {
            // 레드닷 차일드들은 제거한다
            foreach (var binder in bindersOfChildren)
            {
                RedDotBinder redDotBinder = binder as RedDotBinder;
                if (redDotBinder.IsReferenceNull() == false)
                {
                    var childBinderListOfBindingContainer = redDotBinder.GetComponentsInChildren<Binder>(true);
                    foreach (var redDotbinderChild in childBinderListOfBindingContainer)
                    {
                        if (redDotbinderChild is RedDotBinder)
                            continue;
                        
                        skipBinderList.Add(redDotbinderChild);
                    }
                }
            }
        }
        
        private void ResetDataBinding()
        {
            ViewModelContainer.ClearAll();
            _binders.Clear();
        }
    }
}
