angular.module('resources.paymentDetailProxy', []);
angular.module('resources.paymentDetailProxy').factory('paymentDetailProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('pmt');

    factory.savePMT = function (pmt,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/savePMTDetail',
            method: 'POST',
            data: pmt
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.queryPMTByID = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/getCaPmtDetailById?id=' + id,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deletePMTByID = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/deletePMTDetailById',
            method: 'POST',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deletePMTByIDs = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/deletePMTDetailByIds',
            method: 'POST',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getCustomerByCustomerNum = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/forwarder/getCustomerName?customerNum=' + id,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getBankStatementByTranINC = function (transactionNumber, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetBankStatementByTranINC?transactionNumber=' + transactionNumber,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getInvoiceInfoByNum = function (invoiceNum, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/caBankStatementController/GetInvoiceInfoByNum?invoiceNum=' + invoiceNum,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    return factory;
}]);