using System;
using Com2Verse.AssetSystem;
using Com2Verse.Bridge.Runtime.ShaderKeywordHandler;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Com2Verse.Rendering
{
    public class ShaderKeywordsManager : MonoSingleton<ShaderKeywordsManager>, IDisposable
    {
        private GlobalKeywordsSettings _keywordsSettings;

        public async void Initialize()
        {
            _keywordsSettings  = await C2VAddressables.LoadAssetAsync<GlobalKeywordsSettings>("GlobalShaderKeywords.asset").ToUniTask();
            if (_keywordsSettings) _keywordsSettings.Apply();
        }

        public void Dispose()
        {
            if (_keywordsSettings) _keywordsSettings.Dispose();
        }
    }
}