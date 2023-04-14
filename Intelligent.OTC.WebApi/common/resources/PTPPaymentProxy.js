angular.module('resources.PTPPaymentProxy', []);
angular.module('resources.PTPPaymentProxy').factory('PTPPaymentProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('dailyAging');

    factory.queryPTPPayment = function (index, itemCount, filter, custNum, siteUseId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/PTPPayment/query?pageindex=' + index + '&pagesize=' + itemCount + '&filter=' + filter
            + '&custNum=' + custNum + '&siteUseId=' + siteUseId,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });s
    };

    factory.queryPayer = function (siteUseId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/PTPPayment/getPayer?&siteUseId=' + siteUseId,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.updatePTPPayment = function (model, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/PTPPayment/update',
            method: 'POST',
            data: model
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
