angular.module('app.bankfile', [])
    //*****************************************header***************************s
    .controller('bankFileCtrl',
        ['$scope', '$filter', '$interval', '$uibModalInstance', 'caBankFileProxy', 'caHisDataProxy', 'modalService','bank', '$location', 'FileUploader', 'APPSETTING',
            function ($scope, $filter, $interval, $uibModalInstance, caBankFileProxy, caHisDataProxy, modalService, bank, $location, FileUploader, APPSETTING) {
                $scope.bank = bank;

                var uploader = $scope.uploader = new FileUploader({
                    url: APPSETTING['serverUrl'] + '/api/CaBankFileController/UploadFile'
                });

                uploader.filters.push({
                    name: 'customFilter',
                    fn: function (item, options) {  
                        var filename = item.name.toString().toUpperCase();
                        var index = filename.lastIndexOf(".");
                        var suffix = filename.substr(index + 1);
                        if (suffix === "JS" || suffix === "CMD" || suffix === "EXE" || suffix === "BAT" || suffix === "DLL") {
                            alert("File format is not correct (Not allowed - .Bat | .Exe | .Cmd | .Js | .dll) !");
                        }
                        return this.queue.length < 100;
                    }
                });
                
                uploader.onSuccessItem = function (fileItem, response, status, headers) {
                    document.getElementById("overlay-container").style.display = "none";
                    document.getElementById("overlay-container").style.flg = "";
                    uploader.queue[9] = "";
                    $scope.clearFile("updBSFile");
                    alert(response);                    
                    $scope.pageChanged();
                };

                uploader.onErrorItem = function (fileItem, response, status, headers) {
                    alert(response);
                    document.getElementById("overlay-container").style.display = "none";
                    document.getElementById("overlay-container").style.flg = "";
                    uploader.queue[9] = "";
                    $scope.clearFile("updBSFile");
                    uploadType = "";
                    $scope.pageChanged();
                };


                $scope.updateBSFile = function () {
                    if (uploader.queue[9] === null || uploader.queue[9] === "" || uploader.queue[9] === undefined) {
                        alert("Please select the file you need to upload!");
                        return;
                    }

                    var filename = uploader.queue[9]._file.name.toString().toUpperCase();
                    var index = filename.lastIndexOf(".");
                    var suffix = filename.substr(index + 1);
                    if (suffix === "JS" || suffix === "CMD" || suffix === "EXE" || suffix === "BAT" || suffix === "DLL") {
                        alert("File format is not correct (Not allowed - .bat | .exe | .cmd | .js | .dll) !");
                        uploader.queue[9] = "";
                        $scope.clearFile("updBSFile");
                        uploadType = "";
                        return;
                    }
                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";

                    uploader.queue[9].url = APPSETTING['serverUrl'] + '/api/CaBankFileController/UploadFile' + '?bankId=' + $scope.bank.id + '&legalEntity=' + $scope.bank.legalEntity + "&transactionNum=" + $scope.bank.transactioN_NUMBER;
                    uploader.uploadAll();
                };

                $scope.fileDataList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    noUnselect: false,
                    data: 'fileList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'filE_NAME', displayName: 'File Name', width: '400' },
                        { field: 'filetype', displayName: 'File Type', width: '80' },
                        { field: 'creatE_USER', displayName: 'Create User', width: '120' },
                        { field: 'creatE_TIME', displayName: 'Create Time', width: '120', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        {
                            field: 'fileId', displayName: 'Download', width: '140', enableFiltering: false,
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

                //Detail翻页
                $scope.pageChanged = function () {
                    caBankFileProxy.getBankFilesByBankId($scope.bank.id, function (result) {
                        $scope.fileList = result;
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

                $scope.clearFile = function (items) {
                    if (uploader.queue.length > 0) {
                        uploader.queue[0] = "";
                    }
                    document.getElementById(items).value = "";
                };

                $scope.closeModal = function () {
                    $uibModalInstance.close();
                };

            }]);