/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_Message.cs
* Developer:	sprite
* Date:			2023-07-13 11:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Logger;
using Vuplex.WebView;
using Cysharp.Threading.Tasks;
using System;


namespace Com2Verse.Mice
{
    [Serializable]
    public partial class MiceWebViewMessage
        : INamedLogger<NamedLoggerTag.Sprite>
	{
        public string type;
        public string body;

        public override string ToString()
            => $"<color=lime><{this.type}></color> {this.body}";

        public bool IsTypeNameOf(string typeName) => string.Compare(this.type, typeName, false) == 0;
        public bool IsTypeNameOf<T>() => string.Compare(this.type, typeof(T).Name, false) == 0;

        public virtual void OnInit(IWebView webView) { }
    }

    public partial class MiceWebViewMessage // Factory
    {
        public static MiceWebViewMessage Parse(string json, IWebView webView)
        {
            MiceWebViewMessage msg;

            try
            {
                msg = JsonUtility.FromJson<MiceWebViewMessage>(json);
                if (msg.IsTypeNameOf<HlsEvent>() && MiceWebViewMessage.TryFromJson<HlsEvent>(msg.body, out var hlsEvent, webView))
                {
                    msg = hlsEvent;
                }
                else if (msg.IsTypeNameOf<VideoRes>() && MiceWebViewMessage.TryFromJson<VideoRes>(msg.body, out var videoRes, webView))
                {
                    msg = videoRes;
                }
                else if (msg.IsTypeNameOf<VideoEvent>() && MiceWebViewMessage.TryFromJson<VideoEvent>(msg.body, out var videoEvent, webView))
                {
                    msg = videoEvent;
                }
                else if (msg.IsTypeNameOf<PostCode>() && MiceWebViewMessage.TryFromJson<PostCode>(msg.body, out var postCode, webView))
                {
                    msg = postCode;
                }
                else if (msg.IsTypeNameOf<WebLink>() && MiceWebViewMessage.TryFromJson<WebLink>(msg.body, out var webLink, webView))
                {
                    msg = webLink;
                }
            }
            catch (Exception e)
            {
                NamedLoggerTag.Sprite.LogCategory(nameof(MiceWebViewMessage), $"<color=red><Exception></color> JSON Parse Error.\r\n{json}");

                throw new Exception("Exception Occured!", e);
            }

            return msg;
        }

