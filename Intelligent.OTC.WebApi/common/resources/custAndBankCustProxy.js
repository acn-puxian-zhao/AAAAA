angular.module('resources.custAndBankCustProxy', []);
angular.module('resources.custAndBankCustProxy').factory('custAndBankCustProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('custAndBankCustProxy');

 

    factory.getCustomerMapping = function (page, pageSize, legalEntity, customerNum, bankCustomerName, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/custAndBankCust/getCustomerMapping?page=' + page + "&pageSize=" + pageSize + "&legalEntity=" + legalEntity + "&customerNum=" + customerNum + "&bankCustomerName=" + bankCustomerName,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.getCustomerName = function (num, legalEntity, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/custAndBankCust/getCustomerName?customerNum=' + num + "&legalEntity=" + legalEntity,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.updateCustomer = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/custAndBankCust/customerMapping',
            method: 'POST',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.deleteCustomerMapping = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/custAndBankCust/customerMapping?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.export = function () {
        window.location = APPSETTING['serverUrl'] + '/api/custAndBankCust/exporAll';
    };

    return factory;
} ]);
