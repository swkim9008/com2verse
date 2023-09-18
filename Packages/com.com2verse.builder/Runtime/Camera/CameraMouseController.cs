using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.Builder
{
    [DefaultExecutionOrder(-1)]
    public class CameraMouseController : DestroyableMonoSingleton<CameraMouseController>
    {
        /// <summary>
        /// 벽 컬링을 매번 체크하지 않기 위해, 카메라가 움직였는지 트래킹
        /// </summary>
        public bool Moving { get; set; }
        /// <summary>
        /// 마우스 클릭/릴리즈로 오브젝트 피킹하는 것을 활성화 / 비활성화
        /// </summary>
        public bool InvokeClickEvent { get; set; } = true;

        public Camera MainCamera
        {
            get
            {
                if (_builderCamera.IsUnityNull())
                {
                    var mainCamera = CameraManager.Instance.MainCamera;
                    
                    if (!mainCamera.IsUnityNull())
                        _builderCamera = mainCamera!.GetComponent<Camera>();
                }

                return _builderCamera;
            }
        }
        
        [SerializeField] private float _keyboardTranslationSensitivity = 5;
        [SerializeField] private float _translationSensitivity = 2;
        [SerializeField] private float _zoomSensitivity = 10;
        [SerializeField] private float _rotationSensitivity = 4;
        [SerializeField] private float _sensitiveFactor = 10;

        private Camera _builderCamera = null;

        private Vector3 _pivotPoint;

        private readonly string _mouseHorizontalAxisName = "Mouse X";
        private readonly string _mouseVerticalAxisName = "Mouse Y";
        private readonly string _scrollAxisName = "Mouse ScrollWheel";

        private void Update()
        {
            Moving = false;
            float sensitiveFactor = Input.GetKey(KeyCode.LeftControl) ? _sensitiveFactor : 1;

            KeyboardTranslate(sensitiveFactor);
            MouseTranslate(sensitiveFactor);
            Rotate(sensitiveFactor);
            RotatePivot(sensitiveFactor);
            MouseSelect();
        }

        private void RotatePivot(float sensitiveFactor)
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
            {
                float rotationX = 0, rotationY = 0;

                rotationX = Input.GetAxis(_mouseVerticalAxisName) * _rotationSensitivity;
                rotationY = Input.GetAxis(_mouseHorizontalAxisName) * _rotationSensitivity;

                transform.RotateAround(_pivotPoint, Vector3.up, rotationY / sensitiveFactor);
                transform.RotateAround(_pivotPoint, transform.right, -rotationX / sensitiveFactor);
                
                if (rotationX > 0 || rotationY > 0)
                {
                    Moving = true;
                }
            }
        }

        private void KeyboardTranslate(float sensitiveFactor)
        {
            Vector3 delta = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * _keyboardTranslationSensitivity * Time.deltaTime / sensitiveFactor;
            transform.position += delta;
            _pivotPoint += delta;

            if (delta.sqrMagnitude > 0)
            {
                Moving = true;
            }
        }

        private void MouseTranslate(float sensitiveFactor)
        {
            Vector3 translation = Vector3.zero;

            if (Input.GetMouseButton(2))
            {
                translation.y = -Input.GetAxis(_mouseVerticalAxisName) * _translationSensitivity;
                translation.x = -Input.GetAxis(_mouseHorizontalAxisName) * _translationSensitivity;
                _pivotPoint += (Vector3)(transform.localToWorldMatrix * translation) / sensitiveFactor;
            }

            // zoom
            translation.z = Input.GetAxis(_scrollAxisName) * _zoomSensitivity;

            transform.Translate(translation / sensitiveFactor);
            
            if (translation.sqrMagnitude > 0)
            {
                Moving = true;
            }
        }

        private void Rotate(float sensitiveFactor)
        {
            float rotationX = 0, rotationY = 0;

            if (Input.GetMouseButton(1))
            {
                rotationX = Input.GetAxis(_mouseVerticalAxisName) * _rotationSensitivity;
                rotationY = Input.GetAxis(_mouseHorizontalAxisName) * _rotationSensitivity;
            }

            float initialPivotDistance = Vector3.Distance(transform.position, _pivotPoint);

            transform.Rotate(0, rotationY / sensitiveFactor, 0, Space.World);
            transform.Rotate(-rotationX / sensitiveFactor, 0, 0);

            _pivotPoint = transform.position + transform.forward * initialPivotDistance;

            if (rotationX > 0 || rotationY > 0)
            {
                Moving = true;
            }
        }

        private void MouseSelect()
        {
            if (!InvokeClickEvent) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            bool click = Input.GetMouseButtonDown(0);
            bool release = Input.GetMouseButtonUp(0);
            
            if (click || release)
            {
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    if (click)
                        hit.transform.GetComponentInParent<IObjectRaycastTarget>()?.OnClick(click);
                    else
                        hit.transform.GetComponentInParent<IObjectRaycastTarget>()?.OnRelease(release);
                }
            }
        }
    }
}