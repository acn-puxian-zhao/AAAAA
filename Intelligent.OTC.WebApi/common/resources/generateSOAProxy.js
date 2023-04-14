angular.module('resources.generateSOAProxy', []);
angular.module('resources.generateSOAProxy').factory('generateSOAProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('generateSOA');

    factory.sendEmailUrl = 'api/generateSOA';

    //====================================================================

    factory.getMailInstance = function (customerNums, siteuseid, ids, mType, tileType, successcb) {

        //=========added by alex body中显示附件名+Currency=====================

        $http({
            url: APPSETTING['serverUrl'] + '/api/generateSOA/generate?customerNums=' + customerNums + '&siteUseId=' + siteuseid + '&mType=' + mType + '&fileType=' + tileType,
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });

        //return factory.queryObject({ 'customerNums': customerNums }, successcb, failedcb);
    };

    factory.getGenerateSOACheck = function (customerNums, siteuseid, ids, mType, tileType, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/generateSOA/generateCheck?customerNums=' + customerNums + '&siteUseId=' + siteuseid + '&mType=' + mType + '&fileType=' + tileType,
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getPmtMailInstance = function (customerNums, siteuseid, mType,  successcb) {

        //=========added by alex body中显示附件名+Currency=====================

        $http({
            url: APPSETTING['serverUrl'] + '/api/generateSOA/generatepmt?customerNums=' + customerNums + '&siteUseId=' + siteuseid + '&mType=' + mType,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });

        //return factory.queryObject({ 'customerNums': customerNums }, successcb, failedcb);
    };

    factory.getMailInstById = function (customerNum, id, siteUseId, templateType, templatelang, ids, successcb) {

        //=========added by alex body中显示附件名+Currency=====================
        var ndto = {};
        ndto.customerNums = customerNum;
        ndto.templateId = id;
        ndto.siteUseId = siteUseId;
        ndto.templateType = templateType;
        ndto.templatelang = templatelang;
        ndto.intIds = ids;
        $http({
            //url: APPSETTING['serverUrl'] + '/api/generateSOA/generateTemp?customerNums=' + customerNum + '&templateId=' + id,
            url: APPSETTING['serverUrl'] + '/api/generateSOA/generateTemp?',
            method: 'POST',
            data: ndto
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.geneateSoaByIds = function (intIds, type, customerNum,siteUseId, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/generateSOA/generateAtta?Type=' + type + '&siteUseId=' + siteUseId + '&customerNum=' + customerNum,
            method: 'POST',
            data: intIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);

