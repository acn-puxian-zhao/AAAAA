angular.module('app.userconfig', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

        .when('/admin/userconfig', {
            templateUrl: 'app/admin/userconfig/userconfig-list.tpl.html',
            controller: 'userConfigCtrl',
            resolve: {
                //首次加载第一页
                userListGrid: ['userProxy', function (userProxy) {
                    return userProxy.initUserPaging(1, 15, "");
                } ],
                UserRoleDp: ['userProxy', function (userProxy) {
                    return userProxy.query({ dummyid: "dummy", dummyname: "dummy" });
                } ],
                bdValueClass: ['baseDataProxy', function (baseDataProxy) {
                    return baseDataProxy.SysTypeDetail("014");
                } ]
            }
        });
    } ])

//*****************************************header***************************s
    .controller('userConfigCtrl', ['$scope', 'userListGrid', 'UserRoleDp', 'userProxy', 'modalService', 'bdValueClass',
    function ($scope, userListGrid, UserRoleDp, userProxy, modalService, bdValueClass) {
        //ROLE
        var _DataProcessor = "001"
        var _Collector = "002";
        var _TeamLead = "003";
        var _Administrator = "004";

        //init dropDowenList role
        $scope.RoleList = UserRoleDp;

        var filterStr = ""
        $scope.list = userListGrid[0].results;
        $scope.totalItems = userListGrid[0].count; //查询结果初始化
        $scope.selectedLevel = 15;  //下拉单页容量初始化
        $scope.itemsperpage = 15;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 10; //分页显示的最大页

        //单页容量下拉列表定义
        $scope.pagelevel = [
                            { "id": 15, "levelName": '15' },
                            { "id": 20, "levelName": '20' },
                            { "id": 30, "levelName": '30' },
                            { "id": 40, "levelName": '40' },
                            { "id": 50, "levelName": '50' }
                            ];

        //单页容量变化
        $scope.pagesizechange = function (selectedLevelId) {
            var index = $scope.currentPage;
            userProxy.initUserPaging(index, selectedLevelId, filterStr, function (list) {
                $scope.itemsperpage = selectedLevelId;
                $scope.list = list[0].results;
                $scope.totalItems = list[0].count;
            });
        };

        //翻页
        $scope.pageChanged = function () {
            var index = $scope.currentPage;
            userProxy.initUserPaging(index, $scope.itemsperpage, filterStr, function (list) {
                $scope.list = list[0].results;
                $scope.totalItems = list[0].count;
            }, function (error) {
                alert(error);
            });
        };


        //list列表
        $scope.userList = {
            data: 'list',
            multiSelect: false,
            columnDefs: [{ field: 'name', displayName: 'User Name' },
                            { field: 'eid', displayName: 'EID' },
                            { field: 'dataProcesser', displayName: 'Data Processor',
                                cellTemplate: '<input type="checkbox"  name="dataProcesser" class="checkbox" ng-model="row.entity.dataProcesser" disabled>'
                            },
                            { field: 'collector', displayName: 'Collector',
                                cellTemplate: '<input type="checkbox"   name="collector" class="checkbox" ng-model="row.entity.collector" disabled> '
                            },
                            { field: 'teamLead', displayName: 'Team Lead',
                                cellTemplate: '<input type="checkbox"  name="teamLead" class="checkbox"  ng-model="row.entity.teamLead" disabled> '
                            },
                            { field: 'administrator', displayName: 'Administrator',
                                cellTemplate: '<input type="checkbox"  name="administrator" class="checkbox"  ng-model="row.entity.administrator" disabled> '
                            }
//                            { name: 'op', displayName: 'Operation',
//                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="grid.appScope.New(row.entity)"> New </a>&nbsp;<a ng-click="grid.appScope.Edit(row.entity)"> Edit </a>' +
//                             '&nbsp;<a ng-click="grid.appScope.Del(row.entity)"> Del </a></div>'
//                            }
                            ],
            onRegisterApi: function (gridApi) {

                $scope.gridApi = gridApi;
            }
        };

        //*******************************Edit User*******************************s

        $scope.$on('edit', function (e) {
            $scope.Edit();
        });
        $scope.Edit = function () {
            if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                var row = $scope.gridApi.selection.getSelectedRows()[0];
                var modalDefaults = {
                    templateUrl: 'app/common/user/user-edit.tpl.html',
                    controller: 'userEditCtrl',
                    resolve: { userInfo: function () { return row; }, valueClass: function () { return bdValueClass; }, modifyflag: function () { return 2; } }
                }
            } else {
                alert("请选择");
            };


            modalService.showModal(modalDefaults, {}).then(function (result) {
                var index = $scope.currentPage;
                userProxy.initUserPaging(index, $scope.itemsperpage, filterStr, function (list) {
                    $scope.totalItems = list[0].count;
                    $scope.list = list[0].results;
                })

            });
        };
        //*******************************New User*******************************s
        $scope.$on('new', function (e) {
            $scope.New();
        });

        $scope.New = function () {
            var modalDefaults = {
                templateUrl: 'app/common/user/user-edit.tpl.html',
                controller: 'userEditCtrl',
                resolve: { userInfo: function () { return userProxy.queryObject(); }, valueClass: function () { return bdValueClass; }, modifyflag: function () { return 1; }}
            };


            modalService.showModal(modalDefaults, {}).then(function (result) {
                var index = $scope.currentPage;
                userProxy.initUserPaging(index, $scope.itemsperpage, filterStr, function (list) {
                    $scope.totalItems = list[0].count;
                    $scope.list = list[0].results;
                })

            });
        };
        //*******************************New User*******************************d
        //*******************************Del User*******************************s
        //        $scope.Del = function () {
        //            this.$parent.row.entity.$remove(function () {
        //                alert("Delete Success");
        //                var index = $scope.currentPage;
        //                userProxy.initUserPaging(index, $scope.itemsperpage, filterStr, function (list) {
        //                    $scope.totalItems = list[0].count;
        //                    $scope.list = list[0].results;
        //                })

        //            }, function () {
        //                alert("Delete Error");
        //            });
        //        };

        $scope.$on('delete', function (e) {
            $scope.Del();
        });
        $scope.Del = function () {
            var entity = $scope.gridApi.selection.getSelectedRows()[0];
            entity.$remove(function () {
                alert("Delete Success");
                var index = $scope.currentPage;
                userProxy.initUserPaging(index, $scope.itemsperpage, filterStr, function (list) {
                    $scope.totalItems = list[0].count;
                    $scope.list = list[0].results;
                })

            }, function () {
                alert("Delete Error");
            });
        }


        //*******************************Del User*******************************e

        //*******************************Search*******************************s

        $scope.$on('search', function (e) {
            $scope.searchUser();
        });

        $scope.searchUser = function () {

            //组合过滤条件
            filterStr = ""
            //name
            if ($scope.userName) {
                if (filterStr != "") {
                    filterStr += "and (contains(Name,'" + $scope.userName + "'))";
                } else {
                    filterStr += "&$filter=(contains(Name,'" + $scope.userName + "'))";
                }
            }

            //eid
            if ($scope.eid) {
                if (filterStr != "") {
                    filterStr += "and (contains(EID,'" + $scope.eid + "'))";
                } else {
                    filterStr += "&$filter=(contains(EID,'" + $scope.eid + "'))";
                }
            }

            //team
            if ($scope.team) {
                if (filterStr != "") {
                    filterStr += "and (contains(Team,'" + $scope.team + "'))";
                } else {
                    filterStr += "&$filter=(contains(Team,'" + $scope.team + "'))";
                }
            }

            //role
            if ($scope.role) {
                var role = "";
                //roleCode convert roleName
                switch ($scope.role) {
                    case _Collector: role = "Collector";
                        break;
                    case _DataProcessor: role = "DataProcesser";
                        break;
                    case _TeamLead: role = "TeamLead";
                        break;
                    case _Administrator: role = "Administrator";
                        break;
                    default: role = "";
                }
                if (filterStr != "") {
                    filterStr += "and (" + role + " eq true)";
                } else {
                    filterStr += "&$filter=(" + role + " eq true)";
                }
            }

            userProxy.initUserPaging($scope.currentPage, $scope.itemsperpage, filterStr, function (lists) {
                if (lists != null) {
                    $scope.totalItems = lists[0].count;
                    $scope.list = lists[0].results;
                }
            })

        };
        //*******************************Search*******************************e
        //*******************************reset*******************************e
        $scope.$on('resetSearch', function (e) {
            $scope.resetSearch();
        });

        $scope.resetSearch = function () {
            filstr = "";
            $scope.userName = "";
            $scope.eid = "";
            $scope.role = "";
            $scope.team = "";
            //*******************************reset*******************************e
        }

    } ])