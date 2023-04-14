angular.module('app.cashapplication.bankAttach', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/bsattach', {
                templateUrl: 'app/cashapplication/bankFile/bankFile-list.tpl.html',
                controller: 'bankFileListCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('bankFileListCtrl',
        ['$scope', '$interval', 'caBankFileProxy', 'caHisDataProxy','modalService',
            function ($scope, $interval, caBankFileProxy, caHisDataProxy,modalService) {

                var bankFilefilterShow = false;
                $scope.bankFileOpenFilter = function () {
                    bankFilefilterShow = !bankFilefilterShow;
                    if (bankFilefilterShow) {
                        $("#bankFileDataSearch").show();
                    } else {
                        $("#bankFileDataSearch").hide();
                    }
                };

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];


                // bank grid start
                $scope.startIndex = 0;
                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1; //当前页
                $scope.maxSize = 10; //分页显示的最大页  

                $scope.dataList = {
                    multiSelect: false,
                    enableFullRowSelection: true,
                    enableFiltering: true,
                    noUnselect: true,
                    enableRowSelection: true,
                    enableSelectAll: false,
                    enableRowHeaderSelection: false,
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'transactionNum', displayName: 'Transaction INC.', width: '160' },
                        { field: 'filE_NAME', displayName: 'File Name', width: '400' },
                        { field: 'filetype', displayName: 'File Type', width: '100' },
                        { field: 'creatE_USER', displayName: 'Create User', width: '140' },
                        { field: 'creatE_TIME', displayName: 'Create Time', width: '140', cellFilter: 'date:\'yyyy-MM-dd \'', cellClass: 'center' },
                        {
                            field: 'id', displayName: 'Download', width: '140', enableFiltering: false,
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center">' +
                                '<a style="line-height:30px;vertical-align:middle;text-align:center;display:block;" class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity)" ng-show="true" title="DownLoad"></a>' +
                                '</div>'
                        },
                        {
                            field: 'operation', displayName: 'Action', width: '100', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false,
                            cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.deleteFile(row.entity.id)">delete</a></span>'
                        }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                ////Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {
                    caBankFileProxy.getBankFileList($scope.transactionNum, $scope.fileName, $scope.fileType, $scope.createDateF, $scope.createDateT, $scope.currentPage, selectedLevelId, function (result) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = result.count;
                        $scope.dataList.data = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    })
                };

                //Detail翻页
                $scope.pageChanged = function () {
                    caBankFileProxy.getBankFileList($scope.transactionNum, $scope.fileName, $scope.fileType, $scope.createDateF, $scope.createDateT, $scope.currentPage, $scope.itemsperpage, function (result) {
                        $scope.totalItems = result.count;
                        $scope.dataList.data = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.pageChanged();

                //download
                $scope.download = function (row) {
                    caHisDataProxy.GetFileFromWebApiById(row.id, function (data) {
                        //if (data.byteLength > 0) {
                            var blob = new Blob([data], { type: "application/vnd.ms-excel" });
                            var objectUrl = URL.createObjectURL(blob);
                            var aForExcel = $("<a><span class='forExcel'>下载excel</span></a>").attr("href", objectUrl);
                            aForExcel.attr("download", row.filE_NAME);
                            $("body").append(aForExcel);
                            $(".forExcel").click();
                            aForExcel.remove();
                        //}
                        //else {
                        //    alert("File not find!");
                        //}
                    }, function (ex) { alert(ex) });
                };

                $scope.deleteFile = function (fileId) {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            caBankFileProxy.deleteBankFile(fileId, function (result) {
                                alert("Delete Success.");
                                $scope.pageChanged();
                            }, function (err) {
                                alert(err);
                            });
                        }
                    });
                };

                $scope.init = function(){
                    $scope.currentPage = 1;
                    $scope.pageChanged();
                }
                
            }]);