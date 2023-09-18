/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebViewPointerInputDetector.cs
* Developer:	sprite
* Date:			2023-07-13 13:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Vuplex.WebView;
using System.Linq;
using System;
using Com2Verse.Utils;

namespace Com2Verse.Mice
{
	public sealed class MiceWebViewPointerInputDetector : MonoBehaviour
		, IPointerInputDetector
        , INamedLogger<NamedLoggerTag.Sprite>
    {
        public event EventHandler<EventArgs<Vector2>> BeganDrag;
        public event EventHandler<EventArgs<Vector2>> Dragged;
        public event EventHandler<PointerEventArgs> PointerDown;
        public event EventHandler PointerEntered;
        public event EventHandler<EventArgs<Vector2>> PointerExited;
        public event EventHandler<EventArgs<Vector2>> PointerMoved;
        public event EventHandler<PointerEventArgs> PointerUp;
        public event EventHandler<ScrolledEventArgs> Scrolled;
        public bool PointerMovedEnabled { get; set; }

        [Header("Include Raycast Targets")]
        [SerializeField] private Transform[] _includes;
        [Header("Exclude Raycast Targets")]
        [SerializeField] private Transform[] _excludes;

        private Vector3 _lastMousePos = Vector3.zero;
        private Vector3 _mousePos = Vector3.zero;
        private Vector3 _mouseDelta = Vector3.zero;
        private int _heldDownIndex = -1;
        private string _hittedName = "";
        private Vector2 _normalizedPos = Vector2.zero;
        private int _clickCount = 1;

        private bool _pressed
        {
            get => HasFlag(_flags, PRESSED);
            set => _flags = ModifyFlag(_flags, PRESSED, value);
        }
        private bool _hovering
        {
            get => HasFlag(_flags, HOVERING);
            set => _flags = ModifyFlag(_flags, HOVERING, value);
        }
        private bool _hitted
        {
            get => HasFlag(_flags, HITTED);
            set => _flags = ModifyFlag(_flags, HITTED, value);
        }
        private bool _dragging
        {
            get => HasFlag(_flags, DRAGGING);
            set => _flags = ModifyFlag(_flags, DRAGGING, value);
        }
        private bool _hitAndPress
        {
            get => HasFlag(_flags, HIT_AND_PRESS);
            set => _flags = ModifyFlag(_flags, HIT_AND_PRESS, value);
        }

        private uint _flags = 0;
        const uint PRESSED = 0x01;
        const uint HOVERING = 0x02;
        const uint HITTED = 0x04;
        const uint DRAGGING = 0x08;
        const uint HIT_AND_PRESS = 0x10;

        private static bool HasFlag(uint source, uint flag) => (source & flag) == flag;
        private static uint ModifyFlag(uint source, uint flag, bool value) => value ? source | flag : source & ~flag;

        private void Update()
        {
            // Mouse Wheel Scroll
            if (!Mathf.Approximately(Input.mouseScrollDelta.sqrMagnitude, 0))
            {
                var scrollDelta = -1 * Input.mouseScrollDelta;
                this.Scrolled?.Invoke(this, new(scrollDelta, _normalizedPos));
            }

            _mousePos = Input.mousePosition;
            _mouseDelta = _mousePos - _lastMousePos;

            _clickCount = 1;
            _heldDownIndex = -1;

            if (Input.GetMouseButton(0)) _heldDownIndex = 0;
            else if (Input.GetMouseButton(1)) _heldDownIndex = 1;
            else if (Input.GetMouseButton(2)) _heldDownIndex = 2;
            else if (Input.touchCount > 0)
            {
                _heldDownIndex = 0;
                var touch = Input.GetTouch(0);
                _mousePos = touch.position;
                _mouseDelta = touch.deltaPosition;
                _clickCount = touch.tapCount;
            }

            if (_heldDownIndex >= 0 && !_pressed)
            {
                _pressed = true;

                if (_hitted)
                {
                    _hitAndPress = true;

                    //this.Log($"[{_hittedName}] PointerDown {_mousePos} {_normalizedPos}");

                    var args = new PointerEventArgs()
                    {
                        Point = _normalizedPos,
                        Button = (MouseButton)_heldDownIndex,
                        ClickCount = _clickCount
                    };
                    this.PointerDown?.Invoke(this, args);
                }
            }
            else if (_heldDownIndex < 0 && _pressed)
            {
                _pressed = false;

                _hitAndPress = false;

                if (_hitted)
                {
                    //this.Log($"[{_hittedName}] PointerUp {_mousePos} {_normalizedPos}");

                    var args = new PointerEventArgs()
                    {
                        Point = _normalizedPos,
                        Button = (MouseButton)_heldDownIndex,
                        ClickCount = _clickCount
                    };
                    this.PointerUp?.Invoke(this, args);
                }

                if (_dragging)
                {
                    _dragging = false;

                    this.Log($"[{_hittedName}] EndDrag {_mousePos} {_normalizedPos}");
                }
            }

            if (Mathf.Approximately(_mouseDelta.sqrMagnitude, 0)) return;

            _lastMousePos = _mousePos;

            if (TryGetRaycastHit(_mousePos, _excludes, _includes, out var trHitted, out var texCoord))
            {
                _hitted = true;
                _hittedName = trHitted.name;
                _normalizedPos = new Vector2(texCoord.x, 1.0f - texCoord.y);

                if (!_hovering)
                {
                    _hovering = true;
                    
                    //this.Log($"[{_hittedName}] Enter {_mousePos} {_normalizedPos}");

                    this.PointerEntered?.Invoke(this, null);
                }
                else
                {
                    if (_hitAndPress)
                    {
                        if (_dragging)
                        {
                            //this.Log($"[{_hittedName}] Dragging {_mousePos} {_normalizedPos}");

                            this.Dragged?.Invoke(this, new(_normalizedPos));
                        }
                        else
                        {
                            _dragging = true;
                            //this.Log($"[{_hittedName}] BeginDrag {_mousePos} {_normalizedPos}");

                            this.BeganDrag?.Invoke(this, new(_normalizedPos));
                        }
                    }
                    else
                    {
                        //this.Log($"[{_hittedName}] Hovering {_mousePos} {_normalizedPos}");

                        this.PointerMoved?.Invoke(this, new(_normalizedPos));
                    }
                }
            }
            else if (_hitted)
            {
                if (_hovering)
                {
                    _hovering = false;
                    //this.Log($"[{_hittedName}] Leave {_mousePos} {_normalizedPos}");

                    this.PointerExited?.Invoke(this, new(_normalizedPos));
                }

                _hitted = false;
                _hittedName = "";
            }
        }

        const float MAX_RAYCAST_DISTANCE = 1000.0f;

        private static bool TryGetRaycastHit(Vector3 position, Transform[] excludes, Transform[] includes, out Transform transform, out Vector2 textureCoord)
        {
            transform = null;
            textureCoord = Vector2.zero;

            // UI 단에서 먼저 거른다.
            if (MiceWebViewGraphicRaycasterManager.GetIsHitted(position)) return false;

            var ray = Camera.main.ScreenPointToRay(position);

            var hits = Physics.RaycastAll(ray, MAX_RAYCAST_DISTANCE, Define.LayerMask(Define.eLayer.OBJECT));
            for (int i = 0, cnt = hits.Length; i < cnt; i++)
            {
                var hit = hits[i];

                // 제외 Collider 체크.
                if (excludes?.Any(e => e == hit.collider.transform) ?? false) continue;

                // 포함 Collider 체크.
                if (includes?.Any(e => e == hit.collider.transform) ?? false)
                {
                    transform = hit.collider.transform;
                    textureCoord = hit.textureCoord;
                    return true;
                }
            }

            return false;
        }
    }
}
