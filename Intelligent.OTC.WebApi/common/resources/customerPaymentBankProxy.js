angular.module('resources.customerPaymentbankProxy', []);
angular.module('resources.customerPaymentbankProxy').factory('customerPaymentbankProxy', ['rresource', '$http','APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerPaymentBank');

    factory.forCustomer = function (successcb, failedcb) {
        return factory.query(successcb, failedcb);
    };


    factory.forCustBank = function (num, successcb, failedcb) {
        return factory.query({  num: num }, successcb, failedcb);
    };

    factory.delPaymentBank = function (cusid, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerPaymentBank?id=' + cusid,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.updatePayment = function (cust, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerPaymentBank',
            method: 'POST',
            data: cust
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
