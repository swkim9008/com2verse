/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebViewGraphicRaycasterManager.cs
* Developer:	sprite
* Date:			2023-09-03 14:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace Com2Verse.Mice
{
	public partial class MiceWebViewGraphicRaycasterManager : MonoBehaviour
        , INamedLogger<NamedLoggerTag.Sprite>
	{
        private void Awake()
        {
            this.OnTransformChildrenChanged();
        }

        private void OnDestroy()
        {
            MiceWebViewGraphicRaycasterManager.Clear();   
        }

        private static string GetTransformPath(Transform transform)
        {
#if UNITY_EDITOR
            return UnityEditor.AnimationUtility.CalculateTransformPath(transform, null);
#else
            return transform.gameObject.name;
#endif
        }

        private void OnTransformChildrenChanged()
        {
            for (int i = 0, cnt = this.transform.childCount; i < cnt; i++)
            {
                var child = this.transform.GetChild(i);

                if (!child.TryGetComponent(out GraphicRaycaster gr) || MiceWebViewGraphicRaycasterManager.Contains(gr)) continue;

                MiceWebViewGraphicRaycasterManager.Register(gr);
            }
        }
    }

    public partial class MiceWebViewGraphicRaycasterManager // GraphicRaycaster Contanier.
    {
        const int DEFAULT_CAPACITY = 10;

        private static Dictionary<int, WeakReference<GraphicRaycaster>> _grContainer;

        private static void Register(GraphicRaycaster gr)
        {
            if (gr == null || !gr) return;

            _grContainer ??= new(DEFAULT_CAPACITY);

            var path = GetTransformPath(gr.transform);

            var key = gr.GetHashCode();
            if (_grContainer.ContainsKey(key))
            {
                NamedLoggerTag.Sprite.Log($"Already registered! ({path})");
                return;
            }

            _grContainer.Add(key, new(gr));

            NamedLoggerTag.Sprite.Log($"Registered! ({path})");
        }

        /*
        private static void Unregister(GraphicRaycaster gr)
        {
            if (gr == null || !gr || _grContainer == null || _grContainer.Count == 0) return;

            var key = gr.GetHashCode();
            if (!_grContainer.ContainsKey(key)) return;

            _grContainer.Remove(key);

            var path = GetTransformPath(gr.transform);
            NamedLoggerTag.Sprite.Log($"Unregistered! ({path})");
        }
        */

        private static void Clear()
        {
            _grContainer?.Clear();
            _grContainer = null;
        }

        private static bool Contains(GraphicRaycaster gr)
        {
            if (gr == null || !gr || _grContainer == null || _grContainer.Count == 0) return false;

            return _grContainer.ContainsKey(gr.GetHashCode());
        }

        private static List<RaycastResult> results = new();
        private static PointerEventData _ped = new(null);

        public static bool GetIsHitted(Vector3 position)
        {
            if (_grContainer == null || _grContainer.Count == 0) return false;

            _ped.position = position;

            var keys = _grContainer.Keys.ToList();
            int key;
            for (int i = 0, cnt = keys.Count; i < cnt; i++)
            {
                key = keys[i];

                if (_grContainer.TryGetValue(key, out var wr) && wr.TryGetTarget(out var gr))
                {
                    results.Clear();
                    gr.Raycast(_ped, results);

                    if (results.Count > 0 && results[0].isValid)
                    {
                        //var path = GetTransformPath(gr.transform);
                        //NamedLoggerTag.Sprite.Log($"$$$ '{path}' Hit!");

                        return true;
                    }
                }
                else if (_grContainer.ContainsKey(key))
                {
                    _grContainer.Remove(key);

                    NamedLoggerTag.Sprite.Log($"Shrink! [{key}]");
                }
            }

            //NamedLoggerTag.Sprite.Log($"$$$ Pass.");

            return false;
        }
    }
}
