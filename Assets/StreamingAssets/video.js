
//#region Main
{
	try
	{
		const queryParams = Utils.parseQuery();
		Utils.log(queryParams);

		var player;

		if (queryParams.isHls)
		{
			Utils.log("select hls player.");

			player = hls_player;
		}
		else
		{
			Utils.log("select video player.");

			player = video_player;
		}

		player(queryParams);
	}
	catch (e)
	{
		Utils.log(`<color=red><Exception Occurred></color> <color=white>${e}</color>\r\n${e.stack}`);
	}
}
//#endregion Main

//#region Functions

var video;
var hls;

function sendVideoRes(onReset)
{
	const width = video.videoWidth;
	const height = video.videoHeight;
	const duration = video.duration;

	Utils.log(`<video.onplaying> VideoRes => ${width}x${height} Duration => ${duration}`);

	Utils.postToUnity(Constants.VUPLEX_MSGTYPE.VIDEORES, { type: Constants.VUPLEX_MSGTYPE.VIDEORES, body: Utils.convertToJSON({ width: width, height: height, duration: duration }) });

	// run once.
	if (onReset) onReset();
}

function prepare_videoElement(params)
{
	video = document.getElementById(Constants.VIDEO_ID);
	video.disablePictureInPicture = true;

	if (params.showControls)
	{
		video.setAttribute(Html.Attributes.CONTROLS, 'controls');

		Utils.log("<prepare_videoElement> ShowControls = true");

		setVideoControlsVisible(true);
	}
	else 
	{
		Utils.log("<prepare_videoElement> ShowControls = false");

		setVideoControlsVisible(false);
	//	if (video.hasAttribute(Html.Attributes.CONTROLS))
	//	{
	//		video.removeAttribute(Html.Attributes.CONTROLS);

	//		Utils.log("<prepare_videoElement> Controls removed!");
	//	}
	}

	video.onloadedmetadata = () => sendVideoRes(() => video.onloadedmetadata = null);
	video.onplaying = () => sendVideoRes(() => video.onplaying = null);
	/*
	{
		const width = video.videoWidth;
		const height = video.videoHeight;
		const duration = video.duration;

		Utils.log(`<video.onplaying> VideoRes => ${width}x${height} Duration => ${duration}`);

		Utils.postToUnity(Constants.VUPLEX_MSGTYPE.VIDEORES, { type: Constants.VUPLEX_MSGTYPE.VIDEORES, body: Utils.convertToJSON({ width: width, height: height, duration: duration }) });

		// run once.
		video.onplaying = null;

		//if (video.requestFullscreen) {
		//	Utils.log(`${Constants.VIDEO_ID} => full screen.`);
		//	video.requestFullscreen();
		//}
		//else {
		//	Utils.log(`${Constants.VIDEO_ID} => No full screen.`);
		//}
	};
	*/

	video.onvolumechange = () =>
	{
		Utils.log(`'<video.onvolumechange>' Video Volume => ${video.volume}`);
	};

//#region Handling Video Events.
	video.addEventListener("abort", Utils.onVideoEvent);
	video.addEventListener("canplay", Utils.onVideoEvent);
	video.addEventListener("canplaythrough", Utils.onVideoEvent);
	//video.addEventListener("durationchange", Utils.onVideoEvent);
	video.addEventListener("emptied", Utils.onVideoEvent);
	//video.addEventListener("encrypted", Utils.onVideoEvent);
	video.addEventListener("ended", Utils.onVideoEvent);
	video.addEventListener("error", Utils.onVideoEvent);
	video.addEventListener("loadeddata", Utils.onVideoEvent);
	video.addEventListener("loadedmetadata", Utils.onVideoEvent);
	video.addEventListener("loadstart", Utils.onVideoEvent);
	video.addEventListener("pause", Utils.onVideoEvent);
	video.addEventListener("play", Utils.onVideoEvent);
	video.addEventListener("playing", Utils.onVideoEvent);
	//video.addEventListener("progress", Utils.onVideoEvent);
	//video.addEventListener("ratechange", Utils.onVideoEvent);
	video.addEventListener("seeked", Utils.onVideoEvent);
	video.addEventListener("seeking", Utils.onVideoEvent);
	video.addEventListener("stalled", Utils.onVideoEvent);
	video.addEventListener("suspend", Utils.onVideoEvent);
	video.addEventListener("timeupdate", Utils.onVideoEvent);
	video.addEventListener("volumechange", Utils.onVideoEvent);
	video.addEventListener("waiting", Utils.onVideoEvent);
//#endregion Handling Video Events.

	return video;
}

/**
 * 동영상 파일(.ogg, .mp4, 등등) 플레이어
 * @param {any} params	플레이 인자 객체
 */
function video_player(params)
{
	prepare_videoElement(params);

	Utils.appendSourceElement(video, Constants.SOURCE_ID, params.media_url, params.media_type);

	if (params.showControls)
	{
		// 'nofullscreen nodownload noremoteplayback noplaybackrate'
		video.setAttribute(Html.Attributes.CONTROLSLIST, 'nofullscreen nodownload');
	}

	video.muted = false;
	video.loop = true;
	video.play();

	Utils.log(`${Constants.VIDEO_ID} => play. [muted=${video.muted}][loop=${video.loop}]`);
}

