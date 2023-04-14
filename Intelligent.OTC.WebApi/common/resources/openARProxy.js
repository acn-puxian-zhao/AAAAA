angular.module('resources.openARProxy', []);
angular.module('resources.openARProxy').factory('openARProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('openAR');

    factory.GetOpenAR = function (region,successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/openAR/GetOpenAR?region=' + region,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.agingPagingCount = function (filter, successcb, failedcb) {

        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };


    return factory;
}]);
