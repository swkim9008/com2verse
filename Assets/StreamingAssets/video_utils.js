
//#region HTML 관련 상수값
/**
 * HTML 관련 상수값
 */
const Html =
{
	//#region HTML Element 관련 상수값
	/** HTML Element 관련 상수값*/
	Elements:
	{
		SOURCE:		"source",
	},
	//#endregion HTML Element 관련 상수값

	//#region HTML Element Attribute 관련 상수값
	/** HTML Element Attribute 관련 상수값*/
	Attributes:
	{
        ID:             "id",
        SRC:            "src",
        TYPE:           "type",
        CONTROLS:       "controls",
        CONTROLSLIST:	"controlsList"
	},
	//#endregion HTML Element Attribute 관련 상수값
};
// Html을 불변객체로 만든다.
Object.freeze(Html);
//#endregion HTML 관련 상수값

//#region 전역 상수 관리용
/** 전역 상수 관리용 */
const Constants =
{
	//#region URL Query 관련.
	QUERY_PARAM_MEDIAURL:		"mediaurl",
	QUERY_PARAM_MEDIATYPE:		"mediatype",
	QUERY_PARAM_SHOWCONTROLS:	"showcontrols",
	QUERY_PARAM_DEBUG:			"debug",
	//#endregion URL Query 관련.

	//#region string replace 관련.
	REPLACE_REGEX_QUOTE:		/^["'](.+(?=["']$))["']$/,
	REPLACE_REGEX_FIRSTGROUP:	"$1",
	//#endregion string replace 관련.

	// HLS 구분용 커스텀 Media type
	MEDIA_TYPE_CUSTOM_HLS:		"custom/hls",
	
	//#region Unity 전송 메시지 규약
	/** Unity 전송 메시지 규약 */
	VUPLEX_MSGTYPE:
	{
		LOG:		"Log",
		HLSEVENT:	"HlsEvent",
		VIDEORES:	"VideoRes",
		VIDEOEVENT:	"VideoEvent",
		POSTCODE:	"PostCode",
	},
	//#endregion Unity 전송 메시지 규약


	//#region 일반 상수값
	/** (HTML Element) video(<video ...></video>)의 미리 약속된 id 속성 이름 */
	VIDEO_ID: "video",
	/** (HTML Element) source(<source ...></source>)의 미리 약속된 id 속성 이름 */
	SOURCE_ID:	"videosrc"
	//#endregion
};
// Constants를 불변객체로 만든다.
Object.freeze(Constants);
//#endregion 전역 상수 관리용

/** 도우미 클래스 */
class Utils
{
	constructor()
	{
		if (this instanceof Utils)
		{
			throw Error('A static class cannot be instantiated.');
		}
	}

	/**
	 * URL의 query 로부터 인자값을 가져와 인자값 객체를 만든다.
	 * @returns
	 */
	static parseQuery()
	{
		const queryParams = new URLSearchParams(window.location.search);

		const extractParam = (name) =>
		{
			var param = queryParams.get(name);
			return param ? param.replace(Constants.REPLACE_REGEX_QUOTE, Constants.REPLACE_REGEX_FIRSTGROUP) : undefined;
		};

		const mediaurl = extractParam(Constants.QUERY_PARAM_MEDIAURL);
		const mediatype = extractParam(Constants.QUERY_PARAM_MEDIATYPE);
		const showControls = extractParam(Constants.QUERY_PARAM_SHOWCONTROLS);
		const debug = extractParam(Constants.QUERY_PARAM_DEBUG);

		const isHls = mediatype ? mediatype === Constants.MEDIA_TYPE_CUSTOM_HLS : false;
		const isDebug = debug ? debug === "1" : false;
		const isShowControls = showControls ? showControls === "1" : false;

		Utils.log(`<parseQuery> media_url='${mediaurl}' media_type='${mediatype}' isHls='${isHls}' show_controls='${isShowControls}' isDebug='${isDebug}'`);

		return { media_url: mediaurl, media_type: mediatype, isHls: isHls, showControls: isShowControls, isDebug: isDebug };
	}

	/**
	 * 주어진 객체가 문자열이면 그대로 반환하고 아니면 JSON 형식의 문자열로 변환한다.
	 * @param {any} obj JSON 형식의 문자열로 변환할 객체
	 * @returns
	 */
	static convertToJSON(obj)
	{
		return typeof obj == "string" ? obj : JSON.stringify(obj);
	}

