angular.module('resources.dunningProxy', []);
angular.module('resources.dunningProxy').factory('dunningProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('dunning');

    factory.getDunningMailInstance = function (customerNums,siteuseIds, totalInvoiceAmount, reminderDay, alertType, ids, successcb) {

        //=========added by alex body中显示附件名+Currency======

        $http({
            url: APPSETTING['serverUrl'] + '/api/dunning/dun?customerNums=' + customerNums + '&siteUseIds=' + siteuseIds + '&totalInvoiceAmount=' + totalInvoiceAmount + '&reminderOrHoldDay=' + reminderDay + '&alertType=' + alertType,
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
        //return factory.queryObject({ 'customerNums': customerNums, 'totalInvoiceAmount': totalInvoiceAmount, 'reminderOrHoldDay': reminderDay, 'alertType': alertType }, successcb, failedcb);
    };

    factory.getDunningMailInstanceById = function (customerNums, siteuseIds, totalInvoiceAmount, reminderDay, alertType, id, ids, successcb) {

        //=========added by alex body中显示附件名+Currency======

        $http({
            url: APPSETTING['serverUrl'] + '/api/dunning/dun?customerNums=' + customerNums + '&siteUseIds=' + siteuseIds + '&totalInvoiceAmount=' + totalInvoiceAmount + '&reminderOrHoldDay=' + reminderDay + '&alertType=' + alertType + '&templateId=' + id,
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.dunningPaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.getNoPaging = function (type, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning?ListType=' + type,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //update work flow
    factory.wfchange = function (AlertId, type, AlertType, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning?AlertId=' + AlertId + "&type=" + type + "&AlertType=" + AlertType,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.forConfig = function (customerCode, successcb, failedcb) {
        return factory.query({ customerCode: customerCode }, successcb, failedcb);
    };



    factory.saveDunConfig = function (config, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning',
            method: 'POST',
            data: config
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveDunConfigBySingle = function (alertId, list, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning?AlertId=' + alertId,
            method: 'POST',
            data: list
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveActionDate = function (AlertId, ActionDate, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning?AlertId=' + AlertId + "&Date=" + ActionDate,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.calcu = function (alertId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning?AlertIdFCal=' + alertId,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.checkPermission = function (custs, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Dunning?ColDun=' + custs,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
