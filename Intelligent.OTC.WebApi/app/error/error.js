angular.module('app.error', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/error', {
                templateUrl: 'app/error/404.html',
                controller: 'errorCtrl'
            });
    }])

    .controller('errorCtrl',
        ['$scope',
            function ($scope) {


            }]);