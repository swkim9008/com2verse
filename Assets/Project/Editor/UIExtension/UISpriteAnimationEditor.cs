/*===============================================================
* Product:		Com2Verse
* File Name:	UISpriteAnimationEditor.cs
* Developer:	haminjeong
* Date:			2022-10-11 15:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.UI;

[CustomEditor(typeof(UISpriteAnimation), true)]
public sealed class UISpriteAnimationEditor : Editor
{
    private UISpriteAnimation _uiSpriteAnimation;

    private SerializedProperty _emoticonImage    = null;
    private SerializedProperty _spriteLength     = null;
    private SerializedProperty _spriteAtlas      = null;
    private SerializedProperty _spriteNames      = null;
    private SerializedProperty _sprites          = null;
    private SerializedProperty _spriteStartTimes = null;
    private SerializedProperty _playTime         = null;
    private SerializedProperty _loopCount        = null;
    private SerializedProperty _seAssetRef       = null;
    private SerializedProperty _sePlayTime       = null;
    private SerializedProperty _onFinishAction   = null;

    /// <summary>
    /// 스프라이트 애니메이션에 세팅된 스프라이트 플레이 타임(최대값)
    /// </summary>
    private float _setSpriteAnimationTime = 0f;

    private float _tempRealtimeSinceStartup = 0f;

    void OnEnable()
    {
        _uiSpriteAnimation = target as UISpriteAnimation;

        _emoticonImage    = serializedObject.FindProperty("_emoticonImage");
        _spriteLength     = serializedObject.FindProperty("_spriteLength");
        _spriteAtlas      = serializedObject.FindProperty("_spriteAtlas");
        _spriteNames      = serializedObject.FindProperty("_spriteNames");
        _sprites          = serializedObject.FindProperty("_sprites");
        _spriteStartTimes = serializedObject.FindProperty("_spriteStartTimes");
        _playTime         = serializedObject.FindProperty("_playTime");
        _loopCount        = serializedObject.FindProperty("_loopCount");
        _seAssetRef       = serializedObject.FindProperty("_seAssetRef");
        _sePlayTime       = serializedObject.FindProperty("_sePlaytime");
        _onFinishAction   = serializedObject.FindProperty("_onFinishedAnimationCallback");

        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI()
    {
        _emoticonImage.objectReferenceValue = EditorGUILayout.ObjectField("Target Image",_emoticonImage.objectReferenceValue, typeof(Image), true);

        var tempColor = GUI.color;
        if (_setSpriteAnimationTime >= _playTime.floatValue)        
            GUI.color = Color.red;                    

        _playTime.floatValue = EditorGUILayout.FloatField("Play Time", _playTime.floatValue);
        if (_playTime.floatValue < 0f)
            _playTime.floatValue = 0f;

        GUI.color = tempColor;

        if (_setSpriteAnimationTime >= _playTime.floatValue)
        {
            _setSpriteAnimationTime = -1f;
            EditorGUILayout.HelpBox("스프라이트 애니메이션에 세팅된 플레이 타임보다 커야 합니다.", MessageType.Error);
        }   

        _loopCount.intValue = EditorGUILayout.IntField("Loop Count", _loopCount.intValue);

        EditorGUILayout.Space();
        
        /// set sprites        
        {
            EditorGUI.BeginChangeCheck();
            _spriteAtlas.objectReferenceValue = EditorGUILayout.ObjectField("SpriteAtlas", _spriteAtlas.objectReferenceValue, typeof(SpriteAtlas), true);
            if ((_spriteAtlas.objectReferenceValue != null && _spriteLength.intValue > 0 && _sprites.GetArrayElementAtIndex(0).objectReferenceValue == null) ||
                EditorGUI.EndChangeCheck())
                SetAutoSprites();

            if (_spriteAtlas.objectReferenceValue == null) return;
            
            EditorGUI.BeginChangeCheck();
            _spriteLength.intValue = EditorGUILayout.IntField("Sprite Size", _spriteLength.intValue);
            if (EditorGUI.EndChangeCheck())
                SetSpriteLength(_spriteLength.intValue);

            if(_spriteLength.intValue > 0)
            {
                for (int i = 0; i < _spriteLength.intValue; ++i)
                {
                    EditorGUILayout.LabelField("Animation Sequence " + i);

                    EditorGUI.indentLevel += 2;

                    EditorGUI.BeginChangeCheck();
                    _sprites.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Sprite",
                                                                                                          _sprites.GetArrayElementAtIndex(i).objectReferenceValue,
                                                                                                          typeof(Sprite), true);
                    if (EditorGUI.EndChangeCheck())
                        UpdateSpriteName(i, _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite);
                    EditorGUILayout.LabelField("SpriteName", _spriteNames.GetArrayElementAtIndex(i).stringValue);

                    _spriteStartTimes.GetArrayElementAtIndex(i).floatValue = EditorGUILayout.FloatField("Start Time", _spriteStartTimes.GetArrayElementAtIndex(i).floatValue);
                    _setSpriteAnimationTime = Mathf.Max(_setSpriteAnimationTime, _spriteStartTimes.GetArrayElementAtIndex(i).floatValue);

                    EditorGUI.indentLevel -= 2;
                    EditorGUILayout.Space();
                }
            }
        }
        
        EditorGUILayout.Space();

        /// Image에 sprite 세팅.
        if (!EditorApplication.isPlaying 
            && !_uiSpriteAnimation.IsPlaying
            && _emoticonImage.objectReferenceValue != null 
            && _spriteNames.arraySize > 0
            && !object.ReferenceEquals((_emoticonImage.objectReferenceValue as Image).sprite, _sprites.GetArrayElementAtIndex(0).objectReferenceValue as Sprite))
        {
            (_emoticonImage.objectReferenceValue as Image).sprite = _sprites.GetArrayElementAtIndex(0).objectReferenceValue as Sprite;
        }

        // sound
        {
            EditorGUILayout.PropertyField(_seAssetRef, new GUIContent("SE AssetReference"));
            _sePlayTime.floatValue           = EditorGUILayout.FloatField("Start Time", _sePlayTime.floatValue);
            EditorGUILayout.Space();
        }

        if (_loopCount.intValue > 0)
        {
            EditorGUILayout.PropertyField(_onFinishAction, new GUIContent("On Finished Action"));
        }

        if (!_uiSpriteAnimation.IsPlaying && GUILayout.Button("Animation Test Play"))
        {
            _uiSpriteAnimation.PlayAnimation();
            _tempRealtimeSinceStartup = Time.realtimeSinceStartup;
        }

        if (_uiSpriteAnimation.IsPlaying && GUILayout.Button("Stop"))
        {
            _uiSpriteAnimation.StopAnimation();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void SetSpriteLength(int length)
    {
        _spriteNames.arraySize      = length;
        _sprites.arraySize          = length;
        _spriteStartTimes.arraySize = length;
    }

    private void SetAutoSprites()
    {
        var atlas = _spriteAtlas.objectReferenceValue as SpriteAtlas;
        _uiSpriteAnimation.InitSpriteAtlasController(atlas);
        if (atlas == null)
        {
            SetSpriteLength(0);
            return;
        }
        var count = atlas.spriteCount;
        _spriteLength.intValue = count;
        SetSpriteLength(count);
        var spriteDic      = _uiSpriteAnimation.GetSprites();
        var spriteNameList = spriteDic.Keys.ToList();
        spriteNameList.Sort((lhs, rhs) =>
        {
            int lhsIndex          = -1;
            int rhsIndex          = -1;
            int.TryParse(lhs.Substring(lhs.LastIndexOf("_") + 1), out lhsIndex);
            int.TryParse(rhs.Substring(lhs.LastIndexOf("_") + 1), out rhsIndex);
            return lhsIndex.CompareTo(rhsIndex);
        });
        for (int i = 0; i < spriteNameList.Count; ++i)
        {
            _sprites.GetArrayElementAtIndex(i).objectReferenceValue = spriteDic[spriteNameList[i]];
            _spriteNames.GetArrayElementAtIndex(i).stringValue      = spriteNameList[i];
        }
    }

    private void UpdateSpriteName(int index, Sprite sprite)
    {
        if (_spriteNames.arraySize <= index) return;
        _spriteNames.GetArrayElementAtIndex(index).stringValue = sprite.name;
    }

    private void Update()
    {
        if (!EditorApplication.isPlaying && _uiSpriteAnimation.IsPlaying)
        {
            var deltaTime = Time.realtimeSinceStartup - _tempRealtimeSinceStartup;
            _tempRealtimeSinceStartup = Time.realtimeSinceStartup;
            _uiSpriteAnimation.AnimationUpdateForEditor(deltaTime);
        }
    }
}
