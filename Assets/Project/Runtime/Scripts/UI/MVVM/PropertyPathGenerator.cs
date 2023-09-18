/*===============================================================
* Product:		Com2Verse
* File Name:	PropertyPathGenerator.cs
* Developer:	tlghks1009
* Date:			2023-01-10 18:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Communication;
using Com2Verse.Project.Communication.UI;
using Com2Verse.Tweener;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
    /// <summary>
    /// 자주 사용되는 프로퍼티의 경우 등록 후 사용 - 프로젝트
    /// ReferenceType의 경우 '명시적으로 <object> 해줄 것'
    /// </summary>
    public static class PropertyPathGenerator
    {
        public static void Initialize()
        {
            PropertyPathGen.Initialize();

            PropertyPathAccessors.Register(
                typeof(MetaverseToggle), "isOn",
                (obj) =>
                {
                    if (obj is MetaverseToggle metaverseToggle)
                        return metaverseToggle.isOn;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is MetaverseToggle metaverseToggle) metaverseToggle.isOn = value;
                });


            PropertyPathAccessors.Register<object>(
                typeof(ImagePropertyExtensions), "Sprite",
                (obj) =>
                {
                    if (obj is ImagePropertyExtensions imagePropertyExtensions)
                        return imagePropertyExtensions.Sprite;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is ImagePropertyExtensions imagePropertyExtensions) imagePropertyExtensions.Sprite = value as Sprite;
                });

            PropertyPathAccessors.Register<object>(
                typeof(TransformPropertyExtensions), "Transform",
                (obj) =>
                {
                    if (obj is TransformPropertyExtensions tpe)
                        return tpe.Transform;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is TransformPropertyExtensions tpe) tpe.Transform = value as Transform;
                });


            PropertyPathAccessors.Register(
                typeof(GameObjectPropertyExtensions), "ActiveState",
                (obj) =>
                {
                    if (obj is GameObjectPropertyExtensions gameObjectPropertyExtensions)
                        return gameObjectPropertyExtensions.ActiveState;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is GameObjectPropertyExtensions gameObjectPropertyExtensions) gameObjectPropertyExtensions.ActiveState = value;
                });

            PropertyPathAccessors.Register(
                typeof(GameObjectPropertyExtensions), "ActiveStateReverse",
                (obj) =>
                {
                    if (obj is GameObjectPropertyExtensions gameObjectPropertyExtensions)
                        return gameObjectPropertyExtensions.ActiveStateReverse;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is GameObjectPropertyExtensions gameObjectPropertyExtensions) gameObjectPropertyExtensions.ActiveStateReverse = value;
                });

            PropertyPathAccessors.Register<object>(
                typeof(RawImageController), "Texture",
                (obj) =>
                {
                    if (obj is RawImageController rawImageController)
                        return rawImageController.Texture;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is RawImageController rawImageController)
                        rawImageController.Texture = value as Texture;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(RawImageController), "IsVerticalFlipped",
                (obj) =>
                {
                    if (obj is RawImageController rawImageController)
                        return rawImageController.IsVerticalFlipped;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is RawImageController rawImageController)
                        rawImageController.IsVerticalFlipped = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(RawImageController), "IsHorizontalFlipped",
                (obj) =>
                {
                    if (obj is RawImageController rawImageController)
                        return rawImageController.IsHorizontalFlipped;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is RawImageController rawImageController)
                        rawImageController.IsHorizontalFlipped = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(AudioStateChangedListener), "IsInputAudible",
                (obj) =>
                {
                    if (obj is AudioStateChangedListener ascl)
                        return ascl.IsInputAudible;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is AudioStateChangedListener ascl)
                        ascl.IsInputAudible  = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(AudioStateChangedListener), "IsOutputAudible",
                (obj) =>
                {
                    if (obj is AudioStateChangedListener ascl)
                        return ascl.IsOutputAudible;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is AudioStateChangedListener ascl)
                        ascl.IsOutputAudible = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(AudioStateChangedListener), "IsSpeaking",
                (obj) =>
                {
                    if (obj is AudioStateChangedListener ascl)
                        return ascl.IsSpeaking;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is AudioStateChangedListener ascl)
                        ascl.IsSpeaking = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(TweenToggleController), "IsTweened",
                (obj) =>
                {
                    if (obj is TweenToggleController ttc)
                        return ttc.IsTweened;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is TweenToggleController ttc)
                    {
                        ttc.IsTweened = value;
                    }
                });

            PropertyPathAccessors.Register<object>(
                typeof(RemoteTrackObserver), "User",
                (obj) =>
                {
                    if (obj is RemoteTrackObserver rto)
                        return rto.User;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is RemoteTrackObserver rto)
                        rto.User = value as ICommunicationUser;
                });

            PropertyPathAccessors.Register<object>(
                typeof(RemoteTrackPublishRequester), "User",
                (obj) =>
                {
                    if (obj is RemoteTrackPublishRequester rtpr)
                        return rtpr.User;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is RemoteTrackPublishRequester rtpr)
                        rtpr.User = value as ICommunicationUser;
                });

            PropertyPathAccessors.Register<int>(
                typeof(RectTransformPropertyExtensions), "SetSibling",
                (obj) =>
                {
                    if (obj is RectTransformPropertyExtensions rtpe)
                        return rtpe.SetSibling;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is RectTransformPropertyExtensions rtpe)
                        rtpe.SetSibling = value;
                });

            PropertyPathAccessors.Register<float>(
                typeof(RectResizer), "Value",
                (obj) =>
                {
                    if (obj is RectResizer rr)
                        return rr.Value;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is RectResizer rr)
                        rr.Value = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(RawImageController), "IsVisible",
                (obj) =>
                {
                    if (obj is RawImageController rawImageController)
                        return rawImageController.IsVisible;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is RawImageController rawImageController)
                        rawImageController.IsVisible = value;
                });

            PropertyPathAccessors.Register<float>(
                typeof(CanvasGroupAlphaController), "Value",
                (obj) =>
                {
                    if (obj is CanvasGroupAlphaController cgac)
                        return cgac.Value;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is CanvasGroupAlphaController cgac)
                        cgac.Value = value;
                });

            PropertyPathAccessors.Register<bool>(
                typeof(ChatUserViewModel), "IsOnChatEmoticon",
                (obj) =>
                {
                    if (obj is ChatUserViewModel chatUserViewModel)
                        return chatUserViewModel.IsOnChatEmoticon;

                    return false;
                },
                (obj, value) =>
                {
                    if (obj is ChatUserViewModel chatUserViewModel)
                        chatUserViewModel.IsOnChatEmoticon = value;
                });

            PropertyPathAccessors.Register<object>(
                typeof(ChatUserViewModel), "ChatBalloonTransform",
                (obj) =>
                {
                    if (obj is ChatUserViewModel chatUserViewModel)
                        return chatUserViewModel.ChatBalloonTransform;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is ChatUserViewModel chatUserViewModel)
                        chatUserViewModel.ChatBalloonTransform = value as Transform;
                });

            PropertyPathAccessors.Register<int>(
                typeof(Canvas), "sortingOrder",
                (obj) =>
                {
                    if (obj is Canvas canvas)
                        return canvas.sortingOrder;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is Canvas canvas)
                        canvas.sortingOrder = value;
                });

            PropertyPathAccessors.Register<float>(
                typeof(TextMeshProUGUI), "fontSize",
                (obj) =>
                {
                    if (obj is TextMeshProUGUI textMeshProUGUI)
                        return textMeshProUGUI.fontSize;

                    return 0;
                },
                (obj, value) =>
                {
                    if (obj is TextMeshProUGUI textMeshProUGUI)
                        textMeshProUGUI.fontSize = value;
                });

            PropertyPathAccessors.Register<object>(
                typeof(ChatUserViewModel), "GestureRootTransform",
                (obj) =>
                {
                    if (obj is ChatUserViewModel chatUserViewModel)
                        return chatUserViewModel.GestureRootTransform;

                    return null;
                },
                (obj, value) =>
                {
                    if (obj is ChatUserViewModel chatUserViewModel)
                        chatUserViewModel.GestureRootTransform = value as Transform;
                });

            PropertyPathGen.InitializationComplete();
        }
    }
}
