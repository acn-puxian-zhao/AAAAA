angular.module('app.masterdata.userUpdatePwd', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        .when('/user/updatePwd', {
            templateUrl: 'app/masterdata/user/user-updatePwd.tpl.html',
            controller: 'userUpdatePwdCtrl',
            resolve: {
            }
        });
    } ])

    .controller('userUpdatePwdCtrl', ['$scope', 'userProxy',
        function ($scope, userProxy) {

            $scope.savePwd = function () {
                if ($scope.newPwd != $scope.confirmPwd)
                {
                    alert("The new password is inconsistent with the confirm password");
                    return;
                }
                if ($scope.oldPwd == $scope.newPwd)
                {
                    alert("The new password is consistent with the old password");
                    return;
                }
                userProxy.updatePwd($scope.oldPwd, $scope.newPwd, $scope.confirmPwd, function (res) {
                    alert(res);
                    $scope.newPwd = '';
                    $scope.confirmPwd = '';
                    $scope.oldPwd = '';
                }, function (res) {
                    alert(res);
                });
            }
       
    } ]);