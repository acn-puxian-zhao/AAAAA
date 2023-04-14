angular.module('resources.appFilesProxy', []);
angular.module('resources.appFilesProxy').factory('appFilesProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('appFiles');

    factory.deleteFile = function(id) {
        var httpPromise = $http.post(APPSETTING['serverUrl'] + "/api/appFiles?id=" + id, { params: defaultParams });
        return httpPromise;
    }

    return factory;
} ]);
