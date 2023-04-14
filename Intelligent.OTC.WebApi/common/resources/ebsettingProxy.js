angular.module('resources.ebsettingProxy', []);
angular.module('resources.ebsettingProxy').factory('ebsettingProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('ebsetting');

    factory.getEBSetting = function (region, legalEntity,ebname, collector,page, pageSize, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/ebsetting/getlist?legalEntity=' + legalEntity + '&ebname=' + ebname + '&collector=' + collector + '&region=' + region + '&page=' + page + "&pageSize=" + pageSize,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.downloadEBList = function (region, legalEntity, ebname, collector,  successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/ebsetting/downloadEBList?legalEntity=' + legalEntity + '&ebname=' + ebname + '&collector=' + collector + '&region=' + region ,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.updateLegalEB = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/ebsetting/updateLegalEB',
            method: 'POST',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.deleteLegalEB = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/ebsetting/deleteLegalEB?id=' + id,
            method: 'DELETE'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
