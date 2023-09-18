/*===============================================================
* Product:		Com2Verse
* File Name:	RelativeAnchor.cs
* Developer:	haminjeong
* Date:			2022-07-20 11:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.UIExtension
{
#if UNITY_EDITOR
    [CustomEditor(typeof(RelativeAnchor), true)]
    public sealed class RelativeAnchorEditor : Editor
    {
        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    [ExecuteInEditMode]
    public sealed class RelativeAnchor : UIBehaviour
    {
        public enum RefreshCycle
        {
            OnStart,
            OnEnable,
            OnUpdate
        }

        public enum HorizonOffsetPosition
        {
            None,
            Left,
            Right,
            Center
        }

        public enum VerticalOffsetPosition
        {
            None,
            Top,
            Bottom,
            Center
        }

        [System.Serializable]
        public struct HorizonTargetAnchor
        {
            public HorizonOffsetPosition targetAnchor;
            public float offset;
        }

        [System.Serializable]
        public struct VerticalTargetAnchor
        {
            public VerticalOffsetPosition targetAnchor;
            public float offset;
        }

        public RefreshCycle refreshCycle;
        public RectTransform target;
        public HorizonTargetAnchor left, right;
        public VerticalTargetAnchor bottom, top;

        protected override void Start()
        {
            if (refreshCycle == RefreshCycle.OnStart)
            {
                UpdateAnchor();
            }
        }

        protected override void OnEnable()
        {
            if (refreshCycle == RefreshCycle.OnEnable)
            {
                UpdateAnchor();
            }
        }

        private void Update()
        {
            if (refreshCycle == RefreshCycle.OnUpdate)
            {
                UpdateAnchor();
            }
        }

        public void UpdateAnchor()
        {
            if (target == null) return;
            RectTransform rt = GetComponent<RectTransform> ();
            if (rt == null) return;

            rt.anchorMin = rt.anchorMax = rt.pivot = target.anchorMin = target.anchorMax = new Vector2 (0.5f, 0.5f);
            Vector3 targetPos = target.localPosition;
            targetPos.x += (0.5f - target.pivot.x) * target.sizeDelta.x;
            targetPos.y += (0.5f - target.pivot.y) * target.sizeDelta.y;
            Vector3 pos = targetPos;
            Vector2 sizeDelta = Vector2.zero;
            if (right.targetAnchor != HorizonOffsetPosition.None)
            {
                if (right.targetAnchor == HorizonOffsetPosition.Left)
                {
                    sizeDelta.x = targetPos.x - target.sizeDelta.x / 2 + right.offset;
                    pos.x = sizeDelta.x;
                }
                else if (right.targetAnchor == HorizonOffsetPosition.Center)
                {
                    sizeDelta.x = targetPos.x + right.offset;
                    pos.x = sizeDelta.x;
                }
                else
                {
                    sizeDelta.x = targetPos.x + target.sizeDelta.x / 2 + right.offset;
                    pos.x = sizeDelta.x;
                }

                if (left.targetAnchor == HorizonOffsetPosition.Left)
                {
                    sizeDelta.x -= targetPos.x - target.sizeDelta.x / 2 + left.offset;
                    pos.x -= sizeDelta.x / 2;
                }
                else if (left.targetAnchor == HorizonOffsetPosition.Center)
                {
                    sizeDelta.x -= targetPos.x + left.offset;
                    pos.x -= sizeDelta.x / 2;
                }
                else if (left.targetAnchor == HorizonOffsetPosition.Right)
                {
                    sizeDelta.x -= targetPos.x + target.sizeDelta.x / 2 + left.offset;
                    pos.x -= sizeDelta.x / 2;
                }
                else
                {
                    Vector2 pivot = rt.pivot;
                    pivot.x = 1;
                    rt.pivot = pivot;
                    sizeDelta.x = rt.sizeDelta.x;
                }
            }
            else
            {
                Vector2 pivot = rt.pivot;
                pivot.x = 0;
                if (left.targetAnchor == HorizonOffsetPosition.Left)
                    pos.x = targetPos.x - target.sizeDelta.x / 2 + left.offset;
                else if (left.targetAnchor == HorizonOffsetPosition.Center)
                    pos.x = targetPos.x + left.offset;
                else if (left.targetAnchor == HorizonOffsetPosition.Right)
                    pos.x = targetPos.x + target.sizeDelta.x / 2 + left.offset;
                else
                {
                    pivot.x = 0.5f;
                    pos.x = rt.localPosition.x;
                }
                rt.pivot = pivot;
                sizeDelta.x = rt.sizeDelta.x;
            }

            if (bottom.targetAnchor != VerticalOffsetPosition.None)
            {
                if (bottom.targetAnchor == VerticalOffsetPosition.Top)
                {
                    sizeDelta.y = targetPos.y + target.sizeDelta.y / 2 + bottom.offset;
                    pos.y = sizeDelta.y;
                }
                else if (bottom.targetAnchor == VerticalOffsetPosition.Center)
                {
                    sizeDelta.y = targetPos.y + bottom.offset;
                    pos.y = sizeDelta.y;
                }
                else
                {
                    sizeDelta.y = targetPos.y - target.sizeDelta.y / 2 + bottom.offset;
                    pos.y = sizeDelta.y;
                }

                if (top.targetAnchor == VerticalOffsetPosition.Top)
                {
                    sizeDelta.y -= targetPos.y + target.sizeDelta.y / 2 + top.offset;
                    pos.y -= sizeDelta.y / 2;
                }
                else if (top.targetAnchor == VerticalOffsetPosition.Center)
                {
                    sizeDelta.y -= targetPos.y + top.offset;
                    pos.y -= sizeDelta.y / 2;
                }
                else if (top.targetAnchor == VerticalOffsetPosition.Bottom)
                {
                    sizeDelta.y -= targetPos.y - target.sizeDelta.y / 2 + top.offset;
                    pos.y -= sizeDelta.y / 2;
                }
                else
                {
                    Vector2 pivot = rt.pivot;
                    pivot.y = 0;
                    rt.pivot = pivot;
                    sizeDelta.y = rt.sizeDelta.y;
                }
            }
            else
            {
                Vector2 pivot = rt.pivot;
                pivot.y = 1;
                if (top.targetAnchor == VerticalOffsetPosition.Top)
                    pos.y = targetPos.y + target.sizeDelta.y / 2 + top.offset;
                else if (top.targetAnchor == VerticalOffsetPosition.Center)
                    pos.y = targetPos.y + top.offset;
                else if (top.targetAnchor == VerticalOffsetPosition.Bottom)
                    pos.y = targetPos.y - target.sizeDelta.y / 2 + top.offset;
                else
                {
                    pivot.y = 0.5f;
                    pos.y = rt.localPosition.y;
                }
                rt.pivot = pivot;
                sizeDelta.y = rt.sizeDelta.y;
            }

            rt.sizeDelta = sizeDelta;
            rt.localPosition = pos;
        }
    }
}
