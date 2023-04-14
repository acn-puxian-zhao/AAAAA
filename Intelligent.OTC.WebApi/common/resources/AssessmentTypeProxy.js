angular.module('resources.assessmentTypeProxy', []);
angular.module('resources.assessmentTypeProxy').factory('assessmentTypeProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('assessmentType');

    factory.getAssessmentType = function (filter, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/assessmentType',
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);

