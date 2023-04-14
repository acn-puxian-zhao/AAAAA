angular.module('resources.statisticsCollectProxy', []);
angular.module('resources.statisticsCollectProxy').factory('statisticsCollectProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('StatisticsCollect');

    factory.GetStatisticsCollect = function (region, pageindex, pagesize ,successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/statisticsCollect/GetStatisticsCollect?region='
            + region + '&pageindex=' + pageindex + '&pagesize=' + pagesize,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });

    };

    factory.downloadReport = function (region, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/statisticsCollect/downloadCustomer?region=' + region,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadCollectorReport = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/statisticsCollect/downloadCollector',
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetStatisticsCollector = function (pageindex, pagesize, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/statisticsCollect/GetStatisticsCollector?pageindex=' + pageindex + '&pagesize=' + pagesize,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetStatisticsCollectSum = function (successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/statisticsCollect/GetStatisticsCollectSum',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
