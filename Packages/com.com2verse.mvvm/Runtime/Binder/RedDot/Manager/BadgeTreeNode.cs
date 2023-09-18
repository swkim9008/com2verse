/*===============================================================
* Product:		Com2Verse
* File Name:	BadgeTreeNode.cs
* Developer:	NGSG
* Date:			2023-04-19 11:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

namespace Com2Verse
{
    [System.Serializable]
	public sealed class BadgeTreeNode<T> : IEnumerable<BadgeTreeNode<T>>
	{
        // public T Data { get; set; }
        // public BadgeTreeNode<T> Parent { get; set; }
        // public ICollection<BadgeTreeNode<T>> Children { get; set; }
        
        public T Data;
        public BadgeTreeNode<T> Parent { get; set; }
        public List<BadgeTreeNode<T>> Children;

        public bool IsRoot
        {
            get { return Parent == null; }
        }

        public bool IsLeaf
        {
            get { return Children.Count == 0; }
        }

        public int Level
        {
            get
            {
                if (this.IsRoot)
                    return 0;
                return Parent.Level + 1;
            }
        }


        public BadgeTreeNode(T data)
        {
            this.Data = data;
            //this.Children = new LinkedList<BadgeTreeNode<T>>();
            this.Children = new List<BadgeTreeNode<T>>();

            //this.ElementsIndex = new LinkedList<BadgeTreeNode<T>>();
            this.ElementsIndex = new List<BadgeTreeNode<T>>();
            this.ElementsIndex.Add(this);
        }

        public BadgeTreeNode<T> AddChild(T child)
        {
            BadgeTreeNode<T> childNode = new BadgeTreeNode<T>(child) { Parent = this };
            this.Children.Add(childNode);

            this.RegisterChildForSearch(childNode);

            return childNode;
        }

        public override string ToString()
        {
            return Data != null ? Data.ToString() : "[data null]";
        }


        #region searching
        
        private ICollection<BadgeTreeNode<T>> ElementsIndex { get; set; }

        private void RegisterChildForSearch(BadgeTreeNode<T> node)
        {
            ElementsIndex.Add(node);
            if (Parent != null)
                Parent.RegisterChildForSearch(node);
        }

        public BadgeTreeNode<T> FindTreeNode(Func<BadgeTreeNode<T>, bool> predicate)
        {
            return this.ElementsIndex.FirstOrDefault(predicate);
        }

        #endregion


        #region iterating
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<BadgeTreeNode<T>> GetEnumerator()
        {
            yield return this;
            foreach (var directChild in this.Children)
            {
                foreach (var anyChild in directChild)
                    yield return anyChild;
            }
        }

        #endregion
	}
}
