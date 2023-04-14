angular.module('resources.agingReportProxy', []);
angular.module('resources.agingReportProxy').factory('agingReportProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('agingreport');

    factory.queryReport = function (region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, index, itemCount,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/aging/query?region=' + region + '&legalentity=' + legalentity + '&custName=' + custName + '&siteUseId=' + siteUseId
                + '&invoicecode=' + invoicecode + '&status=' + status + '&docType=' + docType + '&poNum=' + poNum + '&soNum=' + soNum + '&creditTerm=' + creditTerm + '&invoiceMemo=' + invoiceMemo
                + '&eb=' + eb + '&invoiceDateFrom=' + invoiceDateFrom + '&invoiceDateTo=' + invoiceDateTo + '&DuedateFrom=' + DuedateFrom + '&DuedateTo=' + DuedateTo + '&pageindex=' + index + '&pagesize=' + itemCount,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.querysummary = function (region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, index, itemCount, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/aging/querysummary?region=' + region + '&legalentity=' + legalentity + '&custName=' + custName + '&siteUseId=' + siteUseId
                + '&invoicecode=' + invoicecode + '&status=' + status + '&docType=' + docType + '&poNum=' + poNum + '&soNum=' + soNum + '&creditTerm=' + creditTerm + '&invoiceMemo=' + invoiceMemo
                + '&eb=' + eb + '&invoiceDateFrom=' + invoiceDateFrom + '&invoiceDateTo=' + invoiceDateTo + '&DuedateFrom=' + DuedateFrom + '&DuedateTo=' + DuedateTo + '&pageindex=' + index + '&pagesize=' + itemCount,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadReport = function (region, legalentity, custName, siteUseId, invoicecode, status, docType, poNum, soNum, creditTerm, invoiceMemo, eb, invoiceDateFrom, invoiceDateTo, DuedateFrom, DuedateTo, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/aging/download?region=' + region + '&legalentity=' + legalentity + '&custName=' + custName + '&siteUseId=' + siteUseId
                + '&invoicecode=' + invoicecode + '&status=' + status + '&docType=' + docType + '&poNum=' + poNum + '&soNum=' + soNum + '&creditTerm=' + creditTerm + '&invoiceMemo=' + invoiceMemo
                + '&eb=' + eb + '&invoiceDateFrom=' + invoiceDateFrom + '&invoiceDateTo=' + invoiceDateTo + '&DuedateFrom=' + DuedateFrom + '&DuedateTo=' + DuedateTo,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    return factory;
}]);
