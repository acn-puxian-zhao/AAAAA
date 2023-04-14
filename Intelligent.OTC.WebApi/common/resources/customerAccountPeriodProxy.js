angular.module('resources.customerAccountPeriodProxy', []);
angular.module('resources.customerAccountPeriodProxy').factory('customerAccountPeriodProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerAccountPeriod');

    factory.searchcustomer = function (filterStr, successcb, failedcb) {

        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.getByNumAndSiteUseId = function (customerAccountPeriod, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAccountPeriod/GetByNumAndSiteUseId',
            method: 'POST',
            data: customerAccountPeriod
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveOrUpdateAccountPeriod = function (isAdd, accountPeriod, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAccountPeriod/SaveOrUpdateAccountPeriod?isAdd=' + isAdd,
            method: 'POST',
            data: accountPeriod
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.delAccountPeriod = function (apId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAccountPeriod?id=' + apId,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);

