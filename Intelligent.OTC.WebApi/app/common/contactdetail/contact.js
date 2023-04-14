angular.module('app.common.contactdetail', ['ui.bootstrap'])

    .controller('contactNoticeCtrl',
        // 'siteuseid','legalentity',
        ['$scope', 'siteUseId', 'customerNo', 'proamount', 'legalEntity', 'custnum', 'invoiceIds', 'contactId', 'relatedEmail', 'contactPerson', '$uibModalInstance', 'contactProxy', 'collectorSoaProxy', 'baseDataProxy',
            'PTPPaymentProxy',
            //siteuseid,legalentity,
            function ($scope, siteUseId, customerNo, proamount, legalEntity, custnum, invoiceIds, contactId, relatedEmail, contactPerson, $uibModalInstance, contactProxy, collectorSoaProxy, baseDataProxy, PTPPaymentProxy) {
                var result = "";
                $scope.payer = "";
                $scope.promissDate = "";
                $scope.proamount = proamount;
                $scope.partialpay = "false";
                $scope.IsForwarder = false;

                PTPPaymentProxy.queryPayer(siteUseId, function (list) {
                    $scope.paryerList = list;
                    $scope.paryerListTotla = list;
                }, function (res) {
                    alert(res);
                })

                $scope.searchPayer = function () {
                    if ($scope.payer == undefined || $scope.payer == "") {
                        $scope.paryerList = $scope.paryerListTotla;
                        return;
                    }
                    var val = $scope.payer;
                    var i = 0;
                    var temp = new Array();
                    var searchFlag = true;
                    angular.forEach($scope.paryerListTotla, function (x) {
                        if (x == null || x == "") {
                            searchFlag = false;
                        }
                        if (searchFlag && x.indexOf(val) >= 0) {
                            temp[i] = x;
                            i++;
                        }

                    });
                    $scope.paryerList = temp;
                }

                $scope.payerClick = function (v) {
                    $scope.payer = v;
                };

                baseDataProxy.SysTypeDetail("010", function (list) {
                    $scope.payMethodList = list;
                });

                $scope.trackerList = [
                    { "id": 1, "trackerName": 'Payment Notice Received' },
                    { "id": 2, "trackerName": 'Contra' },
                    { "id": 3, "trackerName": 'Breakdown' }
                ];

                if (relatedEmail) {
                    $scope.relatedEmail = relatedEmail;
                }

                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };
                //   alert($scope.proamount)

                $scope.submit = function () {
                    if ($scope.promissDate == "") {
                        alert("Please choose promise date !");
                        return;
                    }
                    else if ($scope.IsForwarder == true && $scope.payer == "") {
                        alert("Please input payer !");
                        return;
                    }
                    else if ($("#parpay").val() == "true" && $scope.proamount == undefined) {
                        alert("Please fill the Promise Amount !");
                        return;
                    }
                    else {
                        var result = [];
                        result.push('submit');
                        result.push($scope.trackStatus);
                        result.push($scope.promissDate);

                        var list = [];
                        //0
                        list.push('3');//3:notice
                        //invoice list[1]
                        list.push(invoiceIds.join(','));
                        //custnum list[2]
                        list.push(customerNo);
                        //contactid 3
                        list.push(contactId);
                        //contactperson 4
                        list.push(contactPerson);
                        //trackstatus 5
                        list.push($scope.trackStatus);
                        //promissdate 6
                        list.push($scope.promissDate);
                        //partialpay 7
                        list.push($scope.partialpay);
                        //payer 8
                        list.push($scope.payer);
                        //paymethod 9
                        list.push($scope.paymethod);
                        //contracter 10
                        list.push($scope.contacter);
                        //contracter 11
                        list.push($("#comments").val());
                        //12amount
                        list.push($scope.proamount);
                        //13
                        list.push(siteUseId);
                        //14
                        list.push(legalEntity);
                        //15
                        list.push($scope.IsForwarder);

                        collectorSoaProxy.savecommon(list, function () {
                            $uibModalInstance.close(result);
                        })
                    }
                    //for (i = 0; i < invoiceIds.length; i++) {
                    //    contactProxy.insertDatasForNotice(custnum, invoiceIds[i], contactId, contactPerson, $("#comments").val(),
                    //    function (res) {
                    //        $uibModalInstance.close(result);
                    //    }, function (error) {
                    //        alert(error);
                    //    })
                    //}
                };
                $scope.dateOptions = {
                    //dateDisabled: disabled,
                    formatYear: 'yy',
                    maxDate: new Date(2099, 5, 22),
                    // minDate: new Date(),
                    startingDay: 1
                };
                // Disable weekend selection
                function disabled(data) {
                    var date = data.date,
                        mode = data.mode;
                    return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
                }
                $scope.openPromissDate = function () {
                    $scope.popupPromissDate.opened = true;
                };
                $scope.popupPromissDate = {
                    opened: false
                };
            }])

    //added by zhangYu
    .controller('contactCallCtrl',
        ['$scope', 'callInstance', 'custnum', 'legalEntity', 'invoiceIds', 'siteuseId', '$uibModalInstance', '$interval', 'contactCustomerProxy', 'modalService',
            function ($scope, callInstance, custnum, legalEntity, invoiceIds, siteuseId, $uibModalInstance, $interval, contactCustomerProxy, modalService) {
                $scope.callInstance = callInstance;
                $scope.invoiceIds = invoiceIds;
                if (callInstance.title == "Call Detail" || callInstance.title == "Call View") {
                    var beginTime = new Date($scope.callInstance.beginTime);
                    var endTime = new Date($scope.callInstance.beginTime);
                    $scope.callInstance.beginTime = beginTime.format("yyyy-MM-dd hh:mm");
                    $scope.callInstance.endTime = endTime.format("yyyy-MM-dd hh:mm");
                }
                $scope.dateOptions = {
                    // dateDisabled: 'disabled',
                    formatYear: 'yy',
                    maxDate: new Date(2099, 5, 22),
                    minDate: new Date(),
                    startingDay: 1
                };
                var result = "";
                $interval(function () {
                    $scope.setAttr();
                }, 0, 1);
                $scope.setAttr = function () {
                    if (callInstance.title == "Call Create") {
                        $("#btnSubmit").show();
                        $("#btnSaveCall").hide();
                        $("#txtContact").removeAttr("readonly");
                        $("#imgContactor").hide();
                        var now = new Date();
                        $scope.callInstance.beginTime = now.format("yyyy-MM-dd hh:mm");
                    }
                    if (callInstance.title == "Call Detail") {
                        $("#btnSaveCall").show();
                        $("#btnSubmit").hide();
                        $("#txtContact").removeAttr("readonly");
                        $("#imgContactor").hide();
                    }
                    //invoice Log 
                    if (callInstance.title == "Call View") {
                        $("#btnSaveCall").hide();
                        $("#btnSubmit").hide();
                        $("#txtContact").removeAttr("readonly");
                        $("#imgContactor").hide();
                    }
                    if (callInstance.title == "Dispute Call") {
                        $("#btnSubmit").show();
                        $("#btnSaveCall").hide();
                        $("#imgContactor").show();
                        $("#txtContact").removeAttr("readonly");
                        var now = new Date();
                        $scope.callInstance.beginTime = now.format("yyyy-MM-dd hh:mm:ss");
                    }

                }

                $scope.pickupToAddress = function () {

                    var contacterEmArr = new Array();
                    // show contact list to pick up.
                    var modalDefaults = {
                        templateUrl: 'app/common/contactor/contactor-pickup.tpl.html',
                        controller: 'contactorPickupCtrl',
                        size: 'lg',
                        resolve: {
                            custNum: function () { return custnum; },
                            contacterEmArr: function () { return contacterEmArr; },
                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults).then(function (contacts) {
                        var address = '';
                        var collectorNames = '';
                        angular.forEach(contacts, function (cont) {
                            if (!address) {
                                address += cont.emailAddress;
                            } else {
                                address += ';' + cont.emailAddress;
                            }
                            collectorNames += (cont.name + ', ');
                        });

                        $scope.callInstance.contacterId = address;
                    });
                };

                $scope.submit = function () {
                    if ($scope.callInstance.contacterId == null || $scope.callInstance.contacterId == "") {
                        alert("Please input the contact");
                        return;
                    }

                    if (callInstance.title == "Dispute Call") {
                        if ($scope.callInstance.endTime == null) {
                            var now = new Date();
                            $scope.callInstance.endTime = now.format("yyyy-MM-dd hh:mm:ss");
                        }
                    } else {
                        if ($scope.callInstance.endTime == null) {
                            var now = new Date();
                            $scope.callInstance.endTime = now.format("yyyy-MM-dd hh:mm");
                        }
                    }

                    $scope.callInstance.invoiceIds = invoiceIds;
                    $scope.callInstance.customerNum = custnum;
                    $scope.callInstance.siteuseId = siteuseId;
                    $scope.callInstance.LegalEntity = legalEntity;
                    $scope.callInstance.$save().then(function (result) {
                        result = "submit";
                        $uibModalInstance.close(result);
                    })
                }


                $scope.closeWindow = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                }

            }])//contactCallCtrl End

    .controller('contactResponseCtrl',
        ['$scope', 'responseInstance', 'custnum', 'legalEntity', 'invoiceIds', 'siteuseId', '$uibModalInstance', '$interval', 'contactCustomerProxy', 'modalService', 'contactHistoryProxy',
            function ($scope, responseInstance, custnum, legalEntity, invoiceIds, siteuseId, $uibModalInstance, $interval, contactCustomerProxy, modalService, contactHistoryProxy) {
                $scope.responseInstance = responseInstance;
                $scope.submitContent = "Submit";
                if (responseInstance["title"] != "Response Create") {
                    $scope.submitContent = "Save";
                }

                $scope.invoiceIds = invoiceIds;
                $scope.dateOptions = {
                    formatYear: 'yy',
                    maxDate: new Date(2099, 5, 22),
                    minDate: new Date(),
                    startingDay: 1
                };
                var result = "";

                $interval(function () {
                    $scope.setAttr();
                }, 0, 1);

                $scope.setAttr = function () {
                    if (responseInstance["title"] == "Response Create") {
                        $("#btnSubmit").show();
                        $("#btnSave").hide();
                    }
                    if (responseInstance["title"] == "Response Detail") {
                        $("#btnSave").show();
                        $("#btnSubmit").hide();
                    }

                    var now = new Date();
                    $scope.responseInstance.contactDate = now.format("yyyy-MM-dd");
                }

                $scope.submit = function () {

                    $scope.responseInstance.CustomerNum = custnum;
                    $scope.responseInstance.InvoiceIds = invoiceIds;
                    $scope.responseInstance.ContactType = "Response";
                    $scope.responseInstance.LegalEntity = legalEntity;
                    $scope.responseInstance.SiteuseId = siteuseId;
                    $scope.responseInstance.IsCostomerContact = true;

                    contactHistoryProxy.create($scope.responseInstance, function () {
                        result = "submit";
                        $uibModalInstance.close(result);
                    }, function () {
                        alert("");
                    })
                }

                $scope.save = function () {
                    contactHistoryProxy.update($scope.responseInstance, function () {
                        result = "submit";
                        $uibModalInstance.close(result);
                    }, function () {
                        alert("");
                    })
                }

                $scope.closeWindow = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                }

            }])

    .controller('contactOverdueCtrl',
        ['$scope', 'overdueReasonInstance', 'overdueReasons', '$uibModalInstance', 'modalService', 'invoiceProxy', 'baseDataProxy',
            function ($scope, overdueReasonInstance, overdueReasons, $uibModalInstance, modalService, invoiceProxy, baseDataProxy) {
                $scope.overdueReasonInstance = overdueReasonInstance;
                $scope.reasons = overdueReasons;

                var result = "";

                $scope.submit = function () {
                    invoiceProxy.saveOverdueReason($scope.overdueReasonInstance, function () {
                        result = "submit";
                        $uibModalInstance.close(result);
                    }, function () {
                        alert("");
                    })
                }

                $scope.closeWindow = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                }

            }])

    //contactBreakPTPCtrl Start
    .controller('contactBreakPTPCtrl',
        ['$scope', 'Instance', 'custnum', 'invoiceIds', '$uibModalInstance', 'breakPtpProxy', 'contactId',
            function ($scope, Instance, custnum, invoiceIds, $uibModalInstance, breakPtpProxy, contactId) {

                $scope.Instance = Instance;
                $scope.confirm = function () {
                    if ($scope.Instance.discription == null || $scope.Instance.discription == "") {
                        if (confirm("No Comments Input ! continue ?")) {

                        }
                        else {
                            return;
                        }
                    }

                    $scope.Instance.invoiceIds = invoiceIds;
                    $scope.Instance.customerNum = custnum;
                    $scope.Instance.logAction = "BREAK PTP";
                    $scope.Instance.logType = "6";
                    $scope.Instance.newStatus = "004010";
                    $scope.Instance.newTrack = "007";
                    $scope.Instance.proofId = contactId;
                    //$scope.Instance.discription = $scope.Instance.discription;
                    breakPtpProxy.saveBreakPTP(Instance, function (res) {
                        $uibModalInstance.close("submit");
                    });
                }
                $scope.closeWindow = function () { $uibModalInstance.close("cancel"); }

            }])//contactBreakPTPCtrl End
    //added by zhangYu

    //contactHoldCustomerCtrl Start
    .controller('contactHoldCustomerCtrl',
        ['$scope', 'Instance', 'custnum', 'invoiceIds', '$uibModalInstance', 'holdCustomerProxy', 'contactId',
            function ($scope, Instance, custnum, invoiceIds, $uibModalInstance, holdCustomerProxy, contactId) {

                $scope.Instance = Instance;
                $scope.confirm = function () {
                    if ($scope.Instance.discription == null || $scope.Instance.discription == "") {
                        if (confirm("No Comments Input ! continue ?")) {

                        }
                        else {
                            return;
                        }
                    }
                    $scope.Instance.invoiceIds = invoiceIds;
                    $scope.Instance.customerNum = custnum;
                    $scope.Instance.logAction = "HOLD";
                    $scope.Instance.logType = "8";//hold customer
                    $scope.Instance.newStatus = "004011";
                    $scope.Instance.newTrack = "011";
                    $scope.Instance.proofId = contactId;
                    //$scope.Instance.discription = $scope.Instance.discription;
                    holdCustomerProxy.saveHoldCustomer(Instance, function (res) {
                        $uibModalInstance.close("submit");
                    });
                }
                $scope.closeWindow = function () { $uibModalInstance.close("cancel"); }

            }])//contactHoldCustomerCtrl End
    //added by zhangYu

    .controller('contactPtpCtrl',
        ['$scope', 'siteUseId', 'customerNo', 'legalEntity', 'proamount', 'custnum', 'invoiceIds', 'contactId', 'relatedEmail', 'contactPerson', '$uibModalInstance', 'contactProxy', 'collectorSoaProxy', 'baseDataProxy', 'PTPPaymentProxy',
            function ($scope, siteUseId, customerNo, legalEntity, proamount, custnum, invoiceIds, contactId, relatedEmail, contactPerson, $uibModalInstance, contactProxy, collectorSoaProxy, baseDataProxy, PTPPaymentProxy) {
                baseDataProxy.SysTypeDetail("010", function (list) {
                    $scope.payMethodList = list;
                })
                var result = "";
                $scope.proamount = proamount;
                $scope.partialpay = "false";
                $scope.promiseDate = "";
                $scope.canEdit = true;
                $scope.IsForwarder = false;
                $scope.payer = "";

                PTPPaymentProxy.queryPayer(siteUseId, function (list) {
                    $scope.paryerList = list;
                    $scope.paryerListTotla = list;
                }, function (res) {
                    alert(res);
                })

                $scope.searchPayer = function () {
                    if ($scope.payer == undefined || $scope.payer == "") {
                        $scope.paryerList = $scope.paryerListTotla;
                        return;
                    }
                    var val = $scope.payer;
                    var i = 0;
                    var temp = new Array();
                    var searchFlag = true;
                    angular.forEach($scope.paryerListTotla, function (x) {
                        if (x == null || x == "") {
                            searchFlag = false;
                        }
                        if (searchFlag && x.indexOf(val) >= 0) {
                            temp[i] = x;
                            i++;
                        }

                    });
                    $scope.paryerList = temp;
                }

                if (relatedEmail) {
                    $scope.relatedEmail = relatedEmail;
                }
                $scope.partialpayChange = function () {
                    if ($scope.partialpay === "true") {
                        $scope.canEdit = false;
                    }
                    else {
                        $scope.proamount = proamount;
                        $scope.canEdit = true;
                    }
                };
                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };
                $scope.submit = function () {
                    if ($scope.promiseDate == "") {
                        alert("Please choose promise date !");
                        return;
                    }
                    else if ($scope.IsForwarder == true && $scope.payer == "") {
                        alert("Please input payer !");
                        return;
                    }
                    else if ($("#parpay").val() == "true" && $scope.proamount == undefined) {
                        alert("Please fill the Promise Amount !");
                        return;
                    }
                    else {
                        //added by zhangYu part refrsh dunning invoice list Start
                        var result = []
                        result.push('submit');// = "submit";
                        // result.push($("#comments").val());
                        //  result.push($scope.promiseDate);
                        //added by zhangYu part refrsh dunning invoice list End
                        var list = [];
                        list.push('4');//4:ptp
                        //invoice list[1]
                        list.push(invoiceIds.join(','));
                        //custnum list[2]
                        list.push(customerNo);
                        //contactid 3
                        list.push(contactId);
                        //contactperson 4
                        list.push(contactPerson);
                        //promissdate 5
                        list.push($scope.promiseDate);
                        //partialpay 6
                        list.push($scope.partialpay);
                        //payer 7
                        list.push($scope.payer);
                        //paymethod 8
                        list.push($scope.paymethod);
                        //contracter 9
                        list.push($scope.contacter);
                        //comments 10
                        list.push($("#comments").val());
                        //11amount
                        list.push($scope.proamount);
                        //12
                        list.push(siteUseId);
                        //13
                        list.push(legalEntity);
                        //14
                        list.push($scope.IsForwarder);
                        collectorSoaProxy.savecommon(list, function () {
                            $uibModalInstance.close(result);
                        })
                        //    alert(
                    }
                    //for (i = 0; i < invoiceIds.length; i++) {
                    //    contactProxy.insertDatasForPtp(custnum, invoiceIds[i], contactId, contactPerson, $("#comments").val(), $scope.promissDate,
                    //    function (res) {
                    //        $uibModalInstance.close(result);
                    //    }, function (error) {
                    //        alert(error);
                    //    })
                    //}
                };
                $scope.dateOptions = {
                    //dateDisabled: disabled,
                    formatYear: 'yy',
                    maxDate: new Date(2099, 5, 22),
                    minDate: new Date(),
                    startingDay: 1
                };
                // Disable weekend selection
                function disabled(data) {
                    var date = data.date,
                        mode = data.mode;
                    return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
                }
                $scope.openPromiseDate = function () {
                    $scope.popupPromiseDate.opened = true;
                };
                $scope.popupPromiseDate = {
                    opened: false
                };
            }])

    .controller('contactDisputeCtrl',
        ['$scope', 'legalEntity', 'siteUseId', 'disInvInstance', 'custnum', 'invoiceIds', 'contactId', 'relatedEmail', 'contactPerson', '$uibModalInstance', 'contactCustomerProxy', 'baseDataProxy',
            function ($scope, legalEntity, siteUseId, disInvInstance, custnum, invoiceIds, contactId, relatedEmail, contactPerson, $uibModalInstance, contactCustomerProxy, baseDataProxy) {
                var result = "";
                $scope.disInvInstance = disInvInstance;
                baseDataProxy.SysTypeDetail("025", function (list) {
                    $scope.disputeReasonList = list;
                });
                baseDataProxy.SysTypeDetail("038", function (list) {
                    $scope.actionOwnerDeptList = list;
                });

                if (relatedEmail) {
                    $scope.relatedEmail = relatedEmail;
                }

                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

                $scope.submit = function () {
                    if ($scope.issue == "undefined" || $scope.issue == null) {
                        alert("Please choose a Dispute Reason !");
                        return;
                    }
                    result = "submit";

                    $scope.disInvInstance.customerNum = custnum;
                    $scope.disInvInstance.invoiceIds = invoiceIds;
                    $scope.disInvInstance.contactId = contactId;
                    $scope.disInvInstance.relatedEmail = relatedEmail;
                    $scope.disInvInstance.contactPerson = contactPerson;
                    $scope.disInvInstance.comments = $("#comments").val();
                    $scope.disInvInstance.issue = $scope.issue;
                    $scope.disInvInstance.actionOwnerDepartment = $scope.actionownerdept;
                    $scope.disInvInstance.siteUseId = siteUseId;
                    $scope.disInvInstance.LegalEntity = legalEntity;
                    $scope.disInvInstance.callContact = $scope.contacter;
                    $scope.disInvInstance.$save(function (res) {
                        $uibModalInstance.close(result);
                    }, function (err) {
                        alert(err);
                    })
                }

            }])


    .controller('contactCommentCtrl',
        ['$scope', 'siteUseId', 'customerNo', 'legalEntity', 'comment', '$uibModalInstance', 'contactProxy', 'collectorSoaProxy',
            function ($scope, siteUseId, customerNo, legalEntity, comment, $uibModalInstance, contactProxy, collectorSoaProxy) {
                $scope.comss = comment;

                var result = "";
                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };
                $scope.submit = function () {
                    var result = []
                    result.push('submit');// = "submit";
                    collectorSoaProxy.saveCustomerAgingComments(legalEntity, customerNo, siteUseId, $scope.comss, function () {
                        $uibModalInstance.close(result);
                    })
                };
            }])
