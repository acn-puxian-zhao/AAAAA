angular.module('app.maildetail', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/maillist/maildetail/:mailId', {
                templateUrl: 'app/maillist/maildetail/maildetail.tpl.html',
                controller: 'maildetailCtrl',
                resolve: {
                }
            })
            .when('/maillist/maildetail', {
                templateUrl: 'app/maillist/maildetail/maildetail.tpl.html',
                controller: 'maildetailCtrl'
            });
    }])

    .controller('maildetailCtrl',
    ['$scope', 'baseDataProxy', 'modalService', 'mailTemplateProxy', '$interval', 'APPSETTING',
        'mailProxy', 'FileUploaderCommon', '$q', 'appFilesProxy', '$location', '$routeParams', 'mailInstanceService', 'collectorSoaProxy',
        '$anchorScroll', 'contactHistoryProxy', 'generateSOAProxy','invoiceProxy', 'uiGridConstants', 'cookieWatcherService',
        function ($scope, baseDataProxy, modalService, mailTemplateProxy, $interval, APPSETTING,
            mailProxy, FileUploaderCommon, $q, appFilesProxy, $location, $routeParams, mailInstanceService, collectorSoaProxy,
            $anchorScroll, contactHistoryProxy, generateSOAProxy, invoiceProxy, uiGridConstants, cookieWatcherService
        ) {
            $scope.$parent.helloAngular = "OTC - Mail Detail";
            //alert($location.search()['custNum']);
            //alert($location.search()['siteUseId']);
            // $scope.custNo = $location.search()['custNum'];
            //  $scope.suid = $location.search()['siteUseId'];
            $scope.arrCusts = [];
            $scope.hideGrid = true;
            $scope.inputClear = false;
            // $scope.suid = $routeParams.m


            $scope.DateInt = function () {

                var date = new Date();
                //var str = "";
                //str += date.getFullYear();
                //str += date.getMonth() > 9 ? (date.getMonth() + 1).toString() : '0' + (date.getMonth() + 1);
                //str += date.getDate() > 9 ? date.getDate().toString() : '0' + date.getDate();
                var currentDate = new Date(date.getFullYear(), date.getMonth() + 1, date.getDate());
                //$scope.currentDate = parseInt(str);
                $scope.currentDate = currentDate;
                //alert(currentDate);
            }

            var mailId = $routeParams.mailId;
            $scope.invoiceList = {
                enableFiltering: true,
                // data:'mock',
                columnDefs: [
                    {
                        field: 'invoiceNum', name: 'tmp', displayName: 'Invoice NO.', enableCellEdit: false, width: '100', pinnedLeft: true
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceNum}}</a></div>'
                        , filterHeaderTemplate: '<div class="ui-grid-filter-container" ><input class="form-control input-sm" ng-keyup="grid.appScope.findinvs()" id="invfilter"></div><div><a class="glyphicon glyphicon-remove" ng-show="grid.appScope.inputClear" ng-click="grid.appScope.removeInput()"></a></div>'
                    },
                    { field: 'customerNum', enableCellEdit: false, displayName: 'CustomerNo.', width: '105' },
                    { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    { field: 'class', enableCellEdit: false, displayName: 'Class.', width: '95' },
                    //{ field: 'customerNum', enableCellEdit: false, displayName: 'CustomerNo.', width: '105' },
                    { field: 'siteUseId', enableCellEdit: false, displayName: 'SiteUseId.', width: '105' },
                    { field: 'legalEntity', enableCellEdit: false, displayName: 'Legal Entity.', width: '110' },
                    { field: 'dueDate', enableCellEdit: false, displayName: 'Due Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '95' },
                    { field: 'trackStatesName', enableCellEdit: false, displayName: 'Current Status', width: '105' },
                    { field: 'creditTrem', enableCellEdit: false, displayName: 'Payment Term Name', width: '140' },
                    { field: 'currency', enableCellEdit: false, displayName: 'Inv Curr Code.', width: '100' },
                    { field: 'daysLateSys', enableCellEdit: false, displayName: 'Due Days', width: '100' },
                    { field: 'balanceAmt', enableCellEdit: false, displayName: 'Amt Remaining', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'woVat_AMT', enableCellEdit: false, displayName: 'Amount Wo Vat', width: '140', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'agingBucket', enableCellEdit: false, displayName: 'Aging Bucket', width: '122' },
                    { field: 'eb', enableCellEdit: false, displayName: 'Eb', width: '100' },
                    { field: 'vatNo', enableCellEdit: false, displayName: 'VAT NO.', width: '90' },
                    { field: 'vatDate', enableCellEdit: false, displayName: 'VAT Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    { field: 'tracK_DATE', enableCellEdit: false, displayName: 'Last Update Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '120' },
                    { field: 'pTPIdentified', enableCellEdit: false, displayName: 'PTP Identified Date', width: '150' },
                    { field: 'ptpDate', enableCellEdit: false, displayName: 'PTP date ', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    { field: 'payment_Date', enableCellEdit: false, displayName: 'Payment date ', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                    { field: 'dispute', enableCellEdit: false, displayName: 'Dispute(Y/N)', width: '100' },
                    { field: 'disputeIdentifiedDate', enableCellEdit: false, displayName: 'Dispute Identified Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '155' },
                    { field: 'disputeReason', enableCellEdit: false, displayName: 'Dispute Reason', width: '110' },
                    { field: 'actionOwnerDepartment', enableCellEdit: false, displayName: 'Action Owner-Department', width: '170' },
                    { field: 'collectorName', enableCellEdit: false, displayName: 'Collector', width: '90' },
                    { field: 'nextActionDate', enableCellEdit: false, displayName: 'Next Action Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '120' },
                    // { field: 'id', display:'none' },
                    {
                        field: 'comments', name: 'tmp1', displayName: 'Invoice Memo', width: '110',
                        cellTemplate: '<div><a class="glyphicon glyphicon-pencil" ng-click="grid.appScope.editMemoShow(row.entity.id,row.entity.invoiceNum,row.entity.comments)"></a>'
                        + '<label id="lbl{{row.entity.invoiceId}}" ng-mouseMove="grid.appScope.memoShow(row.entity.invoiceNum,row.entity.comments,$event)" ng-mouseOut="grid.appScope.memoHide()">{{row.entity.comments.substring(0,7)}}...</label></div>'
                    }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            }

            $scope.view = function (mailId) {
                mailInstanceService.getViewMail(mailId).then(function (instance) {
                    $scope.mailInstance = instance;

                    if ($scope.mailInstance) {

                        //search CustomerNums & Name by mailId
                        //angular.forEach(instance.customerMails, function (customerMail) {
                        //    $scope.customerVisable += customerMail.customerNum;
                        //})
                        $scope.getCusByMailId();

                        if (instance.category == "Draft") {
                            $scope.hideReply = true;
                            $scope.hideForward = true;
                        }

                    }
                });
            }

            $scope.isShowAssignCustomer = function () {
                if (!$scope.mailInstance) {
                    return false;
                }
                if ($scope.mailInstance.category == "Sent"
                    || $scope.mailInstance.category == "Processed" || $scope.mailInstance.title == "Mail Reply" || $scope.mailInstance.title == "Mail Forward") {
                    return false;
                }

                return true;
            }

            $scope.showPending = function () {
                if (!$scope.mailInstance) {
                    return false;
                }

                // Don't allow pending if mail in below status.
                if ($scope.mailInstance.category == "Draft"
                    || $scope.mailInstance.category == "Sent"
                    || $scope.mailInstance.category == "Processed"
                    || $scope.mailInstance.category == "Pending") {
                    return false;
                }

                if ($scope.mailInstance.title == "Mail Reply"
                    || $scope.mailInstance.title == "Mail Forward") {
                    return false;
                }

                return true;
            }

            $scope.showFinish = function () {
                if (!$scope.mailInstance) {
                    return false;
                }

                // Don't allow pending if mail in below status.
                if ($scope.mailInstance.category == "Draft"
                    || $scope.mailInstance.category == "Sent"
                    || $scope.mailInstance.category == "Processed") {
                    return false;
                }

                if ($scope.mailInstance.title == "Mail Reply"
                    || $scope.mailInstance.title == "Mail Forward") {
                    return false;
                }

                return true;
            }

            $scope.showReplyAndForward = function () {
                if (!$scope.mailInstance) {
                    return false;
                }

                if ($scope.mailInstance.title == "Mail Reply"
                    || $scope.mailInstance.title == "Mail Forward"
                    || $scope.mailInstance.category == "Draft") {
                    return false;
                }

                return true;
            }

            $scope.showgenerateSOA = function () {
                if (!$scope.mailInstance) {
                    return false;
                }

                if ($scope.mailInstance.title == "Mail Reply"
                    || $scope.mailInstance.title == "Mail Forward"
                    || $scope.mailInstance.category == "Draft") {
                    return true;
                }

                return false;
            }

            if (typeof mailId == "undefined") {
                mailInstanceService.getNewMail().then(function (instance) {
                    $scope.mailInstance = instance;
                });
            } else {
                $scope.view(mailId);
            }
            $scope.DateInt();

            //remove customer from html & DB
            $scope.removeCus = function (cusNum, siteUseId) {
                mailProxy.removeCus($scope.mailInstance.messageId, cusNum, siteUseId, function (res) {
                    $("#lbl" + cusNum).remove();

                    var tempNums = "";
                    var custList = [];
                    var siteUseIdList = [];
                    angular.forEach($scope.customers, function (cust) {
                        if (cust.customerNum != cusNum && cust.siteUseId != siteUseId) {
                            custList.push(cust);
                            siteUseIdList.push(cust.siteUseId);
                            tempNums += cust.customerNum + ',';
                        }
                    });

                    $scope.customers = custList;
                    $scope.custNums = tempNums.substring(0, tempNums.length - 1);
                    $scope.siteUseId = siteUseIdList.join(',');
                    //var tempNums = "";
                    //angular.forEach($scope.customers, function (cust) {
                    //    tempNums += cust.customerNum + ',';
                    //})
                    //$scope.custNums = tempNums.substring(0, tempNums.length - 1);

                    // $scope.getCusts();
                    $scope.readData();
                }, function (error) {
                    alert(error);
                });
                //var tempCusNum = cusNum + ',';
                //var tempCustNums = $scope.custNums;
                //if ($scope.custNums.indexOf(tempCusNum) >= 0) {
                //    $scope.custNums = $scope.custNums.replace(tempCusNum, "");
                //} else {
                //    $scope.custNums = $scope.custNums.replace(cusNum, '');
                //}
                //$("#lbl" + cusNum).remove();

                //var arrcust = [];
                //var CustomerMails = [];
                //var arrcust = $scope.custNums.split(',');
                //angular.forEach(arrcust, function (cust) {
                //    CustomerMails.push({ MessageId: $scope.mailInstance.messageId, CustomerNum: cust });
                //    $scope.mailInstance.CustomerMails = CustomerMails;
                //})
                //$scope.mailInstance.customerMails = $scope.refreshCus();
                //$scope.readData();
            }

            $scope.mailDefaults = [];
            $scope.mailDefaults.showCancelBtn = true;
            $scope.modalInstance = {
                close: function (res, instance) {
                    if (res == 'sent') {
                        alert('Sent successed.');
                        var watchingUrl = '';
                        if (typeof $routeParams.mailId == 'undefined') {
                            watchingUrl = '/maillist/maildetail/new';
                        } else {
                            watchingUrl = '/maillist/maildetail/' + mailId;
                        }

                        // write cookie to indicate the mail dialog closing.
                        cookieWatcherService().getWatchItem(watchingUrl).done();
                        // sent call back
                        window.close();
                    } else {
                        $scope.view(mailId);
                    }
                },
                dismiss: function () { }
            }

            $scope.reply = function () {
                mailInstanceService.getReplyMail(mailId).then(function (instance) {
                    $scope.mailInstance = instance;
                    $scope.mailInstance.title = "Mail Reply";
                });
            }

            $scope.forward = function () {
                mailInstanceService.getForwardMail(mailId).then(function (instance) {
                    $scope.mailInstance = instance;
                    $scope.mailInstance.title = "Mail Forward";
                });
            }

            $scope.finish = function () {
                mailProxy.updateMailCategory([mailId], 'Processed', function (res) {
                    alert('Successfully set to processed.')
                    $scope.mailInstance.category = 'Processed';
                    cookieWatcherService().getWatchItem('/maillist/maildetail/' + mailId).done();
                    window.close();
                }, function (error) {
                    alert(error);
                })
            }

            $scope.pending = function () {
                mailProxy.updateMailCategory([mailId], 'Pending', function (res) {
                    alert('Successfully set to Pending.')
                    $scope.mailInstance.category = 'Pending';
                }, function (error) {
                    alert(error);
                })
                $scope.readData();
            }

            $scope.AssignCustomer = function () {

                var modalDefaults = {
                    templateUrl: 'app/maillist/contactcustomeredit/contactcustomer-edit.tpl.html',
                    controller: 'contactCustomerEditCtrl',
                    size: 'lg',
                    resolve: {
                        messageId: function () { return $scope.mailInstance.messageId; },
                        customers: function () { return $scope.custNums; }
                    },
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    //$scope.getCusts();
                    if (result != null) {
                        $scope.getCusByMailId();
                    }
                    //$scope.readData();
                });

            }

            //get customer information by mailId
            $scope.getCusByMailId = function () {
                var tempNums = "";
                var msgid = $scope.mailInstance.id == null ? $routeParams.mailId : $scope.mailInstance.id;
                mailProxy.query({ mailId: msgid }, function (customers) {
                    $scope.customers = customers;
                    var siteUseIdList = [];
                    angular.forEach($scope.customers, function (cust) {
                        tempNums += cust.customerNum + ',';
                        siteUseIdList.push(cust.siteUseId);
                    })
                    $scope.custNums = tempNums.substring(0, tempNums.length - 1);
                    $scope.siteUseId = siteUseIdList.join(',');

                    $scope.readData();

                    //angular.forEach($scope.customers, function (cust) {
                    //    $scope.arrCusts.push(cust.customerNum);
                    //})
                    //$scope.custNums = $scope.arrCusts.join(',');
                })
            }

            ////add assigned new customer
            //$scope.addCusts = function (custs) {
            //    //var tempNums = custs.join(',');
            //    //if ($scope.custNums != "") {
            //    //    $scope.custNums += ',' + tempNums;
            //    //} else {
            //    //    $scope.custNums += tempNums;
            //    //}
            //    $scope.custNums = custs.custnum;
            //    $scope.siteUseId = custs.siteUseId;

            //    //mailProxy.query({ mailCustNums: $scope.custNums, siteUseId: $scope.siteUseId }, function (customers) {
            //    //    $scope.customers = customers;
            //    //})
            //    //angular.forEach(custs, function (cust) {
            //    //    $scope.mailInstance.CustomerMails.push({ MessageId: $scope.mailInstance.messageId, CustomerNum: cust });
            //    //})
            //    //$scope.mailInstance.customerMails = $scope.refreshCus();
            //    $scope.readData();
            //    $scope.getCusByMailId ();
            //}

            //$scope.refreshCus = function () {
            //    var arrcust = [];
            //    var CustomerMails = [];
            //    var arrcust = $scope.custNums.split(',');
            //    angular.forEach(arrcust, function (cust) {
            //        CustomerMails.push({ MessageId: $scope.mailInstance.messageId, CustomerNum: cust });
            //    })
            //    if ($scope.mailInstance.title == "Mail View") {
            //        //数据库直接保存
            //        mailProxy.updateMailCus($scope.mailInstance.messageId, $scope.custNums, function (res) {
            //            //  $scope.getCusts();
            //            $scope.readData();
            //        }, function (error) {
            //            alert(error);
            //        })
            //    }
            //    return CustomerMails;
            //}


            //$scope.getCusts = function () {

            //    mailProxy.query({ mailCustNums: $scope.custNums }, function (customers) {
            //        $scope.customers = customers;

            //        var tempNums = "";
            //        angular.forEach($scope.customers, function (cust) {
            //            tempNums += cust.customerNum + ',';
            //        })
            //        $scope.custNums = tempNums.substring(0, tempNums.length - 1);
            //    })
            //}

            //refresh data if invoicelist is show
            $scope.readData = function () {
                if (!$scope.hideGrid) {
                    $scope.getInvoice();

                    //$window.location.hash = "#divInvoice";
                }
                //$scope.goto();
            }


            $scope.goto = function () {
                $location.hash('divInvoice');
                $anchorScroll();
            }

            //read invoice data
            $scope.getInvoice = function () {
                $scope.customer = [];
                $scope.cus = [];
                var custKeyList = [];

                angular.forEach($scope.customers, function (cust) {
                    var custKey = {};
                    custKey.CustomerNum = cust.customerNum;
                    custKey.SiteUseId = cust.siteUseId;
                    custKeyList.push(custKey);
                });

                $scope.custKeyList = custKeyList;

                mailProxy.getMailInvoicebyCus($scope.custKeyList, function (invoices) {
                    $scope.invoiceList.data = invoices
                }, function (err) {
                });

                //if ($scope.custNums != "") {
                //    //get invoiceList by customerNum
                //    //mailProxy.query({ mailCustNumsForInv: $scope.mailInstance.messageId }, function (invoices) 
                //    mailProxy.getMailInvoicebyCus($scope.custKeyList , function (invoices) {
                //        $scope.invoiceList.data = invoices
                //    }, function (err) {
                //    })
                //}
            }

            $scope.getInvoiceByInput = function (inputs) {
                if (inputs != "") {
                    $scope.inputClear = true;
                    //get invoiceList by customerNum and input nums
                    mailProxy.query({ mailCustNumsForInv: $scope.mailInstance.messageId, inputNums: inputs }, function (invoices) {
                        $scope.invoiceList.data = invoices
                    })

                } else {
                    $scope.inputClear = false;
                    $scope.getInvoice();

                }
            }


            $scope.clearPTP = function () {
                if ($scope.gridApi.selection.getSelectedRows().length == 0) {
                    alert("Please choose 1 invoice at least .");
                    return;
                }
                if (confirm("Are you sure clear PTP")) {
                    var idList = new Array();
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                        if (rowItem.id != 0) {
                            idList.push(rowItem.id + '|' + rowItem.invoiceNum);
                        }
                    });

                    invoiceProxy.clearPTP(idList, function (res) {
                        alert(res);
                        $scope.readData();
                    }, function (res) {
                        alert(res);
                    });
                }

            }

            //##########################
            //change invoice status
            //##########################
            $scope.changetab = function (type) {
                //get selected invoiceIds
                $scope.inv = [];
                $scope.pm = [];
                $scope.invStatus = [];

                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        $scope.inv.push(rowItem.id);
                        $scope.pm.push(rowItem.balanceAmt);
                    }
                });
                var ccount = 0;
                if ($scope.pm != null) {
                    for (var i = 0; i < $scope.pm.length; i++) {

                        ccount = ccount + $scope.pm[i];
                    }
                }

                if (($scope.inv == "" || $scope.inv == null) && type != "ptp") {
                    alert("Please choose 1 invoice at least .")
                    return;
                }

                if (type == "dispute") {
                    $scope.inv = [];
                    $scope.invStatus = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {

                        //if (rowItem.id != 0) {
                        //    $scope.inv.push(rowItem.id);
                        //}
                        if (rowItem.id != 0) {
                            $scope.inv.push(rowItem.id);
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
                    if (isonecustomer == false) {
                        alert("Please choose 1 SiteUseId at most .")
                        return;
                    }
                    for (var i = 0; i < $scope.invStatus.length; i++) {
                        if ($scope.invStatus[i].status.indexOf(type) >= 0) {
                            alert("the selected invoices contains dispute invoice already!");
                            return;
                        }
                    }

                    var relatedMail = "";
                    relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");

                    contactHistoryProxy.queryObject({ type: 'dispute' }, function (disInvInstance) {
                        disInvInstance["title"] = "Dispute Reason";
                        var modalDefaults = {
                            templateUrl: 'app/common/contactdetail/contact-dispute.tpl.html',
                            controller: 'contactDisputeCtrl',
                            size: 'lg',
                            resolve: {
                                disInvInstance: function () {
                                    return disInvInstance;
                                },
                                custnum: function () {
                                    return $scope.custNo;
                                },
                                invoiceIds: function () {
                                    return $scope.inv;
                                },
                                contactId: function () {
                                    return $scope.mailInstance.messageId;
                                },
                                relatedEmail: function () {
                                    return relatedMail;
                                },
                                contactPerson: function () {
                                    return $scope.mailInstance.to;
                                },
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
                                $scope.readData();
                            }
                        });
                    });
                } else if (type == "ptp") {
                    $scope.inv = [];
                    $scope.invStatus = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;
                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {

                        //if (rowItem.id != 0) {
                        //    $scope.inv.push(rowItem.id);
                        //}
                        if (rowItem.id != 0) {
                            $scope.inv.push(rowItem.id);
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

                    if (isonecustomer == false) {
                        alert("Please choose 1 customer at most .")
                        return;
                    }

                    //if ($scope.inv == "" || $scope.inv == null) {
                    //    alert("Please choose 1 invoice at least .")
                    //    return;
                    //}

                    for (var i = 0; i < $scope.invStatus.length; i++) {
                        if ($scope.invStatus[i].status.indexOf(type) >= 0) {
                            alert("the selected invoices contains ptp invoice already!");
                            return;
                        }
                    }

                    var relatedMail = "";
                    relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");

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
                                return $scope.suid;
                            },
                            customerNo: function () {
                                return $scope.custNo;
                            },
                            legalEntity: function () {
                                return $scope.legalentty;
                            },
                            contactId: function () {
                                return $scope.mailInstance.messageId;
                            },
                            relatedEmail: function () {
                                return relatedMail;
                            },
                            contactPerson: function () {
                                return $scope.mailInstance.to;
                            },
                            proamount: function () {
                                return ccount.toFixed(2);
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] == "submit") {
                            $scope.readData();
                        }
                    });
                } else if (type == "notice") {
                    $scope.inv = [];
                    $scope.suid = "";
                    $scope.custNo = "";
                    $scope.legalentty = "";
                    var isfirstrow = true;
                    var isonecustomer = true;

                    angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {

                        if (rowItem.id != 0) {
                            $scope.inv.push(rowItem.id);
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
                        //if (rowItem.siteUseId != 0) {
                        //    $scope.suid.push(rowItem.siteUseId);
                        //}
                        //if (rowItem.customerNum != 0) {
                        //    $scope.custNo.push(rowItem.customerNum);
                        //}
                        //if (rowItem.legalEntity != 0) {
                        //    $scope.legalentty.push(rowItem.legalEntity);
                        //}
                    });

                    if (isonecustomer == false) {
                        alert("Please choose 1 customer at most .")
                        return;
                    }


                    if ($scope.inv == "" || $scope.inv == null) {
                        alert("Please choose 1 invoice at least .")
                        return;
                    }

                    var relatedMail = "";
                    relatedMail = $scope.mailInstance.from + "  " + $scope.mailInstance.subject + " " + $scope.mailInstance.createTime.replace("T", " ");

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
                                return $scope.suid;
                            },
                            customerNo: function () {
                                return $scope.custNo;
                            },
                            legalEntity: function () {
                                return $scope.legalentty;
                            },
                            contactId: function () {
                                return $scope.mailInstance.messageId;
                            },
                            relatedEmail: function () {
                                return relatedMail;
                            },
                            contactPerson: function () {
                                return $scope.mailInstance.to;
                            },
                            proamount: function () {
                                return ccount.toFixed(2);
                            }
                        },
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] == "submit") {
                            $scope.readData();
                        }
                    });
                }
            }


            //##########################
            //invoice memo edit
            //##########################
            //********************** edit one memo //**********************s
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

            $scope.saveBack = function (invoiceId, memo) {

                angular.forEach($scope.gridApi.grid.rows, function (rowItem) {
                    if (rowItem.entity.id == invoiceId) {
                        rowItem.entity.comments = memo;
                    }
                });

            }

            $scope.editMemoClose = function () {
                $("#boxEdit").hide();
            }
            //********************** edit one memo //**********************e

            //******************************* edit batch memo *******************************s
            $scope.batcheditMemoShow = function () {
                $scope.inv = [];
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        $scope.inv.push(rowItem.id);
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

            $scope.batchsaveBack = function (memo) {
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        rowItem.comments = memo + '\r\n' + rowItem.comments;
                    }
                });
            }

            $scope.batcheditMemoClose = function () {
                $("#boxEditBatch").hide();
            }
            //******************************* edit batch memo *******************************e
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

            //##########################
            //generate SOA attachment
            //##########################
            $scope.generateSoa = function () {
                $scope.inv = [];
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        $scope.inv.push(rowItem.id);
                        $scope.custNo = rowItem.customerNum;
                        $scope.suid = rowItem.siteUseId;
                    }
                });

                if ($scope.inv == "" || $scope.inv == null) {
                    alert("Please choose 1 invoice at least .")
                } else {

                    generateSOAProxy.geneateSoaByIds($scope.inv, "001", $scope.custNo, $scope.suid, function (res) {
                        angular.forEach(res, function (att) {
                            $scope.mailInstance.attachments.push(att);
                        });

                        $scope.mailInstance.attachment += ("," + $scope.mailInstance.attachments.join(","));
                    }
                        , function (err) {
                        })
                }

            }

            //##########################
            //list params
            //##########################
            //******************************** days late ******************************
            $scope.calDaysLate = function (obj) {
                var aDate = obj.dueDate.toString().substring(0, 10).split('-');
                var date = new Date(aDate[0], aDate[1], aDate[2]);
                //date = date.getDate();
                //return (parseInt($scope.currentDate - obj.dueDate.toString().replace(/-/g, '').substring(0, 8)));
                return (($scope.currentDate - date) / 86400000);
            }

            //******************************** get status ******************************
            $scope.getStatus = function (obj) {
                var str = "";
                if (obj.states == '004001') {
                    str = "Open";
                } else if (obj.states == '004002') {
                    str = "PTP";
                } else if (obj.states == '004004') {
                    str = "Dispute";
                } else if (obj.states == '004008') {
                    str = "PartialPay";
                } else if (obj.states == '004010') {
                    str = "Broken PTP";
                } else if (obj.states == '004011') {
                    str = "Hold";
                } else if (obj.states == '004012') {
                    str = "Payment";
                } else if (obj.states == '004003') {
                    str = "Paid";
                } else if (obj.states == '004005') {
                    str = "Cancelled";
                } else if (obj.states == '004009') {
                    str = "Closed";
                } else if (obj.states == '004006') {
                    str = "Uncollectable";
                } else if (obj.states == '004007') {
                    str = "WriteOff";
                }
                return str;
            }

            //******************************** get track status ******************************
            $scope.getTrackStatus = function (obj) {
                var str = "";
                if (obj.trackStates == '001') {
                    str = "SOA Sent";
                } else if (obj.trackStates == '002') {
                    str = "Second Reminder Sent";
                } else if (obj.trackStates == '003') {
                    str = "Final Reminder Sent";
                } else if (obj.trackStates == '004') {
                    str = "Dispute";
                } else if (obj.trackStates == '005') {
                    str = "PTP";
                } else if (obj.trackStates == '006') {
                    str = "Payment Notice Received";
                } else if (obj.trackStates == '007') {
                    str = "Broken PTP";
                } else if (obj.trackStates == '008') {
                    str = "First Broken Sent";
                } else if (obj.trackStates == '009') {
                    str = "Second Broken Sent";
                } else if (obj.trackStates == '011') {
                    str = "Hold";
                } else if (obj.trackStates == '012') {
                    str = "Agency Sent";
                } else if (obj.trackStates == '013') {
                    str = "Write Off";
                } else if (obj.trackStates == '014') {
                    str = "Paid";
                } else if (obj.trackStates == '015') {
                    str = "Bad Debit";
                } else if (obj.trackStates == '016') {
                    str = "Open";
                } else if (obj.trackStates == '017') {
                    str = "Close";
                } else if (obj.trackStates == '018') {
                    str = "Contra";
                } else if (obj.trackStates == '019') {
                    str = "Breakdown";
                }
                return str;
            }

            //*********************************** open invoice history *********************************
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

            //********************
            $scope.findinvs = function () {
                $scope.getInvoiceByInput($("#invfilter").val());
                ////alert($("#invfilter").val());
                //$scope.colFilter.listTerm = $("#invfilter").val().split(',');
                ////$scope.colFilter.listTerm = [];

                ////filinvs.forEach(function (filinv) {
                ////    $scope.colFilter.listTerm.push(filinv);
                ////});

                //$scope.colFilter.term = $scope.colFilter.listTerm.join(',');
                //$scope.colFilter.condition = new RegExp($scope.colFilter.listTerm.join('|'));

                //if ($elm) {
                //    $elm.remove();
                //}


            }

            //********************
            $scope.removeInput = function () {
                $("#invfilter").val("");
                $scope.getInvoice();
                $scope.inputClear = false;
            }

        }])

    .filter('mapStatus', function () {
        var typeHash = {
            '004001': 'Open',
            '004002': 'PTP',
            '004003': 'Paid',
            '004004': 'Dispute',
            '004005': 'Cancelled',
            '004006': 'Uncollectable',
            '004007': 'WriteOff',
            '004008': 'PartialPay',
            '004010': 'Broken PTP',
            '004009': 'Closed',
            '004011': 'Hold',
            '004012': 'Payment'

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
            '001': 'SOA Sent',
            '002': 'Second Reminder Sent',
            '003': 'Final Reminder Sent',
            '004': 'Dispute',
            '005': 'PTP',
            '006': 'Payment Notice Received',
            '007': 'Broken PTP',
            '008': 'First Broken Sent',
            '009': 'Second Broken Sent',
            '011': 'Hold',
            '012': 'Agency Sent',
            '013': 'Write Off',
            '014': 'Paid',
            '015': 'Bad Debit',
            '016': 'Open',
            '017': 'Close',
            '018': 'Contra',
            '019': 'Breakdown'
        };
        return function (input) {
            if (!input) {
                return '';
            } else {
                return typeHash[input];
            }
        };
    });
