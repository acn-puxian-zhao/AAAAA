angular.module('resources.disputeReportProxy', []);
angular.module('resources.disputeReportProxy').factory('disputeReportProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('disputereport');

    factory.queryReport = function (index, itemCount, filter, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dispute/query?pageindex=' + index + '&pagesize=' + itemCount + '&filter=' + filter,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.downloadReport = function (filter,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dispute/download?filter=' + filter,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
