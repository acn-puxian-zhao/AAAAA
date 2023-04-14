angular.module('resources.baseDataProxy', []);
angular.module('resources.baseDataProxy').factory('baseDataProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('baseData');

    factory.SysTypeDetail = function (code, successcb, failedcb) {
        return factory.query({ 'strTypecode': code }, successcb, failedcb);

    };

    factory.SysTypeDetails = function (codes, successcb, failedcb) {
        return factory.query({ 'strTypeCodes': codes }, successcb, failedcb);

    };

    factory.initialUser = function (authCode, userMail, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/baseData?authCode=' + authCode +'&userMail='+ userMail,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getAuthentication = function (successcb, failedcb) {
        return factory.query(successcb, failedcb);
    }

    factory.saveCollectionCalendarConfig = function (customerNum, legalEntity, list, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/baseData?customerNum=' + customerNum + '&legalEntity=' + legalEntity,
            method: 'POST',
            data: list
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