/**
 * HLS 플레이어
 * @param {any} params	플레이 인자 객체
 */
function hls_player(params)
{				
	prepare_videoElement(params);

	hls = new Hls({ debug: params.isDebug, });
	hls.loadSource(params.media_url);
	hls.attachMedia(video);
	hls.on(Hls.Events.MEDIA_ATTACHED, () =>
	{
		if (params.showControls)
		{
			// 'nofullscreen nodownload noremoteplayback noplaybackrate'
			video.setAttribute(Html.Attributes.CONTROLSLIST, 'nofullscreen nodownload noplaybackrate');
		}

		video.muted = false;
		video.loop = true;
		video.play();
	});

//#region Handling HLS Events.
	hls.on(Hls.Events.MEDIA_ATTACHING, Utils.onHlsEvent);
	hls.on(Hls.Events.MEDIA_ATTACHED, Utils.onHlsEvent);
	hls.on(Hls.Events.MEDIA_DETACHING, Utils.onHlsEvent);
	hls.on(Hls.Events.MEDIA_DETACHED, Utils.onHlsEvent);
	//hls.on(Hls.Events.MANIFEST_LOADING, Utils.onHlsEvent);
	//hls.on(Hls.Events.MANIFEST_LOADED, Utils.onHlsEvent);
	//hls.on(Hls.Events.MANIFEST_PARSED, Utils.onHlsEvent);
	//hls.on(Hls.Events.STEERING_MANIFEST_LOADED, Utils.onHlsEvent);
	//hls.on(Hls.Events.LEVEL_SWITCHING, Utils.onHlsEvent);
	//hls.on(Hls.Events.LEVEL_SWITCHED, Utils.onHlsEvent);
	//hls.on(Hls.Events.LEVEL_LOADING, Utils.onHlsEvent);
	//hls.on(Hls.Events.LEVEL_LOADED, Utils.onHlsEvent);
	//hls.on(Hls.Events.FPS_DROP, Utils.onHlsEvent);
	//hls.on(Hls.Events.FPS_DROP_LEVEL_CAPPING, Utils.onHlsEvent);
	hls.on(Hls.Events.ERROR, Utils.onHlsEvent);
	hls.on(Hls.Events.DESTROYING, Utils.onHlsEvent);
	//hls.on(Hls.Events.KEY_LOADING, Utils.onHlsEvent);
	//hls.on(Hls.Events.KEY_LOADED, Utils.onHlsEvent);
	//hls.on(Hls.Events.NON_NATIVE_TEXT_TRACKS_FOUND, Utils.onHlsEvent);
	//hls.on(Hls.Events.CUES_PARSED, Utils.onHlsEvent);
//#endregion Handling HLS Events.
}
//#endregion Functions

//#region HLS/VIDEO Control Functions
function hls_detachMedia()
{
	if (hls) hls.detachMedia();
}

function hls_destroy()
{
	if (hls)
	{
		hls.destroy();
		hls = null;
	}
}

function getVideoVolume()
{
	if (video) return video.volume;
	return 0;
}

function setVideoVolume(volume)
{
	if (video) video.volume = volume;
}

function playVideo()
{
	if (video) video.play();
}

function pauseVideo()
{
	if (video) video.pause();
}

function getVideoIsPaused()
{
	if (video) video.paused;
	return false;
}

function getVideoRes()
{
	const result = { width: 0, height: 0 };

	if (video)
	{
		result.width = video.videoWidth;
		result.height = video.videoHeight;
	}

	return Utils.convertToJSON(result);
}

function getCurrentVideoTime()
{
	if (video) return video.currentTime;
	return 0;
}

function setCurrentVideoTime(seconds)
{
	if (video) video.currentTime = seconds;
}

function setCurrentVideoTimeFast(seconds)
{
	if (video) video.fastSeek(seconds);
}

function setVideoControlsVisible(visible)
{
	if (!video) return;

	if (visible)
	{
		video.setAttribute(Html.Attributes.CONTROLS, 'controls');
		video.setAttribute(Html.Attributes.CONTROLSLIST, 'nofullscreen nodownload');

		Utils.log("<setVideoControlsVisible> 'controls' added!");
	}
	else 
	{
		if (video.hasAttribute(Html.Attributes.CONTROLS))
		{
			video.removeAttribute(Html.Attributes.CONTROLS);
			Utils.log("<setVideoControlsVisible> 'controls' removed!");
		}

		if (video.hasAttribute(Html.Attributes.CONTROLSLIST))
		{
			video.removeAttribute(Html.Attributes.CONTROLSLIST);
			Utils.log("<setVideoControlsVisible> 'controlsList' removed!");
		}
	}
}

function setVideoPlaybackRate(rate)
{
	if (video) video.playbackRate = rate;
}

function setVideoDefaultPlaybackRate(rate)
{
	if (video) video.defaultPlaybackRate = rate;
}

function getVideoPlaybackRate()
{
	if (video) return video.playbackRate;

	return 1.0;
}
//#endregion HLS/VIDEO Control Functions
