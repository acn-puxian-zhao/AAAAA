angular.module('app.sendDun', ['ui.grid.edit', 'ui.grid.grouping'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            //referenceNo  --added by alex
            .when('/dunning/sendDun/:nums/:type/:alertType/:alertId/:referenceNo', {
                templateUrl: 'app/dunning/senddun/sendDun.tpl.html',
                controller: 'senddunCL',
                resolve: {
                    languagelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("013");
                    }],
                    inUse: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("018");
                    }]
                }
            });
    }])
    .controller('senddunCL',
    ['$scope', 'baseDataProxy', 'modalService', '$interval', 'invoiceProxy', '$routeParams', 'contactProxy', 'mailProxy', 'customerPaymentbankProxy',
        'customerPaymentcircleProxy', 'collectorSoaProxy', 'permissionProxy', 'customerProxy', 'generateSOAProxy', 'contactProxy', 'contactCustomerProxy',
        '$q', 'languagelist', 'inUse', 'FileUploader', 'APPSETTING', 'dunningProxy', 'contactCustomerProxy', 'appFilesProxy', 'siteProxy', 'uiGridConstants', '$sce',
        function ($scope, baseDataProxy, modalService, $interval, invoiceProxy, $routeParams, contactProxy, mailProxy, customerPaymentbankProxy,
            customerPaymentcircleProxy, collectorSoaProxy, permissionProxy, customerProxy, generateSOAProxy, contactProxy, contactCustomerProxy,
            $q, languagelist, inUse, FileUploader, APPSETTING, dunningProxy, contactCustomerProxy, appFilesProxy, siteProxy, uiGridConstants, $sce
        ) {
            $scope.languagelist = languagelist;
            $scope.inUselist = inUse;
            $scope.mailDats = ['contactCtrl'];

            //######################### Add by Alex #############
            //contact add button show or hide
            $scope.isContactAddBtnShow = false;
            //payment bank  add button show or hide
            $scope.isPayBankAddBtnShow = false;
            //contact domain add button show or hide 
            $scope.isDomainAddBtnShow = false;
            //######################### Add by Alex #############

            siteProxy.GetLegalEntity("type", function (legal) {
                $scope.legallist = legal;
            }, function () {
            });
            var userId = "";
            $scope.options = [];
            $scope.gridApis = [];
            $scope.custInfo = [];
            $scope.custInfo[2] = $routeParams.nums; //custInfo[8]
            $scope.custInfo[3] = true; //custInfo[9]
            $scope.alertType = $routeParams.alertType;
            $scope.selMailId = "";
            //$scope.causeObjectNumber = $routeParams.causeObjectNumber;
            $scope.alertId = $routeParams.alertId;
            $scope.num = $routeParams.referenceNo.split(',');
            $scope.selectText = "";
            $scope.entityFlg = "";
            $scope.soapaymentCircle = "";
            $scope.currCustNum = "";
            if ($scope.finishflg == 'complete') {
                $scope.isreadonly = true;
            }
            //added by alex
            $scope.referenceNo = $routeParams.referenceNo;

            //$scope.contactList = {
            //    columnDefs: [
            //         { field: 'customerNum', displayName: 'Customer Num', width: '130', grouping: { groupPriority: 0 } },
            //         { field: 'contactDate', displayName: 'Contact date', width: '130', cellFilter: 'date:\'yyyy-MM-dd\'' },
            //         { field: 'contactType', displayName: 'Contact Type', width: '200' },
            //         { field: 'comments', displayName: 'Comments', width: '500' }
            //         , {
            //             name: 'tmp', displayName: 'Operation', width: '120',
            //             cellTemplate: '<div>' +
            //                           '<a ng-click="grid.appScope.getDetail(row.entity)"> detail </a></div>'
            //         }
            //    ],
            //};

            dunningProxy.query({ ColDun: $scope.referenceNo, AlertType: $scope.alertType, AlertId: $scope.alertId }, function (list) {
                $scope.sendsoa = list;
                var i = 0;
                angular.forEach($scope.sendsoa, function (row) {
                    row['soapaymentCircle'] = "";
                    row['entityFlg'] = "";
                    row['hisg'] = {
                        data: row.subContactHistory,
                        columnDefs: [
                            { field: 'sortId', displayName: '#' },
                            { field: 'contactDate', displayName: 'Contact date', width: '130', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'contactType', displayName: 'Contact Type', width: '200' },
                            { field: 'comments', displayName: 'Comments', width: '500' },
                            {
                                name: 'tmp', displayName: 'Operation', width: '120',
                                cellTemplate: '<div>' +
                                '<a ng-click="grid.appScope.getDetail(row.entity)"> detail </a></div>'
                            }
                        ]
                    };
                    row['cg'] = {
                        //data: row.subContact,
                        columnDefs: [
                            { field: 'name', displayName: 'Contact Name' },
                            { field: 'emailAddress', displayName: 'Email', width: '210' },
                            { field: 'department', displayName: 'Department' },
                            { field: 'title', displayName: 'Title' },
                            { field: 'number', displayName: 'Contact Number' },
                            { field: 'comment', displayName: 'Comment' },
                            {
                                field: 'toCc', displayName: 'To/Cc',
                                cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckType(row.entity)}}</div>'
                            },
                            {
                                field: 'isGroupLevel', displayName: 'Is Group Level',
                                cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckGroupLevel(row.entity)}}</div>', width: '90'
                            },
                            {
                                name: 'o', displayName: 'Operation', width: '90',
                                cellTemplate: '<div>&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditContacterInfo(row.entity)" class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.Delcontacter(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                            }
                        ]
                    };
                    row['cgshow'] = false;
                    row['pbg'] = {
                        //data: row.subPaymentBank,
                        columnDefs: [
                            { field: 'bankAccountName', displayName: 'Account Name' },
                            { field: 'bankName', displayName: 'Bank Name' },
                            { field: 'bankAccount', displayName: 'Bank Account' },
                            { field: 'createDate', displayName: 'Create Date', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'createPersonId', displayName: 'Create Person' },
                            { field: 'inUse', displayName: 'InUse' },
                            { field: 'description', displayName: 'Description' }
                            ,
                            {
                                name: 'o', displayName: 'Operation', width: '90',
                                cellTemplate: '<div>&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditBankInfo(row.entity)" class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelBankInfo(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                            }
                        ]
                    };
                    row['pbgshow'] = false;
                    row['pcg'] = {
                        //data: row.subPaymentCalender,
                        columnDefs: [
                            { field: 'sortId', displayName: '#' },
                            { field: 'legalEntity', displayName: 'Legal Entity' },
                            { field: 'paymentDay', displayName: 'Payment Day', cellFilter: 'date:\'yyyy-MM-dd\'' },
                            { field: 'weekDay', displayName: 'Week Day' }
                            ,
                            {
                                name: 'o', displayName: 'Operation', width: '90',
                                cellTemplate: '<div>&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelPaymentcircle(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                            }
                        ]
                    };
                    //add by jiaxing for show ContactorDomain
                    row['cdgshow'] = false;
                    row['cdg'] = {
                        data: row.subContactDomain,
                        columnDefs: [
                            { field: 'sortId', displayName: '#', width: '120', },
                            { field: 'customerNum', displayName: 'Customer Num' },
                            { field: 'legalEntity', displayName: 'Legal Entity' },
                            { field: 'mailDomain', displayName: 'Email Domain' },
                            {
                                name: 'o', displayName: 'Operation', width: '90',
                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditCustDomain(row.entity)"  class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelCustDomain(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                            }
                        ]
                    }
                    row['pcgshow'] = false;
                    angular.forEach(row.subLegal, function (rowItem) {
                        rowItem['gridoption'] =
                            {
                                data: rowItem.subInvoice,
                                enableFiltering: true,
                                showGridFooter: true,
                                enableFiltering: true,
                                columnDefs: [
                                    {
                                        field: 'invoiceNum', name: 'tmp', displayName: 'Invoice #', enableCellEdit: false, width: '100', pinnedLeft: true
                                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceNum}}</a></div>'
                                    },
                                    { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                                    { field: 'creditTerm', enableCellEdit: false, displayName: 'Credit Term', width: '100' },
                                    { field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '95' },
                                    { field: 'purchaseOrder', enableCellEdit: false, displayName: 'PO Num', width: '160' },
                                    { field: 'saleOrder', enableCellEdit: false, displayName: 'SO Num', width: '122' },
                                    { field: 'rbo', enableCellEdit: false, displayName: 'RBO', width: '120' },
                                    { field: 'invoiceCurrency', enableCellEdit: false, displayName: 'Currency', width: '90' },
                                    {
                                        field: 'outstandingInvoiceAmount', enableCellEdit: false, displayName: 'Outstanding Invoice Amount', cellFilter: 'number:2', type: 'number', width: '200'
                                        , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                            if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                                return 'uigridred';
                                            }
                                        }
                                    },
                                    {
                                        field: 'originalInvoiceAmount', enableCellEdit: false, displayName: 'Original Invoice Amount', cellFilter: 'number:2', type: 'number', width: '200'
                                        , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                            if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                                return 'uigridred';
                                            }
                                        }
                                    },
                                    { field: 'daysLate', enableCellEdit: false, displayName: 'Days Late', width: '80', type: 'number', cellClass: 'right' },
                                    {
                                        field: 'status', enableCellEdit: false, displayName: 'Status', width: '80'
                                        , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                            if (grid.getCellValue(row, col) === 'Dispute') {
                                                return 'uigridred';
                                            }
                                        }
                                        , filter: {
                                            term: '',
                                            type: uiGridConstants.filter.SELECT,
                                            selectOptions: [
                                                { value: 'Open', label: 'Open' },
                                                { value: 'PTP', label: 'PTP' },
                                                { value: 'Dispute', label: 'Dispute' },
                                                { value: 'PartialPay', label: 'PartialPay' },
                                                { value: 'Broken PTP', label: 'Broken PTP' },
                                                { value: 'Hold', label: 'Hold' },
                                                { value: 'Payment', label: 'Payment' }]
                                        }
                                        //, cellFilter: 'mapStatus'
                                    },
                                    {
                                        field: 'invoiceTrack', enableCellEdit: false, displayName: 'Invoice Track', width: '100'
                                        , filter: {
                                            term: '',
                                            type: uiGridConstants.filter.SELECT,
                                            selectOptions: [
                                                { value: 'SOA Sent', label: 'SOA Sent' },
                                                { value: 'Second Reminder Sent', label: 'Second Reminder Sent' },
                                                { value: 'Final Reminder Sent', label: 'Final Reminder Sent' },
                                                { value: 'Dispute', label: 'Dispute' },
                                                { value: 'PTP', label: 'PTP' },
                                                { value: 'Payment Notice Received', label: 'Payment Notice Received' },
                                                { value: 'Broken PTP', label: 'Broken PTP' },
                                                { value: 'First Broken Sent', label: 'First Broken Sent' },
                                                { value: 'Second Broken Sent', label: 'Second Broken Sent' },
                                                { value: 'Hold', label: 'Hold' },
                                                { value: 'Agency Sent', label: 'Agency Sent' },
                                                { value: 'Write Off', label: 'Write Off' },
                                                { value: 'Paid', label: 'Paid' },
                                                { value: 'Bad Debit', label: 'Bad Debit' },
                                                { value: 'Open', label: 'Open' },
                                                { value: 'Close', label: 'Close' },
                                                { value: 'Contra', label: 'Contra' },
                                                { value: 'Breakdown', label: 'Breakdown' }
                                            ]
                                        }
                                        //, cellFilter: 'mapTrack'
                                    },
                                    { field: 'ptpDate', enableCellEdit: false, displayName: 'PTP Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                                    {
                                        field: 'documentType', enableCellEdit: false, displayName: 'Document Type', width: '120'
                                        , filter: {
                                            term: '',
                                            type: uiGridConstants.filter.SELECT,
                                            selectOptions: [
                                                { value: 'DM', label: 'DM' },
                                                { value: 'CM', label: 'CM' },
                                                { value: 'INV', label: 'INV' },
                                                { value: 'Payment', label: 'Payment' }]
                                        }, cellFilter: 'mapClass'
                                    },
                                    {
                                        field: 'comments', name: 'tmp1', displayName: 'Invoice Memo', width: '120',
                                        cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.invoiceId,row.entity.invoiceNum,row.entity.comments)"></a>'
                                        + '<label id="lbl{{row.entity.invoiceId}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum,row.entity.comments,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.comments.substring(0,7)}}...</label></div>'
                                    }
                                ],
                                onRegisterApi: function (gridApi) {
                                    $scope.gridApis[i++] = gridApi;
                                    gridApi.edit.on.afterCellEdit($scope, function (rowEntity, colDef, newValue, oldValue) {
                                        //alert('rowEntity:' + rowEntity.invoiceId + ',colDef:' + colDef.name + ',newValue:' + newValue + ',oldValue:' + oldValue);
                                        if (rowEntity.invoiceId != 0) {
                                            var list = [];
                                            list.push('2');
                                            list.push(rowEntity.invoiceId);
                                            list.push(newValue);
                                            collectorSoaProxy.savecommon(list, function () {
                                                //alert('success!');
                                            })
                                        }
                                    });
                                    gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                                        if (row.entity.invoiceId != 0) {
                                            $scope.invoiceSum(row.entity.customerNum, row.entity.legalEntity);
                                        }
                                    });
                                    gridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
                                        $scope.invoiceSumBatch(rows);
                                    });
                                }
                            };
                    });

                });

                $interval(function () {
                    $scope.selectall();
                    $scope.getAlertStatus();
                    //$scope.getCurrentTracking();
                }, 0, 1);

                // $scope.$broadcast("MAIL_DATAS_REFRESH", $scope.mailDats[0]);
            });

            $scope.editMemoShow = function (invoiceId, invoiceNum, memo) {
                if (invoiceId != 0) {
                    $scope.selectText = memo;
                    var h = document.documentElement.clientHeight;
                    var w = document.documentElement.clientWidth;
                    var content = document.getElementById('boxEdit');
                    var contentWidth = $('#boxEdit').css('width').replace('px', '');
                    var contentHeight = $('#boxEdit').css('height').replace('px', '');
                    var stop = self.pageYOffset;
                    var sleft = self.pageXOffset;
                    var left = w / 2 - contentWidth / 2 + sleft;
                    var top = h / 2 - contentHeight / 2 + stop;
                    $('#boxEdit').css({ 'left': left + 'px', 'top': top + 'px' });
                    $('#txtBox').css({ 'width': contentWidth - 20 + 'px', 'height': contentHeight - 100 + 'px' });
                    var str = '';
                    var str1 = '';
                    str = 'Invoice :"' + invoiceNum + '" Memo : ';
                    str1 = memo;
                    $("#hiddenInvId").val(invoiceId);
                    $("#lblBoxTitle").html(str);
                    $("#boxEdit").show();
                }
            }

            $scope.batcheditMemoShow = function () {
                $scope.inv = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                }
                if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .")
                } else {
                    var h = document.documentElement.clientHeight;
                    var w = document.documentElement.clientWidth;
                    var content = document.getElementById('boxEdit');
                    var contentWidth = $('#boxEdit').css('width').replace('px', '');
                    var contentHeight = $('#boxEdit').css('height').replace('px', '');
                    var stop = self.pageYOffset;
                    var sleft = self.pageXOffset;
                    var left = w / 2 - contentWidth / 2 + sleft;
                    var top = h / 2 - contentHeight / 2 + stop;
                    $('#boxEditBatch').css({ 'left': left + 'px', 'top': top + 'px' });
                    $('#batchtxtBox').css({ 'width': contentWidth - 20 + 'px', 'height': contentHeight - 120 + 'px' });
                    var str = '';
                    str = "All Selected Invoices' Memo Will Be Entirely Updated By Follow:"
                    $("#batchhiddenInvId").val($scope.inv);
                    $("#batchtxtBox").val("");
                    $("#batchlblBoxTitle").html(str);
                    $("#boxEditBatch").show();
                }
            }

            $scope.editMemoSave = function () {
                var list = [];
                var invoiceId = $("#hiddenInvId").val();
                var memo = $("#txtBox").val();
                list.push('2');
                list.push(invoiceId);
                list.push(memo);
                collectorSoaProxy.savecommon(list, function () {
                    $scope.saveBack(invoiceId, memo);
                    $scope.editMemoClose();
                });
            }

            $scope.batcheditMemoSave = function () {
                var list = [];
                var invoiceIds = $("#batchhiddenInvId").val().toString();
                var memo = $("#batchtxtBox").val();
                list.push("5");
                list.push(invoiceIds);
                list.push(memo);
                collectorSoaProxy.savecommon(list, function () {
                    $scope.batchsaveBack(memo);
                    $scope.batcheditMemoClose();
                });
            }

            $scope.saveBack = function (invoiceId, memo) {
                for (k = 0; k < $scope.gridApis.length; k++) {
                    angular.forEach($scope.gridApis[k].grid.rows, function (rowItem) {
                        if (rowItem.entity.invoiceId == invoiceId) {
                            rowItem.entity.comments = memo;
                        }
                    });
                }
            }
            $scope.batchsaveBack = function (memo) {
                for (k = 0; k < $scope.gridApis.length; k++) {
                    angular.forEach($scope.gridApis[k].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            rowItem.comments = memo + '\r\n' + rowItem.comments;
                        }
                    });
                }
            }

            $scope.editMemoClose = function () {
                $("#boxEdit").hide();
            }
            $scope.batcheditMemoClose = function () {
                $("#boxEditBatch").hide();
            }

            $scope.memoShow = function (invNum, memo, e) {
                $('#box').css({ 'left': e.pageX - 410 + 'px', 'top': e.pageY + 10 + 'px' });
                var str = '';
                str = 'Invoice :"' + invNum + '" Memo : <br>' + memo;
                $("#box").html(str);
                $("#box").show();

            }

            $scope.memoHide = function () {
                $("#box").hide();
            }

            $scope.selectall = function () {
                for (k = 0; k < $scope.gridApis.length; k++) {
                    //$scope.gridApis[k].selection.selectAllRows();
                    if ($routeParams.alertType == 2) {
                        angular.forEach($scope.gridApis[k].grid.rows, function (rowItem) {
                            if (rowItem.entity.status === "Open" && rowItem.entity.invoiceTrack === "SOA Sent") {
                                rowItem.isSelected = true;
                            }
                            $scope.invoiceSum(rowItem.entity.customerNum, rowItem.entity.legalEntity);
                        });
                    } else if ($routeParams.alertType == 3) {
                        angular.forEach($scope.gridApis[k].grid.rows, function (rowItem) {
                            if (rowItem.entity.status === "Open" && rowItem.entity.invoiceTrack === "Second Reminder Sent") {
                                rowItem.isSelected = true;
                            }
                            $scope.invoiceSum(rowItem.entity.customerNum, rowItem.entity.legalEntity);
                        });
                    }
                }
            }

            var totalInvoiceAmount = 0;
            //mapping id with name
            $scope.CheckType = function (obj) {
                if (obj.toCc == "1") {
                    return "To";
                } else {
                    return "Cc";
                }
            }
            $scope.CheckGroupLevel = function (obj) {
                if (obj.isGroupLevel == 1) {
                    return "Y";
                } else {
                    return "N";
                }
            }
            ////////////////////////////*********************************
            $scope.check = function () {
                if ($scope.custInfo[1] == "Only select one mail!") {
                    alert($scope.custInfo[1]);
                    return;
                }

                var language = "";
                $scope.inv = [];
                $scope.cus = [];
                $scope.selMailId = $scope.custInfo[1].fileId;
                var blc = 0;
                $scope.shortsub = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);

                            blc += rowItem.standardInvoiceAmount;
                            if ($scope.cus.indexOf(rowItem.customerNum) < 0) {
                                $scope.cus.push(rowItem.customerNum);
                                $scope.shortsub.push(rowItem.customerNum);
                                $scope.shortsub.push(rowItem.customerName.replace('*', '').replace('<', '').replace('>', '').replace('|', '').replace('?', '').replace('%', '').replace('#', '').replace(/\//g, '').replace(/\\/g, ''));
                                //$scope.shortsub.push('($' + $scope.formatNumber(rowItem.standardInvoiceAmount, 2, 1) + ')');
                                blc = rowItem.standardInvoiceAmount;
                            } else {
                                //$scope.shortsub.pop();
                                //$scope.shortsub.push('($' + $scope.formatNumber(blc, 2, 1) + ')');
                            }
                        }
                    });
                }
                totalInvoiceAmount = $scope.formatNumber(blc, 2, 1);

                //alert($scope.inv);
                if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .")
                } else {
                    //alert($scope.selMailId);
                    if (!$scope.custInfo[1].fileId) {
                        if (confirm("No Mail Selected ! Continue ?")) {
                        } else {
                            return;
                        }
                    }
                    $scope.strcus = $scope.cus.join(",");

                    var modalDefaults = {
                        templateUrl: 'app/common/mail/mail-instance.tpl.html',
                        controller: 'mailInstanceCtrl',
                        size: 'customSize',
                        resolve: {
                            custnum: function () { return $scope.strcus; },
                            //                                    selectedInvoiceId: function () { return $scope.inv; },
                            instance: function () {
                                return getMailInstance($scope.strcus);
                            },
                            mType: function () {
                                return "001";
                            },
                            mailDefaults: function () {
                                return {
                                    mailType: 'NE',
                                    mailUrl: generateSOAProxy.sendEmailUrl,
                                    templateChoosenCallBack: selectMailInstanceById
                                };
                            }
                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "sent") {
                            $scope.reSeachContactList();
                            $scope.refreshTrack();
                        }
                    }, function (err) {
                        alert(err);
                    });

                }
            }

            //############################# Get MailInstance ############################# start
            var getMailInstanceMain = function (custNums, ids) {

                var instanceDefered = $q.defer();
                var reminderDay = '';
                if ($scope.alertType == "2") {
                    reminderDay = $scope.reminder3thDate;
                } else if ($scope.alertType == "3") {
                    reminderDay = $scope.holdDate;
                }

                dunningProxy.getDunningMailInstance(custNums, totalInvoiceAmount, reminderDay, $scope.alertType, ids, function (res) {
                    var instance = res;
                    var messageIdStr = $scope.custInfo[1].messageId;
                    if (messageIdStr != undefined && messageIdStr != null) {
                        //added by zhangYu 2016-01-18 attach soa body Start
                        mailProxy.queryObject({ messageId: $scope.custInfo[1].messageId }, function (mailInstance) {
                            instance.body = instance.body + "<hr width=100% size=1>" + mailInstance.body
                        });
                    }
                    //added by zhangYu 2016-01-18 attach soa body End
                    renderInstance(instance, custNums);
                    instanceDefered.resolve(instance);
                }, function (error) {
                    alert(error);
                });

                return instanceDefered.promise;
            };

            var getMailInstanceTo = function (custNums) {

                var toDefered = $q.defer();
                //TO
                contactProxy.query({
                    customerNums: custNums
                }, function (contactor) {
                    //2016-01-14 start
                    var to_cc = {};
                    to_cc.to = new Array();
                    to_cc.cc = new Array();
                    //var cons = new Array();
                    //var contName = '';
                    //2016-01-14 end

                    angular.forEach(contactor, function (item) {
                        //2016-01-14 start
                        if (item.toCc == "2") {
                            if (to_cc.cc.indexOf(item.emailAddress) < 0) {
                                to_cc.cc.push(item.emailAddress);
                            }
                        }
                        else if (item.toCc == "1") {
                            if (to_cc.to.indexOf(item.emailAddress) < 0) {
                                to_cc.to.push(item.emailAddress);
                            }
                        }
                        //2016-01-14 End

                        //if (cons.indexOf(item.emailAddress) < 0) {
                        //    cons.push(item.emailAddress);
                        //    //contName = item.name + ',';
                        //}
                    });

                    //var greeting = '<p>Dear ' + contName.substring(1, contName.length - 1) + '</p>';
                    //Mailinstance.body = greeting + Mailinstance.body;
                    //toDefered.resolve(cons.join(";"));
                    toDefered.resolve(to_cc);
                });
                return toDefered.promise;
            };

            //=========added by alex body中显示附件名+Currency==============

            //var getMailInstanceAttachment = function (custNums) {
            //    var attachDefered = $q.defer();
            //    var attach = {};

            //    //mail Attachment
            //    generateSOAProxy.geneateSoaByIds(custNums, $scope.alertType, function (res) {
            //        attach.attachments = res;
            //        attach.attachment = attach.attachments.join(",");
            //        if ($scope.custInfo[7]) {
            //            attach.attachments.push($scope.custInfo[7]);
            //            attach.attachment += "," + $scope.custInfo[7];
            //        }
            //        attachDefered.resolve(attach);
            //    });

            //    return attachDefered.promise;
            //};

            //=============================================================

            var getMailInstance = function (custNums) {
                var instance = {};
                var allDefered = $q.defer();

                $q.all([
                    getMailInstanceMain(custNums, $scope.inv),
                    getMailInstanceTo(custNums)
                    //=========added by alex body中显示附件名+Currency======
                    //getMailInstanceAttachment($scope.inv)
                    //=====================================================
                ])
                    .then(function (results) {
                        instance = results[0];
                        //2016-01-14 start
                        //instance.to = results[1];
                        instance.to = "";
                        instance.cc = "";
                        instance.to = results[1].to.join(";");
                        instance.cc = results[1].cc.join(";");
                        //2016-01-14 End

                        //=========added by alex body中显示附件名+Currency======
                        //instance.attachments = results[2].attachments;
                        //instance.attachment = results[2].attachment;
                        //=====================================================
                        allDefered.resolve(instance);
                    });

                return allDefered.promise;
            };

            var selectMailInstanceById = function (custNums, id) {
                var instance = {};
                var instanceDefered = $q.defer();

                var reminderDay = '';
                if ($scope.alertType == "2") {
                    reminderDay = $scope.reminder3thDate;
                } else if ($scope.alertType == "3") {
                    reminderDay = $scope.holdDate;
                }
                //=========added by alex body中显示附件名+Currency=== $scope.inv 追加 ======
                dunningProxy.getDunningMailInstanceById(custNums, totalInvoiceAmount, reminderDay, $scope.alertType, id, $scope.inv, function (res) {
                    instance = res;
                    renderInstance(instance, custNums);

                    instanceDefered.resolve(instance);
                });

                return instanceDefered.promise;
            };

            var renderInstance = function (instance, custNums) {
                //subject
                if ($scope.alertType == 2) {
                    instance.subject = '2nd Reminder-' + $scope.shortsub.join('-');
                } else if ($scope.alertType == 3) {
                    instance.subject = 'Final Reminder-' + $scope.shortsub.join('-');
                }
                //invoiceIds
                instance.invoiceIds = $scope.inv;
                //soaFlg
                instance.soaFlg = "1";
                //Bussiness_Reference
                var customerMails = [];
                angular.forEach(custNums.split(','), function (cust) {
                    customerMails.push({ MessageId: instance.messageId, CustomerNum: cust });
                });
                instance.CustomerMails = customerMails; //$routeParams.nums;
                //mailTitle
                instance["title"] = "Dunning Reminder";
                //mailType
                if ($scope.alertType == 2) {
                    instance.mailType = "002,Second Reminder Sent";
                } else if ($scope.alertType == 3) {
                    instance.mailType = "003,Final Reminder Sent";
                }
            };
            //############################# Get MailInstance ############################# end

            //$scope.isshow = true;
            $scope.show = function (id) {
                $("#" + id).removeClass('ng-hide');
                $("#ishide" + id).show();
                $("#isshow" + id).hide();
            }

            $scope.hide = function (id) {

                $("#" + id).addClass('ng-hide');
                $("#isshow" + id).show();
                $("#ishide" + id).hide();
            }

            $scope.openInvoiceH = function (inNum) {
                var modalDefaults = {
                    templateUrl: 'app/soa/invhistory/invhistory.tpl.html',
                    controller: 'invHisCL',
                    size: 'lg',
                    resolve: {
                        inNum: function () { return inNum; }
                    }
                    , windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {

                });

            }

            $scope.refreshTrack = function () {
                var str = "";
                if ($routeParams.alertType == 2) {
                    str = "Second Reminder Sent";
                } else if ($routeParams.alertType == 3) {
                    str = "Final Reminder Sent";
                }
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            rowItem.invoiceTrack = str;
                        }
                    });
                }
            }
            //After ckick [ptp][payment][disput] button,refresh the value [invoiceStatus][TrackStatus][memo] to  invoiceList
            $scope.refreshListVal = function (invStatus, trackStatus, memo, ptpDate) {

                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            rowItem.status = invStatus;
                            rowItem.invoiceTrack = trackStatus;
                            if (memo != null) { rowItem.comments = memo + rowItem.comments; }
                            if (ptpDate != null) { rowItem.ptpDate = ptpDate; }
                        }
                    });
                }
            }

            $scope.changeWF = function (type) {

                dunningProxy.wfchange($routeParams.alertId, type, $routeParams.alertType, function (result) {
                    if (type == "pause" || type == "resume") {
                        $scope.getAlertStatus();
                    } else if (type == "finish" || type == "cancel" || type == "restart") {
                        window.close();
                    }
                });
            }

            $scope.getAlertStatus = function () {

                dunningProxy.queryObject({ AlertId: $routeParams.alertId }, function (re) {
                    if (re.status == "Initialized" || re.status == "Processing" || re.status == "Resume") {

                        $("#asend").removeClass("ng-hide");
                        $("#apause").removeClass("ng-hide");
                        $("#aresume").addClass("ng-hide");
                        $("#afinish").removeClass("ng-hide");
                        $("#acancel").removeClass("ng-hide");
                        $("#arestart").addClass("ng-hide");
                    }
                    else if (re.status == null || re.status == "Finish") {

                        $("#asend").addClass("ng-hide");
                        $("#apause").addClass("ng-hide");
                        $("#aresume").addClass("ng-hide");
                        $("#afinish").addClass("ng-hide");
                        $("#acancel").addClass("ng-hide");
                        $("#arestart").removeClass("ng-hide");
                    }
                    else if (re.status == "Pause") {

                        $("#asend").addClass("ng-hide");
                        $("#apause").addClass("ng-hide");
                        $("#aresume").removeClass("ng-hide");
                        $("#afinish").addClass("ng-hide");
                        $("#acancel").addClass("ng-hide");
                        $("#arestart").addClass("ng-hide");
                    }
                });

            }
            //############################### Dunning Config ####################### s
            $scope.finter = "";
            $scope.sinter = "";
            $scope.ptat = "";
            $scope.rinter = "";
            $scope.dcdes = "";
            $scope.currentDate = "";
            $scope.soaDate = "";
            $scope.reminder2thDate = "";
            $scope.reminder3thDate = "";
            $scope.holdDate = "";
            $scope.closeDate = "";
            //$scope.getCurrentTracking = function () {
            //$scope.changeColor = function (b) {
            //    var s = b;
            //    return {"backgroud-color":'red'}
            //}
            dunningProxy.queryObject({ AlertIdForCT: $routeParams.alertId }, function (re) {
                //        $scope.soaDate = re.soaDate;
                //alert(re.reminder2thDate);
                //alert(re.reminder2thDate.toString().substring(0, 10));
                if ($routeParams.alertType == 2) {
                    $scope.reminder2thDate = re.tempDate.toString().substring(0, 10);
                    $scope.reminder3thDate = re.reminder3thDate.toString().substring(0, 10);
                } else if ($routeParams.alertType == 3) {
                    $scope.reminder2thDate = re.reminder2thDate.toString().substring(0, 10);
                    $scope.reminder3thDate = re.tempDate.toString().substring(0, 10);
                }
                //$scope.reminder2thDate = re.reminder2thDate.toString().substring(0,10);
                //$scope.reminder3thDate = re.reminder3thDate.toString().substring(0, 10);
                $scope.holdDate = re.holdDate.toString().substring(0, 10);
                //        $scope.closeDate = re.closeDate;
                $scope.currentDate = re.currentDate.toString().substring(0, 10);
                //        $scope.finter = re.firstInterval;
                //        $scope.sinter = re.secondInterval;
                //        $scope.ptat = re.paymentTat;
                //        $scope.rinter = re.riskInterval;
                //        $scope.dcdes = re.desc;
                //if (re.soaStatus == 1) {
                //    $('#lbl' + re.soaId).css({ 'background': '#ddffe1' });
                //    //    document.getElementById("lbl" + re.soaId).style.color = "#FF0000";
                //} else if (re.soaStatus == 0) {
                //    $('#lbl' + re.soaId).css({ 'color': '#FF0000' });
                //} else {
                //    $('#lbl' + re.soaId).css({ 'color': '#3e4244' });
                //}

                //if (re.reminder2thStatus == 1) {
                //    $('#lbl' + re.r2Id).css({ 'background': '#ddffe1' });
                //} else if (re.reminder2thStatus == 0) {
                //    $('#lbl' + re.r2Id).css({ 'color': '#FF0000' });
                //} else {
                //    $('#lbl' + re.r2Id).css({ 'color': '#3e4244' });
                //}

                //if (re.reminder3thStatus == 1) {
                //    $('#lbl' + re.r3Id).css({ 'background': '#ddffe1' });
                //} else if (re.reminder3thStatus == 0) {
                //    $('#lbl' + re.r3Id).css({ 'color': '#FF0000' });
                //} else {
                //    $('#lbl' + re.r3Id).css({ 'color': '#3e4244' });
                //}

                //if (re.holdStatus == 1) {
                //    $('#lbl' + re.holdId).css({ 'background': '#ddffe1' });
                //} else if (re.holdStatus == 0) {
                //    $('#lbl' + re.holdId).css({ 'color': '#FF0000' });
                //} else {
                //    $('#lbl' + re.holdId).css({ 'color': '#3e4244' });
                //}
                // if (re.soaStatus == 1) { $scope.changeStyle = { color: 'red' }; }

            })

            //}
            //############################### Dunning Config ####################### e

            $scope.conshow = function (id) {
                $scope.conallhide();

                $("#conhide" + id).show();
                $("#conshow" + id).hide();

                //############ Add by Alex #####
                $("#pbhide" + id).hide();
                $("#pbshow" + id).show();
                $("#pchide" + id).hide();
                $("#pcshow" + id).show();
                $("#cdgshow" + id).show();
                $("#cdghide" + id).hide();
                //contact add button show or hide
                $scope.isContactAddBtnShow = true;
                //payment bank  add button show or hide
                $scope.isPayBankAddBtnShow = false;
                $scope.isDomainAddBtnShow = false;
                //##############################

                collectorSoaProxy.query({ CustNumFCon: id }, function (list) {
                    $scope.currCustNum = id;
                    angular.forEach($scope.sendsoa, function (row) {
                        if (row.customerCode == id) {
                            row['cg'].data = list;
                            row['cgshow'] = true;
                            //##### Add by Alex ######
                            row['pbgshow'] = false;
                            row['pcgshow'] = false;
                            row['cdgshow'] = false;
                            //########################
                        }
                    });
                });
            }

            $scope.conhide = function (id) {
                $("#conshow" + id).show();
                $("#conhide" + id).hide();
                angular.forEach($scope.sendsoa, function (row) {
                    if (row.customerCode == id) {
                        row['cgshow'] = false;
                    }
                });
            }

            $scope.pbshow = function (id) {
                $("#pbhide" + id).show();
                $("#pbshow" + id).hide();

                //############ Add by Alex #####
                $("#conshow" + id).show();
                $("#conhide" + id).hide();
                $("#pcshow" + id).show();
                $("#pchide" + id).hide();
                $("#cdgshow" + id).show();
                $("#cdghide" + id).hide();
                //contact add button show or hide
                $scope.isContactAddBtnShow = false;
                //payment bank  add button show or hide
                $scope.isPayBankAddBtnShow = true;
                $scope.isDomainAddBtnShow = false;
                //##############################

                collectorSoaProxy.query({ CustNumFPb: id }, function (list) {
                    angular.forEach($scope.sendsoa, function (row) {
                        if (row.customerCode == id) {
                            row['pbg'].data = list;
                            row['pbgshow'] = true;
                            //##### Add by Alex ######
                            row['cgshow'] = false;
                            row['pcgshow'] = false;
                            row['cdgshow'] = false;
                            //########################
                        }
                    });
                });
            }

            $scope.pbhide = function (id) {
                $("#pbshow" + id).show();
                $("#pbhide" + id).hide();
                angular.forEach($scope.sendsoa, function (row) {
                    if (row.customerCode == id) {
                        row['pbgshow'] = false;
                    }
                });
            }

            $scope.pcshow = function (id) {
                $scope.pcallhide();
                $("#pc" + id).removeClass('ng-hide');
                $("#pchide" + id).show();
                $("#pcshow" + id).hide();

                //############ Add by Alex #####
                $("#conshow" + id).show();
                $("#conhide" + id).hide();
                $("#pbshow" + id).show();
                $("#pbhide" + id).hide();
                $("#cdgshow" + id).show();
                $("#cdghide" + id).hide();
                //contact add button show or hide
                $scope.isContactAddBtnShow = false;
                //payment bank  add button show or hide
                $scope.isPayBankAddBtnShow = false;
                $scope.isDomainAddBtnShow = false;
                //##############################

                collectorSoaProxy.query({ CustNumFPc: id }, function (list) {
                    angular.forEach($scope.sendsoa, function (row) {
                        if (row.customerCode == id) {
                            row['pcg'].data = list;
                            row['pcgshow'] = true;
                            //##### Add by Alex ######
                            row['cgshow'] = false;
                            row['pbgshow'] = false;
                            row['cdgshow'] = false;
                            //########################
                        }
                    });
                });
                $scope.initUpload();
            }

            $scope.pchide = function (id) {
                $("#pcshow" + id).show();
                $("#pchide" + id).hide();
                angular.forEach($scope.sendsoa, function (row) {
                    if (row.customerCode == id) {
                        row['pcgshow'] = false;
                    }
                });
                $scope.entityFlg = "";
            }

            $scope.pcallhide = function () {
                angular.forEach($scope.sendsoa, function (row) {
                    $("#pcshow" + row.customerCode).show();
                    $("#pchide" + row.customerCode).hide();
                    row['pcgshow'] = false;
                });
                $scope.entityFlg = "";
            }

            $scope.conallhide = function () {
                angular.forEach($scope.sendsoa, function (row) {
                    $scope.conhide(row.customerCode);
                });
            }

            $scope.cdgshow = function (id) {
                $("#cdgshow" + id).hide();
                $("#cdghide" + id).show();
                //############ Add by Alex #####
                $("#conhide" + id).hide();
                $("#conshow" + id).show();
                $("#pbhide" + id).hide();
                $("#pbshow" + id).show();
                $("#pchide" + id).hide();
                $("#pcshow" + id).show();

                //contact add button show or hide
                $scope.isContactAddBtnShow = false;
                //payment bank  add button show or hide
                $scope.isPayBankAddBtnShow = false;
                $scope.isDomainAddBtnShow = true;
                //##############################

                collectorSoaProxy.query({ CustNumFPd: id }, function (list) {
                    angular.forEach($scope.sendsoa, function (row) {
                        if (row.customerCode == id) {
                            row['cdg'].data = list;
                            row['cdgshow'] = true;
                            //##### Add by Alex ######
                            row['pbgshow'] = false;
                            row['pcgshow'] = false;
                            row['cgshow'] = false;
                            //########################
                        }
                    });
                });
            }

            $scope.cdghide = function (id) {
                $("#cdgshow" + id).show();
                $("#cdghide" + id).hide();
                angular.forEach($scope.sendsoa, function (row) {
                    if (row.customerCode == id) {
                        row['cdgshow'] = false;
                    }
                });
            }

            //**********************************contact History********************
            $scope.reSeachContactList = function () {
                angular.forEach($scope.num, function (cus) {
                    collectorSoaProxy.query({ CustNumsFCH: cus }, function (list) {
                        angular.forEach($scope.sendsoa, function (row) {
                            if (row.customerCode == cus) {
                                row['hisg'].data = list
                            }
                        });
                        //$scope.contactList.data = list;
                    });
                })
            }
            //collectorSoaProxy.query({ CustNumsFCH: $routeParams.nums }, function (list) {
            //    angular.forEach($scope.sendsoa, function (row) {
            //        if (row.customerCode == $routeParams.nums) {
            //            row['hisg'].data = list
            //        }
            //    });
            //})

            //**********************************edit special notes********************
            $scope.saveNote = function (cus, legal, note) {
                var list = [];
                list.push(1);//1:SpeicalNotes;2:InvoiceComm
                list.push(cus);
                list.push(legal);
                list.push(note);
                collectorSoaProxy.savecommon(list, function () {
                    alert('success!');
                })
            }

            //**********************************add/edit/del contactors paymentbank paycircle********************
            //##############  add  ######################
            $scope.addContactor = function (cus, deal) {
                //var num = $scope.cust.customerNum;
                //customerProxy.queryObject({ num: cus }, function (entity) {
                //var deal = entity.deal;
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                    controller: 'contactorEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return new contactProxy();
                        },
                        num: function () {
                            return cus;
                        },
                        site: function () {
                            return deal;
                        },
                        langs: function () {
                            return $scope.languagelist;
                        },
                        isHoldStatus: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.conshow(cus);
                });
                //})
            }

            $scope.addPayBank = function (cus) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/paymentbank/paymentbank-edit.tpl.html',
                    controller: 'paymentbankEditCtrl',
                    size: 'lg',
                    resolve: {
                        custInfo: function () {
                            return customerPaymentbankProxy.queryObject({ type: "new" });
                        }, num: function () {
                            return cus;
                        }, flg: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.pbshow(cus);
                });
            }

            $scope.addPayBankCircle = function (cus, paymentCircle) {
                if (paymentCircle == "" || paymentCircle == null) {
                    alert("Please select Payment Date!");
                }
                else {
                    if ($scope.entityFlg == null || $scope.entityFlg == "") {
                        alert("Please Select Legal Entity First!");
                    } else {
                        var num = cus;
                        var paymentCircleArray = [];
                        paymentCircleArray.push(paymentCircle);
                        paymentCircleArray.push(num);
                        paymentCircleArray.push($scope.entityFlg);
                        customerPaymentcircleProxy.addPaymentCircle(paymentCircleArray, function (res) {
                            //collectorSoaProxy.query({ CustNumFPc: cus }, function (list) {
                            customerPaymentcircleProxy.searchPaymentCircle(cus, $scope.entityFlg, function (paydate) {
                                angular.forEach($scope.sendsoa, function (subrow) {
                                    if (subrow.customerCode == cus) {
                                        subrow['pcg'].data = paydate;
                                        subrow['pcgshow'] = true;
                                    }
                                });
                            });
                            alert(res);
                        }, function (res) {
                            alert(res);
                        });
                    }
                }
            }
            $scope.addContactDomain = function (cus) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customerdomain/custdomain-edit.tpl.html',
                    controller: 'custdomainEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return new contactProxy();
                        },
                        num: function () {
                            return cus;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.cdgshow(cus);
                });
            };
            //##############  edit  ######################
            $scope.EditContacterInfo = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                    controller: 'contactorEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        num: function () {
                            // return row.customerNum;
                            return $scope.currCustNum;
                        },
                        site: function () {
                            return row.deal;
                        },
                        langs: function () {
                            return $scope.languagelist;
                        },
                        isHoldStatus: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.conshow($scope.currCustNum);
                    //$scope.conshow(row.bkcustomerNum);
                });
            };

            $scope.EditBankInfo = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/paymentbank/paymentbank-edit.tpl.html',
                    controller: 'paymentbankEditCtrl',
                    size: 'lg',
                    resolve: {
                        custInfo: function () {
                            return row;
                        }, num: function () {
                            return row.customerNum;
                        }, flg: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.pbshow(row.customerNum);
                });
            };
            $scope.EditCustDomain = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customerdomain/custdomain-edit.tpl.html',
                    controller: 'custdomainEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        num: function () {
                            return row.customerNum;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.cdgshow(row.customerNum);
                });
            };
            //##############  remove  ######################
            $scope.Delcontacter = function (row) {
                var cusid = row.id;
                contactProxy.delContactor(cusid, function () {
                    // $scope.conshow(row.customerNum);
                    //     var cus = row.bkCustomerNum;
                    $scope.conshow($scope.currCustNum);
                    alert("Delete Success");
                }, function () {
                    alert("Delete Error");
                });
            };

            $scope.DelBankInfo = function (row) {
                var cusid = row.id;
                customerPaymentbankProxy.delPaymentBank(cusid, function () {
                    $scope.pbshow(row.customerNum);
                    alert("Delete Success");
                }, function () {
                    alert("Delete Error");
                });
            };

            $scope.DelPaymentcircle = function (row) {
                var cusid = row.id;
                customerPaymentcircleProxy.delPaymentCircle(cusid, function () {
                    var cus = row.customerNum;
                    //collectorSoaProxy.query({ CustNumFPc: cus }, function (list) {
                    customerPaymentcircleProxy.searchPaymentCircle(cus, $scope.entityFlg, function (paydate) {
                        angular.forEach($scope.sendsoa, function (subrow) {
                            if (subrow.customerCode == cus) {
                                subrow['pcg'].data = paydate;
                                subrow['pcgshow'] = true;
                            }
                        });
                    });
                    alert("Delete Success");
                }, function () {
                    alert("Delete Error");
                });
            };
            $scope.DelCustDomain = function (row) {
                var cusid = row.id;
                contactProxy.delCustDomain(cusid, function (res) {
                    alert("Delete Success");
                    $scope.cdgshow(row.customerNum);
                    alert("Delete Success");
                }, function (err) {
                    alert("Delete Error");
                });
            }

            //##############  upload  ######################

            //初始化uploader
            var uploader = $scope.uploader = new FileUploader();

            $scope.initUpload = function () {
                uploader = $scope.uploader = new FileUploader({
                    url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle'
                });

                uploader.filters.push({
                    name: 'customFilter',
                    fn: function (item /*{File|FileLikeObject}*/, options) {
                        return;
                    }
                });
            }

            $scope.addFileCircle = function (cus) {
                if (uploader.queue[2] == null) {
                    alert("Please select File");
                } else {
                    if ($scope.entityFlg == null || $scope.entityFlg == "") {
                        alert("Please Select Legal Entity First!");
                    } else {
                        if ((uploader.queue[2]._file.name.toString().toUpperCase().split(".").length > 1)
                            && uploader.queue[2]._file.name.toString().toUpperCase().split(".")[1] != "CSV") {
                            alert("File format is not correct !");
                            return;
                        }
                        var num = cus;
                        var legal = $scope.entityFlg;
                        uploader.queue[2].url = APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle?customerNum=' + num + '&legal=' + legal;
                        uploader.uploadAll();
                    }
                    // CALLBACKS
                    uploader.onSuccessItem = function (fileItem, response, status, headers) {
                        alert(response);
                        //collectorSoaProxy.query({ CustNumFPc: cus }, function (list) {
                        customerPaymentcircleProxy.searchPaymentCircle(cus, $scope.entityFlg, function (paydate) {
                            angular.forEach($scope.sendsoa, function (row) {
                                if (row.customerCode == cus) {
                                    row['pcg'].data = paydate;
                                    row['pcgshow'] = true;
                                }
                            })
                        })
                        //});
                    };
                    uploader.onErrorItem = function (fileItem, response, status, headers) {
                        alert(response);
                    };
                }
            }
            $scope.changeLegal = function (custNum, legal) {
                customerPaymentcircleProxy.searchPaymentCircle(custNum, legal, function (paydate) {
                    angular.forEach($scope.sendsoa, function (row) {
                        if (row.customerCode == custNum) {
                            row['pcg'].data = paydate;
                            row['pcgshow'] = true;
                        }
                    });
                    $scope.entityFlg = legal;
                    uploader.queue[2] = null;
                    document.getElementById("uploadCalendar").value = "";
                },
                    function () {
                    });
            }
            //********************contactList********************<a> ng-click********************start
            //contactList Detail
            $scope.getDetail = function (row) {
                if (row.contactType != null) {
                    if (row.contactType == "Mail") {
                        if (row.contactId) {
                            mailProxy.queryObject({ messageId: row.contactId }, function (mailInstance) {
                                //mailType
                                mailInstance["title"] = "Mail View";
                                mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);

                                var modalDefaults = {
                                    templateUrl: 'app/common/mail/mail-instance.tpl.html',
                                    controller: 'mailInstanceCtrl',
                                    size: 'customSize',
                                    resolve: {
                                        custnum: function () { return mailInstance.customerNum; },
                                        instance: function () { return mailInstance },
                                        mailDefaults: function () {
                                            return {
                                                mailType: 'VI'
                                            };
                                        }
                                    },
                                    windowClass: 'modalDialog'
                                };
                                modalService.showModal(modalDefaults, {}).then(function (result) {

                                });
                            }); //mailProxy
                        } else { alert('contactId is null') }
                    } //if mail end
                    else if (row.contactType == "Call") {

                        if (row.contactId) {
                            contactCustomerProxy.queryObject({ contactId: row.contactId }, function (callInstance) {
                                callInstance["contacterId"] = row.contacterId;
                                callInstance["title"] = "Call Detail";
                                var modalDefaults = {
                                    templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                                    controller: 'contactCallCtrl',
                                    size: 'lg',
                                    resolve: {
                                        callInstance: function () { return callInstance; },
                                        custnum: function () { return ""; },
                                        invoiceIds: function () { return ""; }
                                    },
                                    windowClass: 'modalDialog'
                                };
                                modalService.showModal(modalDefaults, {}).then(function (result) {
                                    $scope.reSeachContactList();
                                });
                            }); //contactCustomerProxy
                        } //if contactId
                        else { alert("contact Id is null"); }
                    } //if call end
                    else { alert("not map the contact type"); }
                }
            } //contactList Detail end


            //********************contactList******************<a> ng-click********************end

            $scope.changetab = function (type) {
                //get selected invoiceIds
                $scope.inv = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                }
                if (type == "call") {
                    if ($scope.inv == "" || $scope.inv == null) {
                        if (confirm("No Invoice Selected ! Continue ?")) {
                        }
                        else {
                            return;
                        }
                    }

                    if ($scope.custInfo[1] == "Only select one mail!") {
                        alert($scope.custInfo[1]);
                        return;
                    }

                    contactCustomerProxy.queryObject({ contactId: '0' }, function (callInstance) {
                        callInstance["title"] = "Call Create";
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                            controller: 'contactCallCtrl',
                            size: 'lg',
                            resolve: {
                                callInstance: function () { return callInstance; },
                                custnum: function () { return $routeParams.referenceNo; },
                                invoiceIds: function () { return $scope.inv; }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            $scope.reSeachContactList();
                        });
                    }); //contactCustomerProxy
                }
                else if (type == "ptp") {
                    $scope.inv = [];
                    for (j = 0; j < $scope.gridApis.length; j++) {
                        angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                            //alert(rowItem.invoiceNum);
                            if (rowItem.invoiceId != 0) {
                                $scope.inv.push(rowItem.invoiceId);
                            }
                        });
                    }

                    //alert($scope.inv);
                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }

                    var relatedMail = "";

                    if ($scope.custInfo[1] == "Only select one mail!") {
                        alert($scope.custInfo[1]);
                        return;
                    }

                    if (!$scope.custInfo[1].id) {
                        if (confirm("No mail selected ! Continue ?")) {
                        }
                        else {
                            return;
                        }
                    } else {
                        if ($scope.custInfo[1].createTime == null || $scope.custInfo[1].createTime == "") {
                            relatedMail = $scope.custInfo[1].from + "  " + $scope.custInfo[1].subject + " " + $scope.custInfo[1].createTime;
                        } else {
                            relatedMail = $scope.custInfo[1].from + "  " + $scope.custInfo[1].subject + " " + $scope.custInfo[1].createTime.replace("T", " ");
                        }
                    }

                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-ptp.tpl.html',
                        controller: 'contactPtpCtrl',
                        size: 'lg',
                        resolve: {
                            custnum: function () {
                                return $routeParams.nums;
                            },
                            invoiceIds: function () {
                                return $scope.inv;
                            },
                            contactId: function () {
                                return $scope.custInfo[1].messageId;
                            },
                            relatedEmail: function () {
                                return relatedMail;
                            },
                            contactPerson: function () {
                                return $scope.custInfo[1].to;
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] == "submit") {
                            $scope.refreshListVal("PTP", "PTP", result[1], result[2]);
                        }
                    });
                }
                else if (type == "notice") {
                    $scope.inv = [];
                    for (j = 0; j < $scope.gridApis.length; j++) {
                        angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                            //alert(rowItem.invoiceNum);
                            if (rowItem.invoiceId != 0) {
                                $scope.inv.push(rowItem.invoiceId);
                            }
                        });
                    }
                    //alert($scope.inv);
                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }

                    var relatedMail = "";

                    if ($scope.custInfo[1] == "Only select one mail!") {
                        alert($scope.custInfo[1]);
                        return;
                    }

                    if (!$scope.custInfo[1].id) {
                        if (confirm("No mail selected ! Continue ?")) {
                        }
                        else {
                            return;
                        }
                    } else {
                        if ($scope.custInfo[1].createTime == null || $scope.custInfo[1].createTime == "") {
                            relatedMail = $scope.custInfo[1].from + "  " + $scope.custInfo[1].subject + " " + $scope.custInfo[1].createTime;
                        } else {
                            relatedMail = $scope.custInfo[1].from + "  " + $scope.custInfo[1].subject + " " + $scope.custInfo[1].createTime.replace("T", " ");
                        }
                    }

                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-notice.tpl.html',
                        controller: 'contactNoticeCtrl',
                        size: 'lg',
                        resolve: {
                            custnum: function () {
                                return $routeParams.nums;
                            },
                            invoiceIds: function () {
                                return $scope.inv;
                            },
                            contactId: function () {
                                return $scope.custInfo[1].messageId;
                            },
                            relatedEmail: function () {
                                return relatedMail;
                            },
                            contactPerson: function () {
                                return $scope.custInfo[1].to;
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] == "submit") {
                            $scope.refreshListVal("Payment", result[1], result[2], null);
                        }
                    });
                }//payment notice           
            }

            //****format number
            /* 
                将数值四舍五入后格式化. 
                @param num 数值(Number或者String) 
                @param cent 要保留的小数位(Number) 
                @param isThousand 是否需要千分位 0:不需要,1:需要(数值类型); 
                @return 格式的字符串,如'1,234,567.45' 
                @type String 
            */
            $scope.formatNumber =
                function (num, cent, isThousand) {
                    num = num.toString().replace(/\$|\,/g, '');
                    if (isNaN(num))//检查传入数值为数值类型. 
                        num = "0";
                    if (isNaN(cent))//确保传入小数位为数值型数值. 
                        cent = 0;
                    cent = parseInt(cent);
                    cent = Math.abs(cent);//求出小数位数,确保为正整数. 
                    if (isNaN(isThousand))//确保传入是否需要千分位为数值类型. 
                        isThousand = 0;
                    isThousand = parseInt(isThousand);
                    if (isThousand < 0)
                        isThousand = 0;
                    if (isThousand >= 1) //确保传入的数值只为0或1 
                        isThousand = 1;
                    sign = (num == (num = Math.abs(num)));//获取符号(正/负数) 
                    //Math.floor:返回小于等于其数值参数的最大整数 
                    num = Math.floor(num * Math.pow(10, cent) + 0.50000000001);//把指定的小数位先转换成整数.多余的小数位四舍五入. 
                    cents = num % Math.pow(10, cent); //求出小数位数值. 
                    num = Math.floor(num / Math.pow(10, cent)).toString();//求出整数位数值. 
                    cents = cents.toString();//把小数位转换成字符串,以便求小数位长度. 
                    while (cents.length < cent) {//补足小数位到指定的位数. 
                        cents = "0" + cents;
                    }
                    if (isThousand == 0) //不需要千分位符. 
                        return (((sign) ? '' : '-') + num + '.' + cents);
                    //对整数部分进行千分位格式化. 
                    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
                        num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                            num.substring(num.length - (4 * i + 3));
                    return (((sign) ? '' : '-') + num + '.' + cents);
                }

            $scope.saveDunningConfig = function (f, s, p, r, d) {
                if ($scope.checkNumber(f) == 1 || $scope.checkNumber(s) == 1
                    || $scope.checkNumber(p) == 1 || $scope.checkNumber(r) == 1) {
                    alert("There is out of range number ,Please check it out.");
                } else {
                    var list = [];
                    list.push(f.toString());
                    list.push(s.toString());
                    list.push(p.toString());
                    list.push(r.toString());
                    list.push(d);
                    dunningProxy.saveDunConfigBySingle($routeParams.alertId, list, function () {
                        $scope.saveflg = 1;
                        alert("Save Success");
                    }, function () {
                        alert("Save Error");
                    });
                }
            }

            $scope.checkNumber = function (val) {
                if (!/^([1-9]\d|\d)$/.test(val)) {
                    return 1;
                } else {
                    return 0;
                }
            }

            $scope.checkNumberT = function (val, t) {
                if (!/^([1-9]\d|\d)$/.test(val)) {
                    if (t == 1) {
                        this.subl.subTracking.firstInterval = 1;
                    } else if (t == 2) {
                        this.subl.subTracking.secondInterval = 1;
                    } else if (t == 3) {
                        this.subl.subTracking.paymentTat = 1;
                    } else {
                        this.subl.subTracking.riskInterval = 1;
                    }
                }
            }

            $scope.editDunDateShow = function (type, trackId) {
                if (type == $routeParams.alertType) {
                    alert("Could Not Edit Current Task Date,Please.");
                } else {
                    $scope.rEditTitle = "";
                    $scope.editDunDate = "";
                    if (type == 2) {
                        $scope.rEditTitle = "Seconde Reminder";
                        $scope.editDunDate = this.subl.subTracking.reminder2thDate.toString().substring(0, 10);
                    } else if (type == 3) {
                        $scope.rEditTitle = "Final Reminder";
                        $scope.editDunDate = this.subl.subTracking.reminder3thDate.toString().substring(0, 10);
                    } else if (type == 4) {
                        $scope.rEditTitle = "Hold";
                        $scope.editDunDate = this.subl.subTracking.holdDate.toString().substring(0, 10);
                    }
                    var h = document.documentElement.clientHeight;
                    var w = document.documentElement.clientWidth;
                    var contentWidth = $('#boxEdit').css('width').replace('px', '');
                    var contentHeight = $('#boxEdit').css('height').replace('px', '');
                    var stop = self.pageYOffset;
                    var sleft = self.pageXOffset;
                    var left = w / 2 - contentWidth / 2 + sleft;
                    var top = h / 2 - contentHeight / 2 + stop;
                    $('#REdit').css({ 'left': left + 'px', 'top': top + 'px' });
                    $("#hiddenTrackId").val(trackId);
                    $("#REdit").show();
                }
            }

            $scope.editDunDateSave = function () {
                dunningProxy.saveActionDate($("#hiddenTrackId").val(), $scope.editDunDate, function () {
                    $("#REdit").hide();
                    $("#" + $("#hiddenTrackId").val()).html($scope.editDunDate);
                    //this.
                }, function () {
                    alert("Save Error");
                })
            }

            $scope.editDunDateClose = function () {
                $("#REdit").hide();
            }
            $scope.dateChange = function () {
                $scope.saveflg = 0;
            }

            $scope.getStyle = function (reminderStatus) {
                if (reminderStatus == 1) {
                    return { 'background': '#ddffe1' }
                }
                if (reminderStatus == 0) {
                    return { 'color': '#FF0000' }
                }
                if (reminderStatus == 2) {
                    return { 'color': '#3e4244' }
                }
            }

            $scope.calcu = function (st) {
                //add by jiaxing 当点击save时才能进行计算
                if ($scope.saveflg == 1) {
                    dunningProxy.calcu($routeParams.alertId, function (tracking) {
                        //subTracking.reminder2thDate
                        st.reminder3thDate = tracking.reminder3thDate;
                        st.holdDate = tracking.holdDate;

                        st.soaStatus = tracking.soaStatus;
                        st.reminder2thStatus = tracking.reminder2thStatus;
                        st.reminder3thStatus = tracking.reminder3thStatus;
                        st.holdStatus = tracking.holdStatus;

                        alert("Calculate Success");
                    }, function () {
                        alert("Calculate Error");
                    })
                } else {
                    alert("Please save over due follow up reminder config first!");
                }
            }

            //calculate invoice list checked total
            $scope.invoiceSum = function (cus, legal) {
                var total = 0;
                var currAry = [];
                var totalAry = [];
                var str = "";
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.customerNum == cus && rowItem.legalEntity == legal) {
                            if (currAry.indexOf(rowItem.invoiceCurrency) < 0) {
                                currAry.push(rowItem.invoiceCurrency);
                                totalAry.push(rowItem.outstandingInvoiceAmount);
                            } else {
                                var index = currAry.indexOf(rowItem.invoiceCurrency);
                                total = totalAry[index] + rowItem.outstandingInvoiceAmount;
                                totalAry.splice(index, 1, total);
                            }
                        }
                    });
                }
                for (var index in currAry) {
                    str += " " + currAry[index] + ':' + $scope.formatNumber(totalAry[index], 2, 1);
                }
                $("#" + cus + legal.replace(' ', '').replace(' ', '')).html(str);
            }

            $scope.invoiceSumBatch = function (rows) {
                var total = 0;
                var currAry = [];
                var totalAry = [];
                var str = "";
                var cus = "";
                var legal = "";
                cus = rows[0].entity.customerNum;
                legal = rows[0].entity.legalEntity;
                //angular.forEach(rows, function (row) {
                //    if (row.entity.invoiceId != 0) {
                //        if (row.isSelected == true) {
                //            if (currAry.indexOf(row.entity.invoiceCurrency) < 0) {
                //                currAry.push(row.entity.invoiceCurrency);
                //                totalAry.push(row.entity.outstandingInvoiceAmount);
                //            } else {
                //                var index = currAry.indexOf(row.entity.invoiceCurrency);
                //                total = totalAry[index] + row.entity.outstandingInvoiceAmount;
                //                totalAry.splice(index, 1, total);
                //            }
                //        }
                //    }
                //});
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.customerNum == cus && rowItem.legalEntity == legal) {
                            if (currAry.indexOf(rowItem.invoiceCurrency) < 0) {
                                currAry.push(rowItem.invoiceCurrency);
                                totalAry.push(rowItem.outstandingInvoiceAmount);
                            } else {
                                var index = currAry.indexOf(rowItem.invoiceCurrency);
                                total = totalAry[index] + rowItem.outstandingInvoiceAmount;
                                totalAry.splice(index, 1, total);
                            }
                        }
                    });
                }
                for (var index in currAry) {
                    str += " " + currAry[index] + ':' + $scope.formatNumber(totalAry[index], 2, 1);
                }
                $("#" + cus + legal.replace(' ', '').replace(' ', '')).html(str);
            }
        }])
    .filter('mapClass', function () {
        var typeHash = {
            'DM': 'DM',
            'CM': 'CM',
            'INV': 'INV',
            'Payment': 'Payment'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    })
    .filter('mapStatus', function () {
        var typeHash = {
            'Open': 'Open',
            'PTP': 'PTP',
            'Dispute': 'Dispute',
            'PartialPay': 'PartialPay',
            'Broken PTP': 'Broken PTP',
            'Hold': 'Hold',
            'Payment': 'Payment'

        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    })
    .filter('mapTrack', function () {
        var typeHash = {
            'SOA Sent': 'SOA Sent',
            'Second Reminder Sent': 'Second Reminder Sent',
            'Final Reminder Sent': 'Final Reminder Sent',
            'Dispute': 'Dispute',
            'PTP': 'PTP',
            'Payment Notice Received': 'Payment Notice Received',
            'Broken PTP': 'Broken PTP',
            'First Broken Sent': 'First Broken Sent',
            'Second Broken Sent': 'Second Broken Sent',
            'Hold': 'Hold',
            'Agency Sent': 'Agency Sent',
            'Write Off': 'Write Off',
            'Paid': 'Paid',
            'Bad Debit': 'Bad Debit',
            'Open': 'Open',
            'Close': 'Close',
            'Contra': 'Contra',
            'Breakdown': 'Breakdown'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    });