/*===============================================================
* Product:		Com2Verse
* File Name:	GPUDebug.cs
* Developer:	minujeong
* Date:			2023-07-13 11:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Com2Verse.Ambience.Runtime.KeywordOverride;
using TMPro;
using UnityEngine.Rendering.Universal;

namespace Com2Verse.GPUDebug
{
    public sealed class GPUDebug : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lightInfoTxt;
        [SerializeField] private TextMeshProUGUI _materialsInfoTxt;
        [SerializeField] private TextMeshProUGUI _fogInfoTxt;

        private static readonly WaitForSeconds Wait = new(1.0f);

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(GPUDebugLoop());
        }

        private IEnumerator GPUDebugLoop()
        {
            var updaters = new Action[]
            {
                UpdateLightInfo,
                UpdateMaterialsInfo,
                UpdateFogSettingsInfo,
            };

            var i = 0;
            var l = updaters.Length;

            while (isActiveAndEnabled)
            {
                updaters[i].Invoke();
                i = (i + 1) % l;
                yield return Wait;
            }
        }

        private void UpdateLightInfo()
        {
            var lightInfos = FindObjectsOfType<Light>().Select(c => $"[{c.name}] intensity: {c.intensity} / color: {c.color} / shadows: {c.shadows} / light type: {c.type}").ToList();
            _lightInfoTxt.text = string.Join("\n", lightInfos);
        }

        private void UpdateMaterialsInfo() => _materialsInfoTxt.text = $"all material count: {Resources.FindObjectsOfTypeAll<Material>().Length}";

        private void UpdateFogSettingsInfo() => _fogInfoTxt.text = $"enabled: {RenderSettings.fog} / color: {RenderSettings.fogColor} / mode: {RenderSettings.fogMode}";

        public void SetGIEnabled(bool isEnabled) => KeywordOverride<DisableBakedGIImpl>.Set(!isEnabled);

        public void SetPostprocessEnabled(bool isEnabled)
        {
            foreach (var c in FindObjectsOfType<Camera>())
            {
                var uc = c.GetComponent<UniversalAdditionalCameraData>();
                if (uc) uc.renderPostProcessing = isEnabled;
            }
        }

        public void SetFogEnabled(bool isEnabled) => RenderSettings.fog = isEnabled;
    }
}