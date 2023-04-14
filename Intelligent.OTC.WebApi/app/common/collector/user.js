angular.module('app.common.collector', [])

    .controller('userPickupCtrl', ['$scope', 'entity', '$uibModalInstance', 'userProxy', 'permissionProxy', function ($scope, entity, $uibModalInstance, userProxy, permissionProxy) {
        var eid;
        if (entity) {
            eid = entity.collector;

            userProxy.query({ dummyid: "eid", dummyname: "dummy" }, function (userRole) {
                $scope.RoleList = userRole;
            });



            userProxy.queryObject({ eid: eid }, function (operator) {
                $scope.operating = operator;
                $scope.team = operator.team;

                //                permissionProxy.query({ eid: eid, dummy: "GetTeamMemeber" }, function (users) {
                //                    $scope.list = users;
                //                });


                $scope.searchCollection();

            })
        } else {
            permissionProxy.getCurrentUser("dummy", function (currUser) {
                eid = currUser.eid;

                userProxy.queryObject({ eid: eid }, function (operator) {
                    $scope.operating = operator;
                    $scope.team = operator.team;
                    $scope.searchCollection();
                });
            });

        }


        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.operateList = {
            data: 'list',
            multiSelect: false,
            columnDefs: [
                            { field: 'eid', displayName: 'EID' },
                            { field: 'name', displayName: 'Name' },
                            { field: 'dataProcesser', displayName: 'Data Processor',
                                cellTemplate: '<input type="checkbox"  name="dataProcesser"  ng-model="row.entity.dataProcesser"disabled>'
                            },
                            { field: 'collector', displayName: 'Collector',
                                cellTemplate: '<input type="checkbox"   name="collector"  ng-model="row.entity.collector"disabled>'
                            },
                            { field: 'teamLead', displayName: ' Team Lead',
                                cellTemplate: '<input type="checkbox"  name="teamLead"  ng-model="row.entity.teamLead" disabled>'
                            },
                            { field: 'administrator', displayName: 'Administrator',
                                cellTemplate: '<input type="checkbox"  name="administrator"  ng-model="row.entity.administrator" disabled>'
                            },
                            { field: 'team', displayName: 'Team'
                            }
                            ], onRegisterApi: function (gridApi) {
                                //set gridApi on scope
                                $scope.gridApi = gridApi;
                            }
        };



        //条件查询
        $scope.searchCollection = function () {

            //alert('ddf');
            //组合过滤条件
            var filterStr = '';
            var _Collector = '002';
            var _DataProcessor = '001';
            var _TeamLead = '003';
            if ($scope.operateEid) {
                if (filterStr != "") {
                    filterStr += "and (contains(EID,'" + $scope.operateEid + "'))";
                } else {
                    filterStr += "&$filter=(contains(EID,'" + $scope.operateEid + "'))";
                }
            }

            if ($scope.operateName) {
                if (filterStr != "") {
                    filterStr += "and (contains(Name,'" + $scope.operateName + "'))";
                } else {
                    filterStr += "&$filter=(contains(Name,'" + $scope.operateName + "'))";
                }
            }
            if ($scope.operateRole) {
                var role = "";
                switch ($scope.operateRole) {
                    case _Collector: role = "Collector";
                        break;
                    case _DataProcessor: role = "DataProcesser";
                        break;
                    case _TeamLead: role = "TeamLead";
                        break;
                    default: role = "administrator";
                }

                if (filterStr != "") {
                    filterStr += "and (" + role + " eq true)";
                } else {
                    filterStr += "&$filter=(" + role + " eq true)";
                }
            }
            if ($scope.team) {
                if (filterStr != "") {
                    filterStr += "and (Team eq '" + $scope.team + "')";
                } else {
                    filterStr += "&$filter=(Team eq '" + $scope.team + "')";
                }
            }

            userProxy.odataQuery(filterStr, function (users) {
                $scope.list = users;
            });
        };

        //点击ok天转到initaing页面
        $scope.changItem = function () {
            if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                $uibModalInstance.close($scope.gridApi.selection.getSelectedRows()[0]);
            } else {
                alert("请选择collector");
            }
        }


    } ]);