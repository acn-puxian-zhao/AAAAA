angular.module('resources.customerAssessmentHistoryProxy', []);
angular.module('resources.customerAssessmentHistoryProxy').factory('customerAssessmentHistoryProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerAssessmentHistory');

    factory.getCustomerAssessmentHistory = function (filterStr, successcb, failedcb) {
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.customerAssessmentHistoryPaging = function (condition, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessmentHistory/getCustomerAssessmentHistory',
            method: 'Post',
            data: condition
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.exportCustomerAssessment = function (condition, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessmentHistory/exportCustomerAssessmentHistory',
            method: 'Post',
            data: condition
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

    factory.getCustomerAssessmentHistoryCount = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessmentHistory/getCustomerAssessmentHistoryCount',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);

