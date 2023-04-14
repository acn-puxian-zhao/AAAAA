angular.module('resources.contactProxy', []);
angular.module('resources.contactProxy').factory('contactProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('contact');

    factory.forCustomer = function (customerCode, successcb, failedcb) {
        return factory.query({ customerCode: customerCode }, successcb, failedcb);
    };

    factory.delContactor = function (cusid, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact?id=' + cusid,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.updateContact = function (cont, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact',
            method: 'POST',
            data: cont
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.updateCustDomain = function (cont, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact?type=domain',
            method: 'POST',
            data: cont
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.delCustDomain = function (cusid, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact?domainid=' + cusid,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getContacts = function (siteUseId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact/getbysiteuseid?siteUseId=' + siteUseId,
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.copyContacts = function (dto, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact/CopyContactors',
            method: 'post',
            data: dto
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.batchUpdate = function (dto, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact/batchupdate',
            method: 'post',
            data: dto
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportByCondition = function (customerNum, name, siteUseId, legalEntity, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contact/export?custnum=' + customerNum + '&name=' + name + '&siteUseId=' + siteUseId + '&legalEntity=' + legalEntity,
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
