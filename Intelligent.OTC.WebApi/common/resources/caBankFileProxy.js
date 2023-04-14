angular.module('resources.caBankFileProxy', []);
angular.module('resources.caBankFileProxy').factory('caBankFileProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('caBankFile');

    factory.getBankFilesByBankId = function (bankId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/CaBankFileController/GetFilesByBankId?bankId=' + bankId,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getBankFileList = function (transactionNum, fileName, fileType, createDateF, createDateT, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/CaBankFileController/GetFileList?transactionNum=' + transactionNum + '&fileName=' + fileName +
                '&fileType=' + fileType + '&createDateF=' + createDateF + '&createDateT=' + createDateT + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deleteBankFile = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/CaBankFileController/deleteFileById?fileId=' + id,
            method: 'GET',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
