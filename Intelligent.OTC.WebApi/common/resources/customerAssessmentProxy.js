angular.module('resources.customerAssessmentProxy', []);
angular.module('resources.customerAssessmentProxy').factory('customerAssessmentProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customerAssessment');

    factory.getCustomerAssessment = function (filterStr, successcb, failedcb) {
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.customerAssessmentPaging = function (condition, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessment/getCustomerAssessment',
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
            url: APPSETTING['serverUrl'] + '/api/customerAssessment/exportCustomerAssessment',
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

    factory.getCustomerAssessmentCount = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessment/getCustomerAssessmentCount',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.updateCustomerAssessment = function (dataList, successcb) {
        
        $http({
            url: APPSETTING['serverUrl'] + '/api/customerAssessment/updateCustomerAssessment',
            method: 'POST',
            data: dataList
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);

