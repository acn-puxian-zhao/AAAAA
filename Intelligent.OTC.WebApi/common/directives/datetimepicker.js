angular.module('directives.datetimepicker', [])
    .directive('datetimepicker', function () {
        return {
            restrict: 'A',
            controller: 'datepickerCtrl',
            scope: {
                'readonly': '=',
                'value': '=',
                'format': '@',
                'minView': '@minview',
                'startView': '@startview'
            },
            //templateUrl: 'app/directive/datepicker.html',
            template: '<div class="input-group date form_datetime" >'
                        + '<input type="text" class="form-control" size="16" ng-model="value" ng-readonly="readonly" />'
                        + '<span class="input-group-addon"><span class="glyphicon glyphicon-remove-circle" ng-click="onClick()"></span></span>'
                        + '<span class="input-group-addon"><span class="glyphicon glyphicon-th"></span></span>'
                    + '</div>'
        };
    })

    .controller('datepickerCtrl', function ($scope) {
        var format = 'yyyy-mm-dd';
        var startView = 2, minView = 2;


        $scope.onClick = function () {
            $scope.value = '';
        }

        if ($scope.format) {
            format = $scope.format;
        }

        if ($scope.startView) {
            startView = getPickerView($scope.startView);
        }

        if ($scope.minView) {
            minView = getPickerView($scope.minView);
        }

        $('.form_datetime').datetimepicker({
            weekStart: 1,
            todayBtn: 1,
            autoclose: 1,
            todayHighlight: 1,
            startView: startView,
            minView: minView,
            forceParse: 0,
            showMeridian: 1,
            format: format
        });

        function getPickerView(strView) {
            switch (strView) {
                case 'year':
                    return 4;
                case 'month':
                    return 3;
                case 'date':
                    return 2;
                case 'hour':
                    return 1;
                case 'minute':
                    return 0;
            }
        }
    });