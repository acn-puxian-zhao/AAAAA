angular.module('app.myinvoices.vatimport', ['ui.bootstrap'])

    .controller('vatimportInstanceCtrl', 
    // 'siteuseid','legalentity',
    ['$scope', '$uibModalInstance', 'collectorSoaProxy', 'FileUploader', 'APPSETTING', 
        //siteuseid,legalentity,
        function ($scope, $uibModalInstance, collectorSoaProxy, FileUploader, APPSETTING) {
            var result = "";
            $scope.isVat = "0";
            $scope.alertMessage = "";
            $scope.alertMessageCustomer = "";

            $scope.invoiceList = {
                //multiSelect: true,
                enableFullRowSelection: true,
                //   noUnselect: true,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'siteUseId', displayName: 'SiteUseId', cellClass: 'center', width: '90' },
                    { field: 'invoicE_DATE', displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', width: '90' },
                    { field: 'invoicE_NO', displayName: 'Invoice Number', width: '110' },
                    { field: 'invoicE_AMOUNT', displayName: 'Amount', cellFilter: 'number:2', type: 'number', cellClass: 'right', width: '80'  },
                    { field: 'invoicE_Class', displayName: 'Class', width: '80' },
                    { field: 'invoicE_CurrencyCode', displayName: 'CurrencyCode', width: '80' },
                    { field: 'invoicE_PTPDATE_OLD', displayName: 'ORI PTP', cellFilter: 'date:\'yyyy-MM-dd\'', width: '80' },
                    {
                        field: 'invoicE_PTPDATE', displayName: 'PTP', cellFilter: 'date:\'yyyy-MM-dd\'', width: '80',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridblue center';
                            }
                        }
                    },
                    { field: 'invoicE_DueReason_OLD', displayName: 'ORI DueReason', width: '110' },
                    {
                        field: 'invoicE_DueReason', displayName: 'DueReason', width: '110',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridred';
                            }
                        }
                    },
                    { field: 'invoicE_Comments', displayName: 'ORI Comments', width: '200' },
                    { field: 'invoicE_BalanceMemo', displayName: 'Comments', width: '200' },
                    { field: 'memoExpirationDate', displayName: 'Memo Expiration Date', width: '200' },
                    { field: 'invoicE_Status', displayName: 'Status', width: '100' },
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;

                }
            };

            $scope.customerCommentList = {
                //multiSelect: true,
                enableFullRowSelection: true,
                //   noUnselect: true,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'siteUseId', displayName: 'SiteUseId', cellClass: 'center', width: '90' },
                    { field: 'agingBucket', displayName: 'AgingBucket', cellClass: 'center', width: '90' },
                    { field: 'ptpAmountOld', displayName: 'OldPTPAmount', cellFilter: 'number:2', type: 'number', cellClass: 'right', width: '80' },
                    {
                        field: 'ptpAmount', displayName: 'PTPAmount', cellFilter: 'number:2', type: 'number', cellClass: 'right', width: '80',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridblue right';
                            }
                        }
                    },
                    { field: 'ptpDateOld', displayName: 'OldPTPDate', cellFilter: 'date:\'yyyy-MM-dd\'', width: '80' },
                    {
                        field: 'ptpDate', displayName: 'PTPDate', cellFilter: 'date:\'yyyy-MM-dd\'', width: '80',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridred center';
                            }
                        }
                    },
                    { field: 'odReasonOld', displayName: 'OldODReason', cellClass: 'left', width: '90' },
                    {
                        field: 'odReason', displayName: 'ODReason', cellClass: 'left', width: '90',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridblue';
                            }
                        }
                    },
                    { field: 'commentsOld', displayName: 'OldComments', cellClass: 'left', width: '200' },
                    {
                        field: 'comments', displayName: 'Comments', cellClass: 'left', width: '200',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridred left';
                            }
                        }
                    },
                    { field: 'commentsFromOld', displayName: 'OldSrouce', cellClass: 'left', width: '200' },
                    {
                        field: 'commentsFrom', displayName: 'Source', cellClass: 'left', width: '200',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (grid.getCellValue(row, col) !== '') {
                                return 'uigridred left';
                            }
                        }
                    }
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

                //if ($scope.checkIsDuringJobSchedule()) {
                //    return;
                //}

                if (uploader.queue.length > 0 && uploader.queue[3] !== "" && $scope.getFileExtendName(uploader.queue[3]._file.name.toString()).toUpperCase() != ".XLS"
                    && $scope.getFileExtendName(uploader.queue[3]._file.name.toString()).toUpperCase() != ".XLSX" ) {
                    alert("File format is not correct, \nPlease select the '.xlsx' or '.xls' file !");
                    return;
                }
                if (uploader.queue[3] !== undefined && uploader.queue[3] != "") {
                    if ($scope.isVat=="0") {
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Myinvoices?FileType=SOA-CN';
                    }
                    if ($scope.isVat == "1") {
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
                $scope.alertMessageCustomer = "";
                uploader.uploadAll();
            };

            //-----to check if during job schedule, add on 05-Jul-2019----
            $scope.checkIsDuringJobSchedule = function () {

                var nowDateTime = new Date();
                var starDatetTime = new Date(nowDateTime.toDateString() + ' ' + APPSETTING['jobStartTime']);
                var endDateTime = new Date(nowDateTime.toDateString() + ' ' + APPSETTING['jobEndTime']);
                var starDatetTimeAfternoon = new Date(nowDateTime.toDateString() + ' ' + APPSETTING['jobStartTimeAfternoon']);
                var endDateTimeAfternoon = new Date(nowDateTime.toDateString() + ' ' + APPSETTING['jobEndTimeAfternoon']);

                //console.log('nowDateTime:' + nowDateTime.toString());
                //console.log('starDatetTime:' + starDatetTime.toString());
                //console.log('endDateTime:' + endDateTime.toString());

                if (nowDateTime >= starDatetTime && nowDateTime <= endDateTime) {
                    alert('Please note that could not do this operation during job schedule from ' + APPSETTING['jobStartTime'] + ' to ' + APPSETTING['jobEndTime'] + '!');
                    return true;
                }
                if (nowDateTime >= starDatetTimeAfternoon && nowDateTime <= endDateTimeAfternoon) {
                    alert('Please note that could not do this operation during job schedule from ' + APPSETTING['jobStartTimeAfternoon'] + ' to ' + APPSETTING['jobEndTimeAfternoon'] + '!');
                    return true;
                }
                return false;
            }

            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                if (response != "") {
                    alert(response);
                }
                //检索数据
                collectorSoaProxy.GetInvoicesStatusData(function (list) {
                    //检索数据
                    collectorSoaProxy.GetCustomerCommentStatusData(function (list) {
                        $scope.customerCommentList.data = list;
                        $scope.getAlertMessageCustomer();
                    });
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
                $scope.alertMessage = "InvoiceLevel - PTPDate变更：" + ptp + "，DueReason变更：" + dueReason + "，Comments变更：" + memo;
            }

            $scope.getAlertMessageCustomer = function () {
                var customerptpAmount = 0;
                var customerptpDate = 0;
                var customerdueReason = 0;
                var customercomments = 0;
                for (var i = 0; i < $scope.customerCommentList.data.length; i++) {
                    var item = $scope.customerCommentList.data[i];
                    if ((item.ptpAmountOld && !item.ptpAmount) ||
                        (!item.ptpAmountOld && item.ptpAmount) ||
                        (item.ptpAmountOld && item.ptpAmount && item.ptpAmountOld != item.ptpAmount)) {
                        customerptpAmount += 1;
                    }
                    if ((item.ptpDateOld && !item.ptpDate) ||
                        (!item.ptpDateOld && item.ptpDate) ||
                        (item.ptpDateOld && item.ptpDate && item.ptpDateOld != item.ptpDate)) {
                        customerptpDate += 1;
                    }
                    if ((item.odReasonOld && !item.odReason) ||
                        (!item.odReasonOld && item.odReason) ||
                        (item.odReasonOld && item.odReason && item.odReasonOld != item.odReason)) {
                        customerdueReason += 1;
                    }
                    if ((item.commentsOld && !item.comments) ||
                        (!item.commentsOld && item.comments) ||
                        (item.commentsOld && item.comments && item.commentsOld != item.comments)) {
                        customercomments += 1;
                    }
                }
                $scope.alertMessageCustomer = "CustomerLevel - PTPAmount变更：" + customerptpAmount + "，PTPDate变更：" + customerptpDate + "，DisputeReason变更：" + customerdueReason + "，Comments变更：" + customercomments;
            }

        }])
