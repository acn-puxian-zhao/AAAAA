angular.module('resources.invoiceProxy', []);
angular.module('resources.invoiceProxy').factory('invoiceProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('invoice');

    factory.invoicePaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.invoiceUnPaging = function (filter, successcb, failedcb) {
        return factory.odataQuery(filter, successcb, failedcb);

    };

    factory.invoicePagingCount = function (filter, successcb, failedcb) {

        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.getOverdueReason = function (invoiceNum, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Invoice/overduereason?invoiceNum=' + invoiceNum,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveOverdueReason = function (data, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Invoice/overduereason',
            method: 'Post',
            data: data
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.clearPTP = function (idList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Invoice/clearPTP',
            method: 'Post',
            data: idList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.setNotClear = function (idList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Invoice/setNotClear',
            method: 'Post',
            data: idList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.clearOverdueReason = function (idList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Invoice/clearOverdueReason',
            method: 'Post',
            data: idList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.clearComments = function (idList, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Invoice/clearComments',
            method: 'Post',
            data: idList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportfiles = function (intIds, customerNum, siteUseId, fileType, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/invoice/exportfiles?siteUseId=' + siteUseId + '&customerNum=' + customerNum + '&fileType=' + fileType,
            method: 'POST',
            data: intIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    return factory;
}]);
