
angular.module('resources.customerAttributeProxy', []);
angular.module('resources.customerAttributeProxy').factory('customerAttributeProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerAttributeProxy');

 

    factory.getCustomerAttribute = function (page, pageSize, legalEntity, customerNum,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAttribute/getCustomerAttribute?page=' + page + "&pageSize=" + pageSize + "&legalEntity=" + legalEntity + "&customerNum=" + customerNum ,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.getCustomerName = function (num, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/custAndBankCust/getCustomerName?customerNum=' + num,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.updateCustomerAttribute = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAttribute/attribute',
            method: 'POST',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.deleteCustomerAttribute = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAttribute/attribute?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.export = function () {
        window.location = APPSETTING['serverUrl'] + '/api/customerAttribute/exporAll';
    }

    return factory;
} ]);
