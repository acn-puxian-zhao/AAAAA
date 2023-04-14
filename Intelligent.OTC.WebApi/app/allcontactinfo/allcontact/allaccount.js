angular.module('app.allaccount', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/allcontactinfo/allaccount/:nums', {
                templateUrl: 'app/allcontactinfo/allcontact/allaccount.tpl.html',
                controller: 'allcontactCL',
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
    .controller('allcontactCL',
    ['$scope',
        'baseDataProxy', 'modalService', '$interval', 'invoiceProxy', '$routeParams', 'contactProxy', 'mailProxy', 'customerPaymentbankProxy',
        'customerPaymentcircleProxy', 'allinfoProxy', 'permissionProxy', 'customerProxy', 'generateSOAProxy', 'contactProxy', 'contactCustomerProxy', 'breakPtpProxy',
        '$q', 'languagelist', 'inUse', 'FileUploader', 'APPSETTING', 'siteProxy', 'uiGridConstants', 'dunningProxy', 'commonProxy', '$sce', 'collectorSoaProxy',
        'contactHistoryProxy',
        function ($scope
            , baseDataProxy, modalService, $interval, invoiceProxy, $routeParams, contactProxy, mailProxy, customerPaymentbankProxy,
            customerPaymentcircleProxy, allinfoProxy, permissionProxy, customerProxy, generateSOAProxy, contactProxy, contactCustomerProxy, breakPtpProxy,
            $q, languagelist, inUse, FileUploader, APPSETTING, siteProxy, uiGridConstants, dunningProxy, commonProxy, $sce, collectorSoaProxy, contactHistoryProxy
        ) {
            $scope.languagelist = languagelist;
            $scope.inUselist = inUse;

            $scope.mailDats = ['allcontactCL'];

            //######################### Add by Alex #############
            //contact add button show or hide
            $scope.isContactAddBtnShow = false;
            //payment bank  add button show or hide
            $scope.isPayBankAddBtnShow = false;
            $scope.isDomainAddBtnShow = false;
            //######################### Add by Alex #############

            siteProxy.GetLegalEntity("type", function (legal) {
                $scope.legallist = legal;
            }, function () {
            });
            var userId = "";
            $scope.options = [];
            $scope.gridApis = [];
            $scope.num = $routeParams.nums.split(',');
            var customers = "";
            angular.forEach($scope.num, function (numlegal) {
                customers += numlegal.split(';')[0] + ",";
            })
            customers = customers.substring(0, customers.length - 1);
            $scope.custInfo = [];
            $scope.custInfo[2] = customers; //custInfo[8]
            $scope.custInfo[3] = true; //custInfo[9]
            //$scope.site = $routeParams.sites.split(',');
            $scope.selectText = "";
            $scope.entityFlg = "";
            $scope.currCustNum = "";
            $scope.soapaymentCircle = "";
            $scope.isreadonly = true;
            $interval(function () {
                $scope.initList();

            }, 0, 1);
            $scope.initList = function () {
                allinfoProxy.query({ ColSoa: $routeParams.nums }, function (list) {
                    $scope.sendsoa = list;
                    $scope.specialnote = list[0].subLegal[0].specialNotes; var i = 0;
                    angular.forEach($scope.sendsoa, function (row) {
                        row['soapaymentCircle'] = "";
                        row['entityFlg'] = "";
                        row['hisg'] = {
                            data: row.subContactHistory,
                            columnDefs: [
                                { field: 'sortId', displayName: '#' },
                                { field: 'contactDate', displayName: 'Contact date', width: '130', cellFilter: 'date:\'yyyy-MM-dd\'' },
                                { field: 'contactType', displayName: 'Contact Type', width: '200' },
                                { field: 'comments', displayName: 'Comments', width: '500' }
                                , {
                                    name: 'tmp', displayName: 'Operation', width: '120',
                                    cellTemplate: '<div style="text-align:center">' +
                                    '<a ng-click="grid.appScope.getDetail(row.entity)"> detail </a></div>'
                                }

                            ]

                        };
                        row['disputeList'] = {
                            data: row.subDisputeList,
                            multiSelect: false,
                            columnDefs: [
                                { field: 'sortId', displayName: '#', width: '70' },
                                { field: 'createDate', displayName: 'Dispute date', width: '130', cellFilter: 'date:\'yyyy-MM-dd\'' },
                                {
                                    field: 'issueReason', displayName: 'Issue Reason', width: '200'
                                    //cellTemplate: '<div hello="{valueMember: \'issueReason\', basedata: \'grid.appScope.bdDisputeReasonStatus\'}"></div>'
                                },
                                {
                                    field: 'status', displayName: 'Status', width: '200'
                                    //cellTemplate: '<div hello="{valueMember: \'status\', basedata: \'grid.appScope.bdStatus\'}"></div>'
                                },
                                { field: 'comments', displayName: 'Comments', width: '377' },
                                {
                                    field: '""', displayName: 'Operation', width: '120',
                                    cellTemplate: '<div class="ngCellText" style="text-align:center" ng-class="col.colIndex()">' +
                                    '<a ng-click="grid.appScope.getDisputeDetail(row.entity.id)"> detail </a></div>'
                                }
                            ],

                            onRegisterApi: function (gridApi) {
                                //set gridApi on scope
                                $scope.gridApi = gridApi;
                            }
                        };
                        row['cg'] = {
                            //data: row.subContact,
                            columnDefs: [
                                { field: 'name', displayName: 'Contact Name' },
                                { field: 'legalEntity', displayName: 'Legal Entity' },
                                { field: 'emailAddress', displayName: 'Email', width: '210' },
                                { field: 'department', displayName: 'Department' },
                                { field: 'title', displayName: 'Title' },
                                { field: 'number', displayName: 'Contact Number' },
                                { field: 'comment', displayName: 'Comment' },
                                {
                                    field: 'toCc', displayName: 'To/Cc',
                                    cellTemplate: '<div >{{grid.appScope.CheckType(row.entity)}}</div>'
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
                        row['pcgshow'] = false;

                        row['cdg'] = {
                            //data: row.subContactDomain,
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
                        };
                        row['cdgshow'] = false;
                        angular.forEach(row.subLegal, function (rowItem) {
                            rowItem['gridoption'] =
                                {
                                    data: rowItem.subInvoice,
                                    enableFiltering: true,
                                    showGridFooter: true,
                                    enableFiltering: true,
                                    columnDefs: [
                                        {
                                            field: 'invoiceNum', name: 'tmp', displayName: 'Invoice NO.', enableCellEdit: false, width: '110', pinnedLeft: true
                                            , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceNum}}</a></div>'
                                        },
                                        { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'customerNum', enableCellEdit: false, displayName: 'CustomerNo.', width: '105' },
                                        { field: 'inClass', enableCellEdit: false, displayName: 'Class', width: '105' },
                                        { field: 'siteUseId', enableCellEdit: false, displayName: 'siteUseId.', width: '105', visible: false },
                                        { field: 'legalEntity', enableCellEdit: false, displayName: 'legalEntity', width: '110', visible: false },
                                        { field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'invoiceTrack', enableCellEdit: false, displayName: 'current status', width: '160' },
                                        { field: 'creditTerm', enableCellEdit: false, displayName: 'Payment Term Name', width: '110' },
                                        { field: 'invoiceCurrency', enableCellEdit: false, displayName: 'Inv Curr Code', width: '122' },
                                        { field: 'dueDays', enableCellEdit: false, displayName: 'Due Days', width: '122' },
                                        { field: 'balancE_AMT', enableCellEdit: false, displayName: 'Amt Remaining', width: '122' },
                                        { field: 'woVat_AMT', enableCellEdit: false, displayName: 'Amount Wo Vat', width: '122' },
                                        { field: 'agingBucket', enableCellEdit: false, displayName: 'Aging Bucket', width: '120' },
                                        { field: 'ebname', enableCellEdit: false, displayName: 'Eb', width: '122' },
                                        { field: 'vatNum', enableCellEdit: false, displayName: 'VAT No.', width: '122' },
                                        { field: 'VatDate', enableCellEdit: false, displayName: 'VAT date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'TRACK_DATE', enableCellEdit: false, displayName: 'last update date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'ptpDate', enableCellEdit: false, displayName: 'PTP Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'PTP_Identified_Date', enableCellEdit: false, displayName: 'PTP Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        {
                                            name: 'Dispute_Reason', displayName: 'Dispute(Y / N)', width: '110'
                                            , cellTemplate: '<div style="height:30px;vertical-align:middle">Y</div>'
                                        },
                                        { field: 'Dispute_Date', enableCellEdit: false, displayName: 'Dispute Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        { field: 'Dispute_Reason', enableCellEdit: false, displayName: 'Dispute Reason', width: '110' },
                                        {
                                            field: 'Owner_Department', enableCellEdit: false, displayName: 'Action Owner-Department', cellFilter: 'number:2', type: 'number', width: '200'
                                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                                if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                                    return 'uigridred';
                                                }
                                            }
                                        },
                                        {
                                            field: 'collectoR_NAME', enableCellEdit: false, displayName: 'Collector', cellFilter: 'number:2', type: 'number', width: '200'
                                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                                if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                                    return 'uigridred';
                                                }
                                            }
                                        },
                                        { field: 'Next_Action_Date', enableCellEdit: false, displayName: 'Next Action Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '110' },
                                        {
                                            field: 'comments', name: 'tmp1', displayName: 'Invoice Memo', width: '140',
                                            cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.invoiceId,row.entity.invoiceNum,row.entity.comments)"></a>'
                                            + '<label id="lbl{{row.entity.invoiceId}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum,row.entity.comments,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.comments.substring(0,7)}}...</label></div>'
                                        }
                                        //                        {
                                        //                                field: 'invoiceNum', name: 'tmp', displayName: 'Invoice NO.', enableCellEdit: false, width: '100', pinnedLeft: true
                                        //                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceNum}}</a></div>'
                                        //                        },
                                        //                        { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                                        //                        { field: 'creditTerm', enableCellEdit: false, displayName: 'Credit Term', width: '100' },
                                        //                        { field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '95' },
                                        //                        { field: 'purchaseOrder', enableCellEdit: false, displayName: 'PO Num', width: '160' },
                                        //                        { field: 'saleOrder', enableCellEdit: false, displayName: 'SO Num', width: '122' },
                                        //                        { field: 'rbo', enableCellEdit: false, displayName: 'RBO', width: '120' },
                                        //                        { field: 'invoiceCurrency', enableCellEdit: false, displayName: 'Currency', width: '90' },
                                        //                        {
                                        //                            field: 'outstandingInvoiceAmount', enableCellEdit: false, displayName: 'Outstanding Invoice Amount', cellFilter: 'number:2', type: 'number', width: '200'
                                        //                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                        //                                if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                        //                                    return 'uigridred';
                                        //                                }
                                        //                            }
                                        //                        },
                                        //                        {
                                        //                            field: 'originalInvoiceAmount', enableCellEdit: false, displayName: 'Original Invoice Amount', cellFilter: 'number:2', type: 'number', width: '200'
                                        //                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                        //                                if (parseFloat(grid.getCellValue(row, col)) < 0) {
                                        //                                    return 'uigridred';
                                        //                                }
                                        //                            }
                                        //                        },
                                        //                        { field: 'daysLate', enableCellEdit: false, displayName: 'Days Late', width: '80', type: 'number' },
                                        //                        {
                                        //                            field: 'status', enableCellEdit: false, displayName: 'Status', width: '80'
                                        //                            , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                        //                                if (grid.getCellValue(row, col) === 'Dispute') {
                                        //                                    return 'uigridred';
                                        //                                }
                                        //                            }, filter: {
                                        //                                term: '',
                                        //                                type: uiGridConstants.filter.SELECT,
                                        //                                selectOptions: [
                                        //                                    { value: 'Open', label: 'Open' },
                                        //                                    { value: 'PTP', label: 'PTP' },
                                        //                                    { value: 'Dispute', label: 'Dispute' },
                                        //                                    { value: 'PartialPay', label: 'PartialPay' },
                                        //                                    { value: 'Broken PTP', label: 'Broken PTP' },
                                        //                                    { value: 'Hold', label: 'Hold' },
                                        //                                    { value: 'Payment', label: 'Payment' }]
                                        //                            }
                                        //                        },
                                        //                        {
                                        //                            field: 'invoiceTrack', enableCellEdit: false, displayName: 'Invoice Track', width: '100'
                                        //                            , filter: {
                                        //                                term: '',
                                        //                                type: uiGridConstants.filter.SELECT,
                                        //                                selectOptions: [
                                        //                                    { value: 'SOA Sent', label: 'SOA Sent' },
                                        //                                    { value: 'Second Reminder Sent', label: 'Second Reminder Sent' },
                                        //                                    { value: 'Final Reminder Sent', label: 'Final Reminder Sent' },
                                        //                                    { value: 'Dispute', label: 'Dispute' },
                                        //                                    { value: 'PTP', label: 'PTP' },
                                        //                                    { value: 'Payment Notice Received', label: 'Payment Notice Received' },
                                        //                                    { value: 'Broken PTP', label: 'Broken PTP' },
                                        //                                    { value: 'First Broken Sent', label: 'First Broken Sent' },
                                        //                                    { value: 'Second Broken Sent', label: 'Second Broken Sent' },
                                        //                                    { value: 'Hold', label: 'Hold' },
                                        //                                    { value: 'Agency Sent', label: 'Agency Sent' },
                                        //                                    { value: 'Write Off', label: 'Write Off' },
                                        //                                    { value: 'Paid', label: 'Paid' },
                                        //                                    { value: 'Bad Debit', label: 'Bad Debit' },
                                        //                                    { value: 'Open', label: 'Open' },
                                        //                                    { value: 'Close', label: 'Close' },
                                        //                                    { value: 'Contra', label: 'Contra' },
                                        //                                    { value: 'Breakdown', label: 'Breakdown' }
                                        //                                ]
                                        //                            }
                                        //                        },
                                        //                         { field: 'ptpDate', enableCellEdit: false, displayName: 'PTP Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                                        //                        {
                                        //                            field: 'documentType', enableCellEdit: false, displayName: 'Document Type', width: '120'
                                        //                            , filter: {
                                        //                                term: '',
                                        //                                type: uiGridConstants.filter.SELECT,
                                        //                                selectOptions: [
                                        //                                    { value: 'DM', label: 'DM' },
                                        //                                    { value: 'CM', label: 'CM' },
                                        //                                    { value: 'INV', label: 'INV' },
                                        //                                    { value: 'Payment', label: 'Payment' }]
                                        //                            }, cellFilter: 'mapClass'
                                        //                        },
                                        //                        {
                                        //                            field: 'comments', name: 'tmp1', displayName: 'Invoice Memo', width: '120',
                                        //                            cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.invoiceId,row.entity.invoiceNum,row.entity.comments)"></a>'
                                        //+ '<label id="lbl{{row.entity.invoiceId}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum,row.entity.comments,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.comments.substring(0,7)}}...</label></div>'
                                        //                        }
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
                    }, 0, 1);
                    //$interval(function () {
                    //    $scope.selectall();

                    //}, 0, 1);

                    // $scope.$broadcast("MAIL_DATAS_REFRESH", $scope.mailDats[0]);
                });

            };


            $scope.getDisputeDetail = function (disputeId) {
                window.open('#/disputetracking/dispute/' + disputeId);
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

                $scope.selectInvoice = function () {

                    var isCustContact = $scope.sendsoa[0]["isCostomerContact"];
                    var reconciliationDay = $scope.sendsoa[0]["reconciliationDay"];
                    for (i = 0; i < $scope.gridApis[0].grid.options.data.length; i++) {
                        if ($scope.gridApis[0].grid.options.data[i]["inClass"] == "INV" && (($scope.gridApis[0].grid.options.data[i]["ptpDate"] != null && $scope.gridApis[0].grid.options.data[i]["ptpDate"] <= reconciliationDay) || ($scope.gridApis[0].grid.options.data[i]["ptpDate"] == null && $scope.gridApis[0].grid.options.data[i]["dueDate"] <= reconciliationDay)) || isCustContact == "N") {
                            $scope.gridApis[0].selection.selectRow($scope.gridApis[0].grid.options.data[i]);
                        }
                    }

                    //for (i = 0; i < $scope.gridApis[0].grid.options.data.length; i++) {
                    //    if ($scope.gridApis[0].grid.options.data[i]["inClass"] == "INV") {
                    //        $scope.gridApis[0].selection.selectRow($scope.gridApis[0].grid.options.data[i]);
                    //    }
                    //}

                }
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
                    $scope.gridApis[k].selection.selectAllRows();
                }
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
            $scope.check = function () {
                var language = "";
                $scope.inv = [];
                $scope.cus = [];
                $scope.suid = "";
                $scope.custNo = "";
                $scope.legalentty = "";
                $scope.invNew = [];
                var isfirstrow = true;
                var isonecustomer = true;
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
                                $scope.shortsub.push(rowItem.customerName);
                                //$scope.shortsub.push('($' + $scope.formatNumber(rowItem.standardInvoiceAmount, 2, 1) + ')');
                                blc = rowItem.standardInvoiceAmount;
                            } else {
                                //$scope.shortsub.pop();
                                //$scope.shortsub.push('($' + $scope.formatNumber(blc, 2, 1) + ')');
                            }
                        }
                    });
                }
                angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                    //alert(rowItem.invoiceNum);
                    if (rowItem.invoiceId != 0) {
                        $scope.invNew.push(rowItem.invoiceId);
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
                    var modalDefaults = {
                        templateUrl: 'app/common/mail/mail-instance.tpl.html',
                        controller: 'mailInstanceCtrl',
                        size: 'customSize',
                        resolve: {
                            custnum: function () { return $scope.strcus; },
                            siteuseId: function () { return $scope.suid; },
                            invoicenums: function () { return $scope.invoiceNums; },
                            mType: function () { return "001"; },
                            //                                    selectedInvoiceId: function () { return $scope.inv; },
                            instance: function () {
                                return getMailInstance($scope.strcus, $scope.suid);
                            },
                            mailDefaults: function () {
                                return {
                                    mailType: 'NE',
                                    templateChoosenCallBack: selectMailInstanceById,
                                    mailUrl: generateSOAProxy.sendEmailUrl
                                };
                            }
                        },
                        windowClass: 'modalDialog'
                    };

                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            //$scope.reSeachContactList();
                            if (result == "sent")
                            {
                                $scope.chengbutton();
                                //document.getElementById("asend").style.color = "green";
                            }
                            $scope.initList();
                        }, function (err) {
                            alert(err);
                        });

                }
            }


            var getMailInstanceMain = function (custNums, suid, ids) {

                var instanceDefered = $q.defer();

                generateSOAProxy.getMailInstance(custNums, suid, ids, function (res) {
                    var instance = res;
                    renderInstance(instance, custNums, suid);

                    instanceDefered.resolve(instance);
                }, function (error) {
                    alert(error);
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

            //=========added by alex body??????+Currency=========

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

            var getMailInstance = function (custNums, suid) {
                var instance = {};
                var allDefered = $q.defer();

                $q.all([
                    getMailInstanceMain(custNums, suid, $scope.inv),
                    getMailInstanceTo(custNums, suid)
                    //=========added by alex body??????+Currency======
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
                        //=========added by alex body??????+Currency======
                        //instance.attachments = results[2].attachments;
                        //instance.attachment = results[2].attachment;
                        //=====================================================
                        allDefered.resolve(instance);
                    });

                return allDefered.promise;
            };

            var selectMailInstanceById = function (custNums, id, siteUseId, templateType, templatelang, ids) {
                var instance = {};
                var instanceDefered = $q.defer();
                //=========added by alex body??????+Currency=== $scope.inv ?? ======
                generateSOAProxy.getMailInstById(custNums, id, siteUseId, templateType, templatelang, ids, function (res) {
                    instance = res;
                    renderInstance(instance, custNums);

                    instanceDefered.resolve(instance);
                });

                return instanceDefered.promise;
            };

            var renderInstance = function (instance, custNums) {
                //subject
                instance.subject = $scope.shortsub.join('-');
                //invoiceIds
                instance.invoiceIds = $scope.inv;
                //soaFlg
                instance.soaFlg = "N"; // N: no invoice change
                //Bussiness_Reference
                var customerMails = [];
                angular.forEach(custNums.split(','), function (cust) {
                    customerMails.push({ MessageId: instance.messageId, CustomerNum: cust });
                });
                instance.CustomerMails = customerMails; //$routeParams.nums;
                //mailTitle
                instance["title"] = "Create SOA";
                //mailType
                instance.mailType = "001,SOA";
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



            $scope.conshow = function (id) {
                $scope.conallhide();
                $("#conhide" + id).show();
                $("#conshow" + id).hide();

                //############ Add by Alex #####
                $("#pbhide" + id).hide();
                $("#pbshow" + id).show();
                $("#pchide" + id).hide();
                $("#pcshow" + id).show();

                $("#cdghide" + id).hide();
                $("#cdgshow" + id).show();
                //contact add button show or hide
                $scope.isContactAddBtnShow = true;
                //payment bank  add button show or hide
                $scope.isPayBankAddBtnShow = false;
                $scope.isDomainAddBtnShow = false;
                //##############################

                //collectorSoaProxy.query({ CustNumFCon: id }, function (list) {
                //    $scope.currCustNum = id;
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

                collectorSoaProxy.forContactor(id + ',' + $routeParams.nums.split(';')[2], function (list) {
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
                $scope.isContactAddBtnShow = false;
            }

            $scope.pbshow = function (id) {
                $("#pbhide" + id).show();
                $("#pbshow" + id).hide();

                //############ Add by Alex #####
                $("#conshow" + id).show();
                $("#conhide" + id).hide();
                $("#pcshow" + id).show();
                $("#pchide" + id).hide();
                $("#cdghide" + id).hide();
                $("#cdgshow" + id).show();
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
                $scope.isPayBankAddBtnShow = false;
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
                $("#cdghide" + id).hide();
                $("#cdgshow" + id).show();
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
                $scope.isDomainAddBtnShow = false;
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
                    cus = cus.split(',')[0];
                    allinfoProxy.query({ CustNumsFCH: cus }, function (list) {
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

            //**********************************edit special notes********************
            $scope.saveNote = function (cus, legal, note, siteuseid) {
                var list = [];
                list.push(1);//1:SpeicalNotes;2:InvoiceComm
                list.push(cus);
                list.push(legal);
                list.push(note);
                list.push(siteuseid);
                collectorSoaProxy.savecommon(list, function () {
                    alert('success!');
                })
            }

            //**********************************addContactDomain********************
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
            //##############  edit  ######################
            $scope.EditContacterInfo = function (row) {
                //    row.customerNum = $scope.currCustNum;
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                    controller: 'contactorEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        num: function () {
                            //  return row.customerNum;
                            return $scope.currCustNum + ',' + $routeParams.nums.split(';')[2];
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
                    // $scope.conshow(row.bkcustomerNum);
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
            //##############  remove  ######################
            $scope.Delcontacter = function (row) {
                var cusid = row.id;
                contactProxy.delContactor(cusid, function () {
                    //  $scope.conshow(row.customerNum);
                    //  var cus = row.bkCustomerNum;
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

            //##############  upload  ######################

            //???uploader
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
                        //collectorSoaProxy.query({CustNumFPc: cus}, function (list) {
                        //angular.forEach($scope.sendsoa, function (row) {
                        customerPaymentcircleProxy.searchPaymentCircle(cus, $scope.entityFlg, function (paydate) {
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
                            contactCustomerProxy.queryObject({
                                contactId: row.contactId
                            }, function (callInstance) {
                                //alert(row.contacterId);
                                //callInstance["contacterId"] = row.contacterId;
                                callInstance["title"] = "Call Detail";

                                var modalDefaults = {
                                    templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                                    controller: 'contactCallCtrl',
                                    size: 'lg',
                                    resolve: {
                                        callInstance: function () {
                                            return callInstance;
                                        },
                                        custnum: function () {
                                            return "";
                                        },
                                        invoiceIds: function () {
                                            return "";
                                        }
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

            //****format number
            /* 
                ???????????. 
                @param num ??(Number??String) 
                @param cent ???????(Number) 
                @param isThousand ??????? 0:???,1:??(????); 
                @return ??????,?'1,234,567.45' 
                @type String 
            */
            $scope.formatNumber =
                function (num, cent, isThousand) {
                    num = num.toString().replace(/\$|\,/g, '');
                    if (isNaN(num))//???????????. 
                        num = "0";
                    if (isNaN(cent))//?????????????. 
                        cent = 0;
                    cent = parseInt(cent);
                    cent = Math.abs(cent);//??????,??????. 
                    if (isNaN(isThousand))//????????????????. 
                        isThousand = 0;
                    isThousand = parseInt(isThousand);
                    if (isThousand < 0)
                        isThousand = 0;
                    if (isThousand >= 1) //?????????0?1 
                        isThousand = 1;
                    sign = (num == (num = Math.abs(num)));//????(?/??) 
                    //Math.floor:???????????????? 
                    num = Math.floor(num * Math.pow(10, cent) + 0.50000000001);//?????????????.??????????. 
                    cents = num % Math.pow(10, cent); //???????. 
                    num = Math.floor(num / Math.pow(10, cent)).toString();//???????. 
                    cents = cents.toString();//??????????,????????. 
                    while (cents.length < cent) {//???????????. 
                        cents = "0" + cents;
                    }
                    if (isThousand == 0) //???????. 
                        return (((sign) ? '' : '-') + num + '.' + cents);
                    //?????????????. 
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

            //$scope.calcu = function (customerNum, legalEntity) {
            //    if ($scope.saveflg == 1) {
            //        commonProxy.queryObject({ customerNum: customerNum, legalEntity: legalEntity }, function (tracking) {
            //            currTracking.reminder2thDate = tracking.reminder2thDate;
            //            currTracking.reminder3thDate = tracking.reminder3thDate;
            //            currTracking.holdDate = tracking.holdDate;

            //            currTracking.soaStatus = tracking.soaStatus;
            //            currTracking.reminder2thStatus = tracking.reminder2thStatus;
            //            currTracking.reminder3thStatus = tracking.reminder3thStatus;
            //            currTracking.holdStatus = tracking.holdStatus;
            //        });
            //    }
            //}

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
            //change table
            $scope.changetab = function (type) {
                //get selected invoiceIds
                $scope.inv = [];
                $scope.invObjArr = [];
                $scope.invoObj = { num: {}, status: {} };

                $scope.pm = [];



                for (j = 0; j < $scope.gridApis.length; j++) {
                    angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                        //alert(rowItem.invoiceNum);
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                            $scope.invoObj.num = rowItem.invoiceNum;
                            $scope.pm.push(rowItem.balancE_AMT);
                            $scope.invoObj.status = rowItem.status;
                            $scope.invObjArr.push($scope.invoObj);
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
                    return;
                }

                if (type == "call") {
                    //added by zhangYu
                    $scope.invNew = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;

                    //alert($scope.inv);


                    angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                        //alert(rowItem.invoiceNum);
                        if (rowItem.invoiceId != 0) {
                            $scope.invNew.push(rowItem.invoiceId);
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

                    if (isonecustomer == false) {
                        alert("Please choose 1 customer at most .")
                        return;
                    }

                    var relatedMail = "";

                    if ($scope.custInfo[1] == "Only select one mail!") {
                        alert($scope.custInfo[1]);
                        return;
                    }

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
                                //$scope.reSeachContactList();
                                if (result == "submit") {
                                    //document.getElementById("apause").style.color = "green";
                                    $scope.chengbutton();
                                }
                                $scope.initList();
                            });
                        }); //contactCustomerProxy
                    }
                        //**************************************************************added by zhangYu***********************Start
                    else if (type == "confirmbreakptp") {
                        if ($scope.inv == "" || $scope.inv == null) {
                            alert("Please choose 1 invoice at least .")
                            return;
                        }

                    if ($scope.custInfo[1] == "Only select one mail!") {
                        alert($scope.custInfo[1]);
                        return;
                    }

                    //relate Mail
                    //if (!$scope.custInfo[1].id) {
                    //    if (confirm("No mail selected ! continue ?")) {
                    //    }
                    //    else {
                    //        return;
                    //    }
                    //}

                    breakPtpProxy.getBreakPTP(function (invoLogInstance) {
                        invoLogInstance["title"] = "Confirm Break PTP";
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-breakptp.tpl.html',
                            controller: 'contactBreakPTPCtrl',
                            size: 'lg',
                            resolve: {
                                Instance: function () { return invoLogInstance; },
                                custnum: function () { return customers; },
                                contactId: function () { return $scope.custInfo[1].messageId; },
                                invoiceIds: function () { return $scope.inv; }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {
                        }).then(function (result) {
                            if (result == "submit") {
                                $scope.initList();
                            }

                        });
                    }); //contactCustomerProxy

                }//confirmbreakptp end
                else if (type == "dispute") {
                    $scope.invNew = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;

                    //alert($scope.inv);


                    angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                        //alert(rowItem.invoiceNum);
                        if (rowItem.invoiceId != 0) {
                            $scope.invNew.push(rowItem.invoiceId);
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

                    if (isonecustomer == false) {
                        alert("Please choose 1 customer at most .")
                        return;
                    }

                    var relatedMail = "";
                    //relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");
                    //if ($scope.custInfo[1] == "Only select one mail!") {
                    //    alert($scope.custInfo[1]);
                    //    return;
                    //}

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
                                contactId: function () { return $scope.custInfo[1].messageId; },
                                relatedEmail: function () { return relatedMail; },
                                contactPerson: function () { return $scope.custInfo[1].to; },
                                siteUseId: function () {
                                    return $scope.suid
                                },
                                legalEntity: function () {
                                    return $scope.legalentty;
                                },
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "submit") {
                                $scope.initList();

                                $("#").addClass('ng-hide');
                                $("#conshow").show();
                                $("#conhide").hide();
                                $scope.contactShow = false;
                                $("#payshow").show();
                                $("#payhide").hide();
                                $scope.bankShow = false;
                                $("#calshow").show();
                                $("#calhide").hide();
                                $scope.calenderShow = false;
                                $("#domhide").hide();
                                $("#domshow").show();
                                $scope.domainShow = false;
                                //Get Dispute List
                                contactCustomerProxy.query({
                                    strCusNumber: $routeParams.nums
                                }, function (list) {
                                    $scope.lstDispute = list;
                                });
                            }
                        });
                    });
                }
                else if (type == "ptp") {
                    $scope.invNew = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;
                    angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                        //alert(rowItem.invoiceNum);
                        if (rowItem.invoiceId != 0) {
                            $scope.invNew.push(rowItem.invoiceId);
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
                    //alert($scope.inv);
                    if (isonecustomer == false) {
                        alert("Please choose 1 customer at most .")
                        return;
                    }

                    var relatedMail = "";

                    //if ($scope.custInfo[1] == "Only select one mail!") {
                    //    alert($scope.custInfo[1]);
                    //    return;
                    //}

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
                                return $scope.custInfo[1].messageId;
                            },
                            relatedEmail: function () {
                                return relatedMail;
                            },
                            contactPerson: function () {
                                return $scope.custInfo[1].to;
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
                        }
                    });
                }
                else if (type == "notice") {
                    $scope.invNew = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;

                    angular.forEach($scope.gridApis[0].selection.getSelectedRows(), function (rowItem) {
                        //alert(rowItem.invoiceNum);
                        if (rowItem.invoiceId != 0) {
                            $scope.invNew.push(rowItem.invoiceId);
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
                    //alert($scope.inv);
                    if (isonecustomer == false) {
                        alert("Please choose 1 customer at most .")
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
                                return $scope.custInfo[1].messageId;
                            },
                            relatedEmail: function () {
                                return relatedMail;
                            },
                            contactPerson: function () {
                                return $scope.custInfo[1].to;
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
                        }
                    });
                }

                //
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