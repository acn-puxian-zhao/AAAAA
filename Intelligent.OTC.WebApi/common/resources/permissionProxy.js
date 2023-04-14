angular.module('resources.permissionProxy', []);
angular.module('resources.permissionProxy').factory('permissionProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('permission');

    factory.getCurrentUser = function (code, successcb, failedcb) {
        return factory.queryObject({ 'dummy': code }, successcb, failedcb);

    };

    factory.getTeamUsers = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/permission/teamusers',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.getPermissionAgents = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/permissionagent',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.updatePermissionAgent = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/permissionagent',
            method: 'POST',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.deletePermissionAgent = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/permissionagent?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
