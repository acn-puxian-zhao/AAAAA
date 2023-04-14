angular.module('app.contactMaster', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            //contact Customer
        .when('/contactcustomer/contactmaster/:nums/:legalEntity', {
            templateUrl: 'app/contactcustomer/contactmaster/contactMaster-list.tpl.html',
            controller: 'contactCtrl',
            resolve: {
                //bdStatus: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("026");
                //}],
                //bdDisputeReasonStatus: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("025");
                //}],

                //languagelist: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("013");
                //}],
                //inUse: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("018");
                //}],
                actionType: function ()
                { return 'CC' },
                title: function ()
                { return 'Contact Customer' }
            }
        })
            //break PTP
        .when('/contactcustomer/breakptp/:nums/:legalEntity', {
            templateUrl: 'app/contactcustomer/contactmaster/contactMaster-list.tpl.html',
            controller: 'contactCtrl',
            resolve: {
                //bdStatus: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("026");
                //}],
                //bdDisputeReasonStatus: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("025");
                //}],

                //languagelist: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("013");
                //}],
                //inUse: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("018");
                //}],

                actionType: function () {
                    return 'BPTP';
                },
                title: function ()
                { return 'BREAK PTP' }
            }
        })
            //hold Customer
        .when('/contactcustomer/holdcustomer/:nums/:kbn/:legalEntity', {
            templateUrl: 'app/contactcustomer/contactmaster/contactMaster-list.tpl.html',
            controller: 'contactCtrl',
            resolve: {
                //bdStatus: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("026");
                //}],
                //bdDisputeReasonStatus: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("025");
                //}],

                //languagelist: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("013");
                //}],
                //inUse: ['baseDataProxy', function (baseDataProxy) {
                //    return baseDataProxy.SysTypeDetail("018");
                //}],

                actionType: function () {
                    return 'HC';
                },
                title: function ()
                { return 'Hold Customer' }
            }
        });
    }])

    .controller('contactCtrl',
    ['$scope', 'baseDataProxy', 'contactCustomerProxy', 'contactHistoryProxy', 'actionType', 'generateSOAProxy', '$q',
    '$routeParams', 'mailProxy', 'modalService', 'collectorSoaProxy', 'uiGridConstants',
    'contactProxy', 'customerPaymentbankProxy', 'customerPaymentcircleProxy', 'customerProxy',
    'FileUploader', 'APPSETTING', 'breakPtpProxy', '$interval', 'disputeTrackingProxy', 'holdCustomerProxy', 'siteProxy', 'title', 'unholdCustomerProxy', 'commonProxy', '$sce',
       function ($scope, baseDataProxy, contactCustomerProxy, contactHistoryProxy, actionType, generateSOAProxy, $q,
       $routeParams, mailProxy, modalService, collectorSoaProxy, uiGridConstants,
       contactProxy, customerPaymentbankProxy, customerPaymentcircleProxy, customerProxy,
       FileUploader, APPSETTING, breakPtpProxy, $interval, disputeTrackingProxy, holdCustomerProxy, siteProxy, title, unholdCustomerProxy, commonProxy, $sce) {

           //*****************************************Top*******************************************************s
           $scope.title = title;
           $scope.contactShow = false;
           $scope.bankShow = false;
           $scope.calenderShow = false;
           $scope.domainShow = false;
           $scope.gridApis = [];
           $scope.custInfo = [];
           $scope.custInfo[2] = $routeParams.nums;  //custInfo[8]
           $scope.custInfo[3] = true;   //custInfo[9]
           $scope.languagelist = "";
           $scope.inUselist = "";
           $scope.entityFlg = "";
           $scope.paymentCircle = "";
           $scope.currCustNum = "";
           //######################### Add by Alex #############
           //contact add button show or hide
           $scope.isContactAddBtnShow = false;
           //payment bank  add button show or hide
           $scope.isPayBankAddBtnShow = false;
           //contact domain add button show or hide 
           $scope.isDomainAddBtnShow = false;
           //contact list grid show or hide
           $scope.isContactGridShow = false;
           //dispute list grid show or hide
           $scope.isDisputeGridShow = false;
           //######################### Add by Alex #############

           siteProxy.GetLegalEntity("type", function (legal) {
               $scope.legallist = legal;
           }, function () {
           });

           $scope.actionType = actionType;//ng-show Function button use
           var custNums = $routeParams.nums;
           $scope.hold_unhold = $routeParams.kbn;//hold:[Hold Customer]/Unhold;[Unhold Customer]
           if (actionType == 'HC') { $scope.title = $scope.hold_unhold; }
           $scope.cancelHoldFlg = true;

           //*****************************************Get CustomerName***************************s
           //contactCustomerProxy.queryObject({ customerNum: $routeParams.nums, customerStatus: "contact" }, function (customerName) {
           //    document.getElementById("customerName").innerHTML = customerName;
           //});
           //*****************************************Get CustomerName***************************e

           //change table
           $scope.changetab = function (type) {
               //get selected invoiceIds
               $scope.inv = [];
               $scope.invObjArr = [];
               $scope.invoObj = { num: {}, status: {} };
               for (j = 0; j < $scope.gridApis.length; j++) {
                   angular.forEach($scope.gridApis[j].selection.getSelectedRows(), function (rowItem) {
                       //alert(rowItem.invoiceNum);
                       if (rowItem.invoiceId != 0) {
                           $scope.inv.push(rowItem.invoiceId);
                           $scope.invoObj.num = rowItem.invoiceNum;
                           $scope.invoObj.status = rowItem.status;
                           $scope.invObjArr.push($scope.invoObj);
                       }
                   });
               }
               if (type == "call") {
                   //added by zhangYu

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
                       if (actionType == "BPTP") {
                           callInstance.logAction = "BREAK PTP";
                       } else if (actionType == "CC") {
                           callInstance.logAction = "CONTACT";
                       }

                       var modalDefaults = {
                           templateUrl: 'app/common/contactdetail/contact-call.tpl.html',
                           controller: 'contactCallCtrl',
                           size: 'lg',
                           resolve: {
                               callInstance: function () { return callInstance; },
                               custnum: function () { return $routeParams.nums; },
                               invoiceIds: function () { return $scope.inv; }
                           },
                           windowClass: 'modalDialog'
                       };
                       modalService.showModal(modalDefaults, {}).then(function (result) {
                           $scope.reSeachContactList();
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
                   if (!$scope.custInfo[1].id) {
                       if (confirm("No mail selected ! continue ?")) {
                       }
                       else {
                           return;
                       }
                   }

                   breakPtpProxy.getBreakPTP(function (invoLogInstance) {
                       invoLogInstance["title"] = "Confirm Break PTP";
                       var modalDefaults = {
                           templateUrl: 'app/common/contactdetail/contact-breakptp.tpl.html',
                           controller: 'contactBreakPTPCtrl',
                           size: 'lg',
                           resolve: {
                               Instance: function () { return invoLogInstance; },
                               custnum: function () { return $routeParams.nums; },
                               contactId: function () { return $scope.custInfo[1].messageId; },
                               invoiceIds: function () { return $scope.inv; }
                           },
                           windowClass: 'modalDialog'
                       };
                       modalService.showModal(modalDefaults, {
                       }).then(function (result) {
                           if (result == "submit") {
                               $scope.initList(true);
                           }

                       });
                   }); //contactCustomerProxy

               }//confirmbreakptp end
               else if (type == "sendbreakletter" || type == "sendMail") {
                   if ($scope.custInfo[1] == "Only select one mail!") {
                       alert($scope.custInfo[1]);
                       return;
                   }

                   var sendMailUrl = '';
                   if (actionType == "BPTP") {
                       sendMailUrl = breakPtpProxy.sendEmailUrl;
                   }

                   if (actionType == "HC") {
                       sendMailUrl = holdCustomerProxy.sendEmailUrl;
                   }

                   var modalDefaults = {
                       templateUrl: 'app/common/mail/mail-instance.tpl.html',
                       controller: 'mailInstanceCtrl',
                       size: 'customSize',
                       resolve: {
                           custnum: function () { return $routeParams.nums; },
                           //                                    selectedInvoiceId: function () { return $scope.inv; },
                           instance: function () {
                               return getMailInstance($routeParams.nums);
                           },
                           //mType: function () {
                           //    return "001";
                           //},
                           mailDefaults: function () {
                               return {
                                   mailType: 'NE',
                                   mailUrl: sendMailUrl
                               };
                           }
                       },
                       windowClass: 'modalDialog'
                   };

                   modalService.showModal(modalDefaults, {
                   }).then(function (result) {
                       $scope.reSeachContactList();
                       //$scope.$broadcast("MAIL_DATAS_REFRESH");
                   }, function (err) {
                       alert(err);
                   });

               }//sendBreakLetter end
               else if (type == "changeStatus") {
                   if ($scope.inv == "" || $scope.inv == null) {
                       alert("Please choose 1 invoice at least .")
                       return;
                   }

                   if ($scope.custInfo[1] == "Only select one mail!") {
                       alert($scope.custInfo[1]);
                       return;
                   }

                   //confirm breakPTP & Change Staus Action order 
                   var errMsgType = "";
                   var statusKbn = "";
                   if (actionType == "BPTP") {
                       errMsgType = "Confirm Break PTP";
                       statusKbn = "Broken PTP";
                   }
                   else if (actionType == "HC") {
                       errMsgType = "Hold Customer";
                       statusKbn = "Hold";

                   }
                   for (var i = 0; i < $scope.invObjArr.length; i++) {

                       if ($scope.invObjArr[i].status != statusKbn) {
                           alert("Before change status, please " + errMsgType + " first."  + '\n' + "(Invoice#:" + $scope.invObjArr[i].num + ")");
                           return;
                       }
                   }
                   //relate Mail
                   if (!$scope.custInfo[1].id) {
                       if (confirm("No mail selected ! continue ?")) {
                       }
                       else {
                           return;
                       }
                   }
                   var modalDefaults = {
                       templateUrl: 'app/common/changestatus/changeStatus-list.tpl.html',
                       controller: 'changeStatusCtrl',
                       resolve: {
                           title: function () {
                               return "Change Status";
                           },
                           id: function () {
                               return $scope.inv;
                           },

                           type: function () {
                               if (actionType == "BPTP") {
                                   return "027";
                               }
                               else if (actionType == "HC") {
                                   return "028";
                               }
                           },
                           index: function ()
                           { return ""; },
                           mailId: function ()
                           { return $scope.custInfo[1].messageId; }
                       },
                       windowClass: 'modalDialog'
                   };
                   modalService.showModal(modalDefaults, {}).then(function (result) {
                       disputeTrackingProxy.queryObject({ id: $scope.disputeId }, function (list) {
                           $scope.issReason = list[0];
                           $scope.disputeStatus = list[1];
                           $scope.disputeDate = list[2];
                           $scope.disputeNotes = list[3];
                           $scope.customerNumber = list[4];
                           $scope.disputeStatusCode = list[5];
                       });

                       //Get Dispute Status Change
                       disputeTrackingProxy.query({ disputeid: $scope.disputeId }, function (list) {
                           if (result == "submit") {
                               $scope.initList(true);
                           }
                       });
                   });

               }//changeStatus end
               else if (type == "holdCustomer") {
                   if ($scope.inv == "" || $scope.inv == null) {
                       if (confirm("No invoice selected ! continue ?")) {
                       }
                       else {
                           return;
                       }
                   }

                   if ($scope.custInfo[1] == "Only select one mail!") {
                       alert($scope.custInfo[1]);
                       return;
                   }

                   //relate Mail
                   if (!$scope.custInfo[1].id) {
                       if (confirm("No mail selected ! continue ?")) {
                       }
                       else {
                           return;
                       }
                   }

                   holdCustomerProxy.getHoldCustomer(function (invoLogInstance) {
                       invoLogInstance["title"] = "Hold Customer";
                       invoLogInstance.legalEntity = $routeParams.legalEntity;
                       var modalDefaults = {
                           templateUrl: 'app/common/contactdetail/contact-breakptp.tpl.html',
                           controller: 'contactHoldCustomerCtrl',
                           size: 'lg',
                           resolve: {
                               Instance: function () { return invoLogInstance; },
                               custnum: function () { return $routeParams.nums; },
                               contactId: function () { return $scope.custInfo[1].messageId; },
                               invoiceIds: function () { return $scope.inv; }
                           },
                           windowClass: 'modalDialog'
                       };
                       modalService.showModal(modalDefaults, {
                       }).then(function (result) {
                           if (result == "submit") {
                               $scope.cancelHoldFlg = false;
                               // $scope.initList(false);
                               $scope.initList(true);
                           }

                       });
                   }); //holdCustomerProxy

               }//holdCustomer end
               else if (type == 'cancelHoldAlert') {
                   var customerNum = $routeParams.nums;
                   var legalEntity = $routeParams.legalEntity;
                   if (customerNum != null && legalEntity != null) {

                       holdCustomerProxy.queryObject({
                           customerNum: customerNum, legalEntity: legalEntity
                       }, function (result) {
                           alert("Cancel Hold Alert Success!");

                       }, function (error) {
                           alert(error);
                       }); //holdCustomerProxy
                   }// if custNums
               }//Cancel Hold Alert End
               else if (type == 'unholdCustomer') {

                   if ($scope.custInfo[1] == "Only select one mail!") {
                       alert($scope.custInfo[1]);
                       return;
                   }

                   if (!$scope.custInfo[1].id) {
                       if (confirm("No mail selected ! continue ?")) {
                       }
                       else {
                           return;
                       }
                   }

                   var customerNum = $routeParams.nums;
                   var legalEntity = $routeParams.legalEntity;
                   var reMailId = $scope.custInfo[1].messageId;
                   if (reMailId == null) {
                       reMailId = "";
                   }

                   if (customerNum != null && legalEntity != null) {

                       unholdCustomerProxy.queryObject({
                           customerNum: customerNum, legalEntity: legalEntity, reMailId: reMailId
                       }, function (result) {
                           alert("Unhold Customer Success!");
                           $scope.initList(false);
                       }, function (error) {
                           alert(error);
                       }); //unholdCustomerProxy
                   }// if custNums
               }//unholdCustomer end

                   //unholdCustomer
                   //**************************************************************added by zhangYu************************End
               else if (type == "dispute") {

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
                   contactHistoryProxy.queryObject({ type: 'dispute' }, function (disInvInstance) {
                       disInvInstance["title"] = "Dispute Reason";
                       var modalDefaults = {
                           templateUrl: 'app/common/contactdetail/contact-dispute.tpl.html',
                           controller: 'contactDisputeCtrl',
                           size: 'lg',
                           resolve: {
                               disInvInstance: function () { return disInvInstance; },
                               custnum: function () { return $routeParams.nums; },
                               invoiceIds: function () { return $scope.inv; },
                               contactId: function () { return $scope.custInfo[1].messageId; },
                               relatedEmail: function () { return relatedMail; },
                               contactPerson: function () { return $scope.custInfo[1].to; }
                           },
                           windowClass: 'modalDialog'
                       };
                       modalService.showModal(modalDefaults, {}).then(function (result) {
                           if (result == "submit") {
                               $scope.initList(false);

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
               } else if (type == "ptp") {
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
                           $scope.initList(false);
                       }
                   });
               } else if (type == "notice") {
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
                           $scope.initList(false);
                       }
                   });
               }
           }
           //*******************************************mail & template********************************************//
           var getMailInstance = function (custNums) {
               var instance = {};
               var allDefered = $q.defer();

               $q.all([
                   getMailInstanceMain(custNums)
               ])
               .then(function (results) {
                   instance = results[0];
                   allDefered.resolve(instance);
               });

               return allDefered.promise;
           };

           //getMailInstanceMain
           var getMailInstanceMain = function (custNums) {

               var instanceDefered = $q.defer();
               if (actionType == "BPTP") {
                   breakPtpProxy.getMailInstance(custNums, function (res) {
                       var instance = res;
                       renderInstance(instance, custNums);

                       instanceDefered.resolve(instance);
                   }, function (error) {
                       alert(error);
                   });
               }//BPTP End
               else if (actionType == "HC") {
                   holdCustomerProxy.getMailInstance(custNums, function (res) {
                       var instance = res;
                       renderInstance(instance, custNums);

                       instanceDefered.resolve(instance);
                   }, function (error) {
                       alert(error);
                   });
               }//HC End

               return instanceDefered.promise;
           };
           var renderInstance = function (instance, custNums) {
               //subject
               //instance.subject = 'SOA-' + $scope.shortsub.join('-');
               instance.subject = "";
               //invoiceIds
               //instance.invoiceIds = $scope.inv;
               //soaFlg
               instance.soaFlg = "0";
               //Bussiness_Reference
               var customerMails = [];
               angular.forEach(custNums.split(','), function (cust) {
                   customerMails.push({ MessageId: instance.messageId, CustomerNum: cust });
               });
               instance.CustomerMails = customerMails; //$routeParams.nums;
               
               //mailType
               if (actionType == "BPTP") {
                   instance["title"] = "Send Break Letter";
               } else if (actionType == "HC") {
                   instance["title"] = "Send Mail";
               }

           };

           //*******************************************mail & template********************************************//

           //
           $scope.show = function (type, id) {
               $scope.baseDataGet();
               $scope.showtype = type;
               $scope.showid = id;
               $("#").removeClass('ng-hide');
               if (type == "contact") {
                   $("#conhide").show();
                   $("#conshow").hide();
                   //##### paymentback and paymentcalender clear #### Add by Alex ######
                   $("#payhide").hide();
                   $("#payshow").show();
                   $("#calhide").hide();
                   $("#calshow").show();
                   //####### contactor domain ##########add by jiaxng #####
                   $("#domhide").hide();
                   $("#domshow").show();
                   //###################################################################
                   $scope.contactShow = true;
                   //##### ui grid of paymentback and paymentcalender is hide #### Add by Alex ###
                   $scope.bankShow = false;
                   $scope.calenderShow = false;
                   $scope.domainShow = false;
                   //contact add button is show
                   $scope.isContactAddBtnShow = true;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   //contactor domain add button is hide
                   $scope.isDomainAddBtnShow = false;
                   //#############################################################################
                   collectorSoaProxy.query({ CustNumFCon: id }, function (list) {
                       $scope.currCustNum = id;
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == id) {
                               row['cg'].data = list;
                           }
                       });
                   });
               } else if (type == "paymentBank") {
                   $("#payhide").show();
                   $("#payshow").hide();
                   //##### contact and paymentcalender clear #### Add by Alex ######
                   $("#conhide").hide();
                   $("#conshow").show();
                   $("#calhide").hide();
                   $("#calshow").show();
                   //####### contactor domain ##########add by jiaxng #####
                   $("#domhide").hide();
                   $("#domshow").show();
                   //###############################################################
                   $scope.bankShow = true;
                   //##### ui grid of contact and paymentcalender is hide #### Add by Alex ###
                   $scope.contactShow = false;
                   $scope.calenderShow = false;
                   $scope.domainShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is show
                   $scope.isPayBankAddBtnShow = true;
                   //contactor domain add button is hide
                   $scope.isDomainAddBtnShow = false;
                   //#########################################################################
                   collectorSoaProxy.query({ CustNumFPb: id }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == id) {
                               row['pbg'].data = list;
                           }
                       });
                   });
               } else if (type == "paymentCalender") {

                   $scope.entityFlg = "";
                   $scope.paymentCircle = "";
                   uploader.queue[2] = "";

                   //##### contact and paymentback clear #### Add by Alex ######
                   $("#conhide").hide();
                   $("#conshow").show();
                   $("#payhide").hide();
                   $("#payshow").show();
                   //###########################################################
                   $("#calhide").show();
                   $("#calshow").hide();
                   //####### contactor domain ##########add by jiaxng #####
                   $("#domhide").hide();
                   $("#domshow").show();
                   $scope.calenderShow = true;
                   //##### ui grid of contact and paymentback is hide #### Add by Alex ###
                   $scope.contactShow = false;
                   $scope.bankShow = false;
                   $scope.domainShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   //contactor domain add button is hide
                   $scope.isDomainAddBtnShow = false;
                   //     $scope.items.entityFlg = "";
                   //       $scope.items.paymentCircle = "";
                   //#####################################################################
                   collectorSoaProxy.query({ CustNumFPc: id }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == id) {
                               row['pcg'].data = list;
                           }
                       });
                   });
               } else if (type == "contactDomain") {
                   //##### contact and paymentback clear #### Add by Alex ######
                   $("#conhide").hide();
                   $("#conshow").show();
                   $("#payhide").hide();
                   $("#payshow").show();
                   //###########################################################
                   $("#calhide").hide();
                   $("#calshow").show();
                   //####### contactor domain ##########add by jiaxng #####
                   $("#domhide").show();
                   $("#domshow").hide();
                   $scope.domainShow = true;
                   //##### ui grid of contact and paymentback is hide #### Add by Alex ###
                   $scope.contactShow = false;
                   $scope.bankShow = false;
                   $scope.calenderShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   $scope.isDomainAddBtnShow = true;
                   //     $scope.items.entityFlg = "";
                   //       $scope.items.paymentCircle = "";
                   //#####################################################################
                   collectorSoaProxy.query({ CustNumFPd: id }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == id) {
                               row['pdg'].data = list;
                           }
                       });
                   });
               }
           }
           $scope.hide = function (type, id) {
               $("#").addClass('ng-hide');
               if (type == "contact") {
                   $("#conshow").show();
                   $("#conhide").hide();
                   $scope.contactShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   $scope.isDomainAddBtnShow = false;
               } else if (type == "paymentBank") {
                   $("#payshow").show();
                   $("#payhide").hide();
                   $scope.bankShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   $scope.isDomainAddBtnShow = false;
               } else if (type == "paymentCalender") {
                   $("#calshow").show();
                   $("#calhide").hide();
                   $scope.calenderShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   $scope.isDomainAddBtnShow = false;
               } else if (type == "contactDomain") {
                   $("#domshow").show();
                   $("#domhide").hide();
                   $scope.domainShow = false;
                   //contact add button is hide
                   $scope.isContactAddBtnShow = false;
                   //payment bank add button is hide
                   $scope.isPayBankAddBtnShow = false;
                   $scope.isDomainAddBtnShow = false;
               }
           }

           //*****************************************Top*******************************************************e


           //*****************************************Middle****************************************************s

           $scope.mailDats = ['contactCtrl'];

           $interval(function () {
               $scope.initList(false);
           }, 0, 1);

           $scope.initList = function (choice) {
               contactCustomerProxy.query({
                   custNum: $routeParams.nums, type: actionType, legalEntity: $routeParams.legalEntity
               }, function (list) {
                   $scope.invlist = list;
                   var i = 0;
                   angular.forEach($scope.invlist, function (row) {
                       //row['paymentCircle'] = "";
                       //row['entityFlg'] = "";
                       row['cusCode'] = "";
                       row['cg'] = {
                           data: row.subContact,
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
                               cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckType(row.entity)}}</div>'
                           },
                            {
                                field: 'isGroupLevel', displayName: 'Is Group Level',
                                cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckGroupLevel(row.entity)}}</div>', width: '90'
                            },
                           {
                               name: 'o', displayName: 'Operation', width: '90',
                               cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditContacterInfo(row.entity)"  class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.Delcontacter(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                           }
                           ]
                       };
                       row['pbg'] = {
                           data: row.subPaymentBank,
                           columnDefs: [
                       { field: 'bankAccountName', displayName: 'Account Name' },
                       { field: 'legalEntity', displayName: 'Legal Entity' },
                       { field: 'bankName', displayName: 'Bank Name' },
                       { field: 'bankAccount', displayName: 'Bank Account' },
                       { field: 'createDate', displayName: 'Create Date', cellFilter: 'date:\'yyyy-MM-dd\'' },
                       { field: 'createPersonId', displayName: 'Create Person' },
                       { field: 'inUse', displayName: 'InUse' },
                       { field: 'description', displayName: 'Description' },
                       {
                           name: 'o', displayName: 'Operation', width: '90',
                           cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditBankInfo(row.entity)" class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelBankInfo(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                       }
                           ]
                       };
                       row['pcg'] = {
                           data: row.subPaymentCalender,
                           columnDefs: [
                           { field: 'sortId', displayName: '#' },
                           { field: 'legalEntity', displayName: 'Legal Entity' },
                           { field: 'paymentDay', displayName: 'Payment Day', cellFilter: 'date:\'yyyy-MM-dd\'' },
                           { field: 'weekDay', displayName: 'Week Day' },
                           {
                               name: 'o', displayName: 'Operation', width: '90',
                               cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelPaymentcircle(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                           }
                           ]
                       };
                       //add by jiaxing for show ContactorDomain
                       row['pdg'] = {
                           data: row.subContactDomain,
                           columnDefs: [
                            { field: 'sortId', displayName: '#' , width: '120',},
                            { field: 'customerNum', displayName: 'Customer Num' },
                            { field: 'legalEntity', displayName: 'Legal Entity' },
                            { field: 'mailDomain', displayName: 'Email Domain' },
                            {
                                name: 'o', displayName: 'Operation', width: '90',
                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditCustDomain(row.entity)"  class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelCustDomain(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                            }
                           ]
                       }
                       angular.forEach(row.subLegal, function (rowItem) {
                           rowItem['gridoption'] =
                           {
                               data: rowItem.subInvoice,
                               enableFiltering: true,
                               showGridFooter: true,
                               //enableFullRowSelection: true,
                               columnDefs: [
                               {
                                   name: 'invoiceNum', displayName: 'Invoice #', enableCellEdit: false, width: '100', pinnedLeft: true,
                                   cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.openInvoiceH(row.entity.invoiceNum)">{{row.entity.invoiceNum}}</a></div>'
                               },
                               { field: 'invoiceDate', enableCellEdit: false, displayName: 'Invoice Date', cellFilter: 'date:\'yyyy-MM-dd\'', width: '100' },
                               { field: 'creditTerm', enableCellEdit: false, displayName: 'Credit Term', width: '100', },
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
                                   { field: 'daysLate', enableCellEdit: false, displayName: 'Days Late', type: 'number', cellClass: 'right', width: '80' },
                               {
                                   field: 'status', enableCellEdit: false, displayName: 'Status', width: '80'
                                   , cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                       if (grid.getCellValue(row, col) == "Dispute") {
                                           return 'uigridred';
                                       }
                                   }, filter: {
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
                                   field: 'invoiceTrack', enableCellEdit: false, displayName: 'Invoice Track', width: '100',
                                   filter: {
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
                                   field: 'comments', displayName: 'Invoice Memo', width: '120',
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
                   }, 0, 1);
                   //add by jiaxing for solve  question do initList clear contact list
                   if (choice == true) {
                       var type = $scope.showtype;
                       var id = $scope.showid;
                       if (type == "contact") {
                           collectorSoaProxy.query({ CustNumFCon: id }, function (list) {
                               angular.forEach($scope.invlist, function (row) {
                                   if (row.customerCode == id) {
                                       row['cg'].data = list;
                                   }
                               });
                           });
                       } else if (type == "paymentBank") {
                           collectorSoaProxy.query({ CustNumFPb: id }, function (list) {
                               angular.forEach($scope.invlist, function (row) {
                                   if (row.customerCode == id) {
                                       row['pbg'].data = list;
                                   }
                               });
                           });
                       } else if (type == "paymentCalender") {
                           $scope.changeLegal($scope.showlegal);
                       } else if (type == "contactDomain") {
                           collectorSoaProxy.query({ CustNumFPd: id }, function (list) {
                               angular.forEach($scope.invlist, function (row) {
                                   if (row.customerCode == id) {
                                       row['pdg'].data = list;
                                   }
                               });
                           });
                       }
                   }

                   // $scope.$broadcast("MAIL_DATAS_REFRESH", $scope.mailDats[0]);
               });

           };
           //Default select 
           $scope.selectall = function () {
               if (actionType == "CC") { return; }
               var strPtpDate;
               var frPtpDate;
               var rePtpDate;
               for (k = 0; k < $scope.gridApis.length; k++) {
                   angular.forEach($scope.gridApis[k].grid.rows, function (rowItem) {
                       strPtpDate = rowItem.entity.ptpDate
                       if (strPtpDate != null) {
                           frPtpDate = strPtpDate.substring(0, 10);
                           frPtpDate = frPtpDate + " 23:59:59";
                           rePtpDate = frPtpDate.replace(/-/g, "/");
                       }
                       if (actionType == "BPTP") {
                           if (rowItem.entity.status == "PTP" && rowItem.entity.ptpDate != null) {//
                               if (new Date(rePtpDate) < new Date())
                               { rowItem.setSelected(true); }
                           }
                       }
                       else if (actionType == "HC") {
                           //if (rowItem.entity.status == "Broken PTP") {
                           //     rowItem.setSelected(true); 
                           //}
                       }


                   });
               }
           }
           $scope.openInvoiceH = function (inNum) {
               var modalDefaults = {
                   templateUrl: 'app/soa/invhistory/invhistory.tpl.html',
                   controller: 'invHisCL',
                   size: 'lg',
                   resolve: {
                       inNum: function () {
                           return inNum;
                       }
                   }
                   , windowClass: 'modalDialog'
               };

               modalService.showModal(modalDefaults, {}).then(function (result) {

               });

           }

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
                   $('#boxEdit').css({
                       'left': left + 'px', 'top': top + 'px'
                   });
                   $('#txtBox').css({
                       'width': contentWidth - 20 + 'px', 'height': contentHeight - 100 + 'px'
                   });
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
               $('#box').css({
                   'left': e.pageX - 410 + 'px', 'top': e.pageY + 10 + 'px'
               });
               var str = '';
               str = 'Invoice :"' + invNum + '" Memo : <br>' + memo;
               $("#box").html(str);
               $("#box").show();

           }

           $scope.memoHide = function () {
               $("#box").hide();
           }

           //*****************************************Middle****************************************************e


           //*****************************************Bottom****************************************************s
           $scope.reSeachContactList = function () {
               contactCustomerProxy.query({ strCusNum: $routeParams.nums }, function (list) {
                   $scope.lstContact = list;
               });

               //TODO: need to add contact history and invoice log.

           }

           //area show
           $scope.showList = function (type) {
               $("#").removeClass('ng-hide');
               if (type == "contact") {
                   $("#lblContactShow").hide();
                   $("#lblContactHide").show();
                   $scope.isContactGridShow = true;
                   //Get Contact List
                   contactCustomerProxy.query({
                       strCusNum: $routeParams.nums
                   }, function (list) {
                       $scope.contactList.data = list;
                   });
               } else if (type == "dispute") {
                   $("#lblDisputeShow").hide();
                   $("#lblDisputeHide").show();
                   $scope.isDisputeGridShow = true;
                   //Get Dispute List
                   contactCustomerProxy.query({
                       strCusNumber: $routeParams.nums
                   }, function (list) {
                       $scope.disputeList.data = list;
                   });
               }
           }
           //area hide
           $scope.hideList = function (type) {
               $("#").addClass('ng-hide');
               if (type == "contact") {
                   $("#lblContactShow").show();
                   $("#lblContactHide").hide();
                   $scope.isContactGridShow = false;
               } else if (type == "dispute") {
                   $("#lblDisputeShow").show();
                   $("#lblDisputeHide").hide();
                   $scope.isDisputeGridShow = false;
               }
           }

           $scope.contactList = {
               //data: 'lstContact',
               multiSelect: false,
               columnDefs: [
                    {
                        field: 'sortId', displayName: '#', width: '70'
                    },
                    {
                        field: 'contactDate', displayName: 'Contact date', width: '130', cellFilter: 'date:\'yyyy-MM-dd\''
                    },
                    {
                        field: 'contactType', displayName: 'Contact Type', width: '200'
                    },
                    {
                        field: 'comments', displayName: 'Comments', width: '577'
                    },
                    {
                        field: '""', displayName: 'Operation', width: '120',
                        cellTemplate: '<div class="ngCellText" style="text-align:center" ng-class="col.colIndex()">' +
                                  '<a ng-click=" grid.appScope.getDetail(row.entity)"> detail </a></div>'
                    }
               ],

               onRegisterApi: function (gridApi) {
                   //set gridApi on scope
                   $scope.gridApi = gridApi;
               }
           };
           //********************contactList********************<a> ng-click********************start
           //contactList Detail
           $scope.getDetail = function (row) {
               if (row.contactType == "Mail") {
                   if (row.contactId) {
                       mailProxy.queryObject({
                           messageId: row.contactId
                       }, function (mailInstance) {
                           //mailType
                           mailInstance["title"] = "Mail View";
                           mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);

                           var modalDefaults = {
                               templateUrl: 'app/common/mail/mail-instance.tpl.html',
                               controller: 'mailInstanceCtrl',
                               size: 'customSize',
                               resolve: {
                                   custnum: function () {
                                       return mailInstance.customerNum;
                                   },
                                   instance: function () {
                                       return mailInstance
                                   },
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
                       alert('Contact Id is null')
                   }
               } //if mail end
               else if (row.contactType == "Call") {

                   if (row.contactId) {
                       contactCustomerProxy.queryObject({
                           contactId: row.contactId
                       }, function (callInstance) {
                           callInstance["contacterId"] = row.contacterId;
                           callInstance["title"] = "Call Detail";
                           if (actionType == "BPTP") {
                               callInstance.logAction = "BREAK PTP";
                           } else if (actionType == "CC") {
                               callInstance.logAction = "CONTACT";
                           }

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
                   else {
                       alert("Contact Id is null");
                   }
               } //if call end
               else {
                   alert("Not map the contact type");
               }
           } //contactList Detail end


           //********************contactList******************<a> ng-click********************end
           //$scope.bdDisputeReasonStatus = bdDisputeReasonStatus;
           //$scope.bdStatus = bdStatus;
           $scope.disputeList = {
               //data: 'lstDispute',
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

           //show dispute infomation
           $scope.getDisputeDetail = function (disputeId) {
               window.open('#/disputetracking/dispute/' + disputeId);
           }

           //**********************************add/edit/del contactors paymentbank paycircle********************
           //##############  add  ######################

           //added by alex  2016/01/22
           $scope.baseDataGet = function () {
               if ($scope.languagelist == "")
               {
                   //013
                   baseDataProxy.SysTypeDetail("013", function (list) {
                       $scope.languagelist = list;
                   });
               }

               if ($scope.inUselist == "")
               {
                   //018
                   baseDataProxy.SysTypeDetail("018", function (list) {
                       $scope.inUselist = list;
                   });
               }
           }

           $scope.addContactor = function () {
               customerProxy.queryObject({
                   num: $routeParams.nums
               }, function (entity) {
                   var deal = entity.deal;
                   var modalDefaults = {
                       templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                       controller: 'contactorEditCtrl',
                       size: 'lg',
                       resolve: {
                           cont: function () {
                               return new contactProxy();
                           },
                           num: function () {
                               return $routeParams.nums;
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
                       collectorSoaProxy.query({
                           CustNumFCon: $routeParams.nums
                       }, function (list) {
                           angular.forEach($scope.invlist, function (row) {
                               if (row.customerCode == $routeParams.nums) {
                                   row['cg'].data = list;
                               }
                           });
                       });
                   });
               })
           }

           $scope.addPayBank = function () {
               var modalDefaults = {
                   templateUrl: 'app/masterdata/paymentbank/paymentbank-edit.tpl.html',
                   controller: 'paymentbankEditCtrl',
                   size: 'lg',
                   resolve: {
                       custInfo: function () {
                           return customerPaymentbankProxy.queryObject({
                               type: "new"
                           });
                       }, num: function () {
                           return $routeParams.nums;
                       }, flg: function () {
                           return $scope.inUselist;
                       },
                       legal: function () {
                           return $scope.legallist;
                       }
                   }, windowClass: 'modalDialog'
               };

               modalService.showModal(modalDefaults, {}).then(function (result) {
                   collectorSoaProxy.query({
                       CustNumFPb: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['pbg'].data = list;
                           }
                       });
                   });
               });
           }

           $scope.addPayBankCircle = function (paymentCircle) {
               if (paymentCircle == "" || paymentCircle == null) {
                   alert("Please select Payment Date!");
               }
               else {
                   if ($scope.entityFlg == null || $scope.entityFlg == "") {
                       alert("Please Select Legal Entity First!");
                   } else {
                       var num = $routeParams.nums;
                       var paymentCircleArray = [];
                       paymentCircleArray.push(paymentCircle);
                       paymentCircleArray.push(num);
                       paymentCircleArray.push($scope.entityFlg);
                       customerPaymentcircleProxy.addPaymentCircle(paymentCircleArray, function (res) {
                           //collectorSoaProxy.query({
                           //    CustNumFPc: $routeParams.nums
                           //}, function (list) {
                           //    angular.forEach($scope.invlist, function (row) {
                           //        if (row.customerCode == $routeParams.nums) {
                           //            row['pcg'].data = list;
                           //        }
                           //    });
                           //});

                           customerPaymentcircleProxy.searchPaymentCircle(num, $scope.entityFlg, function (paydate) {
                               angular.forEach($scope.invlist, function (row) {
                                   if (row.customerCode == num) {
                                       row['pcg'].data = paydate;
                                       row['pcgshow'] = true;
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
           $scope.addContactDomain = function () {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customerdomain/custdomain-edit.tpl.html',
                    controller: 'custdomainEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return new contactProxy();
                        },
                        num: function () {
                            return $routeParams.nums;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    collectorSoaProxy.query({
                        CustNumFPd: $routeParams.nums
                    }, function (list) {
                        angular.forEach($scope.invlist, function (row) {
                            if (row.customerCode == $routeParams.nums) {
                                row['pdg'].data = list;
                            }
                        });
                    });
                });
           };
           //##############  edit  ######################
           $scope.EditContacterInfo = function (row) {
     //          row.customerNum = $scope.currCustNum;
               var modalDefaults = {
                   templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                   controller: 'contactorEditCtrl',
                   size: 'lg',
                   resolve: {
                       cont: function () {
                           return row;
                       },
                       num: function () {
                           //     return row.customerNum;
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
                   collectorSoaProxy.query({
                       CustNumFCon: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['cg'].data = list;
                           }
                       });
                   });
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
                   collectorSoaProxy.query({
                       CustNumFPb: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['pbg'].data = list;
                           }
                       });
                   });
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
                   collectorSoaProxy.query({
                       CustNumFPd: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['pdg'].data = list;
                           }
                       });
                   });
               });
           };
           //##############  remove  ######################
           $scope.Delcontacter = function (row) {
               var cusid = row.id;
               contactProxy.delContactor(cusid, function (res) {
                   alert("Delete Success");
                   collectorSoaProxy.query({
                       CustNumFCon: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['cg'].data = list;
                           }
                       });
                   });
               }, function (err) {
                   alert("Delete Error");
               });
           };

           $scope.DelBankInfo = function (row) {
               var cusid = row.id;
               customerPaymentbankProxy.delPaymentBank(cusid, function () {
                   alert("Delete Success");
                   collectorSoaProxy.query({
                       CustNumFPb: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['pbg'].data = list;
                           }
                       });
                   });
               }, function () {
                   alert("Delete Error");
               });
           };

           $scope.DelPaymentcircle = function (row) {
               var cusid = row.id;
               customerPaymentcircleProxy.delPaymentCircle(cusid, function () {
                   alert("Delete Success");
                   var cus = row.customerNum;
                   customerPaymentcircleProxy.searchPaymentCircle(cus, $scope.entityFlg, function (paydate) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == cus) {
                               row['pcg'].data = paydate;
                               row['pcgshow'] = true;
                           }
                       });
                   });
               }, function () {
                   alert("Delete Error");
               });
           };
           $scope.DelCustDomain = function (row) {
               var cusid = row.id;
               contactProxy.delCustDomain(cusid, function (res) {
                   alert("Delete Success");
                   collectorSoaProxy.query({
                       CustNumFPd: $routeParams.nums
                   }, function (list) {
                       angular.forEach($scope.invlist, function (row) {
                           if (row.customerCode == $routeParams.nums) {
                               row['pdg'].data = list;
                           }
                       });
                   });
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

           $scope.addFileCircle = function (cusCode) {
               if (uploader.queue[2] == null || uploader.queue[2] == "") {
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

                       var num = cusCode;
                       var legal = $scope.entityFlg;
                       uploader.queue[2].url = APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle?customerNum=' + num + '&legal=' + legal;
                       uploader.uploadAll();

                   }

                   // CALLBACKS
                   uploader.onSuccessItem = function (fileItem, response, status, headers) {
                       alert(response);
                       //collectorSoaProxy.query({
                       //    CustNumFPc: $routeParams.nums
                       //}, function (list) {
                       //    angular.forEach($scope.invlist, function (row) {
                       //        if (row.customerCode == $routeParams.nums) {
                       //            row['pcg'].data = list;
                       //        }
                       //    });
                       //});
                       customerPaymentcircleProxy.searchPaymentCircle(cusCode, $scope.entityFlg, function (paydate) {
                           angular.forEach($scope.invlist, function (row) {
                               if (row.customerCode == cusCode) {
                                   row['pcg'].data = paydate;
                                   row['pcgshow'] = true;
                               }
                           });
                       });
                   };
                   uploader.onErrorItem = function (fileItem, response, status, headers) {
                       alert(response);
                   };
               }
           }

           $scope.changeLegal = function (legal) {
               $scope.showlegal = legal;
               customerPaymentcircleProxy.searchPaymentCircle($routeParams.nums, legal
                   , function (paydate) {
                       collectorSoaProxy.query({
                           CustNumFPc: $routeParams.nums
                       }, function (list) {
                           angular.forEach($scope.invlist, function (row) {
                               if (row.customerCode == $routeParams.nums) {
                                   row['pcg'].data = paydate;
                               }
                           });
                       });
                       $scope.entityFlg = legal;
                       uploader.queue[2] = null;
                       document.getElementById("uploadCalendar").value = "";
                       //     $scope.paydaylist = paydate;
                   },
                   function () {
                   });
           }

           $scope.checkNumber = function (val) {
               if (!/^([1-9]\d|\d)$/.test(val)) {
                   return 1;
               } else {
                   return 0;
               }
           }
           //save config 
           $scope.getStyle = function (reminderStatus) {
               if (reminderStatus == 1) {
                   return { 'background': '#ddffe1' }
               }
               if (reminderStatus == 0) {
                   return { 'color': '#FF0000' }
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
               for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++)
                   num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                   num.substring(num.length - (4 * i + 3));
               return (((sign) ? '' : '-') + num + '.' + cents);
           }
           //**************************************************break PTP*************************************************//


           //**************************************************break PTP*************************************************//
           //*****************************************Bottom****************************************************s
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