angular.module('app.remittanceattachment', ['ui.bootstrap'])

    .controller('remittanceAttachmentCtrl', 
    // 'siteuseid','legalentity',
    ['$scope', '$uibModalInstance', 'collectorSoaProxy', 'FileUploader', 'APPSETTING', 
        //siteuseid,legalentity,
        function ($scope, $uibModalInstance, collectorSoaProxy, FileUploader, APPSETTING) {
            var result = "";
            $scope.isVat = "1";
            $scope.alertMessage = "";

            $scope.invoiceList = {
                //multiSelect: true,
                enableFullRowSelection: true,
                //   noUnselect: true,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'siteUseId', displayName: 'SiteUseId', cellClass: 'center', width: '120' },
                    { field: 'siteUseId', displayName: 'SiteUseId', cellClass: 'center', width: '120' },
                    
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;

                }
            };

            var uploader = $scope.uploader = new FileUploader({
                url: APPSETTING['serverUrl'] + '/api/Myinvoices/UploadVat'
            });
            uploader.filters.push({
                name: 'customFilter',
                fn: function (item, options) {
                    if (item.name.toString().toUpperCase().split(".")[1] != "XLS"
                        && item.name.toString().toUpperCase().split(".")[1] != "XLSX") {
                        alert("File format is not correct !");
                    }
                    return this.queue.length < 100;
                }
            });

            $scope.updateLevel = function () {
                if (uploader.queue.length > 0 && uploader.queue[3] !== "" && $scope.getFileExtendName(uploader.queue[3]._file.name.toString()).toUpperCase() != ".XLS"
                    && $scope.getFileExtendName(uploader.queue[3]._file.name.toString()).toUpperCase() != ".XLSX" ) {
                    alert("File format is not correct, \nPlease select the '.xlsx' or '.xls' file !");
                    return;
                }
                if (uploader.queue[3] !== undefined && uploader.queue[3] != "") {
                    if ($scope.isVat=="1") {
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices?FileType=SOA';
                    }
                    else if ($scope.isVat == "2") {
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices?FileType=SOA-SAP';
                    }
                    else if ($scope.isVat == "3") {
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices?FileType=SOA-India|Asean';
                    }
                    else if ($scope.isVat == "4") {
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices?FileType=SOA-HK';
                    }
                    else if ($scope.isVat == "5") {
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices?FileType=ANZ';
                    }
                }
                else {
                    alert("Please select file!");
                    return;
                }
                $scope.invoiceList.data = [];
                $scope.alertMessage = "";
                uploader.uploadAll();
            };

            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                if (response != "") {
                    alert(response);
                }
                //检索数据
                collectorSoaProxy.GetInvoicesStatusData(function (list){
                    $scope.invoiceList.data = list;
                    $scope.getAlertMessage();
                });

                $scope.clearFile();
            };

            uploader.onErrorItem = function (fileItem, response, status, headers) {
                $scope.clearFile();
            }

            $scope.clearFile = function () {
                if (uploader.queue.length > 0) {
                    uploader.queue[3] = "";
                    document.getElementById("updVATFile").value = "";
                }
            }

            $scope.cancel = function () {

                collectorSoaProxy.DelInvoicesStatusData(function (returnFlag) {
                    if (returnFlag == "true") {
                        $uibModalInstance.close(result);
                    }
                    else {
                        alert(returnFlag);
                    }
                });
                result = "cancel";
                $uibModalInstance.close(result);
            };

            $scope.submit = function () {
                var result = [];
                result.push('submit');

                collectorSoaProxy.SetInvoicesStatusData(function (returnFlag) {
                    if (returnFlag == "true") {
                        alert("保存成功!");
                        $uibModalInstance.close(result);
                    }
                    else {
                        alert(returnFlag);
                    }
                });
            }

            $scope.getFileExtendName = function (str) {
                var d = /\.[^\.]+$/.exec(str);
                return d.toString();
            }

            $scope.getAlertMessage = function () {
                var ptp = 0;
                var dueReason = 0;
                var memo = 0;
                for (var i = 0; i < $scope.invoiceList.data.length; i++) {
                    var item = $scope.invoiceList.data[i];
                    if ((item.invoicE_PTPDATE_OLD && !item.invoicE_PTPDATE) ||
                        (!item.invoicE_PTPDATE_OLD && item.invoicE_PTPDATE) ||
                        (item.invoicE_PTPDATE_OLD && item.invoicE_PTPDATE && item.invoicE_PTPDATE_OLD != item.invoicE_PTPDATE)) {
                        ptp += 1;
                    }
                    if ((item.invoicE_DueReason_OLD && !item.invoicE_DueReason) ||
                        (!item.invoicE_DueReason_OLD && item.invoicE_DueReason) ||
                        (item.invoicE_DueReason_OLD && item.invoicE_DueReason && item.invoicE_DueReason_OLD != item.invoicE_DueReason)) {
                        dueReason += 1;
                    }
                    if ((item.invoicE_Comments && !item.invoicE_BalanceMemo) ||
                        (!item.invoicE_Comments && item.invoicE_BalanceMemo) ||
                        (item.invoicE_Comments && item.invoicE_BalanceMemo && item.invoicE_Comments != item.invoicE_BalanceMemo)){
                        memo += 1;
                    }
                }

                $scope.alertMessage = "PTP 变更：" + ptp + "，DueReason变更：" + dueReason + "，Comments变更：" + memo;
            }

        }])
