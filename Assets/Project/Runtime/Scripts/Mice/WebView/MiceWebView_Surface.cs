/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_Surface.cs
* Developer:	sprite
* Date:			2023-07-14 19:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
    public partial class MiceWebView	// Surface
	{
        const string SHADER_KEYWORD_FLIPY       = "FLIP_Y";
        const string SHADER_PROPERTY_FLIPY      = "_FlipY";
        const string SHADER_PROPERTY_MAINTEX    = "_MainTex";

        public interface ISurface
        {
            bool IsValid { get; }
            Material Material { get; }
            Vector3 Size { get; }

            UniTask Init();
            void Flip(bool value, bool useMaterialProperty = false);
            void SetTexture(Texture texture);
            void SetMaterial(Material material);
            void ResetMaterial();
            void SetTexCoord(bool apply, Rect texCoord = default);

            /// <summary>
            /// 주어진 Material을 ISurface 객체 설정과 동일하게 적용한다.(Flip 제외)
            /// </summary>
            /// <param name="material"></param>
            public void ApplyTo(Material material)
            {
                material.mainTexture = this.Material.mainTexture;

                var texCoord = ISurface.GetMaterialTexCoord(this.Material);
                ISurface.SetMaterialTexCoord(material, true, texCoord);
            }

            public static class Factory
            {
                public static ISurface CreateFrom<T>(T source)
                    where T : Component
                {
                    switch (source)
                    {
                        case CanvasRenderer canvasRenderer  when source is CanvasRenderer:  return new CanvasSurface(canvasRenderer);
                        case SpriteRenderer spriteRenderer  when source is SpriteRenderer:  return new SpriteSurface(spriteRenderer);
                        case Renderer       renderer        when source is Renderer:        return new RendererSurface(renderer);
                    }

                    return default;
                }
            }

            public static void MaterialFlip(Material material, bool originalValue, bool currentValue, bool useMaterialProperty = false)
            {
                var appliedValue = currentValue ? !originalValue : originalValue;

                if (useMaterialProperty)
                {
                    material.SetFloat(SHADER_PROPERTY_FLIPY, appliedValue ? 1 : 0);
                }
                else
                {
                    if (appliedValue)
                    {
                        material.EnableKeyword(SHADER_KEYWORD_FLIPY);
                    }
                    else
                    {
                        material.DisableKeyword(SHADER_KEYWORD_FLIPY);
                    }
                }
            }

            public static void SetMaterialTexCoord(Material material, bool apply, Rect texCoord = default)
            {
                Vector2 scale = Vector2.one;
                Vector2 offset = Vector2.zero;

                if (apply)
                {
                    offset = texCoord.position;
                    scale = texCoord.size;
                }

                if (material.HasProperty(SHADER_PROPERTY_MAINTEX))
                {
                    material.SetTextureScale(SHADER_PROPERTY_MAINTEX, scale);
                    material.SetTextureOffset(SHADER_PROPERTY_MAINTEX, offset);
                }
            }

            public static Rect GetMaterialTexCoord(Material material)
            {
                Vector2 scale = Vector2.one;
                Vector2 offset = Vector2.zero;

                if (material.HasProperty(SHADER_PROPERTY_MAINTEX))
                {
                    scale = material.GetTextureScale(SHADER_PROPERTY_MAINTEX);
                    offset = material.GetTextureOffset(SHADER_PROPERTY_MAINTEX);
                }

                return new Rect(offset, scale);
            }
        }

        internal struct OriginalFlipYEnablity
        {
            public bool OriginalFlipYKeywordEnablity;
            public bool OriginalFlipYPropertyEnablity;

            public void Refresh(Material material)
            {
                this.OriginalFlipYKeywordEnablity = material.IsKeywordEnabled(SHADER_KEYWORD_FLIPY);
                this.OriginalFlipYPropertyEnablity = material.HasFloat(SHADER_PROPERTY_FLIPY) ? Mathf.Approximately(material.GetFloat(SHADER_PROPERTY_FLIPY), 1) : false;

                NamedLoggerTag.Sprite.Log($"[{material.name}] Original FlipY Enablity (KeyWord={this.OriginalFlipYKeywordEnablity}, Property={this.OriginalFlipYPropertyEnablity})");
            }

            public bool GetValue(bool useMaterialProperty = false)
                => useMaterialProperty ? this.OriginalFlipYKeywordEnablity : this.OriginalFlipYPropertyEnablity;
        }

        public class RendererSurface : ISurface
        {
            public bool IsValid => _isInitialzed && _renderer != null && _renderer && this.Material != null && this.Material;
            public Material Material => _renderer.material;
            public Vector3 Size => _renderer.localBounds.size;

            private bool _isInitialzed;
            private Renderer _renderer;
            private Material[] _defaultMaterials;
            private OriginalFlipYEnablity _originalFlipYEnablity = new();

            public RendererSurface(Renderer renderer)
            {
                _renderer = renderer;
            }

            public virtual async UniTask Init()
            {
                if (_isInitialzed) return;

                NamedLoggerTag.Sprite.Log("Wait for Material...");
                await UniTask.WaitUntil(() => this.Material != null);
                NamedLoggerTag.Sprite.Log("Done.");

                _defaultMaterials = _renderer.materials;
                _originalFlipYEnablity.Refresh(this.Material);

                _isInitialzed = true;
            }

            public virtual void Flip(bool value, bool useMaterialProperty = false)
                => ISurface.MaterialFlip(this.Material, _originalFlipYEnablity.GetValue(useMaterialProperty), value, useMaterialProperty);

            public virtual void SetTexture(Texture texture)
            {
                this.Material.mainTexture = (texture != null && texture) ? texture : _defaultMaterials[0].mainTexture;
            }

            public void SetMaterial(Material material)
            {
                if (this.Material == material)
                {
                    NamedLoggerTag.Sprite.Log("Same material!");
                    return;
                }

                _renderer.materials = new[] { material };
                _originalFlipYEnablity.Refresh(this.Material);
            }

            public void ResetMaterial()
            {
                if (this.Material == _defaultMaterials[0])
                {
                    NamedLoggerTag.Sprite.Log("Same material!");
                    return;
                }

                _renderer.materials = _defaultMaterials;
                _originalFlipYEnablity.Refresh(this.Material);
            }

            public void SetTexCoord(bool apply, Rect texCoord = default)
                => ISurface.SetMaterialTexCoord(this.Material, apply, texCoord);
        }

        public class SpriteSurface : RendererSurface
        {
            private SpriteRenderer _spriteRenderer;

            public SpriteSurface(SpriteRenderer spriteRenderer)
                : base(spriteRenderer)
            {
            }

            public override void Flip(bool value, bool useMaterialProperty = false)
                => _spriteRenderer.flipY = value;
        }

        public class CanvasSurface : ISurface
        {
            public bool IsValid => _isInitialzed && _canvasRenderer != null && _canvasRenderer && this.Material != null && this.Material;
            public Material Material => _canvasRenderer.GetMaterial();
            public Vector3 Size { get; private set; } = Vector3.zero;

            private bool _isInitialzed;
            private CanvasRenderer _canvasRenderer;
            private Material _defaultMaterial;
            private OriginalFlipYEnablity _originalFlipYEnablity = new();

            public CanvasSurface(CanvasRenderer canvasRenderer)
            {
                _canvasRenderer = canvasRenderer;
            }

            public UniTask Init()
            {
                if (_isInitialzed) return UniTask.CompletedTask;

                _defaultMaterial = _canvasRenderer.GetMaterial(0);
                _originalFlipYEnablity.Refresh(this.Material);
                this.Size = _canvasRenderer.GetComponent<RectTransform>().rect.size;

                _isInitialzed = true;

                return UniTask.CompletedTask;
            }

            public void Flip(bool value, bool useMaterialProperty = false)
                => ISurface.MaterialFlip(this.Material, _originalFlipYEnablity.GetValue(useMaterialProperty), value, useMaterialProperty);

            public virtual void SetTexture(Texture texture)
            {
                _canvasRenderer.GetMaterial().mainTexture = (texture != null && texture) ? texture : _defaultMaterial.mainTexture;
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_canvasRenderer.transform as RectTransform);
            }

            public void SetMaterial(Material material)
            {
                if (this.Material == material)
                {
                    NamedLoggerTag.Sprite.Log("Same material!");
                    return;
                }

                _canvasRenderer.SetMaterial(material, 0);
                _originalFlipYEnablity.Refresh(this.Material);
            }

            public void ResetMaterial()
            {
                if (this.Material == _defaultMaterial)
                {
                    NamedLoggerTag.Sprite.Log("Same material!");
                    return;
                }

                _canvasRenderer.SetMaterial(_defaultMaterial, 0);
                _originalFlipYEnablity.Refresh(this.Material);
            }

            public void SetTexCoord(bool apply, Rect texCoord = default)
                => ISurface.SetMaterialTexCoord(this.Material, apply, texCoord);
        }
    }
}
