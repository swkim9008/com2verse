/*===============================================================
* Product:		Com2Verse
* File Name:	UISpriteAnimation.cs
* Developer:	haminjeong
* Date:			2022-10-11 15:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Com2Verse.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

[RequireComponent(typeof(Image))][ExecuteAlways]
public sealed class UISpriteAnimation : MonoBehaviour
{
    #region variables
    
    private SpriteAtlasController _spriteAtlasController;
    [SerializeField]
    private Image _emoticonImage = null;
    [SerializeField]
    private int _spriteLength;
    [SerializeField]
    private SpriteAtlas _spriteAtlas = null;
    [SerializeField]
    private string[] _spriteNames = null;
    [SerializeField]    
    private Sprite[] _sprites = null;
    [SerializeField]
    private float[] _spriteStartTimes = null;
    [SerializeField]
    private float _playTime = 0;
    [SerializeField]
    private int _loopCount = 1;
    [SerializeField]
    private bool _isPlaying = false;

    [SerializeField]
    private AssetReference _seAssetRef = null;
    [SerializeField]
    private float _sePlaytime = 0;
    private bool _isSEPlayed = false;

    [SerializeField]
    private UnityEvent _onFinishedAnimationCallback = null;

    #endregion

    #region properties
    public bool IsAutoPlay { get; set; } = true;
    public bool IsPlaying
    {
        private set { _isPlaying = value; }
        get { return _isPlaying; }
    }
    public int LoopCount {get => _loopCount; set => _loopCount = value;}
    public float PlayTime {get => _playTime; set => _playTime = value;}
    public int SpriteIndex { get; private set; } = 0;
    private int CurrentLoopCount { get; set; } = 0;
    private float CurrentPlayTime { get; set; } = 0f;
    public Sprite[] Sprites {   get => _sprites;   }
    public float[] SpriteStartTimes {   get => _spriteStartTimes;   }

    public UnityEvent OnFinishedAnimationCallback => _onFinishedAnimationCallback;

    #endregion

    private void OnEnable()
    {
        InitSpriteAtlasController();
        _emoticonImage = GetComponent<Image>();
        if(IsAutoPlay)
            PlayAnimation();
    }

    public void InitSpriteAtlasController(SpriteAtlas atlas = null)
    {
        if (_spriteAtlasController != null) return;
        if (atlas != null)
            _spriteAtlas = atlas;
        if (_spriteAtlas == null)
        {
            _spriteAtlasController?.Destroy();
            _spriteAtlasController = null;
            return;
        }
        string assetAddressableName = $"{_spriteAtlas.name}.spriteatlasv2";
        _spriteAtlasController = new SpriteAtlasController(_spriteAtlas, assetAddressableName, true);
    }

    public Dictionary<string, Sprite> GetSprites()
    {
        if (_spriteAtlasController == null) return null;
        return _spriteAtlasController.Sprites;
    }

    private void InitAnimation()
    {
        // 항상 첫 번째 프레임 노출.
        IsPlaying        = false;
        SpriteIndex      = 0;
        CurrentPlayTime  = 0f;
        CurrentLoopCount = 0;
        _isSEPlayed      = false;
    }

    public void CopyData(ref Image image, Sprite[] sprites, float[] spriteStartTimes, float playTime, int loopCount)
    {
        InitAnimation();

        _sprites = sprites;
        _spriteStartTimes = spriteStartTimes;
        _playTime = playTime;
        _loopCount = loopCount;

        if(image != null)
            _emoticonImage = image;
    }

    public void PlayAnimation()
    {
        InitAnimation();

        if (_sprites != null && _spriteStartTimes != null && _sprites.Length > 0 && _spriteStartTimes.Length > 0)
        {
            IsPlaying = true;
            SetSpriteIndex(SpriteIndex);
        }
        else
            C2VDebug.LogError("invalid variable data.");
    }

    public void StopAnimation()
    {
        InitAnimation();
    }


    private void Update()
    {
        AnimationUpdate(Time.deltaTime);
    }

    private void AnimationUpdate(float deltaTime)
    {
        if (IsPlaying)
        {
            CurrentPlayTime += deltaTime;

            if (CurrentPlayTime >= _playTime)
            {
                ++CurrentLoopCount;
                if (_loopCount > 0 && CurrentLoopCount >= _loopCount)
                {
                    IsPlaying = false;
                    _onFinishedAnimationCallback?.Invoke();
                    return;
                }

                SpriteIndex = 0;
                CurrentPlayTime -= _playTime;
                SetSpriteIndex(SpriteIndex);
            }

            var nextSpriteIndex = SpriteIndex + 1;
            if (_spriteStartTimes.Length > nextSpriteIndex)
            {
                if (_spriteStartTimes[nextSpriteIndex] < CurrentPlayTime)
                {
                    ++SpriteIndex;
                    SetSpriteIndex(SpriteIndex);
                }
            }

            if (_seAssetRef != null && !_seAssetRef.Asset.IsUnityNull() && CurrentPlayTime > _sePlaytime && !_isSEPlayed)
            {
                if (SoundManager.InstanceExists)
                    SoundManager.Instance.PlayUISound(_seAssetRef);
                _isSEPlayed = true;
            }
        }
    }

    private void SetSpriteIndex(int spriteIndex)
    {
        if (_spriteAtlasController == null) return;
        _emoticonImage.sprite = _spriteAtlasController.GetSprite(_spriteNames[spriteIndex]);
    }

#if UNITY_EDITOR

    public void AnimationUpdateForEditor(float deltaTime)
    {
        AnimationUpdate(deltaTime);
    }

#endif
}
