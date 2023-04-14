angular.module('ui.bootstrap.demo', ['ui.tinymce', 'angularFileUpload']);
angular.module('ui.bootstrap.demo')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/demo', {
                templateUrl: 'demo/main.html'
            })
            .when('/demo/tooltips', {
                templateUrl: 'demo/tooltips.html'
            })
            .when('/demo/pagination', {
                templateUrl: 'demo/pagination.html'
            })
            .when('/demo/modal', {
                templateUrl: 'demo/modal.html'
            })
            .when('/demo/datepicker', {
                templateUrl: 'demo/datepicker.html'
            })
            .when('/demo/grid', {
                templateUrl: 'demo/grid.html'
            })
            .when('/demo/tinyMCE', {
                templateUrl: 'demo/tinyMCE.html'
            })
            .when('/demo/sanitize', {
                templateUrl: 'demo/sanitize.html'
            });

    } ])

    .controller('demoMainCtrl', function ($scope) {
        $scope.demoList = [{ 'tooltips': 'demo/tooltips.html' }, { 'pagination': 'demo/pagination.html'}];

    });