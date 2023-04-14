angular.module('resources.siteProxy', []);
angular.module('resources.siteProxy').factory('siteProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('sites');

    factory.Site = function (code, successcb, failedcb) {
        return factory.query({}, successcb, failedcb);
    };
    
    factory.GetLegalEntity = function (type, successcb) {

            $http({
                url: APPSETTING['serverUrl'] + '/api/sites?type=' + type,
                method: 'GET',
            }).then(function (result) {
                successcb(result.data);
            }).catch(function (result) {
                alert(result.data);
            });
    }
    return factory;
} ]);
