angular.module('resources.dailyAgingProxy', []);
angular.module('resources.dailyAgingProxy').factory('dailyAgingProxy',['rresource', '$http', 'APPSETTING',function (rresource, $http, APPSETTING) {
    var factory = rresource('dailyAging');

    factory.queryDailyAgingReport = function (index, itemCount, filter, legalEntity, custName, custNum, siteUseId,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dailyAging/query?pageindex=' + index + '&pagesize=' + itemCount + '&filter=' + filter
            + '&legalEntity=' + legalEntity + '&custName=' + custName + '&custNum=' + custNum + '&siteUseId=' + siteUseId,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadReport = function (filter, legalEntity, custName, custNum, siteUseId, successcb ) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dailyAging/download?filter=' + filter
            + '&legalEntity=' + legalEntity + '&custName=' + custName + '&custNum=' + custNum + '&siteUseId=' + siteUseId,
            //url: APPSETTING['serverUrl'] + '/api/dailyAging/download',
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadReportNew = function (filter, legalEntity, custName, custNum, siteUseId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dailyAging/downloadnew?filter=' + filter
                + '&legalEntity=' + legalEntity + '&custName=' + custName + '&custNum=' + custNum + '&siteUseId=' + siteUseId,
            //url: APPSETTING['serverUrl'] + '/api/dailyAging/download',
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;

    factory.agingPagingCount = function (filter, successcb, failedcb) {

        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };


    return factory;
}]);
