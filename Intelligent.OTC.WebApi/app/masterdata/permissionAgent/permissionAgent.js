angular.module('app.masterdata.permissionAgent', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/admin/permissionAgent', {
                templateUrl: 'app/masterdata/permissionAgent/permissionAgent-list.tpl.html',
                controller: 'permissionAgentListCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('permissionAgentListCtrl',
    ['$scope', '$interval', 'permissionProxy', 'modalService',
        function ($scope, $interval, permissionProxy, modalService) {
            $scope.$parent.helloAngular = "OTC - Permission Agent";

            $scope.dataList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: true,
                columnDefs: [
                    { name: 'RowNo', displayName: '', width: '40',  cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'eId', displayName: 'User', width: '200' },
                    { field: 'agent', displayName: 'Agent User', width: '200' },
                    { field: 'lastUpdateUser', displayName: 'LastUpdateUser', width: '200' },
                    { field: 'lastUpdateTime', displayName: 'LastUpdateTime', width: '200' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            permissionProxy.getTeamUsers(function (result) {
                $scope.teamUsers = result;
            }, function (error) {
                alert(error);
            })

            $scope.initData = function () {
                permissionProxy.getPermissionAgents(function (result) {
                    $scope.dataList.data = result;
                }, function (error) {
                    alert(error);
                })
            };

            $scope.initData();

            $scope.add = function () {
                $scope.editModal({});
            };
           
            $scope.edit = function () {
                if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                    var row = $scope.gridApi.selection.getSelectedRows()[0];
                    $scope.editModal(row);
                } else {
                    alert("please select one record");
                }
            };

            $scope.editModal = function (row) {

                var modalDefaults = {
                    templateUrl: 'app/masterdata/permissionAgent/permissionAgent-edit.tpl.html',
                    controller: 'permissionAgentEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        teamUsers: function () {
                            return $scope.teamUsers;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function () {
                    $scope.initData();
                });
            };

            $scope.delete = function () {
                if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            var row = $scope.gridApi.selection.getSelectedRows()[0];
                            permissionProxy.deletePermissionAgent(row.id, function () {
                                $scope.initData();
                            }, function (error) {
                                alert(error);
                            })
                        }
                    });
                } else {
                    alert("please select one record");
                }
            }
        }])

    .controller('permissionAgentEditCtrl', ['$scope', '$uibModalInstance', 'cont', 'teamUsers','permissionProxy',
        function ($scope, $uibModalInstance, cont, teamUsers, permissionProxy) {

            $scope.teamUsers = teamUsers;
            $scope.cont = cont;

            $scope.closeModal = function () {
                $uibModalInstance.close();
            };

            $scope.save = function () {

                if (!$scope.cont.eId || !$scope.cont.agent) {
                    alert("Input canot be null");
                    return;
                }

                if ($scope.cont.eId == $scope.cont.agent) {
                    alert("Agent and Principal cannot be same");
                    return;
                }

                permissionProxy.updatePermissionAgent($scope.cont,
                    function () {
                        $uibModalInstance.close();
                    },
                    function (res) {
                        alert(res);
                    });
            };

        }])
    ;