        public static bool TryFromJson<T>(string json, out T obj, IWebView webView)
            where T : MiceWebViewMessage
        {
            obj = default;

            bool result;

            try
            {
                obj = JsonUtility.FromJson<T>(json);
                obj.OnInit(webView);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }

    public class MiceWebViewMessage<TBody> : MiceWebViewMessage
    {
        protected TBody bodyObj { get; private set; }

        public override void OnInit(IWebView webView)
        {
            this.bodyObj = JsonUtility.FromJson<TBody>(this.body);
        }
    }

    public partial class MiceWebViewMessage // Messages
    {
        [Serializable]
        public class HlsEvent : MiceWebViewMessage
        {
            public enum Evt
            {
                invalid = -1,

                // Fired before MediaSource is attaching to media element
                hlsMediaAttaching,
                // Fired when MediaSource has been successfully attached to media element
                hlsMediaAttached,
                // Fired before detaching MediaSource from media element
                hlsMediaDetaching,
                // Fired when MediaSource has been detached from media element
                hlsMediaDetached,
                // Fired when the buffer is going to be reset
                hlsBufferReset,
                // Fired when we know about the codecs that we need buffers for to push into - data: {tracks : { container, codec, levelCodec, initSegment, metadata }}
                hlsBufferCodecs,
                // fired when sourcebuffers have been created - data: { tracks : tracks }
                hlsBufferCreated,
                // fired when we append a segment to the buffer - data: { segment: segment object }
                hlsBufferAppending,
                // fired when we are done with appending a media segment to the buffer - data : { parent : segment parent that triggered BUFFER_APPENDING, pending : nb of segments waiting for appending for this segment parent}
                hlsBufferAppended,
                // fired when the stream is finished and we want to notify the media buffer that there will be no more data - data: { }
                hlsBufferEos,
                // fired when the media buffer should be flushed - data { startOffset, endOffset }
                hlsBufferFlushing,
                // fired when the media buffer has been flushed - data: { }
                hlsBufferFlushed,
                // fired to signal that a manifest loading starts - data: { url : manifestURL}
                hlsManifestLoading,
                // fired after manifest has been loaded - data: { levels : [available quality levels], audioTracks : [ available audio tracks ], url : manifestURL, stats : LoaderStats }
                hlsManifestLoaded,
                // fired after manifest has been parsed - data: { levels : [available quality levels], firstLevel : index of first quality level appearing in Manifest}
                hlsManifestParsed,
                // fired when a level switch is requested - data: { level : id of new level }
                hlsLevelSwitching,
                // fired when a level switch is effective - data: { level : id of new level }
                hlsLevelSwitched,
                // fired when a level playlist loading starts - data: { url : level URL, level : id of level being loaded}
                hlsLevelLoading,
                // fired when a level playlist loading finishes - data: { details : levelDetails object, level : id of loaded level, stats : LoaderStats }
                hlsLevelLoaded,
                // fired when a level's details have been updated based on previous details, after it has been loaded - data: { details : levelDetails object, level : id of updated level }
                hlsLevelUpdated,
                // fired when a level's PTS information has been updated after parsing a fragment - data: { details : levelDetails object, level : id of updated level, drift: PTS drift observed when parsing last fragment }
                hlsLevelPtsUpdated,
                // fired to notify that levels have changed after removing a level - data: { levels : [available quality levels] }
                hlsLevelsUpdated,
                // fired to notify that audio track lists has been updated - data: { audioTracks : audioTracks }
                hlsAudioTracksUpdated,
                // fired when an audio track switching is requested - data: { id : audio track id }
                hlsAudioTrackSwitching,
                // fired when an audio track switch actually occurs - data: { id : audio track id }
                hlsAudioTrackSwitched,
                // fired when an audio track loading starts - data: { url : audio track URL, id : audio track id }
                hlsAudioTrackLoading,
                // fired when an audio track loading finishes - data: { details : levelDetails object, id : audio track id, stats : LoaderStats }
                hlsAudioTrackLoaded,
                // fired to notify that subtitle track lists has been updated - data: { subtitleTracks : subtitleTracks }
                hlsSubtitleTracksUpdated,
                // fired to notify that subtitle tracks were cleared as a result of stopping the media
                hlsSubtitleTracksCleared,
                // fired when an subtitle track switch occurs - data: { id : subtitle track id }
                hlsSubtitleTrackSwitch,
                // fired when a subtitle track loading starts - data: { url : subtitle track URL, id : subtitle track id }
                hlsSubtitleTrackLoading,
                // fired when a subtitle track loading finishes - data: { details : levelDetails object, id : subtitle track id, stats : LoaderStats }
                hlsSubtitleTrackLoaded,
                // fired when a subtitle fragment has been processed - data: { success : boolean, frag : the processed frag }
                hlsSubtitleFragProcessed,
                // fired when a set of VTTCues to be managed externally has been parsed - data: { type: string, track: string, cues: [ VTTCue ] }
                hlsCuesParsed,
                // fired when a text track to be managed externally is found - data: { tracks: [ { label: string, kind: string, default: boolean } ] }
                hlsNonNativeTextTracksFound,
                // fired when the first timestamp is found - data: { id : demuxer id, initPTS: initPTS, timescale: timescale, frag : fragment object }
                hlsInitPtsFound,
                // fired when a fragment loading starts - data: { frag : fragment object }
                hlsFragLoading,
                // fired when a fragment loading is progressing - data: { frag : fragment object, { trequest, tfirst, loaded } }
                // FRAG_LOAD_PROGRESS = 'hlsFragLoadProgress',
                // Identifier for fragment load aborting for emergency switch down - data: { frag : fragment object }
                hlsFragLoadEmergencyAborted,
                // fired when a fragment loading is completed - data: { frag : fragment object, payload : fragment payload, stats : LoaderStats }
                hlsFragLoaded,
                // fired when a fragment has finished decrypting - data: { id : demuxer id, frag: fragment object, payload : fragment payload, stats : { tstart, tdecrypt } }
                hlsFragDecrypted,
                // fired when Init Segment has been extracted from fragment - data: { id : demuxer id, frag: fragment object, moov : moov MP4 box, codecs : codecs found while parsing fragment }
                hlsFragParsingInitSegment,
                // fired when parsing sei text is completed - data: { id : demuxer id, frag: fragment object, samples : [ sei samples pes ] }
                hlsFragParsingUserdata,
                // fired when parsing id3 is completed - data: { id : demuxer id, frag: fragment object, samples : [ id3 samples pes ] }
                hlsFragParsingMetadata,
                // fired when data have been extracted from fragment - data: { id : demuxer id, frag: fragment object, data1 : moof MP4 box or TS fragments, data2 : mdat MP4 box or null}
                // FRAG_PARSING_DATA = 'hlsFragParsingData',
                // fired when fragment parsing is completed - data: { id : demuxer id, frag: fragment object }
                hlsFragParsed,
                // fired when fragment remuxed MP4 boxes have all been appended into SourceBuffer - data: { id : demuxer id, frag : fragment object, stats : LoaderStats }
                hlsFragBuffered,
                // fired when fragment matching with current media position is changing - data : { id : demuxer id, frag : fragment object }
                hlsFragChanged,
                // Identifier for a FPS drop event - data: { currentDropped, currentDecoded, totalDroppedFrames }
                hlsFpsDrop,
                // triggered when FPS drop triggers auto level capping - data: { level, droppedLevel }
                hlsFpsDropLevelCapping,
                // Identifier for an error event - data: { type : error type, details : error details, fatal : if true, hls.js cannot/will not try to recover, if false, hls.js will try to recover,other error specific data }
                hlsError,
                // fired when hls.js instance starts destroying. Different from MEDIA_DETACHED as one could want to detach and reattach a media to the instance of hls.js to handle mid-rolls for example - data: { }
                hlsDestroying,
                // fired when a decrypt key loading starts - data: { frag : fragment object }
                hlsKeyLoading,
                // fired when a decrypt key loading is completed - data: { frag : fragment object, keyInfo : KeyLoaderInfo }
                hlsKeyLoaded,
                // deprecated; please use BACK_BUFFER_REACHED - data : { bufferEnd: number }
                hlsLiveBackBufferReached,
                // fired when the back buffer is reached as defined by the backBufferLength config option - data : { bufferEnd: number }
                hlsBackBufferReached,
                // fired after steering manifest has been loaded - data: { steeringManifest: SteeringManifest object, url: steering manifest URL }
                hlsSteeringManifestLoaded,
            }

            public Evt EventType { get; private set; }

            public override string ToString()
                => $"<color=lime><{nameof(HlsEvent)}></color> <color=yellow>({this.type})</color> {this.body}";

            public override void OnInit(IWebView webView)
            {
                base.OnInit(webView);

                this.EventType = Evt.invalid;

                if (Enum.TryParse<Evt>(this.type, out var evt))
                {
                    this.EventType = evt;
                }
            }
        }

        [Serializable]
        public class VideoRes : MiceWebViewMessage<VideoRes.Resolution>
        {
            [Serializable]
            public struct Resolution
            {
                public int width;
                public int height;
                public float duration;  // 동영상 길이(초)
            }

            public Resolution resolution => this.bodyObj;

            public override string ToString()
                => $"<color=lime><{nameof(VideoRes)}></color> <color=yellow>({this.type})</color> Resolution: {this.resolution.width}x{this.resolution.height} Duration: {this.resolution.duration}";
        }

        [Serializable]
        public class VideoEvent : MiceWebViewMessage
        {
            public enum Evt
            {
                invalid = -1,

                // Fires when the loading of an audio/video is aborted
                abort,
                // Fires when the browser can start playing the audio/video
                canplay,
                // Fires when the browser can play through the audio/video without stopping for buffering
                canplaythrough,
                // Fires when the duration of the audio/video is changed
                durationchange,
                // Fires when the current playlist is empty
                emptied,
                encrypted,
                // Fires when the current playlist is ended
                ended,
                // Fires when an error occurred during the loading of an audio/video
                error,
                // Fires when the browser has loaded the current frame of the audio/video
                loadeddata,
                // Fires when the browser has loaded meta data for the audio/video
                loadedmetadata,
                // Fires when the browser starts looking for the audio/video
                loadstart,
                // Fires when the audio/video has been paused
                pause,
                // Fires when the audio/video has been started or is no longer paused
                play,
                // Fires when the audio/video is playing after having been paused or stopped for buffering
                playing,
                // Fires when the browser is downloading the audio/video
                progress,
                // Fires when the playing speed of the audio/video is changed
                ratechange,
                // Fires when the user is finished moving/skipping to a new position in the audio/video
                seeked,
                // Fires when the user starts moving/skipping to a new position in the audio/video
                seeking,
                // Fires when the browser is trying to get media data, but data is not available
                stalled,
                // Fires when the browser is intentionally not getting media data
                suspend,
                // Fires when the current playback position has changed
                timeupdate,
                // Fires when the volume has been changed
                volumechange,
                // Fires when the video stops because it needs to buffer the next frame
                waiting,
            }

            public Evt EventType { get; private set; }

            public override string ToString()
                => $"<color=lime><{nameof(VideoEvent)}></color> <color=yellow>({this.type})</color> {this.body}";

            public override void OnInit(IWebView webView)
            {
                base.OnInit(webView);

                this.EventType = Evt.invalid;

                if (Enum.TryParse<Evt>(this.type, out var evt))
                {
                    this.EventType = evt;
                }

                this.Process(webView).Forget();
            }

            private async UniTask Process(IWebView webView)
            {
                switch (this.EventType)
                {
                    case Evt.loadedmetadata:
                    {
                        var resolution = await this.GetVideoResolution(webView);

                        C2VDebug.Log($"<color=cyan>[WebView Message]</color>: [VideoEvent]<{this.type}> Resolution = {resolution.width}x{resolution.height}");
                        break;
                    }
                }
            }

            public async UniTask<VideoRes.Resolution> GetVideoResolution(IWebView webView)
            {
                VideoRes.Resolution res;

                try
                {
                    var json = await webView.ExecuteJavaScript("getVideoRes()");
                    res = JsonUtility.FromJson<VideoRes.Resolution>(json);
                }
                catch (Exception e)
                {
                    C2VDebug.Log($"<color=cyan>[WebView Message]</color>: <color=red><Exception></color> {e.Message}");

                    res = new VideoRes.Resolution() { width = 0, height = 0 };
                }

                return res;
            }
        }


        [Serializable]
        public class PostCode : MiceWebViewMessage<PostCode.Data>
        {
            [Serializable]
            public struct Data
            {
                public string postcode;
                public string address;
                public string extraAddress;

                public bool isValid => !string.IsNullOrEmpty(this.postcode) && !string.IsNullOrEmpty(this.address);
            }

            public Data data => this.bodyObj;

            public override string ToString()
                => $"<color=lime><{nameof(PostCode)}></color> <color=yellow>({this.type})</color> PostCode: {this.data.postcode}, Address:{this.data.address}, ExtraAddress:{this.data.extraAddress}";
        }

        [Serializable]
        public class WebLink : MiceWebViewMessage<WebLink.Data>
        {
            [Serializable]
            public struct Data
            {
                public string linkUrl;
            }

            public Data data => this.bodyObj;

            public override string ToString()
                => $"<color=lime><{nameof(WebLink)}></color> <color=yellow>({this.type})</color> Link URL: '{this.data.linkUrl}'";
        }
    }
}
