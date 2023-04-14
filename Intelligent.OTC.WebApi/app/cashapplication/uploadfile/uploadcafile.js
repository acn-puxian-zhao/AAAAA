angular.module('app.uploadcafile', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/upload', {
                templateUrl: 'app/cashapplication/uploadfile/uploadcafile.tpl.html',
                controller: 'uploadFileCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('uploadFileCtrl',
        ['$scope', '$filter', '$interval', 'caHisDataProxy', 'caCommonProxy', 'modalService', '$location', 'FileUploader', 'APPSETTING',
            function ($scope, $filter, $interval, caHisDataProxy, caCommonProxy, modalService, $location, FileUploader, APPSETTING) {
                $scope.$parent.helloAngular = "OTC - Upload Files";

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                $scope.goback = function () {
                    $location.path("/ca/index");
                };

                var uploader = $scope.uploader = new FileUploader({
                    url: APPSETTING['serverUrl'] + '/api/CAUploadFile'
                });

                uploader.filters.push({
                    name: 'uploadFilter',
                    fn: function (item /*{File|FileLikeObject}*/, options) {
                        var filename = item.name.toString().toUpperCase();
                        var indexofpoint = filename.lastIndexOf(".");
                        if (indexofpoint > 0 && filename.substr(indexofpoint + 1) !== "XLS"
                            && filename.substr(indexofpoint + 1) !== "XLSX") {
                            alert("File format is not correct, \nPlease select the '.xls' or '.xlsx' file !");
                            return;
                        }
                        return this.queue.length < 100;
                    }
                });


                var uploadType = "";
                uploader.onSuccessItem = function (fileItem, response, status, headers) {
                    document.getElementById("overlay-container").style.display = "none";
                    document.getElementById("overlay-container").style.flg = "";
                    //Bank Statement
                    if (uploadType === "updBSFile") {
                        uploader.queue[9] = "";
                        $scope.clearFile("updBSFile",9);
                        uploadType = "";
                        alert(response);
                    }
                    //Remittance
                    if (uploadType === "updRemittanceFile") {
                        alert(response);
                        uploader.queue[10] = "";
                        $scope.clearFile("updRemittanceFile",10);
                        uploadType = "";
                    }
                    //Remittance batch
                    if (uploadType === "updpmtFile") {
                        alert(response);
                        uploader.queue[13] = "";
                        $scope.clearFile("updpmtFile",13);
                        uploadType = "";
                    }
                    $scope.pageChanged();
                };

                uploader.onErrorItem = function (fileItem, response, status, headers) {
                    document.getElementById("overlay-container").style.display = "none";
                    document.getElementById("overlay-container").style.flg = "";
                    if (uploadType === "updBSFile") {
                        uploader.queue[9] = "";
                        $scope.clearFile("updBSFile");
                        uploadType = "";
                        alert(response);  
                    }
                    if (uploadType === "updRemittanceFile") {
                        uploader.queue[10] = "";
                        $scope.clearFile("updRemittanceFile");
                        uploadType = "";
                       
                        if (response.indexOf("fileId:") >= 0) {
                            var resp = response.split(";;");
                            if (resp.length == 2) {
                                var fileId = resp[0].substr(7);
                                var rsp = resp[1];

                                var modalDefaults = {
                                    templateUrl: 'app/cashapplication/uploadfile/delConfirm.tpl.html',
                                    controller: 'uploadDelConfirmCtrl',
                                    resolve: {
                                        errmsg: function () {
                                            return rsp;
                                        }
                                    },
                                    windowClass: 'modalDialog'
                                };
                                modalService.showModal(modalDefaults, {}).then(function (result) {
                                    if (result == "Yes") {
                                        caHisDataProxy.uploadPMTByFileId(fileId, function () {
                                            alert("upload Success!");
                                            $scope.pageChanged();
                                        }, function (error) {
                                            alert(error);
                                        })
                                    }
                                });
                            }
                        } else {
                            alert(response);
                        }

                    }
                    if (uploadType === "updpmtFile") {
                        uploader.queue[13] = "";
                        $scope.clearFile("updpmtFile");
                        uploadType = "";
                        alert(response);  
                    }
                    $scope.pageChanged();
                };


                $scope.downloadtemplete = function (fileType) {
                    caCommonProxy.downloadtemplete(fileType);
                }

                $scope.updateBSFile = function (fileType) {
                    
                    uploadType = fileType;
                    if (uploader.queue[9] === null || uploader.queue[9] === "" || uploader.queue[9] === undefined) {
                        alert("Please select the file you need to upload!");
                        return;
                    }
                    var filename = uploader.queue[9]._file.name.toString().toUpperCase();
                    var indexofpoint = filename.lastIndexOf(".");
                    if (indexofpoint > 0 && filename.substr(indexofpoint + 1)!== "XLS"
                        && filename.substr(indexofpoint + 1) !== "XLSX") {
                        alert("File format is not correct, \nPlease select the '.xls' or '.xlsx' file !");
                        return;
                    }

                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";

                    uploader.queue[9].url = APPSETTING['serverUrl'] + '/api/CAUploadFile' + '?fileType=' + fileType;
                    uploader.uploadAll();
                };

                $scope.updateRemittanceFile = function (fileType) {

                    uploadType = fileType;
                    if (uploader.queue[10] === null || uploader.queue[10] === "" || uploader.queue[10] === undefined) {
                        alert("Please select the file you need to upload!");
                        return;
                    }
                    var filename = uploader.queue[10]._file.name.toString().toUpperCase();
                    var indexofpoint = filename.lastIndexOf(".");
                    if (indexofpoint > 0 && filename.substr(indexofpoint + 1) !== "XLS"
                        && filename.substr(indexofpoint + 1) !== "XLSX") {
                        alert("File format is not correct, \nPlease select the '.xls' or '.xlsx' file !");
                        return;
                    }

                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";

                    uploader.queue[10].url = APPSETTING['serverUrl'] + '/api/CAUploadFile' + '?fileType=' + fileType ;
                    uploader.uploadAll();
                };


                $scope.updatepmtFile = function (fileType) {

                    uploadType = fileType;
                    if (uploader.queue[13] === null || uploader.queue[13] === "" || uploader.queue[13] === undefined) {
                        alert("Please select the file you need to upload!");
                        return;
                    }

                    var filename = uploader.queue[13]._file.name.toString().toUpperCase();
                    var indexofpoint = filename.lastIndexOf(".");
                    if (indexofpoint > 0 && filename.substr(indexofpoint + 1) !== "XLS"
                        && filename.substr(indexofpoint + 1) !== "XLSX") {
                        alert("File format is not correct, \nPlease select the '.xls' or '.xlsx' file !");
                        return;
                    }

                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";

                    uploader.queue[13].url = APPSETTING['serverUrl'] + '/api/CAUploadFile' + '?fileType=' + fileType;
                    uploader.uploadAll();
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
                    data: 'taskList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'taskName', displayName: 'Task Name', width: '400' },
                        { field: 'taskTypeName', displayName: 'Task Type', width: '120' },
                        { field: 'createUser', displayName: 'Create User', width: '120' },
                        { field: 'createTime', displayName: 'Create Time', width: '160', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'statusName', displayName: 'Status', width: '120' },
                        {
                            field: 'downLoadFlg', displayName: 'Source Download', width: '140', enableFiltering: false,
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center">' +
                                '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity.fileId,0)" ng-show="true" title="DownLoad"></a>' +
                                '</div>'
                        },
                        {
                            field: 'downLoadFlg', displayName: 'Result Download', width: '140', enableFiltering: false,
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center">' +
                                '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity.fileId,1)" ng-show="row.entity.resultFileFlag>0" title="DownLoad"></a>' +
                                '</div>'
                        }

                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                //Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getTaskDataDetailsByType($scope.currentPage, selectedLevelId, function (result) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    });
                };

                //Detail翻页
                $scope.pageChanged = function () {
                    caHisDataProxy.getTaskDataDetailsByType($scope.currentPage, $scope.itemsperpage, function (result) {
                        $scope.totalItems = result.count;
                        $scope.taskList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                };

                //download
                $scope.download = function (fileId, fileIndex) {
                    let fileIds = fileId.split(";");
                    if (fileIds.length > fileIndex) {
                        window.open(APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + fileIds[fileIndex], "_blank");            
                    } else {
                        alert("File not find!");
                    };
                };

                $scope.pageChanged();
                // task grid end

                $scope.clearFile = function (items, id) {
                    if (uploader.queue.length > 0) {
                        uploader.queue[id] = "";
                    }
                    document.getElementById(items).value = "";    
                };

                $scope.toTaskListPage = function () {
                    $location.path("/ca/reconTask");
                }

                $scope.actiontask = function () {
                    $location.path("/ca/actiontask");
                }

                $scope.reuploadPost = function () {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/reuploadPost.tpl.html',
                        controller: 'reuploadPostCtrl',
                        size: 'customSize',
                        resolve: {

                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        $scope.pageChanged();
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.reuploadPostClear = function () {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/reuploadPostClear.tpl.html',
                        controller: 'reuploadPostClearCtrl',
                        size: 'customSize',
                        resolve: {

                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        $scope.pageChanged();
                    }, function (err) {
                        alert(err);
                    });
                };
            }])
    .controller('uploadDelConfirmCtrl', ['$scope', '$uibModalInstance','errmsg',
        function ($scope, $uibModalInstance, errmsg) {
            $scope.err = errmsg;
            $scope.confirm = function () {
                $uibModalInstance.close("Yes");
            }

            $scope.cancel = function () {
                $uibModalInstance.close("No");
            };
        }]);