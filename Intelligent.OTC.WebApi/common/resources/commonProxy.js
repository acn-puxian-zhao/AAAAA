angular.module('resources.commonProxy', []);
angular.module('resources.commonProxy').factory('commonProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('common');

    factory.search = function (successcb, failedcb) {
        return factory.query('', successcb, failedcb);
    };

    //update status
    factory.updateStatus = function (id, status, statusFlg, mailId, actionownerdept, disputereason,  successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/common?id=' + id + '&status=' + status + '&statusFlg=' + statusFlg + '&mailId=' + mailId + '&actionownerdept=' + actionownerdept + '&disputereason=' + disputereason,
            method: 'POST',
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //update invoice status
    factory.updateInvoiceStatus = function (disputeId,status, invNums, successcb) {
        var pstData = {};
        pstData.disputeId = disputeId;
        pstData.status = status;
        pstData.invIds = invNums;
        $http({
            url: APPSETTING['serverUrl'] + '/api/common/UpdateInvoiceStatus',
            method: 'POST',
            data: pstData,
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
