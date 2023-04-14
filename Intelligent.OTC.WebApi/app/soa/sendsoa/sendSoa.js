angular.module('app.sendSoa', ['ui.grid.edit', 'ui.grid.grouping'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/soa/sendSoa/:nums/:type/:siteuseid', {
                templateUrl: 'app/soa/sendsoa/sendSoa.tpl.html',
                controller: 'sendsoaCL',
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
    .filter('finishedStatusFilter', function ($filter) {
        return function (value) {
            if (value == null) { value = '0';}
            switch (value.toString()) {
                case '0':
                    return "No";
                    break;
                case '1':
                    return "Yes";
                    break;
            }
        };
    })
    .controller('sendsoaCL',
    ['$scope', 'baseDataProxy', 'modalService', '$interval', 'invoiceProxy', '$routeParams', 'mailProxy', 'customerPaymentbankProxy',
        'customerPaymentcircleProxy', 'collectorSoaProxy', 'permissionProxy', 'customerProxy', 'generateSOAProxy', 'contactProxy', 'contactCustomerProxy',
        '$q', 'languagelist', 'inUse', 'FileUploader', 'APPSETTING', 'siteProxy', 'uiGridConstants', 'dunningProxy', 'commonProxy', '$sce',
        'contactHistoryProxy',
        function ($scope, baseDataProxy, modalService, $interval, invoiceProxy, $routeParams, mailProxy, customerPaymentbankProxy,
            customerPaymentcircleProxy, collectorSoaProxy, permissionProxy, customerProxy, generateSOAProxy, contactProxy, contactCustomerProxy,
            $q, languagelist, inUse, FileUploader, APPSETTING, siteProxy, uiGridConstants, dunningProxy, commonProxy, $sce
            , contactHistoryProxy
        ) {
            $scope.pagereadonly = false;
            $scope.languagelist = languagelist;
            $scope.inUselist = inUse;
            $scope.$parent.helloAngular = "OTC - Follow Up Detail";
            //######################### Add by Alex #############
            //contact add button show or hide
            $scope.isContactAddBtnShow = false;
            //payment bank  add button show or hide
            $scope.isPayBankAddBtnShow = false;
            //contact domain add button show or hide 
            $scope.isDomainAddBtnShow = false;
            //######################### Add by Alex #############

            $scope.fileTypeList = [
                { "id": "ALL", "levelName": 'ALL' },
                { "id": "PDF", "levelName": 'PDF' },
                { "id": "XLS", "levelName": 'XLS' }
            ];

            $scope.mType = "001";
            $scope.fileType = "XLS";

            siteProxy.GetLegalEntity("type", function (legal) {
                $scope.legallist = legal;
            }, function () {
            });
            var userId = "";
            $scope.options = [];
            $scope.gridApis = [];
            $scope.num = $routeParams.nums.split(',');
            $scope.selectText = "";
            $scope.entityFlg = "";
            $scope.soapaymentCircle = "";
            $scope.finishflg = $routeParams.type;
            $scope.custInfo = [];
            $scope.custInfo[2] = "10000"; //custInfo[8]
            $scope.custInfo[3] = true; //custInfo[9]
            $scope.timeaxis = function () {
                $scope.items = [{ versionid: 10, content: '2017.11.01~1st Confirm PTP' }, { versionid: 9, content: '2017.11.05~2nd Confirm PTP' }, { versionid: 8, content: '2017.11.10~Reminding' }, { versionid: 7, content: '2017.11.15~1st Dunning' }, { versionid: 6, content: '2017.11.20~2nd Dunning', content: '2017.11.25~Close' }];
            };
            $scope.timeaxis();

            $interval(function () {
                $scope.initList();

            }, 0, 1);
            //click show contact list then put current custNum  => put Group down to Customer
            //       $scope.currCustNum = "";
            //add by jiaxing
            if ($scope.finishflg == 'complete') {
                $scope.isreadonly = true;
            }
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
            $scope.initList = function () {
                collectorSoaProxy.query({ ColSoa: $routeParams.nums, Type: $routeParams.type, siteuseid: $routeParams.siteuseid }, function (list) {
                    if (!list || list.length == 0) {
                        $scope.pagereadonly = true;
                        alert('Soa do not exist or have no authority.');
                        return;
                    }
                    $scope.sendsoa = list;
                    $scope.specialnote = list[0].subLegal[0].specialNotes;
                    var i = 0;
                    angular.forEach($scope.sendsoa, function (row) {
                        row['soapaymentCircle'] = "";
                        row['entityFlg'] = "";
                        row['hisg'] = {
                            data: row.subContactHistory,
                            columnDefs: [
                                { field: 'sortId', displayName: '#', width: '80' },
                                { field: 'contactDate', displayName: 'Contact date', width: '130', cellFilter: 'date:\'yyyy-MM-dd\'' },
                                { field: 'contactType', displayName: 'Contact Type', width: '200' },
                                { field: 'comments', displayName: 'Comments', width: '800' }
                                , {
                                    name: 'tmp', displayName: 'Operation', width: '100',
                                    cellTemplate: '<div style="text-align:center">' +
                                    '<a ng-click="grid.appScope.getDetail(row.entity)"> detail </a></div>'
                                }

                            ]

                        };
                        row['cg'] = {
                            //data: row.subContact,
                            columnDefs: [
                                { field: 'name', displayName: 'Contact Name', width: '120' },
                                //{ field: 'legalEntity', displayName: 'Legal Entity', width: '100' },
                                { field: 'emailAddress', displayName: 'Email', width: '150' },
                                { field: 'department', displayName: 'Department', width: '160' },
                                { field: 'title', displayName: 'Title', width: '160' },
                                { field: 'number', displayName: 'Contact Number', width: '150' },
                                { field: 'comment', displayName: 'Comment', width: '160' },
                                {
                                    field: 'toCc', displayName: 'To/Cc', width: '150',
                                    cellTemplate: '<div style="margin-top:5px;margin-left:5px;width:210px">{{grid.appScope.CheckType(row.entity)}}</div>'
                                },
                                //{
                                //    field: 'isGroupLevel', displayName: 'Is Group Level',
                                //    cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckGroupLevel(row.entity)}}</div>', width: '90'
                                //},
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
                                { field: 'legalEntity', displayName: 'Legal Entity' },
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
                                { field: 'sortId', displayName: '#', width: '60' },
                                //{ field: 'legalEntity', displayName: 'Legal Entity' },
                                {
                                    field: 'reconciliation_Day', displayName: 'Reconciliation Day', cellFilter: 'date:\'yyyy-MM-dd\'',
                                    width: '150'
                                },
                                //{ field: 'paymentDay', displayName: 'Payment Day', cellFilter: 'date:\'yyyy-MM-dd\'' },
                                //{ field: 'weekDay', displayName: 'Week Day' }
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
                                    columnDefs: [
                                        { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },

                                        {
                                            field: 'invoiceNum', name: 'tmp', displayName: 'Invoice NO.', enableCellEdit: false, width: '110', pinnedLeft: true
                                            , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceD(row.entity.invoiceNum,row.entity.invoiceId,\'invdetaillist\')">{{row.entity.invoiceNum}}</a></div>'
                                        },
                                        //{
                                        //    field: 'isBanlance', enableCellEdit: false, displayName: 'Reconciled', width: '100',
                                        //    cellTemplate: '<div style="margin-top:5px;color:#FFA500">{{row.entity.isBanlance}}</div>'
                                        //},
                                        { field: 'notClear', enableCellEdit: false, displayName: 'NotClear', width: '80' },
                                        { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '90' },
                                        { field: 'customerNum', enableCellEdit: false, displayName: 'CustomerNo.', width: '105' },
                                        { field: 'inClass', enableCellEdit: false, displayName: 'Class', width: '60' },
                                        { field: 'siteUseId', enableCellEdit: false, displayName: 'siteUseId.', width: '105', visible: false },
                                        { field: 'legalEntity', enableCellEdit: false, displayName: 'legalEntity', width: '110', visible: false },
                                        {
                                            field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '90',
                                            cellTemplate: '<div style="margin-top:5px;color:#CD3333;text-align:center">{{row.entity.dueDate|date:\'yyyy-MM-dd\'}}</div>'
                                        },
                                        {
                                            field: 'ptpDate', enableCellEdit: false, displayName: 'PTP Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110',
                                            cellTemplate: '<div style="margin-top:5px;color:#1E90FF;text-align:center">{{row.entity.ptpDate|date:\'yyyy-MM-dd\'}}</div>'
                                        },
                                        //{ field: 'purchaseOrder', enableCellEdit: false, displayName: 'Current Status', width: '160' },
                                        { field: 'overdueReason', enableCellEdit: false, displayName: 'Due Reason', width: '110' },
                                        { field: 'invoiceCurrency', enableCellEdit: false, displayName: 'Inv Curr Code', width: '100' },
                                        { field: 'dueDays', enableCellEdit: false, displayName: 'Due Days', width: '80', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                                        {
                                            field: 'balancE_AMT', enableCellEdit: false, displayName: 'Amt Remaining', width: '122', cellFilter: 'number:2', type: 'number', cellClass: 'right',
                                            cellTemplate: '<div style="margin-top:5px;color:#EE00EE">{{row.entity.balancE_AMT|number:2}}</div>'
                                        },
                                        {
                                            field: 'balanceMemo', name: 'tmp1', displayName: 'Invoice Memo', width: '140',
                                            cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.invoiceId,row.entity.invoiceNum,row.entity.balanceMemo,row.entity.memoExpirationDate)"></a>'
                                            + '<label id="lbl{{row.entity.invoiceId}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum,row.entity.balanceMemo,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.balanceMemo.substring(0,7)}}...</label></div>'
                                        },
                                        {
                                            field: 'memoExpirationDate', enableCellEdit: false, displayName: 'Memo Expiration Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '160'
                                            , cellTemplate: '<div class="ui-grid-cell-contents ng-binding ng-scope" style=""><a style="color:{{row.entity.isExp == 1 ? \'#CD3333\' : \'\'}};line-height:28px" ng-click="grid.appScope.openInvoiceD(row.entity.invoiceNum,row.entity.invoiceId,\'dateHislist\')">{{row.entity.memoExpirationDate|date:\'yyyy-MM-dd\'}}</a></div>'
                                        },
                                        {
                                            field: 'invoiceTrack', enableCellEdit: false, displayName: 'Current Status', width: '160',
                                            cellTemplate: '<div style="margin-top:5px;color:#3CB371"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceTrack}}</a></div>'
                                        },
                                        { field: 'creditTerm', enableCellEdit: false, displayName: 'Payment Term Name', width: '110' },
                                        { field: 'finishStatus', enableCellEdit: false, displayName: 'Finished', width: '80', cellFilter: 'finishedStatusFilter' },
                                        { field: 'agingBucket', enableCellEdit: false, displayName: 'Aging Bucket', width: '120' },
                                        { field: 'ebname', enableCellEdit: false, displayName: 'Eb', width: '122' },
                                        { field: 'vatNum', enableCellEdit: false, displayName: 'VAT No.', width: '122' },
                                        { field: 'vatDate', enableCellEdit: false, displayName: 'VAT date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'trackDate', enableCellEdit: false, displayName: 'last update date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'ptpIdentifiedDate', enableCellEdit: false, displayName: 'PTP Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '160' },
                                        //{
                                        //    name: 'disputeReason', displayName: 'Dispute(Y / N)', width: '110'
                                        //    , cellTemplate: '<div style="height:30px;vertical-align:middle">Y</div>'
                                        //},
                                        { field: 'isDispute', enableCellEdit: false, displayName: 'Dispute(Y / N)', width: '110' },
                                        { field: 'disputeDate', enableCellEdit: false, displayName: 'Dispute Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '180' },
                                        { field: 'disputeReason', enableCellEdit: false, displayName: 'Dispute Reason', width: '110' },
                                        { field: 'ownerDepartment', enableCellEdit: false, displayName: 'Action Owner-Department', width: '190' },
                                        { field: 'woVat_AMT', enableCellEdit: false, displayName: 'Amount Wo Vat', width: '122', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                                        //{
                                        //    field: 'ownerDepartment', enableCellEdit: false, displayName: 'Action Owner-Department', cellFilter: 'number:2', type: 'number', width: '200'
                                        //    , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                        //        if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                        //            return 'uigridred';
                                        //        }
                                        //    }
                                        //},
                                        {
                                            field: 'collectoR_NAME', enableCellEdit: false, displayName: 'Collector', width: '122'
                                        },
                                        { field: 'nextActionDate', enableCellEdit: false, displayName: 'Next Action Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '160' },
                                        { field: 'consignmentNumber', enableCellEdit: false, displayName: 'Consignment Number', width: '150' }

                                        //{ field: 'daysLate', enableCellEdit: false, displayName: 'Days Late', width: '80', type: 'number' },
                                        //{
                                        //    field: 'status', enableCellEdit: false, displayName: 'Status', width: '80'
                                        //    , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                        //        if (grid.getCellValue(row, col) === 'Dispute') {
                                        //            return 'uigridred';
                                        //        }
                                        //    }
                                        //    , filter: {
                                        //        term: '',
                                        //        type: uiGridConstants.filter.SELECT,
                                        //        selectOptions: [
                                        //            { value: 'Open', label: 'Open' },
                                        //            { value: 'PTP', label: 'PTP' },
                                        //            { value: 'Dispute', label: 'Dispute' },
                                        //            { value: 'PartialPay', label: 'PartialPay' },
                                        //            { value: 'Broken PTP', label: 'Broken PTP' },
                                        //            { value: 'Hold', label: 'Hold' },
                                        //            { value: 'Payment', label: 'Payment' }]
                                        //    }
                                        //},
                                        //{
                                        //    field: 'invoiceTrack', enableCellEdit: false, displayName: 'Invoice Track', width: '100'
                                        //    , filter: {
                                        //        term: '',
                                        //        type: uiGridConstants.filter.SELECT,
                                        //        selectOptions: [
                                        //            { value: 'SOA Sent', label: 'SOA Sent' },
                                        //            { value: 'Second Reminder Sent', label: 'Second Reminder Sent' },
                                        //            { value: 'Final Reminder Sent', label: 'Final Reminder Sent' },
                                        //            { value: 'Dispute', label: 'Dispute' },
                                        //            { value: 'PTP', label: 'PTP' },
                                        //            { value: 'Payment Notice Received', label: 'Payment Notice Received' },
                                        //            { value: 'Broken PTP', label: 'Broken PTP' },
                                        //            { value: 'First Broken Sent', label: 'First Broken Sent' },
                                        //            { value: 'Second Broken Sent', label: 'Second Broken Sent' },
                                        //            { value: 'Hold', label: 'Hold' },
                                        //            { value: 'Agency Sent', label: 'Agency Sent' },
                                        //            { value: 'Write Off', label: 'Write Off' },
                                        //            { value: 'Paid', label: 'Paid' },
                                        //            { value: 'Bad Debit', label: 'Bad Debit' },
                                        //            { value: 'Open', label: 'Open' },
                                        //            { value: 'Close', label: 'Close' },
                                        //            { value: 'Contra', label: 'Contra' },
                                        //            { value: 'Breakdown', label: 'Breakdown' }
                                        //        ]
                                        //    }
                                        //},

                                        //{
                                        //    field: 'documentType', enableCellEdit: false, displayName: 'Document Type', width: '120'
                                        //    , filter: {
                                        //        term: '',
                                        //        type: uiGridConstants.filter.SELECT,
                                        //        selectOptions: [
                                        //            { value: 'DM', label: 'DM' },
                                        //            { value: 'CM', label: 'CM' },
                                        //            { value: 'INV', label: 'INV' },
                                        //            { value: 'Payment', label: 'Payment' }]
                                        //    }, cellFilter: 'mapClass'
                                        //},
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
                        //$scope.selectall();
                        $scope.chengbutton();
                        $scope.selectInvoice();
                        $scope.getAlertStatus();
                    }, 0, 1);
                });
            };

            $scope.batcheditNotClear = function (type) {
                $scope.inv = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId !== 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                };
                if ($scope.inv === "" || $scope.inv === null) {
                    alert("Please choose 1 invoice at least .");
                    return;
                };
                if (confirm("Are you sure change NotClear")) {
                    var idList = new Array();
                    idList.push(type);
                    for (j = 0; j < $scope.gridApis.length; j++) {
                        angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.invoiceId !== 0) {
                                idList.push(rowItem.invoiceId);
                            }
                        });
                    };
                    invoiceProxy.setNotClear(idList, function (res) {
                        alert(res);
                        $scope.initList();
                    }, function (res) {
                        alert(res);
                    });
                }
            };

            $scope.editMemoShow = function (invoiceId, invoiceNum, memo, memoDate) {
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
                    $('#txtBox').css({ 'width': contentWidth - 20 + 'px', 'height': contentHeight - 130 + 'px' });
                    var str = '';
                    var str1 = '';
                    str = 'Invoice :"' + invoiceNum + '" Memo : ';
                    str1 = memo;
                    $("#hiddenInvId").val(invoiceId);
                    if (memoDate != null && memoDate != undefined) {
                        $("#boxCommDate").find("input").val(memoDate.substring(0, 10));
                    }
                    else {
                        $("#boxCommDate").find("input").val(memoDate);
                    }                   
                    $("#lblBoxTitle").html(str)
                    $("#boxEdit").show();
                }
            }

            $scope.chengbutton = function () {
                //DunFlag
                var flag = $scope.sendsoa[0].dunFlag;
                if (flag > 0) {
                    switch (flag) {
                        case 1:
                            document.getElementById("asend").style.color = "red";
                            document.getElementById("apause").style.color = "blue";
                            break;

                        case 2:
                            document.getElementById("asend").style.color = "blue";
                            document.getElementById("apause").style.color = "red";
                            break;

                        case 3:
                            document.getElementById("asend").style.color = "red";
                            document.getElementById("apause").style.color = "red";
                            break;

                        case 11:
                            document.getElementById("asend").style.color = "green";
                            document.getElementById("apause").style.color = "red";
                            break;

                        case 12:
                            document.getElementById("asend").style.color = "red";
                            document.getElementById("apause").style.color = "green";
                            break;

                        case 13:
                            document.getElementById("asend").style.color = "green";
                            document.getElementById("apause").style.color = "green";
                            break;
                        case 31:
                            document.getElementById("asend").style.color = "red";
                            document.getElementById("apause").style.color = "green";
                            break;
                        case 32:
                            document.getElementById("asend").style.color = "green";
                            document.getElementById("apause").style.color = "red";
                            break;

                        default:
                            break;
                    }

                }
            }

            $scope.batcheditMemoShow = function () {
                $scope.inv = [];
                $scope.pm = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                            $scope.pm.push(rowItem.balancE_AMT);
                        }
                    });
                }
                var ccount = 0;
                if ($scope.pm != null) {
                    for (var i = 0; i < $scope.pm.length; i++) {

                        ccount = ccount + $scope.pm[i];
                    }
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
                    $('#batchtxtBox').css({ 'width': contentWidth - 20 + 'px', 'height': contentHeight - 155 + 'px' });
                    var str = '';
                    str = "All Selected Invoices' Memo Will Be Entirely Updated By Follow:"
                    $("#batchhiddenInvId").val($scope.inv);
                    $("#batchtxtBox").val("");
                    $("#boxCommentExpirationDate").find("input").val("");
                    $("#batchlblBoxTitle").html(str);
                    $("#boxEditBatch").show();
                }
            }

            $scope.messageFinished = [
                { num: 0, obj: "No" },
                { num: 1, obj: "Yes" }
            ];

            $scope.editMemoSave = function () {
                var list = [];
                var invoiceId = $("#hiddenInvId").val();
                var memo = $("#txtBox").val();
                var memoDate = $("#boxCommDate").find("input").val();  
                list.push('2');
                list.push(invoiceId);
                list.push(memo);
                list.push(memoDate);
                collectorSoaProxy.savecommon(list, function () {
                    $scope.saveBack(invoiceId, memo, memoDate);
                    $scope.editMemoClose();
                });
            }

            $scope.batcheditMemoSave = function () {
                var list = [];
                var invoiceIds = $("#batchhiddenInvId").val().toString();
                var memo = $("#batchtxtBox").val();
                var memoDate = $("#boxCommentExpirationDate").find("input").val();
                if (memo.length > 8000) {
                    alert("input 8000 character at most");
                    return;
                }
                list.push("5");
                list.push(invoiceIds);
                list.push(memo);
                list.push(memoDate);
                collectorSoaProxy.savecommon(list, function () {
                    $scope.batchsaveBack(memo, memoDate);
                    $scope.batcheditMemoClose();
                });
            }

            $scope.saveBack = function (invoiceId, memo,memoDate) {
                for (k = 0; k < $scope.gridApis.length; k++) {
                    angular.forEach($scope.gridApis[k].grid.rows, function (rowItem) {
                        if (rowItem.entity.invoiceId == invoiceId) {
                            rowItem.entity.balanceMemo = memo;
                            rowItem.entity.memoExpirationDate = memoDate;
                        }
                    });
                }
            }

            $scope.batchsaveBack = function (memo, memoDate) {
                for (k = 0; k < $scope.gridApis.length; k++) {
                    angular.forEach($scope.gridApis[k].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            rowItem.balanceMemo = memo + '\r\n' + rowItem.balanceMemo;
                            rowItem.memoExpirationDate = memoDate;
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
                    $scope.gridApis[k].selection.selectAllRows();
                }
            }

            $scope.selectInvoice = function () {
                var isCustContact = $scope.sendsoa[0]["isCostomerContact"];
                var reconciliationDay = $scope.sendsoa[0]["reconciliationDay"];
                for (i = 0; i < $scope.gridApis[0].grid.options.data.length; i++) {
                    //if ((($scope.gridApis[0].grid.options.data[i]["ptpDate"] != null && $scope.gridApis[0].grid.options.data[i]["ptpDate"] <= reconciliationDay) || ($scope.gridApis[0].grid.options.data[i]["ptpDate"] == null && $scope.gridApis[0].grid.options.data[i]["dueDate"] <= reconciliationDay)) || isCustContact == "N") {
                        $scope.gridApis[0].selection.selectRow($scope.gridApis[0].grid.options.data[i]);
                    //}
                }

                //if ($scope.gridApis[0].data['inClass'] =="INV")
                //{
                //    $scope.gridApis[0].selection.selectRow($scope.gridApis[0].gridoption.data[0]);
                //}

            }

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

            $scope.check = function (mtype) {
                var language = "";
                $scope.inv = [];
                $scope.cus = [];
                $scope.suid = "";
                $scope.custNo = "";
                $scope.legalentty = "";
                var isfirstrow = true;
                var isonecustomer = true;
                var blc = 0;
                $scope.mType = mtype;
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
                            if (isfirstrow == false) {
                                if ($scope.suid != rowItem.siteUseId || $scope.custNo != rowItem.customerNum || $scope.legalentty != rowItem.legalEntity) {
                                    isonecustomer = false;
                                }
                            } else {
                                $scope.suid = rowItem.siteUseId;
                                $scope.custNo = rowItem.customerNum;
                                $scope.legalentty = rowItem.legalEntity;
                                isfirstrow = false;
                            }

                        }
                    });
                }

                if (isonecustomer == false) {
                    alert("Please choose 1 customer at most .")
                    return;
                }
                //alert($scope.inv);
                if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .")
                } else {
                    //********************************Added by zhangYu******************************************//
                    $scope.strcus = $scope.cus.join(",");
                    $scope.invoiceNums = $scope.inv.join(",");

                    generateSOAProxy.getGenerateSOACheck($scope.strcus, $scope.suid,  $scope.inv, $scope.mType, $scope.fileType, function (res) {

                        var modalDefaults = null;
                        modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return $scope.strcus; },
                                siteuseId: function () { return $scope.suid; },
                                invoicenums: function () { return $scope.invoiceNums; },
                                mType: function () { return $scope.mType; },
                                //                                    selectedInvoiceId: function () { return $scope.inv; },
                                instance: function () {
                                    return getMailInstance($scope.strcus, $scope.suid, $scope.mType);
                                },
                                mailDefaults: function () {
                                    return {
                                        mailType: 'NE',
                                        templateChoosenCallBack: selectMailInstanceById,
                                        mailUrl: generateSOAProxy.sendEmailUrl,
                                        checkCallBack: checkSOAMail,
                                    };
                                }
                            },
                            windowClass: 'modalDialog'
                        };

                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "sent") {
                                //ADD jiaxing
                                $scope.reSeachContactList();
                                //refresh invoice track
                                $scope.refreshTrack();
                                document.getElementById("asend").style.color = "green";
                            }
                        }, function (err) {
                            alert(err);
                        });

                    }, function (error) {
                        alert(error);
                        return;
                    });

                }
            }

            var getMailInstanceMain = function (custNums, suid, ids, mType) {

                var instanceDefered = $q.defer();
                generateSOAProxy.getMailInstance(custNums, suid, ids, mType, $scope.fileType, function (res) {
                    var instance = res;
                    renderInstance(instance, custNums, suid, mType);

                    instanceDefered.resolve(instance);
                }, function (error) {
                    alert(error);
                    return;
                });

                return instanceDefered.promise;
            };

            var getMailInstanceTo = function (custNums, suid) {

                var toDefered = $q.defer();
                //TO
                contactProxy.query({
                    customerNums: custNums, siteUseid: suid
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

            //=========added by alex body中显示附件名+Currency=========

            //var getMailInstanceAttachment = function(custNums){
            //    var attachDefered = $q.defer();
            //    var attach = {};

            //    //mail Attachment
            //    generateSOAProxy.geneateSoaByIds(custNums,1, function (res) {
            //        attach.attachments = res;
            //        attach.attachment = attach.attachments.join(",");
            //        attachDefered.resolve(attach);
            //    });
            //    return attachDefered.promise;
            //};

            //=========================================================

            var getMailInstance = function (custNums, suid,mType) {
                var instance = {};
                var allDefered = $q.defer();

                $q.all([
                    getMailInstanceMain(custNums, suid, $scope.inv, mType),
                    //getMailInstanceTo(custNums, suid)
                    //=========added by alex body中显示附件名+Currency======
                    //getMailInstanceAttachment($scope.inv)
                    //=====================================================
                ])
                    .then(function (results) {
                        instance = results[0];
                        //2016-01-14 start
                        //instance.to = results[1];
                        //instance.to = "";
                        //instance.cc = "";
                        //instance.to = results[0].to.join(";");
                        //instance.cc = results[0].cc.join(";");
                        //2016-01-14 End
                        //=========added by alex body中显示附件名+Currency======
                        //instance.attachments = results[2].attachments;
                        //instance.attachment = results[2].attachment;
                        //=====================================================
                        allDefered.resolve(instance);
                    });

                return allDefered.promise;
            };

            var checkSOAMail = function (instance) {
                if (!instance.attachments) {
                    if (!confirm("Not Include Attachment ,continue ?")) {
                        return false;
                    }
                    return true;
                }
                return true;
            }

            var selectMailInstanceById = function (custNums, id, siteUseId, templateType, templatelang, ids) {
                var instance = {};
                var instanceDefered = $q.defer();
                //=========added by alex body中显示附件名+Currency=== $scope.inv 追加 ======
                generateSOAProxy.getMailInstById(custNums, id, siteUseId, templateType, templatelang, ids, function (res) {
                    instance = res;
                    renderInstance(instance, custNums, siteUseId);

                    instanceDefered.resolve(instance);
                });

                return instanceDefered.promise;
            };

            var renderInstance = function (instance, custNums, suid, mType) {
                //subject
                //instance.subject = 'SOA-' + $scope.shortsub.join('-');
                //invoiceIds
                instance.invoiceIds = $scope.inv;
                //soaFlg
                instance.soaFlg = "1";
                //Bussiness_Reference
                var customerMails = [];
                angular.forEach(custNums.split(','), function (cust) {
                    customerMails.push({ MessageId: instance.messageId, CustomerNum: cust, SiteUseId: suid });
                });
                instance.CustomerMails = customerMails; //$routeParams.nums;
                //mailTitle
                instance["title"] = "Create SOA";
                //mailType
                instance.mailType = mType + ",SOA";
            };

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

            $scope.resultHis = "";
            $scope.openInvoiceD = function (inNum, invId,flag) {
                var modalDefaults = {
                    templateUrl: 'app/soa/invdetail/invdetail.tpl.html',
                    controller: 'invDetCL',
                    size: 'lg',
                    resolve: {
                        inNum: function () { return inNum; },
                        invId: function () { return invId; },
                        flag: function () { return flag; }
                    }
                    , windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {

                });

            }
            
            $scope.openMemmoDateHis = function (customerCode, siteUseId) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customer/CustomerExpirationDate.tpl.html',
                    controller: 'customerExpirationDateCTL',
                    size: 'lg',
                    resolve: { 
                        customerCode: function () { return customerCode; },
                        siteUseId: function () { return siteUseId; }
                    }
                    , windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {

                });

            }


            $scope.refreshTrack = function () {
                //for (j = 0; j < $scope.gridApis.length; j++) {
                //    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                //        if (rowItem.invoiceId != 0) {
                //            rowItem.invoiceTrack = "SOA Sent";
                //        }
                //    });
                //}
            }

            $scope.changeWF = function (type) {

                collectorSoaProxy.wfchange($routeParams.nums, type, function (result) {
                    if (type == "pause" || type == "resume") {
                        $scope.getAlertStatus();
                    } else if (type == "finish" || type == "cancel" || type == "restart") {
                        window.close();
                    }
                });
            }

            $scope.getAlertStatus = function () {

                collectorSoaProxy.queryObject({ ReferenceNo: $routeParams.nums }, function (re) {
                    if (re.status == "Initialized" || re.status == "Processing" || re.status == "Resume") {

                        $("#asend").removeClass("ng-hide");
                        $("#apause").removeClass("ng-hide");
                        $("#aresponse").removeClass("ng-hide");
                        $("#aresume").removeClass("ng-hide");
                        $("#afinish").removeClass("ng-hide");
                        $("#acancel").removeClass("ng-hide");
                        $("#arestart").removeClass("ng-hide");
                        $("#arclearptp").removeClass("ng-hide");
                        $("#aoverdueReason").removeClass("ng-hide");
                        $("#aclearOverdueReason").removeClass("ng-hide");
                    }
                    else if (re.status == null || re.status == "Finish") {

                        $("#asend").removeClass("ng-hide");
                        $("#apause").removeClass("ng-hide");
                        $("#aresponse").removeClass("ng-hide");
                        $("#aresume").removeClass("ng-hide");
                        $("#afinish").removeClass("ng-hide");
                        $("#acancel").removeClass("ng-hide");
                        $("#arestart").removeClass("ng-hide");
                        $("#arclearptp").removeClass("ng-hide");
                        $("#aoverdueReason").removeClass("ng-hide");
                        $("#aclearOverdueReason").removeClass("ng-hide");
                    }
                    else if (re.status == "Pause") {

                        $("#asend").removeClass("ng-hide");
                        $("#apause").removeClass("ng-hide");
                        $("#aresponse").removeClass("ng-hide");
                        $("#aresume").removeClass("ng-hide");
                        $("#afinish").removeClass("ng-hide");
                        $("#acancel").removeClass("ng-hide");
                        $("#arestart").removeClass("ng-hide");
                        $("#arclearptp").removeClass("ng-hide");
                        $("#aoverdueReason").removeClass("ng-hide");
                        $("#aclearOverdueReason").removeClass("ng-hide");
                    }
                });

            }

            $scope.conshow = function (id, siteUseId) {
                if (siteUseId == null || siteUseId == undefined || siteUseId == 'undefined') {
                    siteUseId = $routeParams.siteUseId;
                }
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

                //collectorSoaProxy.query({ CustNumFCon: id }, function (list) {
                //    angular.forEach($scope.sendsoa, function (row) {
                //        if (row.customerCode == id) {
                //            row['cg'].data = list;
                //            row['cgshow'] = true;
                //            //##### Add by Alex ######
                //            row['pbgshow'] = false;
                //            row['pcgshow'] = false;
                //            row['cdgshow'] = false;
                //            //########################
                //        }
                //    });
                //});
                collectorSoaProxy.forContactor(id + ',' + siteUseId, function (list) {
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
                $scope.entityFlg = "";

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

                collectorSoaProxy.query({ CustNumFPc: id, SiteUseIdFPc: $routeParams.siteuseid }, function (list) {
                    angular.forEach($scope.sendsoa, function (row) {
                        if (row.customerCode == id) {
                            row['pcg'].data = list;
                            row['pcgshow'] = true;
                            //$scope.entityFlg = row['entityFlg'];
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
                });
            }
            //collectorSoaProxy.query({ CustNumsFCH: $routeParams.nums }, function (list) {
            //    $scope.contactList.data = list;
            //})

            //**********************************get Comment Expiration Date History********************
            $scope.getCommDateHistory = function (customerCode, siteUseId) {
                collectorSoaProxy.query({ CustomerCode: customerCode, siteUseId : siteUseId }, function (list) {
                    debugger;
                    var a = list;
                    alert('success!');
                });
            }

            //**********************************edit special notes********************
            $scope.saveNote = function (cus, legal, note, siteuseid, comment, commentExpirationDate) {
                var list = [];
                list.push(1);//1:SpeicalNotes;2:InvoiceComm
                list.push(cus);
                list.push(legal);
                list.push(note);
                list.push(siteuseid);
                list.push(comment);
                list.push(commentExpirationDate);
                collectorSoaProxy.savecommon(list, function () {
                    alert('success!');
                });
            };


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
                    $scope.conshow(cus.split(',')[0], cus.split(',')[1]);
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

            $scope.addPayBankCircle = function (cus, siteUseId, paymentCircle, reconciliationDay) {
                if ((paymentCircle == "" || paymentCircle == null)
                    && (reconciliationDay == "" || reconciliationDay == null)) {
                    alert("Reconciliation Day or Payment Day have to choose one!");
                    return;
                }
                else {
                    var num = cus;
                    var paymentCircleArray = [];
                    paymentCircleArray.push(paymentCircle);
                    paymentCircleArray.push(num);
                    paymentCircleArray.push($scope.entityFlg);
                    paymentCircleArray.push(siteUseId);
                    paymentCircleArray.push(reconciliationDay);
                    customerPaymentcircleProxy.addPaymentCircle(paymentCircleArray, function (res) {
                        //collectorSoaProxy.query({ CustNumFPc: cus }, function (list) {
                        customerPaymentcircleProxy.searchPaymentCircle(cus + ',' + siteUseId, $scope.entityFlg, function (paydate) {
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
                //   row.customerNum = $scope.currCustNum;
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
                            return $scope.currCustNum + ',' + $routeParams.siteuseid;
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
                    $scope.conshow($scope.currCustNum, $routeParams.siteuseid);
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

            $scope.clearPTP = function () {
                $scope.inv = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                } if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .")
                    return;
                }
                if (confirm("Are you sure clear PTP")) {
                    var idList = new Array(); for (j = 0; j < $scope.gridApis.length; j++) {
                        angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.invoiceId != 0) {
                                idList.push(rowItem.invoiceId + '|' + rowItem.invoiceNum);
                            }
                        });

                        invoiceProxy.clearPTP(idList, function (res) {
                            alert(res);
                            $scope.initList();
                        }, function (res) {
                            alert(res);
                            return;
                        });
                    }
                }

            }

            $scope.clearOverdueReason = function () {
                $scope.inv = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                } if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .")
                    return;
                }
                if (confirm("Are you sure clear overdue reason")) {
                    var idList = new Array(); for (j = 0; j < $scope.gridApis.length; j++) {
                        angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.invoiceId != 0) {
                                idList.push(rowItem.invoiceId + '|' + rowItem.invoiceNum);
                            }
                        });

                        invoiceProxy.clearOverdueReason(idList, function (res) {
                            alert(res);
                            $scope.initList();
                        }, function (res) {
                            alert(res);
                            return;
                        });
                    }
                }

            }


            $scope.clearComments = function () {
                $scope.inv = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                } if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .");
                    return;
                }
                if (confirm("Are you sure clear comments")) {
                    var idList = new Array(); for (j = 0; j < $scope.gridApis.length; j++) {
                        angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                            if (rowItem.invoiceId != 0) {
                                idList.push(rowItem.invoiceId + '|' + rowItem.invoiceNum);
                            }
                        });

                        invoiceProxy.clearComments(idList, function (res) {
                            alert(res);
                            $scope.initList();
                        }, function (res) {
                            alert(res);
                            return;
                        });
                    }
                }

            }

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
                    //      var cus = row.bkCustomerNum;
                    $scope.conshow($scope.currCustNum, $routeParams.siteuseid);
                    alert("Delete Success");
                }, function () {
                    alert("Delete Error");
                    return;
                });
            };

            $scope.DelBankInfo = function (row) {
                var cusid = row.id;
                customerPaymentbankProxy.delPaymentBank(cusid, function () {
                    $scope.pbshow(row.customerNum);
                    alert("Delete Success");
                }, function () {
                    alert("Delete Error");
                    return;
                });
            };

            $scope.DelPaymentcircle = function (row) {
                var cusid = row.id;
                customerPaymentcircleProxy.delPaymentCircle(cusid, function () {
                    var cus = row.customerNum;
                    //collectorSoaProxy.query({ CustNumFPc: cus }, function (list) {
                    customerPaymentcircleProxy.searchPaymentCircle(cus + ',' + $routeParams.siteuseid, $scope.entityFlg, function (paydate) {
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
                    return;
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
                    return;
                });
            }

            //##############  upload  ######################

            //初始化uploader
            var uploader = $scope.uploader = new FileUploader({
                url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle'
            });

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    return;
                }
            });

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
                if (uploader.queue[3] == null) {
                    alert("Please select File");
                    return;
                } else {
                    if ((uploader.queue[3]._file.name.toString().toUpperCase().split(".").length > 1)
                        && uploader.queue[3]._file.name.toString().toUpperCase().split(".")[1] != "CSV") {
                        alert("File format is not correct !");
                        return;
                    }
                    var num = cus;
                    var legal = $scope.entityFlg;
                    alert(legal);
                    uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle?customerNum=' + num + '&siteUseId=' + $routeParams.siteuseid + '&legal=' + legal;
                    uploader.uploadAll();

                    // CALLBACKS
                    uploader.onSuccessItem = function (fileItem, response, status, headers) {
                        alert(response);
                        //collectorSoaProxy.query({CustNumFPc: cus}, function (list) {
                        //angular.forEach($scope.sendsoa, function (row) {
                        customerPaymentcircleProxy.searchPaymentCircle(cus + ',' + $routeParams.siteuseid, $scope.entityFlg, function (paydate) {
                            angular.forEach($scope.sendsoa, function (row) {
                                if (row.customerCode == cus) {
                                    row['pcg'].data = paydate;
                                    row['pcgshow'] = true;
                                }
                            })
                        });
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
                        var parmarList = custNum.split(',');
                        if (row.customerCode == parmarList[0]) {
                            row['pcg'].data = paydate;
                            row['pcgshow'] = true;
                        }
                    });
                    $scope.entityFlg = legal;
                    uploader.queue[3] = null;
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
                                $scope.invoiceNums = "";

                                var modalDefaults = {
                                    templateUrl: 'app/common/mail/mail-instance.tpl.html',
                                    controller: 'mailInstanceCtrl',
                                    size: 'customSize',
                                    resolve: {
                                        custnum: function () { return mailInstance.customerNum; },
                                        siteuseId: function () { return $scope.suid; },
                                        invoicenums: function () { return $scope.invoiceNums; },
                                        instance: function () { return mailInstance },
                                        mType: function () { return row.mailType },
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
                                        invoiceIds: function () { return ""; },
                                        siteuseId: function () { return row.siteuseid; },
                                        legalEntity: function () { return row.legalEntity; }
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
                    else if (row.contactType == "Response") {
                        if (row.contactId) {
                            contactHistoryProxy.get(row.contactId, function (responseInstance) {
                                responseInstance["title"] = "Response Detail";
                               
                                var modalDefaults = {
                                    templateUrl: 'app/common/contactdetail/contact-response.tpl.html',
                                    controller: 'contactResponseCtrl',
                                    size: 'lg',
                                    resolve: {
                                        responseInstance: function () { return responseInstance; },
                                        custnum: function () { return $scope.custNo; },
                                        invoiceIds: function () { return $scope.invNew; },
                                        siteuseId: function () { return $scope.suid; },
                                        legalEntity: function () { return $scope.legalentty; }
                                    },
                                    windowClass: 'modalDialog'
                                };
                                modalService.showModal(modalDefaults, {}).then(function (result) {
                                    $scope.reSeachContactList();
                                });
                            }); 
                        } else {
                            alert("contact Id is null");
                        }
                    }
                    else { alert("not map the contact type"); }
                }
            } //contactList Detail end


            //********************contactList******************<a> ng-click********************end

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

            $scope.dateChange = function () {
                $scope.saveflg = 0;
            }

            $scope.checkNumber = function (val) {
                if (!/^([1-9]\d|\d)$/.test(val)) {
                    return 1;
                } else {
                    return 0;
                }
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

            $scope.saveCollectionCalendarConfig = function (customerNum, legalEntity, currTracking) {
                var f, s, p, r, d;
                f = currTracking.firstInterval;
                s = currTracking.secondInterval;
                p = currTracking.paymentTat;
                r = currTracking.riskInterval;
                d = currTracking.desc;

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
                    baseDataProxy.saveCollectionCalendarConfig(customerNum, legalEntity, list, function () {
                        $scope.saveflg = 1;

                        commonProxy.queryObject({ customerNum: customerNum, legalEntity: legalEntity }, function (tracking) {
                            currTracking.reminder2thDate = tracking.reminder2thDate;
                            currTracking.reminder3thDate = tracking.reminder3thDate;
                            currTracking.holdDate = tracking.holdDate;

                            currTracking.soaStatus = tracking.soaStatus;
                            currTracking.reminder2thStatus = tracking.reminder2thStatus;
                            currTracking.reminder3thStatus = tracking.reminder3thStatus;
                            currTracking.holdStatus = tracking.holdStatus;

                        });

                        alert("Save Success");
                    }, function () {
                        alert("Save Error");
                    });
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

                //for (j = 0; j < $scope.gridApis.length; j++) {
                //    angular.forEach($scope.gridApis[j].grid, function (grid) {
                //        //if (rowItem.entity.customerNum == cus && rowItem.entity.legalEntity == legal) {
                //        //    rowItem.isSelected = false;
                //        //}
                //        if (grid.selection.selectAll === true) {
                //            grid.selection.selectAllVisibleRows();
                //        } else if (grid.selection.selectAll === false) {
                //            getVisibleRows(grid).forEach(function (row) {
                //                row.isSelected = false;
                //            })
                //        }
                //    });
                //}

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

            //$scope.findinvs = function () {
            //    alert(1);
            //}
            //##########################
            //change invoice status
            //##########################
            $scope.changetab = function (type) {
                //get selected invoiceIds
                $scope.inv = [];
                $scope.invNew = [];
                $scope.invoiceNums = [];
                $scope.suid = "";
                $scope.custNo = "";
                $scope.legalentty = "";
                var isfirstrow = true;
                var isonecustomer = true;
                $scope.pm = [];
                $scope.invStatus = [];
                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                            $scope.pm.push(rowItem.balancE_AMT);
                            $scope.invoiceNums.push(rowItem.invoiceNum);
                        }
                    });
                }
                var ccount = 0;
                if ($scope.pm != null) {
                    for (var i = 0; i < $scope.pm.length; i++) {
                        ccount = ccount + $scope.pm[i];
                    }
                }

                angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                    //alert(rowItem.invoiceNum);
                    if (rowItem.invoiceId != 0) {
                        $scope.invNew.push(rowItem.invoiceId);
                        $scope.invStatus.push({ id: rowItem.invoiceId, status: rowItem.invoiceTrack.toLowerCase() });
                        if (isfirstrow == false) {
                            if ($scope.suid != rowItem.siteUseId || $scope.custNo != rowItem.customerNum || $scope.legalentty != rowItem.legalEntity) {
                                isonecustomer = false;
                            }
                        } else {
                            $scope.suid = rowItem.siteUseId;
                            $scope.custNo = rowItem.customerNum;
                            $scope.legalentty = rowItem.legalEntity;
                            isfirstrow = false;
                        }
                    }
                });

                if (($scope.inv == "" || $scope.inv == null) && type != "ptp") {
                    alert("Please choose 1 invoice at least .")
                    return;
                }

                if (isonecustomer == false) {
                    alert("Please choose 1 customer at most .")
                    return;
                }

                if (type == "dispute") {
                    //$scope.inv.push("1215283");
                    for (var i = 0; i < $scope.invStatus.length; i++) {
                        if ($scope.invStatus[i].status.indexOf(type) >= 0) {
                            alert("the selected invoices contains dispute invoice already!");
                            return;
                        }
                    }

                    var relatedMail = "";
                    if ($scope.mailInstance != null) {
                        relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");
                    } else {
                        relatedMail = "";
                    }
                    //relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");
                    //relatedMail = "cindy.zhu@ap.averydennison.com" + "  " + "Dispute tracking： from(OTC dev account)" + " " + "2017-11-09T19:12:27.98".replace("T", " ");
                    contactHistoryProxy.queryObject({ type: 'dispute' }, function (disInvInstance) {
                        disInvInstance["title"] = "Dispute Reason";
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-dispute.tpl.html',
                            controller: 'contactDisputeCtrl',
                            size: 'lg',
                            resolve: {
                                disInvInstance: function () { return disInvInstance; },
                                custnum: function () { return $scope.custNo; },
                                invoiceIds: function () { return $scope.invNew; },
                                contactId: function () {
                                    return "";//$scope.mailInstance.messageId; 
                                },
                                relatedEmail: function () {
                                    return "";
                                },
                                contactPerson: function () {
                                    return "";//$scope.mailInstance.to;
                                },
                                siteUseId: function () {
                                    return $scope.suid;
                                },
                                //customerNo: function () {
                                //    return $scope.custNo;
                                //},
                                legalEntity: function () {
                                    return $scope.legalentty;
                                }


                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "submit") {
                                //$scope.readData();

                                $scope.initList();
                            }
                        });
                    });
                }
                else if (type == "ptp") {

                    for (var i = 0; i < $scope.invStatus.length; i++) {
                        if ($scope.invStatus[i].status.indexOf(type) >= 0) {
                            alert("the selected invoices contains ptp invoice already!");
                            return;
                        }
                    }

                    var relatedMail = "";

                    if ($scope.mailInstance != null) {
                        relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");
                    } else {
                        relatedMail = "";
                    }

                    //relatedMail = "cindy.zhu@ap.averydennison.com" + "  " + "Dispute tracking： from(OTC dev account)" + " " + "2017-11-09T19:12:27.98".replace("T", " ");
                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-ptp.tpl.html',
                        controller: 'contactPtpCtrl',
                        size: 'lg',
                        resolve: {
                            custnum: function () {
                                return "";
                            },
                            invoiceIds: function () {
                                return $scope.invNew;
                            },
                            siteUseId: function () {
                                return $scope.suid;
                            },
                            customerNo: function () {
                                return $scope.custNo;
                            },
                            legalEntity: function () {
                                return $scope.legalentty;
                            },
                            contactId: function () {
                                return "";//$scope.mailInstance.messageId;
                            },
                            relatedEmail: function () {
                                return "";
                            },
                            contactPerson: function () {
                                return "";//$scope.mailInstance.to;
                            },
                            proamount: function () {
                                return ccount.toFixed(2);
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] == "submit") {
                            $scope.initList();
                            //  $scope.readData();
                        }
                    });
                }
                else if (type == "notice") {

                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-notice.tpl.html',
                        controller: 'contactNoticeCtrl',
                        size: 'lg',
                        resolve: {
                            custnum: function () {
                                return "";
                            },
                            invoiceIds: function () {
                                return $scope.invNew;
                            },
                            siteUseId: function () {
                                return $scope.suid;
                            },
                            customerNo: function () {
                                return $scope.custNo;
                            },
                            legalEntity: function () {
                                return $scope.legalentty;
                            },
                            contactId: function () {
                                return "";//$scope.mailInstance.messageId;
                            },
                            relatedEmail: function () {
                                return "";
                            },
                            contactPerson: function () {
                                return "";//$scope.mailInstance.to;
                            },
                            proamount: function () {
                                return ccount.toFixed(2);
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] == "submit") {
                            // $scope.readData();
                            $scope.initList();
                        }
                    });
                }
                else if (type == "call") {
                    //added by zhangYu
                    //$scope.inv.push("1215283");

                    contactCustomerProxy.queryObject({ contactId: '0' }, function (callInstance) {
                        callInstance["title"] = "Call Create";

                        callInstance.logAction = "ALL ACCOUNT";

                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                            controller: 'contactCallCtrl',
                            size: 'lg',
                            resolve: {
                                callInstance: function () { return callInstance; },
                                custnum: function () { return $scope.custNo; },
                                invoiceIds: function () { return $scope.invNew; },
                                siteuseId: function () { return $scope.suid; },
                                legalEntity: function () {
                                    return $scope.legalentty;
                                }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {

                            if (result == "submit") {
                                ////重新查询ContactList
                                //angular.forEach($scope.num, function (cus) {
                                //    collectorSoaProxy.query({ CustNumsFCH: cus }, function (list) {
                                //        angular.forEach($scope.sendsoa, function (row) {
                                //            if (row.customerCode == cus) {
                                //                row['hisg'].data = list
                                //            }
                                //        });
                                //        //$scope.contactList.data = list;
                                //    });
                                //});
                                $scope.reSeachContactList();
                                document.getElementById("apause").style.color = "green";
                            }
                        });
                    }); //contactCustomerProxy
                }
                else if (type == "response") {
                    contactHistoryProxy.get("", function (responseInstance) {
                        responseInstance["title"] = "Response Create";
                        responseInstance.logAction = "ALL ACCOUNT";

                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-response.tpl.html',
                            controller: 'contactResponseCtrl',
                            size: 'lg',
                            resolve: {
                                responseInstance: function () { return responseInstance; },
                                custnum: function () { return $scope.custNo; },
                                invoiceIds: function () { return $scope.invNew; },
                                siteuseId: function () { return $scope.suid; },
                                legalEntity: function () {
                                    return $scope.legalentty;
                                }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "submit") {
                                $scope.reSeachContactList();
                            }
                        });
                    }, function () {

                    })
                }
                else if (type == "overdue") {

                    var invoiceNum = "";
                    for (var i = 0; i < $scope.invoiceNums.length; i++) {
                        invoiceNum += $scope.invoiceNums[i];
                        if (i < $scope.invoiceNums.length - 1) {
                            invoiceNum += ",";
                        }
                    }

                    invoiceProxy.getOverdueReason(invoiceNum, function (overdueReasonInstance) {
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-overdue.tpl.html',
                            windowClass: 'modalDialog',
                            controller: 'contactOverdueCtrl',
                            size: 'lg',
                            resolve: {
                                overdueReasonInstance: function () { return overdueReasonInstance; },
                                overdueReasons: ['baseDataProxy', function (baseDataProxy) {
                                    return baseDataProxy.SysTypeDetail("049");
                                }],
                            }
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "submit") {
                                $scope.initList();
                            }
                        });
                    }, function () {

                    })
                }
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