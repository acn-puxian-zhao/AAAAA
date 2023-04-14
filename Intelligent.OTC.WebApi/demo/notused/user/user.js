angular.module('app.user', [])
    .config(['crudRouteProvider', '$routeProvider', function (crudRouteProvider, $routeProvider) {

        var customerCode = ['$route', function ($route) {
            return $route.current.params.customerCode;
        } ];

        //        crudRouteProvider
        //        .routesFor('contact', 'customer', 'customer/:customerCode')
        //            .whenList({
        //                customerCode: customerCode,
        //                conts: ['contactProxy', '$route', function (contactProxy, $route) {
        //                    return contactProxy.forCustomer($route.current.params.customerCode);
        //                } ]
        //            })
        //            .whenEdit({
        //                customerCode: customerCode,
        //                cont: ['contactProxy', '$route', function (contactProxy, $route) {
        //                    return contactProxy.getById($route.current.params.itemId);
        //                } ]
        //            })
        //            .whenNew({
        //                customerCode: customerCode,
        //                cont: ['contactProxy', '$route', function (contactProxy, $route) {
        //                    return new contactProxy();
        //                } ]
        //            });
    } ])


    .controller('userEditCtrl', ['$scope', 'userInfo', '$modalInstance', 'valueClass','modifyflag', function ($scope, userInfo, $modalInstance, valueClass,modifyflag) {
        //ValueClass dropDownList
        $scope.ValueClassList = valueClass;
        //clear 
        if (userInfo.id == 0) {
            userInfo.administrator = "";
            userInfo.teamLead = "";
            userInfo.collector = "";
            userInfo.dataProcesser = "";
            userInfo.eid = ""
            userInfo.email = "";
            userInfo.id = 0;
            userInfo.name = "";
            userInfo.riskClass = "";
            userInfo.valueClass = "";
            userInfo.role = "";
            userInfo.sysUserRole.length = 0;
            userInfo.team = "";
        }

        $scope.userInfo = userInfo;
        if (modifyflag == 1) {
            $scope.isreadonly = false;
        } else {
            $scope.isreadonly = true;
        }

        $scope.closeUser = function () {
            $modalInstance.close();
        };

        $scope.onSave = function () {
            $modalInstance.close();
        };

        $scope.onUpdate = function () {
            $modalInstance.close();
        };

    } ]);

