angular.module('resources.customerContactCoverageProxy', []);
angular.module('resources.customerContactCoverageProxy').factory('customerContactCoverageProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerContactCoverage');

    factory.GetCustomerContactCount = function (year, month,successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/customerContactCoverage/GetCustomerContactCount?year=' + year + '&month=' + month,
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
