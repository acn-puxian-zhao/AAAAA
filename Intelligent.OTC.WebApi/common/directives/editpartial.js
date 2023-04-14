angular.module('directives.editpartial', [])
    .directive('editpartial', function () {
        return {
            restrict: 'E',
            scope: { 'params': '=',
                'callback': '&'
             },
            controller: "@",
            name: "controllerName",
            templateUrl: function (el, attrs) {
                return (angular.isDefined(attrs.templateUrl)) ?
                attrs.templateUrl : 'scripts/views/entry/tpls/empty-partail.html';
            }
        }
    });
