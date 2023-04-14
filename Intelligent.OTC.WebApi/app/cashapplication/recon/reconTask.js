angular.module('app.reconTask', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/reconTask', {
                templateUrl: 'app/cashapplication/recon/reconTask.tpl.html',
                controller: 'reconTaskCtrl',
                resolve: {
                    //首次加载第一页
                    taskTypeList: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("082");
                    }],statusList: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("083");
                    }],
                }
            });
    }])

    //*****************************************header***************************s
    .controller('reconTaskCtrl',
        ['$scope', '$filter', '$interval', 'caCommonProxy', 'caHisDataProxy', 'modalService', '$location', 'taskTypeList', 'statusList', 'APPSETTING',
            function ($scope, $filter, $interval, caCommonProxy, caHisDataProxy, modalService, $location, taskTypeList, statusList, APPSETTING) {

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                $scope.taskTypeList = taskTypeList;
                $scope.statusList = statusList;

                $scope.taskType = '';
                $scope.status = '';
                $scope.taskName = '';
                $scope.dateF = '';
                $scope.dateT = '';

                //查询条件展开、合上
                var taskShow = false;
                $scope.taskOpenFilter = function () {
                    taskShow = !taskShow;
                    if (taskShow) {
                        $("#taskSearch").show();
                    } else {
                        $("#taskSearch").hide();
                    }
                };

                // task grid start
                $scope.startIndex = 0;
                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1; //当前页
                $scope.maxSize = 10; //分页显示的最大页  

                $scope.taskDataList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    enableRowSelection: false,
                    enableRowHeaderSelection: false,
                    data: 'taskList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        //{ field: 'menuregionName', displayName: 'Region', width: '10%' },
                        { field: 'taskTypeName', displayName: 'Task Type', width: '10%' },
                        { field: 'taskName', displayName: 'Task Name', width: '27%' },
                        { field: 'createUser', displayName: 'Create User', width: '10%' },
                        { field: 'createTime', displayName: 'Create Time', width: '10%', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                        { field: 'updateTime', displayName: 'Update Time', width: '10%', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                        { field: 'status', displayName: 'Status', width: '10%', cellFilter: 'mapStatus' },
                        {
                            field: 'action', displayName: 'Action', width: '10%',
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"> '
                                + '<a style="line-height:28px;margin-right:10px;" ng-show="row.entity.taskType!=6" ng-click="grid.appScope.viewdDetail(row.entity)">{{row.entity.status == "2" && "3,4,5,7".indexOf(row.entity.taskType) >= 0?"Detail":""}}</a>'
                                + '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity)" ng-show="row.entity.taskType==6 && row.entity.fileId!=\'\'" title="DownLoad"></a>'
                                + '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.downloadFile(row.entity)" ng-show="(row.entity.taskType==7 && row.entity.status == 2 && row.entity.fileId!=\'\')" title="DownLoad"></a>'
                                + '</div>'
                        }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                //Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getTaskDataDetails($scope.taskType, $scope.status, $scope.taskName, $scope.dateF, $scope.dateT, $scope.currentPage, selectedLevelId, function (result) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    }, function (error) {
                        alert(error);
                    });
                };

                //Detail翻页
                $scope.pageChanged = function () {
                    caHisDataProxy.getTaskDataDetails($scope.taskType, $scope.status, $scope.taskName, $scope.dateF, $scope.dateT, $scope.currentPage, $scope.itemsperpage, function (result) {
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.goback = function () {
                    $location.path("/ca/index");
                };

                $scope.pageChanged();
                // task grid end

                $scope.resetSearch = function () {
                    $scope.taskType = '';
                    $scope.status = '';
                    $scope.taskName = '';
                    $scope.dateF = '';
                    $scope.dateT = '';
                    $scope.init();
                };

                $scope.init = function () {
                    $scope.pageChanged();
                };

                // task operation start
                //跳转
                $scope.viewdDetail = function (entity) {
                    if (entity.taskType == "5" || entity.taskType == "7" ) {
                        localStorage.setItem('isReconAdjustment', false);
                        localStorage.setItem('reconTaskId', entity.id);
                        $location.path("/ca/reconDetail");
                    } else if (entity.taskType == "3") {
                        var modalDefaults = {
                            templateUrl: 'app/cashapplication/actiontask/identify.tpl.html?id=1',
                            controller: 'identifyCtrl',
                            size: 'customSize',
                            resolve: {
                                taskId: function () {
                                    return entity.id;
                                }
                            },
                            windowClass: 'modalDialog modalDialog_width_xlg'
                        };

                        modalService.showModal(modalDefaults, {}).then(function () {
                            //$scope.bankPageChanged();
                        });

                    } else if (entity.taskType == "4") {
                        var modalDefaults = {
                            templateUrl: 'app/cashapplication/actiontask/advisor.tpl.html?id=1',
                            controller: 'advisorCtrl',
                            size: 'customSize',
                            resolve: {
                                taskId: function () {
                                    return entity.id;
                                }
                            },
                            windowClass: 'modalDialog modalDialog_width_xlg'
                        };

                        modalService.showModal(modalDefaults, {}).then(function () {
                            //$scope.bankPageChanged();
                        });
                    }
                    
                };

                $scope.download = function (entity) {
                    if (entity.fileId === null || entity.fileId === "") {
                        alert("There is no need to download the file!");
                    } else {
                        let fileId = entity.fileId;
                        var files = fileId.split(";");
                        // return fieid;
                        files.forEach((item, index, array) => {
                            //下载文件(可能是多个)
                            window.open(APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + item, "_blank");
                        });                       
                        //window.location = fullNamePath;                  
                    }
                };

                $scope.downloadFile = function (entity) {
                    var files = entity.fileId.split(";");
                    // return fieid;
                    files.forEach((item, index, array) => {
                        //下载文件(可能是多个)
                        window.open(APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + item, "_blank");
                    });
                };
                // task operation end

                $scope.toUploadPage = function () {
                    $location.path("/ca/upload");
                }

                $scope.toTaskListPage = function () {
                    $location.path("/ca/reconTask");
                }

                $scope.actiontask = function () {
                    $location.path("/ca/actiontask");
                }
            }])

    .filter('mapStatus', function () {
        var statusHash = {
            1: 'Processing',
            2: 'Complete'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return statusHash[input];
            }
        };
    });