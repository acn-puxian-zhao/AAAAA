angular.module('resources.customerPaymentcircleProxy', []);
angular.module('resources.customerPaymentcircleProxy').factory('customerPaymentcircleProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerPaymentcircle');

    factory.forCustomer = function (successcb, failedcb) {
        return factory.query(successcb, failedcb);
    };

    factory.forPayDate = function (num, successcb, failedcb) {
        return factory.query({ num: num }, successcb, failedcb);
    };

    factory.addPaymentCircle = function (paymentCircle, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle',
            method: 'POST',
            data: paymentCircle
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.delAllPaymentcircle = function (customerNum, siteUseId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle/deleteAll?customerNum=' + customerNum + '&siteUseId=' + siteUseId,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.delPaymentCircle = function (cusid, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle?id='+cusid,
            method: 'POST',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.searchPaymentCircle = function (custNum, legal, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle?custNum=' + custNum + '&legal=' + legal,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    //factory.searchPaymentCircle = function (filterStr, successcb, failedcb) {

    //        return factory.odataQuery(filterStr, successcb, failedcb);
    //};


    return factory;
} ]);
