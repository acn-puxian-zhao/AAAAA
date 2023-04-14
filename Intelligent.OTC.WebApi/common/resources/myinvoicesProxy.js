angular.module('resources.myinvoicesProxy', []);
angular.module('resources.myinvoicesProxy').factory('myinvoicesProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('myinvoices');

    //factory.invoicePaging = function (index, itemCount, filter, siteUseId, successcb, failedcb) {
    //    var itemspage = (index - 1) * itemCount;
    //    var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$count=true" + "&siteUseId=" + siteUseId;
    //    return factory.odataQuery(filterStr, successcb, failedcb);

    //};

    /* 修改者: fujie.wan
     * 日 期:  2018-12-12
     * 描 述:  增加以下方法，AllInvoice查询时，使用此方法进行查询，不使用LINQ，提高查询速度
     */
    factory.invoicePaging = function (index, itemCount, custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate, legal, siteUseid, invoiceNum, poNum, soNum,
        creditTerm, docuType, invoiceTrackStates, memo, ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs, sales, overdueReason, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/myinvoice/query?pageindex=' + index + '&pagesize=' + itemCount 
            + '&custCode=' + custCode + '&custName=' + custName + '&eb=' + eb + '&consignmentNumber=' + consignmentNumber + '&balanceMemo=' + balanceMemo + '&memoExpirationDate=' + memoExpirationDate+ '&legal=' + legal + '&siteUseid=' + siteUseid
                + '&invoiceNum=' + invoiceNum + '&poNum=' + poNum + '&soNum=' + soNum + '&creditTerm=' + creditTerm
                + '&docuType=' + docuType + '&invoiceTrackStates=' + invoiceTrackStates + '&memo=' + memo
                + '&ptpDateF=' + ptpDateF + '&ptpDateT=' + ptpDateT + '&memoDateF=' + memoDateF + '&memoDateT=' + memoDateT + '&invoiceDateF=' + invoiceDateF + '&invoiceDateT=' + invoiceDateT
                + '&dueDateF=' + dueDateF + '&dueDateT=' + dueDateT + '&cs=' + cs + '&sales=' + sales + '&overdueReason=' + overdueReason,
            method: 'Post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportSoa = function (custCode, custName, eb, consignmentNumber, balanceMemo, memoExpirationDate,legal, siteUseid, invoiceNum,
        poNum, soNum, creditTerm, docuType, trackStates, memo,
        ptpDateF, ptpDateT, memoDateF, memoDateT, invoiceDateF, invoiceDateT, dueDateF, dueDateT, cs,
        sales, overdueReason, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/myinvoice/ExpoertSOA?custCode=' + custCode + '&custName=' + custName + '&eb=' + eb + 
                '&consignmentNumber=' + consignmentNumber + '&balanceMemo=' + balanceMemo + '&memoExpirationDate=' + memoExpirationDate + 
                '&legal=' + legal + '&siteUseid=' + siteUseid + '&invoiceNum=' + invoiceNum + '&poNum=' + poNum + 
                '&soNum=' + soNum + '&creditTerm=' + creditTerm + '&docuType=' + docuType + '&invoiceTrackStates=' + trackStates + 
            '&memo=' + memo + '&ptpDateF=' + ptpDateF + '&ptpDateT=' + ptpDateT + '&memoDateF=' + memoDateF + '&memoDateT=' + memoDateT + '&invoiceDateF=' + invoiceDateF + '&invoiceDateT=' + invoiceDateT + 
                '&dueDateF=' + dueDateF + '&dueDateT=' + dueDateT + '&cs=' + cs + '&sales=' + sales + '&overdueReason=' + overdueReason,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.dsoAnalysis = function (packFileName, monthList, packagedays, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/MyInvoice/AnalysisDSO?packFileName=' + packFileName + '&monthList=' + monthList + '&packageDays=' + packagedays,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);