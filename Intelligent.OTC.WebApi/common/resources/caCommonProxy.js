angular.module('resources.caCommonProxy', []);
angular.module('resources.caCommonProxy').factory('caCommonProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('cacommon');

    factory.getCARegionByCurrentUser = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/getCARegionByCurrentUser',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getActionTaskDatas = function (transactionNumber, status,currency, dateF, dateT, page, pageSize, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/getActionTaskList?transactionNumber=' + transactionNumber + "&status=" + status + '&currency=' + currency + "&dateF=" + dateF + '&dateT=' + dateT+ '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.PostClear = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/postAndClear',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.SendPmtDetailMail = function (bankIds, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/SendPmtDetailMail',
            method: 'POST',
            data: bankIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetDateByDay = function (addDays, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/GetDateByDay?addDays=' + addDays,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetDateByMonth = function (addMonths, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/GetDateByMonth?addMonths=' + addMonths,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    
    factory.getCaPostResultCheck = function (valueDateF, valueDateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/getCaPostResultCheck?fDate=' + valueDateF + "&tDate=" + valueDateT,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getCaClearResultCheck = function (valueDateF, valueDateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/getCaClearResultCheck?fDate=' + valueDateF + "&tDate=" + valueDateT,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportPostClearResult = function (valueDateF, valueDateT) {
        window.location = APPSETTING['serverUrl'] + '/api/cacommon/exportPostClearResult?fDate=' + valueDateF + "&tDate=" + valueDateT;
    };


    factory.getbsReport = function (valueDateF, valueDateT, page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/getbsReport?fDate=' + valueDateF + "&tDate=" + valueDateT + "&page=" + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    
    factory.exportbsReport = function (valueDateF, valueDateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/exportbsReport?fDate=' + valueDateF + "&tDate=" + valueDateT,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadtemplete = function (fileType) {
        window.location = APPSETTING['serverUrl'] + '/api/cacommon/downloadtemplete?fileType=' + fileType;
    };

    factory.queryCashApplicationCountReport = function (legalentity, valueDateF, valueDateT, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/queryCashApplicationCountReport?legalentity=' + legalentity + '&fDate=' + valueDateF + "&tDate=" + valueDateT,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportCashApplicationCountReport = function (legalentity, valueDateF, valueDateT) {

        window.location = APPSETTING['serverUrl'] + '/api/cacommon/exportCashApplicationCountReport?legalentity=' + legalentity + '&fDate=' + valueDateF + "&tDate=" + valueDateT;
        
    };

    factory.queryCadaliyReport = function (index, itemCount,legalEntity, bsType, CreateDateFrom, CreateDateTo,
        transNumber, transAmount, ValueDateFrom,
        ValueDateTo, enter, enterMail,
        crossOff, crossOffMail, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/cacommon/queryCadaliyReport?pageindex=' + index + '&pagesize=' + itemCount + '&legalEntity=' + legalEntity + '&bsType=' + bsType + "&CreateDateFrom=" + CreateDateFrom
                + '&CreateDateTo=' + CreateDateTo + "&transNumber=" + transNumber + '&transAmount=' + transAmount + "&ValueDateFrom=" + ValueDateFrom
                + '&ValueDateTo=' + ValueDateTo + "&enter=" + enter + '&enterMail=' + enterMail + "&crossOff=" + crossOff
                + '&crossOffMail=' + crossOffMail,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportCadaliyReport = function (legalEntity, bsType, CreateDateFrom, CreateDateTo,
        transNumber, transAmount, ValueDateFrom,
        ValueDateTo, enter, enterMail,
        crossOff, crossOffMail) {
        window.location = APPSETTING['serverUrl'] + '/api/cacommon/exportCadaliyReport?legalEntity=' + legalEntity + '&bsType=' + bsType + "&CreateDateFrom=" + CreateDateFrom
            + '&CreateDateTo=' + CreateDateTo + "&transNumber=" + transNumber + '&transAmount=' + transAmount + "&ValueDateFrom=" + ValueDateFrom
            + '&ValueDateTo=' + ValueDateTo + "&enter=" + enter + '&enterMail=' + enterMail + "&crossOff=" + crossOff
            + '&crossOffMail=' + crossOffMail;
    };

    return factory;
}]);
