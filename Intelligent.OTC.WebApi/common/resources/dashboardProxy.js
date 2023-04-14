angular.module('resources.dashboardProxy', []);
angular.module('resources.dashboardProxy').factory('dashboardProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('dashboard');

    factory.loadReport = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/DashBoard',
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    return factory;
}]);
