angular.module('directives.basedataBinding', [])

.directive('hello', function () {
    return {
        restrict: 'EA',
        replace: true,
        transclude: true,
        link: function ($scope, $element, $attrs) {
            // get configuration.
            var cfg = $scope.$eval($attrs.hello);

            // get cell value.
            var value = $scope.row.entity[cfg.valueMember];

            // compare cell value with value in base data.
            var t = $scope.$eval(cfg.basedata);

            $scope.bdcallback = function (row) {
                for (var i = 0; i < t.length; i++) {
                    if (t[i].detailValue == row[cfg.valueMember]) {
                        // set display
                        return t[i].detailName;
                    }
                }
            }
            //for (var i = 0; i < t.length; i++) {
            //    if (t[i].detailValue == $scope.row.entity[cfg.valueMember]) {
            //        // set display
            //        $scope.helloValue = t[i].detailName;
            //    }
            //}
        },
        //template: '<div class="ui-grid-cell-contents">{{helloValue}}</div>'
        template: '<div>{{bdcallback(row.entity)}}</div>'
    };
});