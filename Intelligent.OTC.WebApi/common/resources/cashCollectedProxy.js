angular.module('resources.cashCollectedProxy', []);
angular.module('resources.cashCollectedProxy').factory('cashCollectedProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('cashCollectedProxy');

    factory.GetCashCollected = function (year, month, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/cashCollectedController/GetCashCollectedController?year=' + year + '&month=' + month,
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
