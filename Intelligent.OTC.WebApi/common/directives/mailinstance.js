angular.module('directives.mailInstance', [])
    .directive('mailinstance', function () {
        return {
            restrict: 'E',
            scope: {
                'mailInstance': '=instance',
                'mailDefaults': '=',
                'custNums': '=',
                'modalInstance':'='
            },
            replace: true,
            controller: 'mailInstanceDirectiveCtrl',
            templateUrl: 'app/common/mail/mail-instance-directive.tpl.html',
            link: function ($scope, element, attrs) {
                //var custnum = '';
                //if (attrs.custNums) {
                //    $scope.custNums = $scope.$eval(attrs.custNums);;
                //}
                //var instance;
                //if (attrs.instance) {
                //    $scope.mailInstance = $scope.$eval(attrs.instance);
                //}
                //var mailDefaults;
                //if (attrs.mailDefaults) {
                //    $scope.mailDefaults = $scope.$eval(attrs.mailDefaults);
                //}
            }
        }
    });