	/**
	 * (vuplex) Unity로 메시지를 전송한다.
	 * (IWebView.MessageEmitted 이벤트로 수신 가능)
	 * @param {any} type 메시지 타입
	 * @param {any} body 메시지 내용
	 */
	static postToUnity(type, body)
	{
		window.vuplex.postMessage({ type: type, body: Utils.convertToJSON(body) });
	}

	/**
	 * (vuplex) Unity로 로그 메시지를 전송한다.
	 * @param {any} message 로그 메시지
	 */
	static log(message)
	{
		Utils.postToUnity(Constants.VUPLEX_MSGTYPE.LOG, message);
	}

	/**
	 * (vuplex) HLS 이벤트 핸들러
	 * @param {any} event	HLS 이벤트 타입(Hls.Events.~)
	 * @param {any} data	HLS 이벤트 데이터(JSON으로 변환 후 전송한다)
	 */
	static onHlsEvent(event, data)
	{
		Utils.postToUnity(Constants.VUPLEX_MSGTYPE.HLSEVENT, { type: event, body: Utils.convertToJSON(data) });

		// https://github.com/video-dev/hls.js/blob/master/docs/API.md#error-recovery-sample-code
		if (data.fatal)
		{
			switch (data.type)
			{
				case Hls.ErrorTypes.MEDIA_ERROR:
					Utils.log('fatal media error encountered, try to recover');
					hls.recoverMediaError();
					break;
				case Hls.ErrorTypes.NETWORK_ERROR:
					Utils.log(`fatal network error encountered [${data}]`);
					// All retries and media options have been exhausted.
					// Immediately trying to restart loading could cause loop loading.
					// Consider modifying loading policies to best fit your asset and network
					// conditions (manifestLoadPolicy, playlistLoadPolicy, fragLoadPolicy).
					break;
				default:
					// cannot recover
					hls.destroy();
					break;
			}
		}
	}

	/**
	 * (vuplex) VIDEO 이벤트 핸들러
	 * @param {any} event	VIDEO 이벤트
	 */
	static onVideoEvent(event)
	{
		if (video && event.type === "timeupdate")
		{
			event.currentTime = video.currentTime;
			event.duration = video.duration;
		}

		Utils.postToUnity(Constants.VUPLEX_MSGTYPE.VIDEOEVENT, { type: event.type, body: Utils.convertToJSON(event) });
	}

	/**
	 * 주어진 HTML Element의 자식 Element를 그 id 속성값으로 찾는다.
	 * @param {any} element	HTML Element
	 * @param {any} id		id 속성값
	 * @returns
	 */
	static getChildElementById(element, id)
	{
		var children = element.children;
		var child;
		var node;

		if (children !== true && children.length == 0)
		{
			Utils.log(`[${element.getAttribute(Html.Attributes.ID)}] empty child nodes`);
		}

		for (var i = children.length - 1; i >= 0; --i)
		{
			node = children[i];

			if (string.localeCompare(node.getAttribute(Html.Attributes.ID), id) == 0)
			{
				child = node;
			}

			Utils.log(node);
		}

		return child;
	}

	/**
	 * 주어진 HTML Element에 source element(<source ...></source>)를 자식으로 추가한다.
	 * @param {any} element		HTML Element
	 * @param {any} sourceId	source element에 추가 할 id 속성값
	 * @param {any} src			source element에 추가 할 src 속성값
	 * @param {any} type		source element에 추가 할 type 속성값
	 */
	static appendSourceElement(element, id, src, type)
	{
		const elemId = element.getAttribute(Html.Attributes.ID);

		var source = Utils.getChildElementById(element, id)
		if (source !== true)
		{
			Utils.log(`${elemId}:source => create new element.`);

			source = document.createElement(Html.Elements.SOURCE);

			source.setAttribute(Html.Attributes.ID, id);
			element.appendChild(source);
		}
		else
		{
			Utils.log(`${elemId}:source => use previous element.`);
		}

		source.setAttribute(Html.Attributes.SRC, src);
		source.setAttribute(Html.Attributes.TYPE, type);

		Utils.log(`${elemId}:source => <source src='${src}' type='${type}'>`);
	}
}
