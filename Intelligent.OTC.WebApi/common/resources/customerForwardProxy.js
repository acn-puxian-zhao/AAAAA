angular.module('resources.customerForwardProxy', []);
angular.module('resources.customerForwardProxy').factory('customerForwardProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerForward');

    factory.getForwarder = function (page, pageSize, legalEntity, customerNum, forwardNum, forwardName,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/forwarder/getForwarder?page=' + page + "&pageSize=" + pageSize + "&legalEntity=" + legalEntity + "&customerNum=" + customerNum + "&forwardNum=" + forwardNum + "&forwardName=" + forwardName,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.getCustomerName = function (num, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/forwarder/getCustomerName?customerNum=' + num,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.updateForwarder = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/forwarder/addForwarder',
            method: 'POST',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.deleteForwarder = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/forwarder/removeForwarder?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.export = function () {
        window.location = APPSETTING['serverUrl'] + '/api/forwarder/exporAll';
    }

    return factory;
} ]);
