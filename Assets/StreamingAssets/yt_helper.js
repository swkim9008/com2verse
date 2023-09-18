
var video;

function convertToJSON(obj)
{
	return typeof obj == "string" ? obj : JSON.stringify(obj);
}

function postToUnity(type, body)
{
	window.vuplex.postMessage({ type: type, body: convertToJSON(body) });
}

function log(message)
{
	postToUnity("Log", `(YouTube) ${message}`);
}

function onVideoEvent(event)
{
	if (video && event.type === "timeupdate")
	{
		event.currentTime = video.currentTime;
		event.duration = video.duration;
	}

	postToUnity("VideoEvent", { type: event.type, body: convertToJSON(event) });
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

	return convertToJSON(result);
}

function setVideoControlsVisible(visible)
{
	if (!video) return;

	if (visible)
	{
		video.setAttribute("controls", 'controls');
		video.setAttribute("controlsList", 'nofullscreen nodownload');

		log("<setVideoControlsVisible> 'controls' added!");
	}
	else 
	{
		if (video.hasAttribute("controls"))
		{
			video.removeAttribute("controls");
			log("<setVideoControlsVisible> 'controls' removed!");
		}

		if (video.hasAttribute("controlsList"))
		{
			video.removeAttribute("controlsList");
			log("<setVideoControlsVisible> 'controlsList' removed!");
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

function sendVideoRes(onReset)
{
	const width = video.videoWidth;
	const height = video.videoHeight;
	const duration = video.duration;

	log(`<video.onplaying> VideoRes => ${width}x${height} Duration => ${duration}`);

	postToUnity("VideoRes", { type: "VideoRes", body: convertToJSON({ width: width, height: height, duration: duration }) });

	// run once.
	if (onReset) onReset();
}

try
{
    video = document.querySelector('.video-stream');
    video.disablePictureInPicture = true;

	video.onloadedmetadata = () => sendVideoRes(() => video.onloadedmetadata = null);
	video.onplaying = () => sendVideoRes(() => video.onplaying = null);
	/*
	{
		const width = video.videoWidth;
		const height = video.videoHeight;
		const duration = video.duration;

		log(`<video.onplaying> VideoRes => ${width}x${height} Duration => ${duration}`);

		postToUnity("VideoRes", { type: "VideoRes", body: convertToJSON({ width: width, height: height, duration: duration }) });

		// run once.
		video.onplaying = null;
	};
	*/

	video.onvolumechange = () =>
	{
		log(`'<video.onvolumechange>' Video Volume => ${video.volume}`);
	};

	/*
    video.onended = () => 
    {
        log('<video.onended>');
        video.play();
    };
	*/

	video.addEventListener("abort", onVideoEvent);
	video.addEventListener("canplay", onVideoEvent);
	video.addEventListener("canplaythrough", onVideoEvent);
    video.addEventListener("emptied", onVideoEvent);
	video.addEventListener("ended", onVideoEvent);
	video.addEventListener("error", onVideoEvent);
	video.addEventListener("loadeddata", onVideoEvent);
	video.addEventListener("loadedmetadata", onVideoEvent);
	video.addEventListener("loadstart", onVideoEvent);
	video.addEventListener("pause", onVideoEvent);
	video.addEventListener("play", onVideoEvent);
	video.addEventListener("playing", onVideoEvent);
	video.addEventListener("seeked", onVideoEvent);
	video.addEventListener("seeking", onVideoEvent);
	video.addEventListener("stalled", onVideoEvent);
	video.addEventListener("suspend", onVideoEvent);
	video.addEventListener("timeupdate", onVideoEvent);
    video.addEventListener("volumechange", onVideoEvent);
    video.addEventListener("waiting", onVideoEvent);

	/*
	const config = { attribute: false, childList: true, subtree: true }

	const callback = (mutationList, obsrver) =>
	{
		for (const mutation of mutationList)
		{
			if (mutation.type === "childList")
			{
				var nodeList = mutation.addedNodes;
				for (const node of nodeList)
				{
					if (node.tagName === 'A')
					{
						const linkUrl = node.href;

						// Remove all event listeners.
						node.replaceWith(node.cloneNode(true));

						node.addEventListener('click', e =>
						{
							// Send the link URL to the C# script.
							postToUnity("WebLink", { type: "WebLink", body: convertToJSON({ linkUrl: linkUrl }) });

							// Call preventDefault() to prevent the webview from loading the URL.
							e.preventDefault();
						});

						log(`tag='${node.tagName}' id='${node.id}' class='${node.className}' overrided!`)
					}
				}
			}
		}
	};

	const observer = new MutationObserver(callback);

	observer.observe(document, config);
	*/

	/*
    document.addEventListener('click', e =>
    {
        if (e.target.tagName === 'A')
        {
            const linkUrl = e.target.href;

            // Remove all event listeners.
            e.target.replaceWith(e.target.cloneNode(true));

            postToUnity("WebLink", { type: "WebLink", body : convertToJSON({ linkUrl: linkUrl }) });

            // Call preventDefault() to prevent the webview from loading the URL.
            e.preventDefault();
            e.stopPropagation();
        }
    });
	*/

	/*
	// Run this code every 500 ms.
	setInterval(() => {
		const newLinks = document.querySelectorAll('a[href]:not([overridden])');
		for (const link of newLinks) {

			// Remove all event listeners.
			link.replaceWith(link.cloneNode(true));

			link.setAttribute('overridden', true);
			const linkUrl = link.href;

			link.addEventListener('click', event => {
				// Send the link URL to the C# script.
				postToUnity("WebLink", { type: "WebLink", body: convertToJSON({ linkUrl: linkUrl }) });
				// Call preventDefault() to prevent the webview from loading the URL.
				event.preventDefault();
			});
		}
	}, 500);
	*/
}
catch (e)
{
	log(`<color=red><Exception Occurred></color> <color=white>${e}</color>\r\n${e.stack}`);
}
