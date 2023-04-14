angular.module('resources.customerAssessmentLogProxy', []);
angular.module('resources.customerAssessmentLogProxy').factory('customerAssessmentLogProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerAssessmentLog');

    factory.getCustomerAssessmentLog = function (filter, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessmentLog?assessmentLogDate=' + filter,
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getAllAssessmengDate = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessmentLog',
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getAllAssessmengLogCount = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessmentLog/getAssessmentLogCount',
            method: 'post'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);

