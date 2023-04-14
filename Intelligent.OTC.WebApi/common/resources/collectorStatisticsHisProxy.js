angular.module('resources.collectorStatisticsHisProxy', []);
angular.module('resources.collectorStatisticsHisProxy').factory('collectorStatisticsHisProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerContactCoverage');

    factory.GetCollectorStatisticsHis = function (start, end, type, collector, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/collectorStatisticsHis/GetCollectorStatisticsHis?start=' + start + '&end=' + end
            + '&type=' + type + '&collector=' + collector,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetCustomerCountPercent = function (year, month, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/customerContactCoverage/GetCustomerCountPercent?year=' + year + '&month=' + month,
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
