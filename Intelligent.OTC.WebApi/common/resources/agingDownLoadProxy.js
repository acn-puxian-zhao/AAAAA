angular.module('resources.agingDownloadProxy', []);
angular.module('resources.agingDownloadProxy').factory('agingDownloadProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('agingDownload');

    factory.refreshDownloadFile = function (successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/agingDownload',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
