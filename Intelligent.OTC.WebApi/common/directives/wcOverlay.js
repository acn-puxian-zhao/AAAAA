var wcOverlayDirective = function ($q, $timeout, $window, httpInterceptor, APPSETTING) {

    var template = '<div id="overlay-container" class="overlayContainer">' +
                            '<div id="overlay-background" class="overlayBackground"></div>' +
                            '<div id="overlay-content" class="overlayContent" data-ng-transclude>' +
                            '</div>' +
                        '</div>',

            link = function (scope, element, attrs) {
                var overlayContainer = null,
                    timerPromise = null,
                    timerPromiseHide = null,
                    queue = [];

                init();

                function init() {
                    wireUpHttpInterceptor();
                    if ($window.jQuery) wirejQueryInterceptor();
                    overlayContainer = element[0].firstChild; //Get to template
                    overlayBackground = overlayContainer.firstChild; //Get to template
                }

                //Hook into httpInterceptor factory request/response/responseError functions
                function wireUpHttpInterceptor() {

                    httpInterceptor.request = function (config) {
                        if (config.url == APPSETTING['serverUrl'] + '/api/common' && config.method == 'GET') {
                            // do nothing
                        } else {
                            processRequest();
                        }
                        return config || $q.when(config);
                    };

                    httpInterceptor.response = function (response) {
                        if (response.config.url == APPSETTING['serverUrl'] + '/api/common' && response.config.method == 'GET') {
                            // do nothing
                        } else {
                            processResponse();
                        }
                        return response || $q.when(response);
                    };

                    httpInterceptor.responseError = function (rejection) {
                        processResponse();
                        return $q.reject(rejection);
                    };
                }

                //Monitor jQuery Ajax calls in case it's used in an app
                function wirejQueryInterceptor() {
                    $(document).ajaxStart(function () {
                        processRequest();
                    });

                    $(document).ajaxComplete(function () {
                        processResponse();
                    });

                    $(document).ajaxError(function () {
                        processResponse();
                    });
                }

                function processRequest() {
                    queue.push({});
                    if (queue.length == 1) {
                        timerPromise = $timeout(function () {
                            if (queue.length) {
                                showOverlay();
                            }
                        }, scope.wcOverlayDelay ? scope.wcOverlayDelay : 500); //Delay showing for 500 millis to avoid flicker
                    }
                }

                function processResponse() {
                    queue.pop();
                    if (queue.length == 0) {
                        //Since we don't know if another XHR request will be made, pause before
                        //hiding the overlay. If another XHR request comes in then the overlay
                        //will stay visible which prevents a flicker
                        timerPromiseHide = $timeout(function () {
                            //Make sure queue is still 0 since a new XHR request may have come in
                            //while timer was running
                            if (queue.length == 0) {
                                hideOverlay();
                                if (timerPromiseHide) $timeout.cancel(timerPromiseHide);
                            }
                        }, scope.wcOverlayDelay ? scope.wcOverlayDelay : 500);
                    }
                }

                function showOverlay() {
                    var w = 0;
                    var h = 0;
                    if (!$window.innerWidth) {
                        if (!(document.documentElement.clientWidth == 0)) {
                            w = document.documentElement.clientWidth;
                            h = document.documentElement.clientHeight;
                        }
                        else {
                            w = document.body.clientWidth;
                            h = document.body.clientHeight;
                        }
                    }
                    else {
                        w = $window.innerWidth;
                        h = $window.innerHeight;
                    }
                    var content = document.getElementById('overlay-content');
                    var contentWidth = parseInt(getComputedStyle(content, 'width').replace('px', ''));
                    var contentHeight = parseInt(getComputedStyle(content, 'height').replace('px', ''));
                    var stop = self.pageYOffset;
                    var sleft = self.pageXOffset; 
//                    overlayBackground.style.width = "";

                    content.style.top = h / 2 - contentHeight / 2 + stop + 'px';
                    content.style.left = w / 2 - contentWidth / 2 + sleft + 'px';

                    overlayContainer.style.display = 'block';
                    overlayBackground.style.height = document.body.scrollHeight + 'px';

                }

                function hideOverlay() {
                    if (timerPromise) $timeout.cancel(timerPromise);
                    //added by zhangYu  start resove upload file loading dia is missing [import.js] start
                    if (overlayContainer.style.flg == "loading")
                    { return; }
                    //added by zhangYu  start resove upload file loading dia is missing [import.js] End
                    overlayContainer.style.display = 'none';
                }

                var getComputedStyle = function () {
                    var func = null;
                    if (document.defaultView && document.defaultView.getComputedStyle) {
                        func = document.defaultView.getComputedStyle;
                    } else if (typeof (document.body.currentStyle) !== "undefined") {
                        func = function (element, anything) {
                            return element["currentStyle"];
                        };
                    }

                    return function (element, style) {
                        return func(element, null)[style];
                    };
                } ();
            };

    return {
        restrict: 'EA',
        transclude: true,
        scope: {
            wcOverlayDelay: "@"
        },
        template: template,
        link: link
    };
};

angular.module('directives.overlay', []);
angular.module('directives.overlay')

    //Empty factory to hook into $httpProvider.interceptors
    //Directive will hookup request, response, and responseError interceptors
    .factory('httpInterceptor', function () {
        return {};
    })

    //Hook httpInterceptor factory into the $httpProvider interceptors so that we can monitor XHR calls
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('httpInterceptor');
    }])


    .directive('wcOverlay', wcOverlayDirective);
