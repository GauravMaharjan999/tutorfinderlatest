var HlsPlayer = function (useroptions) {

    var options;
    var player;
    var videoElement;
    var displayUTCTimeCodes = false;

    var captionMenu = null;
    var bitrateListMenu = null;
    var trackSwitchMenu = null;
    var menuHandlersList = [];
    var lastVolumeLevel = NaN;
    var seeking = false;
    var videoControllerVisibleTimeout;
    var liveThresholdSecs = 12;
    var isLive = false;
    var bufferData = null;
    var startedPlaying = false;
    var Resolutions = {
        256: { w: 256, h: 144, name: "144P" },
        424: { w: 424, h: 240, name: "240P" },
        640: { w: 640, h: 360, name: "SD" },
        848: { w: 848, h: 480, name: "480P" },
        1280: { w: 1280, h: 720, name: "HD" },
        1920: { w: 1920, h: 1080, name: "FHD" },
        2048: { w: 2048, h: 1080, name: "2K" },
        2560: { w: 2560, h: 1440, name: "QHD" },
        3840: { w: 3840, h: 2160, name: "4K" },
        7680: { w: 7680, h: 4320, name: "8K" }
    };
    options = useroptions;
    videoElement = options.videoElement;
    //************************************************************************************
    // THUMBNAIL CONSTANTS
    //************************************************************************************
    // Maximum percentage of player height that the thumbnail will fill
    var maxPercentageThumbnailScreen = 0.15;
    // Separation between the control bar and the thumbnail (in px)
    var bottomMarginThumbnail = 10;
    // Maximum scale so small thumbs are not scaled too high
    var maximumScale = 2;

    var init = function (url) {
        try {
            var config = {
                xhrSetup: function (xhr, url) {
                    xhr.withCredentials = false; // do send cookies
                    var xurl = new URL(url);
                    xurl.searchParams.set('token', getTokenSync());
                    xhr.open('GET', xurl);
                }
            }
            player = new Hls();
            player.loadSource(url);
            player.attachMedia(videoElement);
            initControls();
            initializeControls();
            var gll = window;
            gll["mero"] = player;
            togglePlayPause();

        } catch (err) {
            console.log("dash err", err);
        }
    };
    var reinit = function (url) {
        var config = {
            xhrSetup: function (xhr, url) {
                xhr.withCredentials = false; // do send cookies
                var xurl = new URL(url);
                xurl.searchParams.set('token', getTokenSync());
                xhr.open('GET', xurl);
            }
        }
        if (player) {
            player.destroy();
            //if ( player.bufferTimer) {
            // clearInterval( player.bufferTimer);
            // player.bufferTimer = undefined;
            // }
            player = null;
            // destroy();
            resetControllers();


        }
        player = new Hls();
        player.loadSource(url);
        player.attachMedia(videoElement);
        registerHlsEvents();

        videoElement.play();
        togglePlayPause();
    }
    var canPlay = function (url) {
        return /\.m3u8$/i.test(url);
    }
    var initControls = function () {
        options.videoController = document.getElementById("videoController");
        options.btnPlayPauseLarge = document.getElementById("playpause");
        options.courseHeader = document.getElementById("course-header");
        options.poster = document.getElementById("posterId");
        options.playPauseBtn = document.getElementById("playPauseBtn");
        options.bitrateListBtn = document.getElementById("bitrateListBtn");
        // options.bitrateListMenu = document.getElementById('bitrateListBtn');
        options.captionBtn = document.getElementById("captionBtn");
        options.trackSwitchBtn = document.getElementById("trackSwitchBtn");
        options.seekbar = document.getElementById("seekbar");
        options.seekbarPlay = document.getElementById("seekbar-play");
        options.seekbarBuffer = document.getElementById("seekbar-buffer");
        options.muteBtn = document.getElementById("muteBtn");
        options.volumebar = document.getElementById("volumebar");
        options.fullscreenBtn = document.getElementById("fullscreenBtn");
        options.timeDisplay = document.getElementById("videoTime");
        options.durationDisplay = document.getElementById("videoDuration");
        options.thumbnailContainer = document.getElementById(
            "thumbnail-container"
        );
        options.thumbnailElem = document.getElementById("thumbnail-elem");
        options.thumbnailTimeLabel = document.getElementById(
            "thumbnail-time-label"
        );
    }
    var togglePlayPause = function () {
        if (!videoElement.paused) {
            showPlayBtn();
        } else {
            showPauseBtn();
        }
    }
    var showPlayBtn = function () {
        var span = document.getElementById("iconPlayPause");
        if (span !== null) {
            span.classList.remove("icon-pause");
            span.classList.add("icon-play");
        }
        options.btnPlayPauseLarge.setAttribute("data-state", "play");
    };
    var showPauseBtn = function () {
        var span = document.getElementById("iconPlayPause");
        if (span !== null) {
            span.classList.remove("icon-play");
            span.classList.add("icon-pause");
            options.poster.classList.remove("hide");
            options.poster.classList.add("hide");
        }
        options.btnPlayPauseLarge.setAttribute("data-state", "pause");
    };
    var onPlayPauseClick = function (/*e*/) {
        togglePlayPause();
        videoElement.paused
            ? videoElement.play()
            : videoElement.pause();
    };

    var onPlaybackPaused = function (/*e*/) {
        togglePlayPause();
    };
    var onPlayStart = function (/*e*/) {
        setTime(
            displayUTCTimeCodes ? player.timeAsUTC() : player.time()
        );
        updateDuration();
        togglePlayPause();
    };
    //************************************************************************************
    // TIME/DURATION
    //************************************************************************************
    var formatUTC = function (time, locales, hour12) {
        var withDate =
            arguments.length <= 3 || arguments[3] === undefined
                ? false
                : arguments[3];

        var dt = new Date(time * 1000);
        var d = dt.toLocaleDateString(locales);
        var t = dt.toLocaleTimeString(locales, {
            hour12: hour12
        });
        return withDate ? t + " " + d : t;
    };
    var convertToTimeCode = function (value) {
        value = Math.max(value, 0);

        var h = Math.floor(value / 3600);
        var m = Math.floor((value % 3600) / 60);
        var s = Math.floor((value % 3600) % 60);
        return (
            (h === 0 ? "" : h < 10 ? "0" + h.toString() + ":" : h.toString() + ":") +
            (m < 10 ? "0" + m.toString() : m.toString()) +
            ":" +
            (s < 10 ? "0" + s.toString() : s.toString())
        );
    };
    var setDuration = function (value) {
        if (isLive) {
            options.durationDisplay.textContent = "● LIVE";
            if (!options.durationDisplay.onclick) {
                options.durationDisplay.onclick = seekLive;
                options.durationDisplay.classList.add("live-icon");
            }
        } else if (!isNaN(value)) {
            options.durationDisplay.textContent = displayUTCTimeCodes
                ? formatUTC(value)
                : convertToTimeCode(value);
            options.durationDisplay.classList.remove("live-icon");
        }
    };
    var updateDuration = function () {
        var duration = videoElement.duration;
        if (duration !== parseFloat(options.seekbar.max)) {
            //check if duration changes for live streams..
            if (!startedPlaying && duration && isLive) {
                seekLive();
                startedPlaying = true;
            }
            // setDuration(displayUTCTimeCodes ? player.durationAsUTC() : duration);
            setDuration(duration);
            options.seekbar.max = duration;
        }
    };
    var setTime = function (value) {
        if (value < 0) {
            return;
        }
        if (isLive && videoElement.duration) {
            var liveDelay = videoElement.duration - value;
            if (liveDelay < liveThresholdSecs) {
                options.durationDisplay.classList.add("live");
                options.timeDisplay.textContent = "";
            } else {
                options.durationDisplay.classList.remove("live");
                options.timeDisplay.textContent =
                    "- " + convertToTimeCode(liveDelay);
            }
        } else if (!isNaN(value)) {
            //options.timeDisplay.textContent = displayUTCTimeCodes ? player.formatUTC(value) : player.convertToTimeCode(value);
            options.timeDisplay.textContent = convertToTimeCode(value);
        }
    };
    var onPlayTimeUpdate = function (/*e*/) {
        updateDuration();
        if (!seeking) {
            // setTime(displayUTCTimeCodes ? player.timeAsUTC() : player.time());
            setTime(videoElement.currentTime);
            if (options.seekbarPlay) {
                if (
                    isLive &&
                    videoElement.duration - videoElement.currentTime <
                    liveThresholdSecs
                ) {
                    options.seekbarPlay.style.width = "100%";
                } else {
                    options.seekbarPlay.style.width =
                        (videoElement.currentTime / videoElement.duration) * 100 +
                        "%";
                }
                options.seekbarPlay.style.width =
                    (videoElement.currentTime / videoElement.duration) * 100 +
                    "%";
            }
            if (options.seekbarBuffer) {
                // options.seekbarBuffer.style.width = ((player.time() + getBufferLevel()) / player.duration() * 100) + '%';
                if (videoElement.buffered.length > 0) {
                    options.seekbarBuffer.style.width =
                        ((videoElement.currentTime + videoElement.buffered.end(0)) /
                            videoElement.duration) *
                        100 +
                        "%";
                }
            }

            if (options.seekbar.getAttribute("type") === "range") {
                options.seekbar.value = videoElement.currentTime;
            }
        }
    };
    var toggleMuteBtnState = function () {
        var span = document.getElementById("iconMute");
        if (videoElement.muted) {
            span.classList.remove("icon-mute-off");
            span.classList.add("icon-mute-on");
        } else {
            span.classList.remove("icon-mute-on");
            span.classList.add("icon-mute-off");
        }
    };
    var onMuteClick = function (/*e*/) {
        if (videoElement.muted && !isNaN(lastVolumeLevel)) {
            setVolume(lastVolumeLevel);
        } else {
            lastVolumeLevel = parseFloat(options.volumebar.value);
            setVolume(0);
        }
        videoElement.muted = videoElement.volume === 0;
        toggleMuteBtnState();
    };
    var setVolume = function (value) {
        if (typeof value === "number") {
            options.volumebar.value = value;
        }
        videoElement.volume = parseFloat(options.volumebar.value);
        videoElement.muted = videoElement.volume === 0;
        if (isNaN(lastVolumeLevel)) {
            lastVolumeLevel = videoElement.volume;
        }
        toggleMuteBtnState();
    };
    var calculateTimeByEvent = function (event) {
        var seekbarRect = options.seekbar.getBoundingClientRect();
        return Math.floor(
            (videoElement.duration * (event.clientX - seekbarRect.left)) /
            seekbarRect.width
        );
    };

    var onSeeking = function (event) {
        //TODO Add call to seek in trick-mode once implemented. Preview Frames.
        seeking = true;
        var mouseTime = calculateTimeByEvent(event);
        if (options.seekbarPlay) {
            options.seekbarPlay.style.width =
                (mouseTime / videoElement.duration) * 100 + "%";
        }
        setTime.bind(this, mouseTime);
        //document.addEventListener('mousemove', onSeekBarMouseMove.bind(this), true);
        //document.addEventListener('mouseup', onSeeked.bind(this), true);
    };
    var onSeeked = function (event) {
        seeking = false;
        //document.removeEventListener('mousemove', onSeekBarMouseMove, true);
        //document.removeEventListener('mouseup', onSeeked, true);

        // seeking
        var mouseTime = calculateTimeByEvent(event);
        if (!isNaN(mouseTime)) {
            videoElement.currentTime = mouseTime;
        }

        onSeekBarMouseMoveOut(event);

        if (options.seekbarPlay) {
            options.seekbarPlay.style.width =
                (mouseTime / videoElement.duration) * 100 + "%";
        }
    };

    var onSeekBarMouseMove = function (event) {
        if (!options.thumbnailContainer || !options.thumbnailElem) return;

        // Take into account page offset and seekbar position
        var elem = options.videoContainer; //|| video;
        var videoContainerRect = elem.getBoundingClientRect();
        var seekbarRect = options.seekbar.getBoundingClientRect();
        var videoControllerRect = options.videoController.getBoundingClientRect();

        // Calculate time position given mouse position
        var left = event.clientX - seekbarRect.left;
        var mouseTime = calculateTimeByEvent.bind(this, event);
        if (isNaN(mouseTime)) return;

        // Update timer and play progress bar if mousedown (mouse click down)
        if (seeking) {
            setTime(mouseTime);
            if (options.seekbarPlay) {
                options.seekbarPlay.style.width =
                    (mouseTime / player.duration()) * 100 + "%";
            }
        }

        // Get thumbnail information
        player.getThumbnail(mouseTime, function (thumbnail) {
            if (!thumbnail) return;

            // Adjust left variable for positioning thumbnail with regards to its viewport
            left += seekbarRect.left - videoContainerRect.left;
            // Take into account thumbnail control
            var ctrlWidth = parseInt(
                window.getComputedStyle(options.thumbnailElem).width
            );
            if (!isNaN(ctrlWidth)) {
                left -= ctrlWidth / 2;
            }

            var scale =
                (videoContainerRect.height * maxPercentageThumbnailScreen) /
                thumbnail.height;
            if (scale > maximumScale) {
                scale = maximumScale;
            }

            // Set thumbnail control position
            options.thumbnailContainer.style.left = left + "px";
            options.thumbnailContainer.style.display = "";
            options.thumbnailContainer.style.bottom +=
                Math.round(videoControllerRect.height + bottomMarginThumbnail) +
                "px";
            options.thumbnailContainer.style.height =
                Math.round(thumbnail.height) + "px";

            var backgroundStyle =
                'url("' +
                thumbnail.url +
                '") ' +
                (thumbnail.x > 0 ? "-" + thumbnail.x : "0") +
                "px " +
                (thumbnail.y > 0 ? "-" + thumbnail.y : "0") +
                "px";
            options.thumbnailElem.style.background = backgroundStyle;
            options.thumbnailElem.style.width = thumbnail.width + "px";
            options.thumbnailElem.style.height = thumbnail.height + "px";
            options.thumbnailElem.style.transform =
                "scale(" + scale + "," + scale + ")";

            if (options.thumbnailTimeLabel) {
                //options.thumbnailTimeLabel.textContent = displayUTCTimeCodes ? player.formatUTC(mouseTime) : player.convertToTimeCode(mouseTime);
                options.thumbnailTimeLabel.textContent = mouseTime;
            }
        });
    };

    var onSeekBarMouseMoveOut = function (/*e*/) {
        if (!options.thumbnailContainer) return;
        options.thumbnailContainer.style.display = "none";
    };

    var getScrollOffset = function () {
        if (window.pageXOffset) {
            return {
                x: window.pageXOffset,
                y: window.pageYOffset
            };
        }
        return {
            x: document.documentElement.scrollLeft,
            y: document.documentElement.scrollTop
        };
    };

    var seekLive = function () {
        videoElement.currentTime = videoElement.duration;
    };
    //************************************************************************************
    // FULLSCREEN
    //************************************************************************************

    var onFullScreenChange = function (/*e*/) {

        if (isFullscreen()) {
            enterFullscreen();
            var icon = document.querySelector(".icon-fullscreen-enter");
            icon.classList.remove("icon-fullscreen-enter");
            icon.classList.add("icon-fullscreen-exit");
        } else {
            exitFullscreen();
            var icon = document.querySelector(".icon-fullscreen-exit");
            icon.classList.remove("icon-fullscreen-exit");
            icon.classList.add("icon-fullscreen-enter");
        }
    };

    var isFullscreen = function () {
        var g = document;
        return (
            g.fullscreenElement ||
            g.msFullscreenElement ||
            g.mozFullScreen ||
            g.webkitIsFullScreen
        );
    };

    var enterFullscreen = function () {
        var element = document.getElementById("playercontainer");// options.videoContainer;//|| video;

        if (element.requestFullscreen) {
            element.requestFullscreen();
        } else if (element.msRequestFullscreen) {
            element.msRequestFullscreen();
        } else if (element.mozRequestFullScreen) {
            element.mozRequestFullScreen();
        } else {
            element.webkitRequestFullScreen();
        }
        options.videoController.classList.add('video-controller-fullscreen');
        options.videoContainer.classList.add('fullscreen');
        window.addEventListener('mousemove', onFullScreenMouseMove.bind(this));
        onFullScreenMouseMove();
    };

    var onFullScreenMouseMove = function () {
        clearFullscreenState();
        var x = this;
        videoControllerVisibleTimeout = setTimeout(function () {
            options.videoController.classList.add('hide');
            options.btnPlayPauseLarge.classList.add('hide');
            options.courseHeader.classList.add('hide');
        }, 4000);
    };

    var clearFullscreenState = function () {
        clearTimeout(videoControllerVisibleTimeout);
        options.videoController.classList.remove('hide');
        options.btnPlayPauseLarge.classList.remove('hide');
        options.courseHeader.classList.remove('hide');
    };

    var exitFullscreen = function () {
        window.removeEventListener("mousemove", onFullScreenMouseMove);
        clearFullscreenState.bind(this);
        var g = document;
        if (g.exitFullscreen) {
            g.exitFullscreen();
        } else if (g.mozCancelFullScreen) {
            g.mozCancelFullScreen();
        } else if (g.msExitFullscreen) {
            g.msExitFullscreen();
        } else {
            g.webkitCancelFullScreen();
        }
        options.videoController.classList.remove('video-controller-fullscreen');
        options.videoContainer.classList.remove('fullscreen');
    };

    var onFullscreenClick = function (/*e*/) {
        if (!isFullscreen()) {
            enterFullscreen();
        } else {
            exitFullscreen();
        }
        if (captionMenu) {
            captionMenu.classList.add("hide");
        }
        if (bitrateListMenu) {
            bitrateListMenu.classList.add("hide");
        }
        if (trackSwitchMenu) {
            trackSwitchMenu.classList.add("hide");
        }
    };
    //************************************************************************************
    // Audio Video MENU
    //************************************************************************************

    var onSubtitlesLoaded = function (e) {
        // Subtitles/Captions Menu //XXX we need to add two layers for captions & subtitles if present.
        if (!captionMenu) {
            var x = this;
            var contentFunc = function (element, index) {
                if (isNaN(index)) {
                    return "OFF";
                }
                var label = x.getLabelForLocale(element.name);
                if (label) {
                    return label + " : " + element.kind;
                }

                return element.lang + " : " + element.kind;
            };
            captionMenu = createMenu(
                { menuType: "caption", arr: player.subtitleTracks },
                contentFunc
            );
            var _this = this;
            var func = function () {
                onMenuClick(captionMenu, options.captionBtn);
            };
            menuHandlersList.push(func);
            options.captionBtn.addEventListener("click", func);
            options.captionBtn.classList.remove("hide");
        }
    };

    var onSourceInitialized = function () {
        startedPlaying = false;
    };
    var onQualityLoaded = function (event, data) {
        isLive = data.details.live;
        // console.log('islive',isLive)
        var contentFunc;
        //Bitrate Menu
        if (options.bitrateListBtn) {
            destroyBitrateMenu();
            var availableBitrates = {
                menuType: "bitrate",
                audio: [],
                video: []
            };
            availableBitrates.audio = [];
            availableBitrates.video = player.levels || [];
            if (
                availableBitrates.audio.length > 1 ||
                availableBitrates.video.length > 1
            ) {
                contentFunc = function (element, index) {
                    var result = isNaN(index)
                        ? " Auto Switch"
                        : Math.floor(element.bitrate / 1000) + " kbps";
                    result +=
                        element && element.width && element.height
                            ? " (" + element.width + "x" + element.height + ")"
                            : "";
                    return result;
                };

                bitrateListMenu = createMenu(availableBitrates, contentFunc);
                var _this = this;
                var func = function () {
                    onMenuClick(bitrateListMenu, options.bitrateListBtn);
                };
                menuHandlersList.push(func);
                //console.log('menu', bitrateListMenu)
                options.bitrateListBtn.addEventListener("click", func);
                options.bitrateListBtn.classList.remove("hide");
            } else {
                options.bitrateListBtn.classList.add("hide");
            }
        }
    };
    var onAudioTracksLoaded = function (/*e*/) {
        updateDuration();
        var contentFunc;
        //Track Switch Menu
        if (!trackSwitchMenu && options.trackSwitchBtn) {
            var availableTracks = { menuType: "track", audio: [], video: [] };
            availableTracks.audio = player.audioTracks;
            availableTracks.video = player.audioTracks; // these return empty arrays so no need to check for null

            if (
                availableTracks.audio.length > 1 ||
                availableTracks.video.length > 1
            ) {
                contentFunc = function (element) {
                    return getLabelForLocale(element.name) || element.lang;
                };
                trackSwitchMenu = createMenu(
                    availableTracks,
                    contentFunc.bind(this)
                );
                var _this = this;
                var func = function () {
                    onMenuClick(_trackSwitchMenu, options.trackSwitchBtn);
                };
                menuHandlersList.push(func);
                options.trackSwitchBtn.addEventListener("click", func);
                options.trackSwitchBtn.classList.remove("hide");
            }
        }

    };

    var onStreamTeardownComplete = function (/*e*/) {
        showPlayBtn();
        options.timeDisplay.textContent = "00:00";
    };

    var createMenu = function (info, contentFunc) {
        var menuType = info.menuType;
        var el = document.createElement("div");

        el.id = menuType + "Menu";
        el.classList.add("menu-container");
        el.classList.add("hide");
        el.classList.add("unselectable");
        el.classList.add("menu-item-unselected");
        //options.videoController.appendChild(el);
        var ul = document.createElement("ul");
        ul.classList.add("player-menu");
        var header = document.createElement("h4");
        switch (menuType) {
            case "caption":
                header.innerText = "Caption";
                el.appendChild(header);
                options.captionBtn.appendChild(el);
                ul = createMenuContent(
                    ul,
                    getMenuContent(menuType, info.arr, contentFunc),
                    "caption",
                    menuType + "-list"
                );
                el.appendChild(ul);
                setMenuItemsState(
                    getMenuInitialIndex(info.arr, menuType),
                    menuType + "-list"
                );

                break;
            case "track":
                header.innerText = "Audio";
                el.appendChild(header);
                options.trackSwitchBtn.appendChild(el);
                if (info.audio.length > 1) {
                    // el.appendChild(createMediaTypeMenu('audio'));
                    ul = createMenuContent(
                        ul,
                        getMenuContent(menuType, info.audio, contentFunc),
                        "audio",
                        "audio-" + menuType + "-list"
                    );
                    el.appendChild(ul);
                    setMenuItemsState(
                        getMenuInitialIndex(info.audio, menuType, "audio"),
                        "audio-" + menuType + "-list"
                    );
                }

                break;
            case "bitrate":
                header.innerText = "Quality";
                el.appendChild(header);
                options.bitrateListBtn.appendChild(el);
                if (info.video.length > 1) {
                    // el.appendChild(createMediaTypeMenu('video'));
                    ul = createMenuContent(
                        ul,
                        getMenuContent(menuType, info.video, contentFunc),
                        "video",
                        "video-" + menuType + "-list"
                    );
                    el.appendChild(ul);
                    setMenuItemsState(
                        getMenuInitialIndex(info.video, menuType, "video"),
                        "video-" + menuType + "-list"
                    );
                }
                if (info.audio.length > 1) {
                    // el.appendChild(createMediaTypeMenu('audio'));
                    ul = createMenuContent(
                        ul,
                        getMenuContent(menuType, info.audio, contentFunc),
                        "audio",
                        "audio-" + menuType + "-list"
                    );
                    el.appendChild(ul);
                    setMenuItemsState(
                        getMenuInitialIndex(info.audio, menuType, "audio"),
                        "audio-" + menuType + "-list"
                    );
                }

                break;
        }

        window.addEventListener("resize", handleMenuPositionOnResize, true);
        return el;
    };

    var getMenuInitialIndex = function (info, menuType, mediaType) {
        return 0;//auto or off
        //if (menuType === "track") {
        //    var mediaInfo = player.getCurrentTrackFor(mediaType);
        //    var idx = 0;
        //    if (info.length > 1) {
        //        let x = this;
        //        info.forEach(function (element: any, index: number) {
        //            if (x.isTracksEqual(element, mediaInfo)) {
        //                idx = index;
        //                return true;
        //            }
        //        });
        //    }

        //    return idx;
        //} else if (menuType === "bitrate") {
        //    var cfg = player.getSettings();
        //    if (
        //        cfg.streaming &&
        //        cfg.streaming.abr &&
        //        cfg.streaming.abr.initialBitrate
        //    ) {
        //        return cfg.streaming.abr.initialBitrate["mediaType"] | 0;
        //    }
        //    return 0;
        //} else if (menuType === "caption") {
        //    return player.getCurrentTextTrackIndex() + 1;
        //}
    };

    var isTracksEqual = function (t1, t2) {
        var sameId = t1.id === t2.id;
        var sameViewpoint = t1.viewpoint === t2.viewpoint;
        var sameLang = t1.lang === t2.lang;
        var sameRoles = t1.roles.toString() === t2.roles.toString();
        var sameAccessibility =
            t1.accessibility.toString() === t2.accessibility.toString();
        var sameAudioChannelConfiguration =
            t1.audioChannelConfiguration.toString() ===
            t2.audioChannelConfiguration.toString();

        return (
            sameId &&
            sameViewpoint &&
            sameLang &&
            sameRoles &&
            sameAccessibility &&
            sameAudioChannelConfiguration
        );
    };

    var getMenuContent = function (type, arr, contentFunc) {
        try {
            var content = [];
            arr.forEach(function (element, index) {
                content.push(contentFunc(element, index));
            });
            if (type !== "track") {
                content.unshift(contentFunc(null, NaN));
            }
            return content;
        } catch (e) {
            console.log("err", e);
        }
    };

    var getBrowserLocale = function () {
        return navigator.languages && navigator.languages.length
            ? navigator.languages
            : [navigator.language];
    };

    var getLabelForLocale = function (labels) {
        var locales = getBrowserLocale();

        for (var i = 0; i < labels.length; i++) {
            for (var j = 0; j < locales.length; j++) {
                if (
                    labels[i].lang &&
                    locales[j] &&
                    locales[j].indexOf(labels[i].lang) > -1
                ) {
                    return labels[i].text;
                }
            }
        }

        return labels.length === 1 ? labels[0].text : null;
    };
    var createMediaTypeMenu = function (type) {
        var div = document.createElement("div");
        var title = document.createElement("div");
        var content = document.createElement("ul");

        div.id = type;

        title.textContent = type === "video" ? "Video" : "Audio";
        title.classList.add("menu-sub-menu-title");

        content.id = type + "Content";
        content.classList.add(type + "-menu-content");

        div.appendChild(title);
        div.appendChild(content);

        return div;
    };

    var createMenuContent = function (menu, arr, mediaType, name) {
        for (var i = 0; i < arr.length; i++) {
            var item = document.createElement("li");
            item.id = name + "Item_" + i;
            item.setAttribute("index", i.toString()); // index = i;
            item.setAttribute("mediaType", mediaType);
            item.setAttribute("selected", false.toString());
            item.setAttribute("xname", name);
            item.textContent = arr[i];

            // item.onmouseover = function (/*e*/) {
            //     if (item.getAttribute("selected") !== "true") {
            //         item.classList.add('menu-item-over');
            //     }
            // };
            // item.onmouseout = function (/*e*/) {
            //     item.classList.remove('menu-item-over');
            // };

            item.onclick = setMenuItemsState.bind(this, item, undefined);

            // var el;
            // if (mediaType === 'caption') {
            //     el = menu.querySelector('ul');
            // } else {
            //     el = menu.querySelector('.' + mediaType + '-menu-content');
            // }

            menu.appendChild(item);
        }

        return menu;
    };

    var onMenuClick = function (menu, btn) {
        if (menu.classList.contains("hide")) {
            menu.classList.remove("hide");
            menu.onmouseleave = function (/*e*/) {
                this.classList.add("hide");
            };
        } else {
            menu.classList.add("hide");
        }
        // menu.style.position = isFullscreen() ? 'fixed' : 'absolute';
        // positionMenu(menu, btn);
    };

    var setMenuItemsState = function (value, type) {
        var self =
            typeof value === "number"
                ? document.getElementById(type + "Item_" + value)
                : value;
        var nodes = self.parentElement.children;

        for (var i = 0; i < nodes.length; i++) {
            nodes[i].selected = false;
            nodes[i].classList.remove("menu-item-selected");
            nodes[i].classList.remove("active");
            nodes[i].classList.add("menu-item-unselected");
        }
        self.setAttribute("selected", true.toString());
        self.classList.remove("menu-item-over");
        self.classList.remove("menu-item-unselected");
        self.classList.add("menu-item-selected");
        self.classList.add("active");
        var name = self.getAttribute("xname");
        var mediaType = self.getAttribute("mediaType");
        var index = self.getAttribute("index");
        //console.log("index", index)
        if (type === undefined) {
            // User clicked so type is part of item binding.
            switch (name) {
                case "video-bitrate-list":
                    player.nextLevel = (index - 1);
                    var bitrates = player.levels;
                    var getQualityFor = player.currentLevel;
                    var activeVideo = bitrates[getQualityFor];
                    var videoResolution = Resolutions[activeVideo.width];
                    //console.log(videoResolution,getQualityFor,activeVideo)
                    if (videoResolution != null) {
                        options.bitrateListBtn.dataset.quality = videoResolution.name;
                    } else {
                        options.bitrateListBtn.dataset.quality = "";
                    }
                case "audio-bitrate-list":

                    player.nextLevel = (index - 1);
                    var bitrates = player.levels;
                    var getQualityFor = player.currentLevel;
                    var activeVideo = bitrates[getQualityFor];
                    var videoResolution = Resolutions[activeVideo.width];
                    /// console.log(videoResolution,getQualityFor,activeVideo)
                    if (videoResolution != null) {
                        options.bitrateListBtn.dataset.quality = videoResolution.name;
                    } else {
                        options.bitrateListBtn.dataset.quality = "";
                    }
                    break;
                case "caption-list":
                    player.subtitleTrack = (index - 1);
                    break;
                case "video-track-list":
                case "audio-track-list":
                    player.audioTrack = (index - 1);
                    // player.setCurrentTrack(
                    //   player.getTracksFor(mediaType)[index]
                    // );
                    break;
            }
        }
    };

    var handleMenuPositionOnResize = function (/*e*/) {
        // if (captionMenu) {
        //     positionMenu(captionMenu, options.captionBtn);
        // }
        // if (bitrateListMenu) {
        //     positionMenu(bitrateListMenu, options.bitrateListBtn);
        // }
        // if (trackSwitchMenu) {
        //     positionMenu(trackSwitchMenu, options.trackSwitchBtn);
        // }
    };

    var positionMenu = function (menu, btn) {
        if (
            btn.offsetLeft + menu.clientWidth >=
            options.videoController.clientWidth
        ) {
            menu.style.right = "0px";
            menu.style.left = "";
        } else {
            menu.style.left = btn.offsetLeft + "px";
        }
        var menu_y =
            options.videoController.offsetTop -
            menu.offsetHeight -
            (isFullscreen() ? 0 : 100);
        menu.style.top = menu_y + "px";
    };

    var destroyBitrateMenu = function () {
        if (bitrateListMenu) {
            var x = this;
            menuHandlersList.forEach(function (item) {
                // console.log('clean', x.options.bitrateListBtn,item)
                options.bitrateListBtn.removeEventListener("click", item);
            });
            // options.videoController.removeChild(bitrateListMenu);
            options.bitrateListBtn.removeChild(bitrateListMenu);
            bitrateListMenu = null;
        }
    };

    //************************************************************************************
    //IE FIX
    //************************************************************************************

    var coerceIEInputAndChangeEvents = function (slider, addChange) {
        var fireChange = function (/*e*/) {
            var changeEvent = document.createEvent("Event");
            changeEvent.initEvent("change", true, true);
            changeEvent.forceChange = true;
            slider.dispatchEvent(changeEvent);
        };

        addEventListener(
            "change",
            function (e) {
                var inputEvent;
                if (!e.forceChange && e.target.getAttribute("type") === "range") {
                    e.stopPropagation();
                    inputEvent = document.createEvent("Event");
                    inputEvent.initEvent("input", true, true);
                    e.target.dispatchEvent(inputEvent);
                    if (addChange) {
                        e.target.removeEventListener("mouseup", fireChange); //TODO can not clean up this event on destroy. refactor needed!
                        e.target.addEventListener("mouseup", fireChange);
                    }
                }
            },
            true
        );
    };

    var isIE = function () {
        return !!navigator.userAgent.match(/Trident.*rv[ :]*11\./);
    };
    var showCurrentPlayback = function (data) {
        var bitrates = player.levels;
        var getQualityFor = player.currentLevel;
        var activeVideo = bitrates[getQualityFor];
        var videoResolution = Resolutions[activeVideo.width];
        //console.log(videoResolution,getQualityFor,activeVideo)
        if (videoResolution != null) {
            options.bitrateListBtn.dataset.quality = videoResolution.name;
        } else {
            options.bitrateListBtn.dataset.quality = "";
        }
    };
    var registerHlsEvents = function () {

        player.on(
            Hls.Events.AUDIO_TRACK_LOADED,
            onAudioTracksLoaded.bind(this),
            this
        );
        player.on(
            Hls.Events.LEVEL_LOADED,
            onQualityLoaded.bind(this),
            this
        );
        player.on(
            Hls.Events.SUBTITLE_TRACK_LOADED,
            onSubtitlesLoaded.bind(this),
            this
        );
        player.on(
            Hls.Events.LEVEL_UPDATED,
            showCurrentPlayback.bind(this),
            this
        );
        player.on(
            Hls.Events.LEVEL_SWITCHED,
            showCurrentPlayback.bind(this),
            this
        );
        //player.autoLevelEnabled=true;
    }
    var initializeControls = function () {
        if (!player) {
            throw new Error(
                "Please pass an instance of MediaPlayer.js when instantiating the ControlBar Object"
            );
        }
        video = videoElement; //player.getVideoElement();
        if (!video) {
            throw new Error(
                "Please call initialize after you have called attachView on MediaPlayer.js"
            );
        }

        displayUTCTimeCodes =
            displayUTCTimeCodes === undefined ? false : displayUTCTimeCodes;


        video.controls = false;
        options.videoContainer = video.parentNode;
        options.captionBtn.classList.add("hide");
        if (options.trackSwitchBtn) {
            options.trackSwitchBtn.classList.add("hide");
        }
        registerHlsEvents();
        // player.on(MediaPlayer.events.PLAYBACK_STARTED, onPlayStart.bind(this), this);
        // player.on(MediaPlayer.events.PLAYBACK_PAUSED, onPlaybackPaused.bind(this), this);
        // player.on(MediaPlayer.events.PLAYBACK_TIME_UPDATED, onPlayTimeUpdate.bind(this), this);

        // player.on(
        //   Hls.Events.MANIFEST_PARSED,
        //   onStreamInitialized.bind(this),
        //   this
        // );


        // player.on(MediaPlayer.events.TEXT_TRACKS_ADDED, onTracksAdded, this);
        // player.on("streamTeardownComplete", onStreamTeardownComplete.bind(this), this);
        // player.on("sourceInitialized", onSourceInitialized.bind(this), this);

        video.addEventListener("timeupdate", onPlayTimeUpdate.bind(this));
        options.btnPlayPauseLarge.addEventListener(
            "click",
            onPlayPauseClick.bind(this)
        );
        options.playPauseBtn.addEventListener(
            "click",
            onPlayPauseClick.bind(this)
        );
        options.muteBtn.addEventListener("click", onMuteClick.bind(this));
        options.fullscreenBtn.addEventListener(
            "click",
            onFullscreenClick.bind(this)
        );
        video.addEventListener("dblclick", onFullscreenClick.bind(this));
        //use full for thumbnails
        options.seekbar.addEventListener(
            "mousedown",
            onSeeking.bind(this),
            true
        );
        options.seekbar.addEventListener(
            "mousemove",
            onSeekBarMouseMove.bind(this),
            true
        );
        options.seekbar.addEventListener(
            "mouseup",
            onSeeked.bind(this),
            true
        );
        // set passive to true for scroll blocking listeners (https://www.chromestatus.com/feature/5745543795965952)
        options.seekbar.addEventListener(
            "touchmove",
            onSeekBarMouseMove.bind(this),
            { passive: true }
        );
        options.seekbar.addEventListener(
            "mouseout",
            onSeekBarMouseMoveOut.bind(this),
            true
        );
        options.seekbar.addEventListener(
            "touchcancel",
            onSeekBarMouseMoveOut.bind(this),
            true
        );
        options.seekbar.addEventListener(
            "touchend",
            onSeekBarMouseMoveOut.bind(this),
            true
        );
        options.volumebar.addEventListener(
            "input",
            setVolume.bind(this),
            true
        );
        document.addEventListener(
            "fullscreenchange",
            onFullScreenChange.bind(this),
            false
        );
        document.addEventListener(
            "MSFullscreenChange",
            onFullScreenChange.bind(this),
            false
        );
        document.addEventListener(
            "mozfullscreenchange",
            onFullScreenChange.bind(this),
            false
        );
        document.addEventListener(
            "webkitfullscreenchange",
            onFullScreenChange.bind(this),
            false
        );

        //IE 11 Input Fix.
        if (isIE()) {
            coerceIEInputAndChangeEvents(options.seekbar, true);
            coerceIEInputAndChangeEvents(options.volumebar, false);
        }
        if (!options.autoplay) {
            options.btnPlayPauseLarge.classList.remove("hide");
        }
    };

    var show = function () {
        options.videoController.classList.remove("hide");
        options.btnPlayPauseLarge.classList.remove("hide");
    };

    var hide = function () {
        setTimeout(() => {
            options.videoController.classList.add("hide");
            options.btnPlayPauseLarge.classList.add("hide");
        }, 4000);
    };

    var disable = function () {
        options.videoController.classList.add("disable");
    };

    var enable = function () {
        options.videoController.classList.remove("disable");
    };

    var resetControllers = function () {
        window.removeEventListener("resize", handleMenuPositionOnResize);
        destroyBitrateMenu();
        var x = this;
        menuHandlersList.forEach(function (item) {
            if (options.trackSwitchBtn)
                options.trackSwitchBtn.removeEventListener("click", item);
            if (options.captionBtn)
                options.captionBtn.removeEventListener("click", item);
            if (options.bitrateListBtn) {
                options.bitrateListBtn.removeEventListener("click", item);
            }

        });
        menuHandlersList = [];
        if (captionMenu) {
            // options.videoController.removeChild(captionMenu);
            options.captionBtn.removeChild(captionMenu);
            captionMenu = null;
            options.captionBtn.classList.add("hide");
        }
        if (trackSwitchMenu) {
            //options.videoController.removeChild(trackSwitchMenu);
            options.trackSwitchBtn.removeChild(trackSwitchMenu);
            trackSwitchMenu = null;
            options.trackSwitchBtn.classList.add("hide");
        }
        seeking = false;

        if (options.seekbarPlay) {
            options.seekbarPlay.style.width = "0%";
        }

        if (options.seekbarBuffer) {
            options.seekbarBuffer.style.width = "0%";
        }
    };

    var destroy = function () {
        resetControllers();

        options.playPauseBtn.removeEventListener(
            "click",
            onPlayPauseClick
        );
        options.btnPlayPauseLarge.removeEventListener(
            "click",
            onPlayPauseClick
        );
        options.muteBtn.removeEventListener("click", onMuteClick);
        options.fullscreenBtn.removeEventListener(
            "click",
            onFullscreenClick
        );
        options.seekbar.removeEventListener("mousedown", onSeeking);
        options.volumebar.removeEventListener("input", setVolume);
        options.seekbar.removeEventListener(
            "mousemove",
            onSeekBarMouseMove
        );
        options.seekbar.removeEventListener(
            "touchmove",
            onSeekBarMouseMove
        );
        options.seekbar.removeEventListener(
            "mouseout",
            onSeekBarMouseMoveOut
        );
        options.seekbar.removeEventListener(
            "touchcancel",
            onSeekBarMouseMoveOut
        );
        options.seekbar.removeEventListener(
            "touchend",
            onSeekBarMouseMoveOut
        );

        // player.off(MediaPlayer.events.PLAYBACK_STARTED, onPlayStart, this);
        // player.off(MediaPlayer.events.PLAYBACK_PAUSED, onPlaybackPaused, this);
        // player.off(MediaPlayer.events.PLAYBACK_TIME_UPDATED, onPlayTimeUpdate, this);
        // player.off(MediaPlayer.events.TEXT_TRACKS_ADDED, onTracksAdded, this);
        //player.off(Hls.Events.STREAM_INITIALIZED, onStreamInitialized, this);
        // player.off("streamTeardownComplete", onStreamTeardownComplete, this);
        // player.off("sourceInitialized", onSourceInitialized, this);
        // player.off(
        //   Hls.Events.MANIFEST_PARSED,
        //   onStreamInitialized.bind(this),
        //   this
        // );
        document.removeEventListener("fullscreenchange", onFullScreenChange);
        document.removeEventListener("MSFullscreenChange", onFullScreenChange);
        document.removeEventListener(
            "mozfullscreenchange",
            onFullScreenChange
        );
        document.removeEventListener(
            "webkitfullscreenchange",
            onFullScreenChange
        );
    };

    var isPaused = function () {
        var x = videoElement;
        return x.paused;
    }

    var getCurrentTime = function () {
        return player.time();
    }
    var isEventSupported = function (type) {
        return true;
    }
    var addEventListener = function (type, listener) {
        if (!isEventSupported(type)) {
            throw Error("Event is not supported");
        }
        // PlayerListenerTypes.map(function (x) {
        //     if (x.Name == type) {
        //         player.on(x.Name, listener);
        //     }
        // })
        //player.on(Dash[type], listener);
    }
    return { init: init, reinit: reinit };
}