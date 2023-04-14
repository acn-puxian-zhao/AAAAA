angular.module('resources.overdueReportProxy', []);
angular.module('resources.overdueReportProxy').factory('overdueReportProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('overduereport');

    factory.queryReport = function (index, itemCount, filter, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/overdue/query?pageindex=' + index + '&pagesize=' + itemCount + '&filter=' + filter,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.downloadReport = function (filter,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/overdue/download?filter=' + filter,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
