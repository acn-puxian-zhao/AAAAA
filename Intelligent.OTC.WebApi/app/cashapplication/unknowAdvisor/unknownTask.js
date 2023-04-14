angular.module('app.unknownTask', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/unknownTask', {
                templateUrl: 'app/cashapplication/unknowAdvisor/unknownTask.tpl.html',
                controller: 'unknownTaskCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('unknownTaskCtrl',
        ['$scope', '$filter', '$interval', 'caHisDataProxy', 'modalService', '$location',
            function ($scope, $filter, $interval, caHisDataProxy, modalService,$location) {

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];


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
                    data: 'taskList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'taskName', displayName: 'Task Name', width: '120' },
                        { field: 'taskType', displayName: 'Task Type', width: '120' },
                        { field: 'createUser', displayName: 'Create User', width: '120' },
                        { field: 'createDate', displayName: 'Create Time', width: '120' },
                        { field: 'status', displayName: 'Status', width: '120' },
                        {
                            field: 'action', displayName: 'Action', width: '120',
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewdDetail(row.entity.id)">Detail</a></div>'
                        }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                //Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getTaskDataDetails($scope.currentPage, selectedLevelId, function (result) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    })
                };

                //Detail翻页
                $scope.pageChanged = function () {
                    caHisDataProxy.getTaskDataDetails($scope.currentPage, $scope.itemsperpage, function (result) {
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.pageChanged();
                // task grid end
                                              
                // task operation start
                //跳转
                $scope.viewdDetail = function (id) {
                    $location.path("/ca/unknownDetail");
                };
                // task operation end
                
            }]);