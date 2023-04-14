angular.module('services.cookieWatcher', []);
angular.module('services.cookieWatcher').factory('cookieWatcherService', ['cookieHelper', function (cookieHelper) {

    return function (watchDuration) {

        var service = {};

        service.watcher = {
            timeout: 900000, // watcher will not regist new timeout event after 15mins loop.
            interval: 500,
            watchOn: function (watchUrl) {
                var watchRes = cookieHelper.getCookie("cookie-otc-watcher-" + watchUrl);
                var that = this;
                if (watchRes != 'done') {
                    // check if the watcher goes out of its timeout period.
                    var now = new Date();
                    if (now - that.startTime > that.timeout) {
                        return;
                    }

                    setTimeout(function () {
                        that.watchOn(watchUrl)
                    }, that.interval);
                } else {
                    // trigger watcher done event
                    that.doneWatch("cookie-otc-watcher-" + watchUrl);
                }
            },
            watch: function (watchUrl) {
                this.startTime = new Date();
                this.watchOn(watchUrl);
            },
            doneWatch: function (item) {
                // clear cookie for this watcher.
                cookieHelper.deleteCookie(item);

                // trigger other done event handlers.
                this.onDoneWatch();
            },
            onDoneWatch: function () {
                // virtual method which can be override in runtime.
            }
        };

        service.getWatcher = function () {
            return this.watcher;
        }

        service.getWatchItem = function (url) {
            this.watchItem.itemUrl = url;
            return this.watchItem;
        }

        service.watchItem = {
            itemUrl: '',
            done: function () {
                // set cookie to complete for current watcher
                document.cookie = "cookie-otc-watcher-" + this.itemUrl + "=done;";
            }
        };

        return service;
    };
}]);

angular.module('services.cookieWatcher').service('cookieHelper', function () {
    // For other cookie function to implement. Please follow on this link: http://www.cnblogs.com/fishtreeyu/archive/2011/10/06/2200280.html
    return {
        getCookie : function(name){
            var cookieName = name + "=";
            var spl = document.cookie.split(';');
            for (var z = 0; z < spl.length; z++) {
                var x = spl[z];
                while (x.charAt(0) == ' ') x = x.substring(1, x.length);
                if (x.indexOf(cookieName) == 0) {
                    return x.substring(cookieName.length, x.length);
                }
            }
            return null;
        },
        deleteCookie: function (name) {
            var exp = new Date();
            exp.setTime(exp.getTime() - 1);
            var cookie = this.getCookie(name);
            if (cookie != null) {
                document.cookie = name + "=" + cookie + ";expires=" + exp.toGMTString();
            }
        },
        addCookie: function (name, value) {
            var minutes = 10;
            var exp = new Date();
            exp.setTime(exp.getTime() + minutes * 60 * 1000);
            document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
        }
    }

});