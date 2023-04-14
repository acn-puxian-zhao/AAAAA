angular.module('app.dispute', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/disputetracking/dispute/:disputeId', {
                templateUrl: 'app/disputetracking/dispute/dispute-list.tpl.html',
                controller: 'disputeCtrl',
                resolve: {
                    bdDisputeReasonStatus: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("025");
                    }]
                }
            });
    }])

    .controller('disputeCtrl',
    ['$scope', '$routeParams', '$interval', 'disputeTrackingProxy', 'commonProxy', 'mailProxy', 'modalService',
        'contactCustomerProxy', 'permissionProxy', 'collectorSoaProxy', 'customerProxy', 'invoiceProxy', '$sce', 'generateSOAProxy', '$q', 'contactProxy',
        function ($scope, $routeParams, $interval, disputeTrackingProxy, commonProxy, mailProxy, modalService,
            contactCustomerProxy, permissionProxy, collectorSoaProxy, customerProxy, invoiceProxy, $sce, generateSOAProxy, $q, contactProxy) {
            $scope.$parent.helloAngular = "OTC - Dispute List";
            $scope.disputeId = $routeParams.disputeId;
            $scope.pagereadonly = false;
            $scope.issReason = "";
            $scope.disputeStatus = "";
            $scope.disputeStatusName = "";
            $scope.disputeDate = "";
            $scope.disputeNotes = "";
            $scope.customerNumber = "";
            $scope.customerName = "";
            $scope.disputeStatusCode = "";
            $scope.disputeStatusName = "";
            $scope.siteUseId = "";
            $scope.invoiceNums = "";
            $scope.disputeReason = "";
            $scope.disputeReasonCode = "";
            $scope.legalEntity = "";
            $scope.customerNameHead = "";
            $scope.actionOwnerDepartmentCode = "";
            $scope.actionOwnerDepartmentName = "";
            custNums=$scope.customerNumber;
            //$scope.custInfo = [];
            //$scope.custInfo[2] = $routeParams.nums; //custInfo[8]
            //$scope.custInfo[3] = true; //custInfo[9]
            $interval(function () {
                $scope.initList();

            }, 0, 1);

            $scope.fileTypeList = [
                { "id": "ALL", "levelName": 'ALL' },
                { "id": "PDF", "levelName": 'PDF' },
                { "id": "XLS", "levelName": 'XLS' }
            ];

            $scope.fileType = "XLS";

            disputeTrackingProxy.queryObject({ id: $scope.disputeId }, function (list) {
                //console.log(list);
                $scope.issReason = list[0];
                $scope.disputeStatus = list[5];
                $scope.disputeDate = list[2];
                $scope.disputeNotes = list[3];
                $scope.customerNumber = list[4];
                $scope.disputeStatusCode = list[5];
                $scope.siteUseId = list[6];
                $scope.legalEntity = list[7];
                $scope.disputeReason = list[8];
                $scope.customerNameHead = list[9];
                $scope.disputeStatusName = list[10];
                $scope.actionOwnerDepartmentCode = list[11];
                $scope.actionOwnerDepartmentName = list[12]; 
                $scope.disputeReasonCode = list[13]; 
                //Get Call List
                commonProxy.query({ customerNum: list[4] }, function (list) {
                    $scope.callhistorylst = list;
                });
                //Get Mail List
                mailProxy.searchMail('&customerNum=' + list[4] + "&customerName=&$filter=(Category ne 'Draft')", '', function (list) {
                    $scope.maillst = list;
                });
                // Get customer name
                customerProxy.getByNum($scope.customerNumber, function (cust) {
                    $scope.customerName = cust.customerName;
                });
            });

            //*****************************************Dispute List***************************s

            $scope.disputeList = {
                data: 'disputelst',
                enableFiltering: true,
                columnDefs: [
                    { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    {
                        field: 'invoice',
                        name: 'invoiceNum', displayName: 'Invoice NO.', enableCellEdit: false, width: '100', pinnedLeft: true,
                        cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceNum}}</a></div>'
                    },
                    { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    { field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '95' },
                    { field: 'trackStates', enableCellEdit: false, displayName: 'Current Status', width: '105' },
                    { field: 'creditTerm', enableCellEdit: false, displayName: 'Payment Term Name', width: '140' },
                    { field: 'invoiceCurrency', enableCellEdit: false, displayName: 'Inv Curr Code.', width: '100' },
                    { field: 'dueDays', enableCellEdit: false, displayName: 'Due Days', width: '100', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'outstandingInvoiceAmount', enableCellEdit: false, displayName: 'Amt Remaining', width: '130', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'woVat_AMT', enableCellEdit: false, displayName: 'Amount Wo Vat', width: '130', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'agingBucket', enableCellEdit: false, displayName: 'Aging Bucket', width: '122' },
                    { field: 'eb', enableCellEdit: false, displayName: 'Eb', width: '140' },
                    { field: 'vatNum', enableCellEdit: false, displayName: 'VAT NO.', width: '90' },
                    { field: 'vatDate', enableCellEdit: false, displayName: 'VAT Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    { field: 'tracK_DATE', enableCellEdit: false, displayName: 'Last Update Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '120' },
                    { field: 'ptpIdentifiedDate', enableCellEdit: false, displayName: 'PTP Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '150' },
                    { field: 'ptpDate', enableCellEdit: false, displayName: 'PTP date ', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    //{ field: 'dispute', enableCellEdit: false, displayName: 'Dispute(Y/N)', width: '100' },
                    //{ field: 'disputeIdentifiedDate', enableCellEdit: false, displayName: 'Dispute Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '155' },
                    //{ field: 'disputeReason', enableCellEdit: false, displayName: 'Dispute Reason', width: '110' },
                    //{ field: 'actionOwnerDepartment', enableCellEdit: false, displayName: 'Action Owner-Department', width: '170' },
                    { field: 'collectorName', enableCellEdit: false, displayName: 'Collector', width: '90' },
                    //{ field: 'nextActionDate', enableCellEdit: false, displayName: 'Next Action Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '120' },
                    {
                        field: 'comments', displayName: 'Invoice Memo', width: '120',
                        cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.invoiceId,row.entity.invoiceNum,row.entity.comments)"></a>'
                        + '<label id="lbl{{row.entity.invoiceId}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum,row.entity.comments,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.comments.substring(0,7)}}...</label></div>'
                    }
                ],

                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };
            //$scope.mockdata =
            //    [
            //        { "invoiceNum": "604020517073", "invoiceDate": "42944", "dueDate": "2017/9/26", "currentStatus": "Wait for First Time Dispute contact", "paymentTerm": "60 NET", "invcurrcode": "USD", "dueDays": "-44", "amtRemaining": "45900    ", "amountWoVat": "45,900", "agingBucket": "total_future_due", "eb": "HK_HK_HongKong_Core_HongKong_HongKong02", "vatNo": "12344672", "vatDate": "2017/7/28", "lastupdatedate": "2017/8/7", "pTPIdentified": "2017/6/18", "pTPdate": "2017/6/28", "dispute": "Y", "disputeIdentifiedDate": "2017/8/7", "disputeReason": "Wrong Credit", "actionOwnerDepartment": " Credit Team", "Collector": "AA", "nextActionDate": "2017/8/12", "comments": "any memos…" },
            //        { "invoiceNum": "604020516985", "invoiceDate": "42944", "dueDate": "2017/9/26", "currentStatus": "Wait for Second Time Dispute contact", "paymentTerm": "60 NET", "invcurrcode": "USD", "dueDays": "-44", "amtRemaining": "640    ", "amountWoVat": "640", "agingBucket": "total_future_due", "eb": "HK_HK_HongKong_Core_HongKong_HongKong02", "vatNo": "12344673", "vatDate": "2017/7/28", "lastupdatedate": "2017/8/7", "pTPIdentified": "2017/8/21", "pTPdate": "2017/8/31", "dispute": "Y", "disputeIdentifiedDate": "2017/8/2", "disputeReason": "Currency Issue", "actionOwnerDepartment": "Billing Team", "Collector": "DD", "nextActionDate": "2017/8/8", "comments": "any memos…" },
            //        { "invoiceNum": "604020516984", "invoiceDate": "42944", "dueDate": "2017/9/26", "currentStatus": "Wait for Dispute respond", "paymentTerm": "60 NET", "invcurrcode": "USD", "dueDays": "-44", "amtRemaining": "640    ", "amountWoVat": "640", "agingBucket": "total_future_due", "eb": "HK_HK_HongKong_Core_HongKong_HongKong02", "vatNo": "12344674", "vatDate": "2017/7/28", "lastupdatedate": "2017/8/7", "pTPIdentified": "2017/8/11", "pTPdate": "2017/8/21", "dispute": "Y", "disputeIdentifiedDate": "2017/8/2", "disputeReason": "Wrong service/billing period", "actionOwnerDepartment": "Billing Team", "Collector": "DD", "nextActionDate": "2017/8/22", "comments": "any memos…" },


            //    ]

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
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.invoiceId != 0) {
                        $scope.inv.push(rowItem.invoiceId);
                    }
                });
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
                if (memo.length > 8000) {
                    alert("input 8000 character at most");
                    return;
                }
                list.push("5");
                list.push(invoiceIds);
                list.push(memo);
                collectorSoaProxy.savecommon(list, function () {
                    $scope.batchsaveBack(memo);
                    $scope.batcheditMemoClose();
                });
            }

            $scope.saveBack = function (invoiceId, memo) {
                angular.forEach($scope.gridApi.grid.rows, function (rowItem) {
                    if (rowItem.entity.invoiceId == invoiceId) {
                        rowItem.entity.comments = memo;
                    }
                });
            }

            $scope.batchsaveBack = function (memo) {
                //for (k = 0; k < $scope.gridApis.length; k++) {
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.invoiceId != 0) {
                        rowItem.comments = memo;
                    }
                });
                //}
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

            //*****************************************Dispute List***************************e
            //Get Dispute List
            $scope.initList = function () {
                disputeTrackingProxy.query({ disId: $scope.disputeId, suid: $scope.siteUseId }, function (list) {
                    if (!list || list.length == 0) {
                        $scope.pagereadonly = true;
                        alert('Dispute do not exist or have no authority.');
                        return;
                    }
                    $scope.disputelst = list;
                });
            };

            $scope.clearPTP = function () {
                if ($scope.gridApi.selection.getSelectedRows().length == 0) {
                    alert("Please choose 1 invoice at least .");
                    return;
                }
                if (confirm("Are you sure clear PTP")) {
                    var idList = new Array();
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            idList.push(rowItem.invoiceId + '|' + rowItem.invoiceNum);
                        }
                    });

                    invoiceProxy.clearPTP(idList, function (res) {
                        alert(res);
                        $scope.initList();
                    }, function (res) {
                        alert(res);
                    });
                }

            }

            //*************************************Dispute Status Change List*****************s

            $scope.disputeStatusChangeList = {
                data: 'disStatusChangelst',
                //   data: 'mockdisStatusChangelst',
                columnDefs: [
                    { field: 'hisDate', displayName: 'Date', cellFilter: 'date:\'yyyy-MM-dd hh:mm:ss\'', width: '140' },
                    { field: 'operator', displayName: 'Collector', width: '290' },
                    { field: 'hisType', displayName: 'Status', width: '335' },
                    { field: 'issuE_REASON', displayName: 'Dispute Reason', width: '330' }
                ],

                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApiStatusChange = gridApi;
                }
            }

            //*************************************Dispute Status Change List*****************e
            //Get Dispute Status Change
            disputeTrackingProxy.query({ disputeid: $scope.disputeId, suid: $scope.siteUseId }, function (list) {
                $scope.disStatusChangelst = list;
            });
            //$scope.mockdisStatusChangelst = [
            //    { "hisDate": "2015/9/9", "operator": "Sanme", "hisType": "Superviower Reponsed" },
            //    { "hisDate": "2015/9/9", "operator": "Sanme", "hisType": "Sent Superviower" },
            //    { "hisDate": "2015/9/10", "operator": "Sanme", "hisType": "Sent CS" },
            //    { "hisDate": "2015/9/11", "operator": "Sanme", "hisType": "Open" },

            //]
            //*************************************Call List**********************************s
            $scope.callHistoryList = {
                data: 'callhistorylst',
                // data: 'mockcallhistorylst',
                columnDefs: [
                    { field: 'contactDate', displayName: 'Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '140' },
                    { field: 'contacterId', displayName: 'Call To', width: '490' },
                    { field: 'comments', displayName: 'Comments', width: '465' }
                ],

                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApiCallHis = gridApi;
                }
            }
            //*************************************Call List**********************************e

            //*************************************Mail List**********************************s
            //$scope.reSeachContactList = function () {
            //    angular.forEach($scope.num, function (cus) {
            //        collectorSoaProxy.query({ CustNumsFCH: cus }, function (list) {
            //            angular.forEach($scope.sendsoa, function (row) {
            //                if (row.customerCode == cus) {
            //                    row['hisg'].data = list
            //                }
            //            });
            //            //$scope.contactList.data = list;
            //        });
            //    })
            //}
            //$scope.mockcallhistorylst = [
            //    { "contactDate": "2015/7/9", "contacterId": "Customer  Json", "comments": "Close The Dispute Confirm with Customer" },
            //    { "contactDate": "2015/6/9", "contacterId": "Customer  Json", "comments": "Explain the Dispute" },
            //    { "contactDate": "2015/3/10", "contacterId": "CS  zhangfang", "comments": "Discuss the Dispute" },
            //    { "contactDate": "2015/2/11", "contacterId": "Customer  Json", "comments": "Discuss the Dispute" },

            //]

            $scope.mailList = {
                data: 'maillst',
                //   data: 'mockmailist',
                columnDefs: [
                    { field: 'createTime', displayName: 'Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '140' },
                    { field: 'type', displayName: 'Type', width: '60' },
                    { field: 'from', displayName: 'Mail From', width: '215' },
                    { field: 'to', displayName: 'Mail To', width: '215' },
                    { field: 'tmp', displayName: 'Comments', width: '455' },
                    //{
                    //    field: 'tmp', displayName: 'Comments', width: '465',
                    //    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">' +
                    //    '<a ng-click=" grid.appScope.getDetail(row.entity)"> {{row.entity.comments}} </a></div>'
                    //}
                ],

                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApiMail = gridApi;
                }
            }
            //$scope.mockmailist = [
            //    { "createTime": "2015/6/9", "type": "IN", "from": "zhangon@gmail.com", "to": "cexin@gmail.com", "tmp": "发件人 + Subject  + 时间" },
            //    { "createTime": "2015/5/9", "type": "OUT", "from": "xiaoyu@gmail.com", "to": "zhangon@gmail.com", "tmp": "发件人 + Subject  + 时间" },
            //    { "createTime": "2015/4/10", "type": "OUT", "from": "fengdu@gmail.com", "to": "fengdu@gmail.com", "tmp": "发件人 + Subject  + 时间" },
            //    { "createTime": "2015/3/11", "type": "IN", "from": "cexin@gmail.com", "to": "xiaoyu@gmail.com", "tmp": "发件人 + Subject  + 时间" },

            //]
            //contactList Detail
            $scope.getDetail = function (row) {
                if (row.messageId) {
                    mailProxy.queryObject({ messageId: row.messageId }, function (mailInstance) {
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
                } else {
                    alert('Mail Id is null')
                }
            }

            //*************************************Mail List**********************************e

            var collectorName = "";
            permissionProxy.getCurrentUser("dummy", function (user) {
                collectorName = user.name;
            }, function (error) {
                alert(error);
            });

            //change table
            $scope.changetab = function (type) {
                if (type == "call") {
                    $scope.inv = [];
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                    contactCustomerProxy.queryObject({ contactId: '0' }, function (callInstance) {
                        callInstance["title"] = "Dispute Call";
                        callInstance.logAction = "DISPUTE";
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                            controller: 'contactCallCtrl',
                            size: 'lg',
                            resolve: {
                                callInstance: function () { return callInstance; },
                                custnum: function () { return $scope.customerNumber; },
                                invoiceIds: function () { return $scope.inv; },
                                siteuseId: function () { return $scope.siteUseId; },
                                legalEntity: function () {
                                    return $scope.legalEntity;
                                }
                            },
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            //Get Call List
                            commonProxy.query({ customerNum: $scope.customerNumber }, function (list) {
                                $scope.callhistorylst = list;
                            });
                        });
                    });
                } else if (type == "changestatus") {
                    var modalDefaults = {
                        templateUrl: 'app/common/changestatus/changeStatus-list.tpl.html',
                        controller: 'changeStatusCtrl',
                        resolve: {
                            title: function () { return "Change Dispute Status"; },
                            id: function () { return $scope.disputeId; },
                            type: function () { return "026"; },
                            index: function () { return $scope.disputeStatusCode; },
                            actionOwnerDepartmentCode: function () { return $scope.actionOwnerDepartmentCode },
                            disputeReasonCode: function () { return $scope.disputeReasonCode },
                            mailId: function () { return ""; }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            disputeTrackingProxy.queryObject({ id: $scope.disputeId }, function (list) {
                                $scope.issReason = list[0];
                                $scope.disputeStatus = list[5];
                                $scope.disputeDate = list[2];
                                $scope.disputeNotes = list[3];
                                $scope.customerNumber = list[4];
                                $scope.disputeStatusCode = list[5];
                                $scope.siteUseId = list[6];
                                $scope.legalEntity = list[7];
                                $scope.disputeReason = list[8];
                                $scope.customerNameHead = list[9];
                                $scope.disputeStatusName = list[10];
                                $scope.actionOwnerDepartmentCode = list[11];
                                $scope.actionOwnerDepartmentName = list[12];
                                $scope.disputeReasonCode = list[13];
                            });
                            $scope.initList();
                            //Get Dispute Status Change
                            disputeTrackingProxy.query({ disputeid: $scope.disputeId }, function (list) {
                                $scope.disStatusChangelst = list;
                            });
                        }
                    });
                } else if (type == "ptp") {
                    $scope.inv = [];
                    var ccount = 0;
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {

                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                            ccount = ccount + rowItem.outstandingInvoiceAmount;
                        }
                    });
                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }

                    var relatedMail = "";

                    //   relatedMail = $scope.mailInstance.CustomerMails + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");
                    //  relatedMail = "cindy.zhu@ap.averydennison.com" + "  " + "Dispute tracking： from(OTC dev account)" + " " + "2017-11-09T19:12:27.98".replace("T", " ");
                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-ptp.tpl.html',
                        controller: 'contactPtpCtrl',
                        size: 'lg',
                        resolve: {
                            custnum: function () {
                                return "";
                            },
                            invoiceIds: function () {
                                return $scope.inv;
                            },
                            siteUseId: function () {
                                return $scope.siteUseId;
                            },
                            customerNo: function () {
                                return $scope.customerNumber;
                            },
                            legalEntity: function () {
                                return $scope.legalEntity;
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

                } else if (type == "changeinvoicestatus") {
                    $scope.inv = [];
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {

                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }
                    var modalDefaults = {
                        templateUrl: 'app/common/changeinvoicestatus/changeInvoiceStatus-list.tpl.html',
                        controller: 'changeInvoiceStatusCtrl',
                        resolve: {
                            status: function () { return $scope.invstatusValue; },
                            invNums: function () { return $scope.inv; },
                            disputeId: function () { return $scope.disputeId; }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            //disputeTrackingProxy.queryObject({ id: $scope.disputeId }, function (list) {
                            //    $scope.issReason = list[0];
                            //    $scope.disputeStatus = list[1];
                            //    $scope.disputeDate = list[2];
                            //    $scope.disputeNotes = list[3];
                            //    $scope.customerNumber = list[4];
                            //    $scope.disputeStatusCode = list[5];
                            //});

                            ////Get Dispute Status Change
                            //disputeTrackingProxy.query({ disputeid: $scope.disputeId }, function (list) {
                            //    $scope.disStatusChangelst = list;
                            //});
                            $scope.initList();
                        }
                    });
                }
                else if (type == "sendmail") {
                    $scope.invoiceNums = "";
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        //console.log(rowItem.id)
                        //console.log($scope.inv)
                        if (rowItem.invoiceId != 0) {
                            $scope.invoiceNums += rowItem.invoiceNum + ",";
                            //$scope.inv.push(rowItem.id);
                        }
                    });
                    if ($scope.invoiceNums == "" || $scope.invoiceNums == null) {
                        alert("Please choose 1 invoice at least .");
                        return;
                    }
                    $scope.inv = [];
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {

                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                        }
                    });
                    $scope.mailtype = "002";
                    disputeTrackingProxy.GetSOAMailInstance($scope.customerNumber, $scope.siteUseId, $scope.mailtype, $scope.inv, $scope.fileType, function (mailInstance) {
                        //SOAFlg
                        mailInstance.soaFlg = "0";
                        //Bussiness_Reference
                        var customerMails = [];
                        angular.forEach($scope.customerNumber.split(','), function (cust) {
                            customerMails.push({ MessageId: mailInstance.messageId, CustomerNum: cust, SiteUseId: $scope.siteUseId });
                        });
                        mailInstance.CustomerMails = customerMails; //$routeParams.nums;
                        //mailInstance.customerNum = $scope.customerNumber;
                        //subject
                        mailInstance.subject = "Dispute tracking： from(" + collectorName + ")";
                        //title
                        mailInstance["title"] = "Dispute Mail";
                        mailInstance.mailType = "002,Dispute";
                        mailInstance.invoiceIds = $scope.inv;

                        $scope.invoiceNumb = $scope.inv.join(",");

                        var modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return $scope.customerNumber; },
                                siteuseId: function () { return $scope.siteUseId; },
                                invoicenums: function () { return $scope.invoiceNumb; },
                                mType: function () {
                                    return "002";
                                },
                                //                                    selectedInvoiceId: function () { return $scope.inv; },
                                instance: function () {
                                    return mailInstance;
                                    //return getMailInstance($scope.customerNumber, $scope.siteUseId);
                                },
                                mailDefaults: function () {
                                    return {
                                        mailType: 'NE',
                                        templateChoosenCallBack: selectMailInstanceById,
                                        mailUrl: disputeTrackingProxy.sendEmailUrl,
                                        //generateSOAProxy
                                    };
                                }
                            },
                            windowClass: 'modalDialog'
                        };

                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "sent") {
                                //Get Mail List
                                //commonProxy.query({ cusNum: $scope.customerNumber }, function (list) {
                                //    $scope.maillst = list;
                                //});

                                mailProxy.searchMail('&customerNum=' + list[4], '', function (list) {
                                    $scope.maillst = list;
                                });
                            }
                        });
                    }, function (res) {
                        alert(res);
                    });
                } else if (type == "payment") {
                    $scope.inv = [];
                    var ccount = 0;
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        //console.log(rowItem.id)
                        //console.log($scope.inv)
                        if (rowItem.invoiceId != 0) {
                            $scope.inv.push(rowItem.invoiceId);
                            ccount = ccount + rowItem.outstandingInvoiceAmount;
                        }
                    });

                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }


                    var relatedMail = "";
                    //  relatedMail = $scope.mailInstance.mailInstanceService + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");
                    //relatedMail = "cindy.zhu@ap.averydennison.com" + "  " + "Dispute tracking： from(OTC dev account)" + " " + "2017-11-09T19:12:27.98".replace("T", " ");
                    var modalDefaults = {
                        templateUrl: 'app/common/contactdetail/contact-notice.tpl.html',
                        controller: 'contactNoticeCtrl',
                        size: 'lg',
                        resolve: {
                            custnum: function () {
                                return "";
                            },
                            invoiceIds: function () {
                                return $scope.inv;
                            },
                            siteUseId: function () {
                                return $scope.siteUseId;
                            },
                            customerNo: function () {
                                return $scope.customerNumber;
                            },
                            legalEntity: function () {
                                return $scope.legalEntity;
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
                            //$scope.readData();
                            $scope.initList();
                        }
                    });
                }
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
            //var getMailInstance = function (custNums, suid) {
            //    var instance = {};
            //    var allDefered = $q.defer();

            //    $q.all([
            //        getMailInstanceMain(custNums, suid, $scope.inv),
            //        getMailInstanceTo(custNums, suid)
            //        //=========added by alex body中显示附件名+Currency======
            //        //getMailInstanceAttachment($scope.inv)
            //        //=====================================================
            //    ])
            //        .then(function (results) {
            //            instance = results[0];
            //            //2016-01-14 start
            //            //instance.to = results[1];
            //            instance.to = "";
            //            instance.cc = "";
            //            instance.to = results[1].to.join(";");
            //            instance.cc = results[1].cc.join(";");
            //            //2016-01-14 End
            //            //=========added by alex body中显示附件名+Currency======
            //            //instance.attachments = results[2].attachments;
            //            //instance.attachment = results[2].attachment;
            //            //=====================================================
            //            allDefered.resolve(instance);
            //        });

            //    return allDefered.promise;
            //};
            //var getMailInstanceMain = function (custNums, suid, ids) {

            //    var instanceDefered = $q.defer();
            //    mType = "002";
            //    generateSOAProxy.getMailInstance(custNums, suid, ids, mType, function (res) {
            //        var instance = res;
            //        renderInstance(instance, custNums, suid);

            //        instanceDefered.resolve(instance);
            //    }, function (error) {
            //        alert(error);
            //    });

            //    return instanceDefered.promise;
            //};

            //var getMailInstanceTo = function (custNums, suid) {

            //    var toDefered = $q.defer();
            //    //TO
            //    contactProxy.query({
            //        customerNums: custNums, siteUseid: suid
            //    }, function (contactor) {
            //        //2016-01-14 start
            //        var to_cc = {};
            //        to_cc.to = new Array();
            //        to_cc.cc = new Array();
            //        //var cons = new Array();
            //        //var contName = '';
            //        //2016-01-14 end
            //        angular.forEach(contactor, function (item) {
            //            //2016-01-14 start
            //            if (item.toCc == "2") {
            //                if (to_cc.cc.indexOf(item.emailAddress) < 0) {
            //                    to_cc.cc.push(item.emailAddress);
            //                }
            //            }
            //            else if (item.toCc == "1") {
            //                if (to_cc.to.indexOf(item.emailAddress) < 0) {
            //                    to_cc.to.push(item.emailAddress);
            //                }
            //            }
            //            //2016-01-14 End
            //            //if (cons.indexOf(item.emailAddress) < 0) {
            //            //    cons.push(item.emailAddress);
            //            //    //contName = item.name + ',';
            //            //}

            //        });

            //        //var greeting = '<p>Dear ' + contName.substring(1, contName.length - 1) + '</p>';
            //        //Mailinstance.body = greeting + Mailinstance.body;
            //        //toDefered.resolve(cons.join(";"));
            //        toDefered.resolve(to_cc);

            //    });
            //    return toDefered.promise;
            //};
            var renderInstance = function (instance, custNums, suid) {
                //subject
                $scope.shortsub = [];
                instance.subject = 'Dispute-' + $scope.shortsub.join('-');
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
                instance["title"] = "Dispute Mail";
                //mailType
                instance.mailType = "002,Dispute";
            };

            //Save Special Notes
            $scope.saveNote = function (note) {
                var list = [];
                list.push($scope.disputeId);
                list.push(note);
                disputeTrackingProxy.savenotes(list, function () {
                    alert('success!');
                })
            }

        }])