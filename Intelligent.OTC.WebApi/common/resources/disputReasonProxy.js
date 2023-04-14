angular.module('resources.disputReasonProxy', []);
angular.module('resources.disputReasonProxy').factory('disputReasonProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('disputReasonProxy');

    factory.GetDisputReason = function (region, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/disputReason/GetDisputReason?region=' + region,
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
