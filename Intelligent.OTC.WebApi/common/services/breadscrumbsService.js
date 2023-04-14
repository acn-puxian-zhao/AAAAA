angular.module('services.breadScrumbs', []);
angular.module('services.breadScrumbs').service('breadScrumbs', ['$rootScope', '$location', function ($rootScope, $location) {

    var breadcrumbs = [];

    //we want to update breadcrumbs only when a route is actually changed 
    //as $location.path() will get updated imediatelly (even if route change fails!) 
    $rootScope.$on('$routeChangeSuccess', function (event, current) {
        var pathElements = $location.path().split('/'), result = [], i;

        var breadcrumbPath = function (index) {
            return '#/' + (pathElements.slice(0, index + 1)).join('/');
        };

        pathElements.shift();

        for (i = 0; i < pathElements.length; i++) {
            result.push({ name: pathElements[i], path: breadcrumbPath(i) });
        }

        breadcrumbs = result;
    });

    this.getAll = function () {
        return breadcrumbs;
    };

    this.getFirst = function () {
        return breadcrumbs[0] || {};
    };

} ]);