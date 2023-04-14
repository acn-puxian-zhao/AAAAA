angular.module('resources.contactReplaceProxy', []);
angular.module('resources.contactReplaceProxy').factory('contactReplaceProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('contactReplace');

    factory.findAll = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contactreplace',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.update = function (cont, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contactreplace',
            method: 'POST',
            data: cont
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.delete = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contactreplace/delete?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deleteAll = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contactreplace/',
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deleteList = function (ids, successcb) {
        var dto = {};
        dto.Ids = ids;

        $http({
            url: APPSETTING['serverUrl'] + '/api/contactreplace/delete',
            method: 'POST',
            data: dto
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.export = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contactreplace/export',
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
