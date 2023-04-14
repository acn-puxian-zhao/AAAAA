angular.module('resources.dailyReportProxy', []);
angular.module('resources.dailyReportProxy').factory('dailyReportProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('dailyReport');

    factory.dailyReportPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= DownloadTime desc" + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.refreshDownloadFile = function (successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/DailyReport',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.collectorReportPaging = function (report, successcb, failedcb) {
        return factory.query({ report: report }, successcb, failedcb);
    }

    return factory;
} ]);
