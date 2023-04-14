angular.module('app.common.mail', ['ngSanitize'])
    .controller('mailInstanceDirectiveCtrl',
    ['$scope', 'baseDataProxy', 'modalService', 'mailTemplateProxy', '$interval', 'APPSETTING',
        'mailProxy', 'FileUploaderCommon', '$q', 'appFilesProxy', '$location', 'mailInstanceService',
        function ($scope, baseDataProxy, modalService, mailTemplateProxy, $interval, APPSETTING,
            mailProxy, FileUploaderCommon, $q, appFilesProxy, $location, mailInstanceService) {

            var orginalMailId = $scope.mailDefaults.orginalMailId;
            var templateChoosenCallBack = $scope.mailDefaults.templateChoosenCallBack;
            var checkCallBack = $scope.mailDefaults.checkCallBack;
            var mailType = $scope.mailDefaults.mailType;
            var mailUrl = $scope.mailDefaults.mailUrl;
            var showCancelBtn = $scope.mailDefaults.showCancelBtn;
            var modalInstance = $scope.modalInstance;
            //$scope.custNums = ""; 已赋值
            $scope.attachments = [];
            if (typeof templateChoosenCallBack == "undefined") {
                templateChoosenCallBack = defaultTemplateChoosenCallBack;
            }
            if (typeof orginalMailId == "undefined") {
                orginalMailId = 0;
            }

            if (templateChoosenCallBack == null) {
                templateChoosenCallBack = defaultTemplateChoosenCallBack;
            }
            if (typeof showCancelBtn == "undefined") {
                showCancelBtn = false;
            }

            if (typeof modalInstance == "undefined") {
                modalInstance = {
                    close: function () { },
                    dismiss: function () { }
                }
            }
            $scope.showUpload = true;

            var hidType = "1";

            $scope.loadInstance = function () {
                hidType = "1";
                if ($scope.mailInstance) {
                    if ($scope.mailInstance.title == "Mail Forward NoCusNum") {
                        hidType = "2";
                        $scope.mailInstance.title = "Mail Forward";
                    }
                    if ($scope.mailInstance.title == "Mail Reply NoCusNum") {
                        hidType = "3";
                        $scope.mailInstance.title = "Mail Reply";
                    }

                    if ($scope.mailInstance.attachment != null && $scope.mailInstance.attachment != "") {
                        appFilesProxy.query({ fileIds: $scope.mailInstance.attachment }).then(function (res) {
                            $scope.attachments = res;
                        });
                    }
                }

                $interval(function () {
                    $scope.setAttr();
                }, 0, 1);
            }

            $scope.$watch('mailInstance', function () {
                if ($scope.mailInstance) {
                    $scope.loadInstance();
                }
            })

            $scope.$watch('mailInstance.attachment', function () {
                if ($scope.mailInstance) {

                    if ($scope.mailInstance.attachment != null && $scope.mailInstance.attachment != "") {
                        appFilesProxy.query({ fileIds: $scope.mailInstance.attachment }).then(function (res) {
                            $scope.attachments = res;
                        });
                    }
                }
            })

            //*************upload **********************start
            var uploader = $scope.uploader = new FileUploaderCommon({
                url: APPSETTING['serverUrl'] + '/api/appFiles',
                //autoUpload :true
            });

            // FILTERS
            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {

                    return this.queue.length < 10;
                }

            });//filters

            //select Completed
            uploader.onAfterAddingAll = function (addedFileItems) {
                //upload file
                if (uploader.queue.length > 0) {
                    var uploadResult = uploader.uploadAll();
                }
            };
            //upload Completed
            uploader.onCompleteItem = function (fileItem, response, status, headers) {
                //add the item from uploader queue
                $scope.attachments.push(response[0]);

                //delete item from uploader queue
                fileItem.remove();

                // update attachment property in current instance.
                var strAtt = "";
                for (var i = 0; i < $scope.attachments.length; i++) {
                    var fId = $scope.attachments[i].fileId;
                    strAtt += fId + ","
                }
                if (strAtt.length > 0) {
                    $scope.mailInstance.attachment = strAtt.substring(0, strAtt.length - 1);
                }

            };
            //*************upload **********************end
            $scope.collectorEID = '';


            $scope.setAttr = function () {
                $scope.deleteshow = true;
                $scope.showCancel = false;

                if ($scope.mailInstance.title == "Send Break Letter"
                    || $scope.mailInstance.title == "Send Mail"
                    || $scope.mailInstance.title == "Dispute Mail") {
                    $("#txtTo").removeAttr("readonly");
                }

                if ($scope.mailInstance.title == "Mail Create") {
                    $("#txtTo").removeAttr("readonly");
                    $("#imgContactor").hide();
                }

                if ($scope.mailInstance.title == "Mail Forward") {
                    $("#txtTo").removeAttr("readonly");
                    $("#txtCc").val("")
                    $("#txtCc").val("")
                }

                if ($scope.mailInstance.title == "Mail View") {
                    $scope.showUpload = false;
                    $("#btnSend").hide();
                    $("#imgContactor").hide();
                    $("#btnChooseTemplate").hide();
                    // $("#deletehide").hide();
                    $scope.deleteshow = false;
                    //draft view 
                    if ($scope.mailInstance.category == "Draft") {
                        $scope.mailInstance.title = "Mail Create";
                        $("#btnSend").show();
                        $("#txtTo").removeAttr("readonly");
                        $scope.showUpload = true;
                        $scope.deleteshow = true;
                        $("#btnChooseTemplate").show();
                    }
                } else {
                    $("#btnSend").show();
                    $("#imgContactor").show();
                    $("#btnChooseTemplate").show();
                    $scope.showUpload = true;
                }

                if (hidType == "2") {
                    $("#imgContactor").hide();
                    $("#txtTo").removeAttr("readonly");
                    $("#txtCc").val("")

                }
                if (hidType == "3") {
                    $("#imgContactor").hide();
                }
                //            $("#txtTo").removeAttr("disabled");
                //            $("#txtTo").focus();
            }

            $scope.send = function () {
                //chek
                if (!$scope.checkMail()) {
                    return;
                }

                if (uploader.isUploading) {

                    alert("File is uploadding,please wait a moment");
                    return;
                }

                var mailDto = {};
                mailDto.mailInstance = $scope.mailInstance;
                mailDto.invoiceNums = $scope.$parent.invoiceNums;

                //send mail
                mailProxy.sendMail(mailUrl, mailDto, function (instance) {
                    $scope.mailInstance = instance;
                    modalInstance.close("sent", instance);
                }, function (err) {
                    if (err != null) {
                        alert(err);
                    }
                });

            };

            $scope.showCancelBtn = function () {
                if (!$scope.mailInstance) {
                    return false;
                }
                return ($scope.mailInstance.title == "Mail Forward"
                    || $scope.mailInstance.title == "Mail Reply") && showCancelBtn;
            }

            $scope.showCancelImg = function () {
                if (!$scope.mailInstance) {
                    return false;
                }
                return !showCancelBtn;
            }

            $scope.cancel = function () {

                if ($scope.mailInstance.id == "0") {
                    // draf identified.
                    if (confirm("Want to save your change?")) {
                        mailProxy.saveMail($scope.mailInstance, function (instance) {
                            $scope.mailInstance = instance;
                            modalInstance.close("saved");
                        }, function (err) {
                            if (err != null) {
                                alert(err);
                            }
                        });
                    }
                    else {
                        modalInstance.close("cancel");
                    }
                } else if ($scope.mailInstance.title != 'Mail View') {
                    if (confirm("All un-saved changes will be discard, continue?")) {
                        modalInstance.close("cancel");
                    }
                } else {
                    modalInstance.close("cancel");
                }
            }

            $scope.enableSave = function () {
                if (!$scope.mailInstance) {
                    return false;
                }
                return $scope.mailInstance.title != 'Mail View';
            }

            $scope.save = function () {
                if ($scope.mailInstance.category == "" || $scope.mailInstance.category == null) {
                    $scope.mailInstance.category = 'Draft';
                }
                mailProxy.saveMail($scope.mailInstance, function (instance) {
                    // successfull saved current instance.
                    $scope.mailInstance = instance;

                    // saveMail have been saved customer relation , no need to save agagin
                    //// save customer relation
                    //if ($scope.$parent.customers != null){
                    //    angular.forEach($scope.$parent.customers, function (cust) {
                    //        $scope.assignMailCustomer($scope.mailInstance.messageId, cust.customerNum, cust.siteUseId);
                    //    });
                    //}
                    //$scope.assignMailCustomer();

                    alert('Saved successfull!');
                }, function (err) {
                    if (err != null) {
                        alert(err);
                    }
                });
            }

            $scope.assignMailCustomer = function (mailMsgId,mailCustNum,mailSiteUseId) {
                mailProxy.assignCustomer(mailMsgId, mailCustNum, mailSiteUseId, function (res) {
                    //alert('success!');
                    //$modalInstance.close();
                }, function (error) {
                    alert(error);
                })
            }

            //Before Send mail check
            $scope.checkMail = function () {
                //To IS NULL?
                if (!$scope.mailInstance.to) {
                    alert("There is no mail receiver,check again please!")
                    return false;
                }

                if (!$scope.mailInstance.subject) {
                    alert("There is no mail subject,check again please!")
                    return false;
                }

                if (!mailInstanceService.checkAddresses($scope.mailInstance.to)
                    || !mailInstanceService.checkAddresses($scope.mailInstance.cc)) {
                    alert("Please enter a valid email address in [Cc]\rExample:[Tom@163.com;Jack@163.com]");

                    return false;
                }

                if (typeof checkCallBack != 'undefined') {
                    if (!checkCallBack($scope.mailInstance)) {
                        return false;
                    }
                }

                return true;
            }

            $scope.pickupToAddress = function () {

                var contacterEmArrTo = new Array();
                var contacterEmArrCC = new Array();
                var contacterEmArr = new Array();
                if (!($scope.mailInstance.to == null || $scope.mailInstance.to == "")) {
                    contacterEmArrTo = $scope.mailInstance.to.split(";");
                }
                //2016-01-14 start
                if (!($scope.mailInstance.cc == null || $scope.mailInstance.cc == "")) {
                    contacterEmArrCC = $scope.mailInstance.cc.split(";");
                }
                contacterEmArr = contacterEmArrTo.concat(contacterEmArrCC);
                //2016-01-14 End


                // show contact list to pick up.
                var modalDefaults = {
                    templateUrl: 'app/common/contactor/contactor-pickup.tpl.html',
                    controller: 'contactorPickupCtrl',
                    size: 'lg',
                    resolve: {
                        custNum: function () { return $scope.custNums; }, 
                        siteUseId: function () { return $scope.$parent.siteUseId; }, 
                        contacterEmArr: function () { return contacterEmArr; },
                    },
                    windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults).then(function (contacts) {
                    //2016-01-14 start
                    var toaddress = '';
                    var ccaddress = '';
                    //2016-01-14 End
                    var collectorNames = '';
                    angular.forEach(contacts, function (cont) {
                        //2016-01-14 start
                        if (cont.toCc == "1") {
                            if (!toaddress) {
                                toaddress += cont.emailAddress;
                            } else {
                                toaddress += ';' + cont.emailAddress;
                            }

                            if (collectorNames.indexOf((cont.name + ',')) < 0) {
                                collectorNames += (cont.name + ', ');
                            }
                        }
                        else if (cont.toCc == "2") {
                            if (!ccaddress) {
                                ccaddress += cont.emailAddress;
                            } else {
                                ccaddress += ';' + cont.emailAddress;
                            }
                        }

                    });
                    //2016-01-14 start
                    $scope.mailInstance.to = toaddress;
                    $scope.mailInstance.cc = ccaddress;
                    //2016-01-14 end

                    var removeIndex = $scope.mailInstance.body.indexOf('</p>') + 4;
                    $scope.mailInstance.body = $scope.mailInstance.body.substring(removeIndex);
                    $scope.mailInstance.body = "<p>Dear " + collectorNames + '</p>' + $scope.mailInstance.body;
                });
            };

            $scope.chooseTemp = function () {

                var modalDefaults = {
                    templateUrl: 'app/common/mailtemplate/mailtemplate-pickup.tpl.html',
                    size: 'lg',
                    controller: 'mailtemplatePickupCtrl',
                    resolve: {
                        templateTypes: ['baseDataProxy', function (baseDataProxy) {
                            return baseDataProxy.SysTypeDetail('012');
                        }],
                        custnum: function () {
                            return $scope.$parent.$resolve.custnum;
                        },
                        siteUseId: function () { return $scope.$parent.siteUseId; },
                    },
                    windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults).then(function (id) {
                    if (id != 'cancel') {
                        if (templateChoosenCallBack == null) return;
                        if (templateChoosenCallBack) {
                            var o_mail = $scope.mailInstance;
                            var invs = [];
                            var siteUseId = $scope.$parent.siteUseId;
                            if ($scope.$parent.invoiceNums != null) {
                                invs = $scope.$parent.invoiceNums.split(',');
                            }
                            // selectMailInstanceById ( custNums, id, siteUseId, templateType, templatelang, ids
                            templateChoosenCallBack($scope.$parent.$resolve.custnum, 0, siteUseId, $scope.$parent.mtype, id, invs).then(function (inst) {
                                $scope.mailInstance = o_mail;
                                $scope.mailInstance.body = inst.body;
                                $scope.attachments = [];
                                if ($scope.mailInstance.attachment != null && $scope.mailInstance.attachment != "") {
                                    appFilesProxy.query({ fileIds: $scope.mailInstance.attachment }).then(function (res) {
                                        $scope.attachments = res;
                                    });
                                }
                            });
                        }
                    }
                });
            };

            function defaultTemplateChoosenCallBack(custNums, id, siteUseId, templateType, templatelang, ids) {
                return mailProxy.getMailInstanceByTemplateId(templatelang, $scope.$parent.mtype);
            };

            $scope.fileClicked = function (fileId) {
                window.location = APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + fileId;
            };

            $scope.removeFile = function (file) {
                //if ($scope.mailInstance.title == "Mail Forward"
                //    || $scope.mailInstance.title == "Mail Reply") {
                //    // for these types, we don't remove the backend attachment really. The remove is applied only to current instance.
                //   // removeFromFileList(file.id);
                //    appFilesProxy.deleteFile(file.id).then(removeFromFileList(file.id));
                //} else {
                    appFilesProxy.deleteFile(file.id).then(removeFromFileList(file.id));
                //}
            }

            var removeFromFileList = function (id) {
                for (var i = $scope.attachments.length - 1; i >= 0; i--) {
                    if ($scope.attachments[i].id == id) {
                        $scope.attachments.splice(i, 1);
                    }
                }
            }
        }])

    .controller('mailInstanceCtrl',
    ['$scope', 'instance', 'mailDefaults', 'custnum', '$uibModalInstance', 'siteuseId', 'invoicenums', 'mType',
        function ($scope, instance, mailDefaults, custnum, $uibModalInstance, siteuseId, invoicenums, mType) {
            $scope.custNums = custnum;
            $scope.siteUseId = siteuseId;
            $scope.siteUseId2 = siteuseId;
            $scope.invoiceNums = invoicenums;
            $scope.mailInstance = instance;
            $scope.mailDefaults = mailDefaults;
            $scope.modalInstance = $uibModalInstance;
            $scope.mtype = mType;
        }])

    .controller('mailListCtrl', ['$scope', '$rootScope' , 'baseDataProxy', 'mailProxy', 'modalService', '$sce', '$interval', 'mailInstanceService', '$q', 'cacheService',
        function ($scope, $rootScope, baseDataProxy, mailProxy, modalService, $sce, $interval, mailInstanceService, $q, cacheService) {

            
            // used to return selected mail information to host page. 
            $scope.mailGrid = $scope.params[0];

            $scope.params[1] = "";

            $scope.customerNum = $scope.params[2]; // old => $scope.params[8]
            $scope.fixedCustomer = $scope.params[3]; // old => $scope.params[9]
            if (!$scope.customerNum) {
                $scope.customerNum = '';
            }
            if (!$scope.customerName) {
                $scope.customerName = '';
            }

            $scope.currUser = cacheService.get('CURR_USER');

            $scope.gridHeight = $scope.params[4]; // old => $scope.params[10]
            if (!$scope.gridHeight) {
                $scope.gridHeight = "200px";
            }
            $scope.mailInstanceService = $scope.params[5]; // old => $scope.params[11]
            if (!$scope.mailInstanceService) {
                $scope.mailInstanceService = mailInstanceService;
            }
            $scope.searchSpecificUser = $scope.params[6];
            if (!$scope.searchSpecificUser) {
                // by default, search is target to all user.
                $scope.searchSpecificUser = false;
            }

            $scope.mailType = "1";
            $scope.selectedLevel = 15;  //下拉单页容量初始化
            $scope.itemsperpage = 15;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页     
            //      var filstr = "&$filter=(CreateTime ge " + now.format("yyyy-MM-dd") + ")";
            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 15, "levelName": '15' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];
            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                $scope.itemsperpage = selectedLevelId;
                $scope.queryMailBox();
            };

            //翻页
            $scope.pageChanged = function () {
                $scope.queryMailBox();
            };

            //计算剩下页数
            $scope.calculate = function (currentPage, itemsperpage, count) {
                if (count == 0) {
                    $scope.fromItem = 0;
                } else {
                    $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
                }
                $scope.toItem = (currentPage - 1) * itemsperpage + count;
            }

            //Do search
            $scope.buildSearchCondition = function (category) {
                var queryStr = '';
                var filterStr = "&$filter=(Category eq '" + category + "')";
                if ($scope.subject) {
                    filterStr += "and (contains(Subject,'" + encodeURIComponent($scope.subject) + "'))";
                }

                if ($scope.Content) {
                    filterStr += "and (contains(Body,'" + $scope.Content + "'))";
                }

                if ($scope.mailFrom) {
                    filterStr += "and (contains(From,'" + $scope.mailFrom + "'))";
                }

                if ($scope.mailTo) {
                    filterStr += "and (contains(To,'" + $scope.mailTo + "'))";
                }

                if ($scope.from) {
                    filterStr += "and (date(MailTime) ge " + $scope.from + ")";
                }

                if ($scope.to) {
                    filterStr += "and (date(MailTime) le " + $scope.to + ")";
                }

                if ($scope.siteUseId){
                    filterStr += "and (contains(SiteUseId,'" + $scope.siteUseId + "'))";
                }

                if ($scope.customerNum){
                    filterStr += "and (contains(CustomerNum,'" + $scope.customerNum + "'))";
                }

                if ($scope.customerName){
                    filterStr += "and (contains(CustomerName,'" + $scope.customerName + "'))";
                }

                //if ($scope.currUser && $scope.searchSpecificUser) {
                //    filterStr += "and (MailBox eq '" + $scope.currUser.email + "')";
                //}

                //queryStr = "&customerNum=" + $scope.customerNum + "&customerName=" + $scope.customerName;
                queryStr = '';

                return filterStr + queryStr;
            };

            $scope.switchBox = function () {
                document.getElementById("in").style.backgroundColor = "transparent";
                document.getElementById("dra").style.backgroundColor = "transparent";
                document.getElementById("seItems").style.backgroundColor = "transparent";
                document.getElementById("process").style.backgroundColor = "transparent";
                document.getElementById("pen").style.backgroundColor = "transparent";
                document.getElementById("unk").style.backgroundColor = "transparent";

                switch ($scope.mailType) {
                    case "1":
                        $scope.ProcessShow = true;
                        $scope.PendingShow = true;
                        $scope.RestoreShow = false;
                        $scope.ForwardShow = true;
                        $scope.ReplyShow = true;
                        $scope.EditShow = false;
                        $scope.DeleteShow = false;
                        $scope.ViewShow = true;
                        document.getElementById("in").style.backgroundColor = "#dbdbdb";
                        break;
                    case "2":
                        $scope.ProcessShow = true;
                        $scope.PendingShow = false;
                        $scope.RestoreShow = true;
                        $scope.ForwardShow = true;
                        $scope.ReplyShow = true;
                        $scope.EditShow = false;
                        $scope.DeleteShow = false;
                        $scope.ViewShow = true;
                        document.getElementById("pen").style.backgroundColor = "#dbdbdb";
                        break;
                    case "3":
                        $scope.ProcessShow = false;
                        $scope.PendingShow = false;
                        $scope.RestoreShow = false;
                        $scope.ForwardShow = false;
                        $scope.ReplyShow = false;
                        $scope.EditShow = true;
                        $scope.DeleteShow = true;
                        $scope.ViewShow = false;
                        document.getElementById("dra").style.backgroundColor = "#dbdbdb";
                        break;
                    case "4":
                        $scope.ProcessShow = false;
                        $scope.PendingShow = false;
                        $scope.RestoreShow = false;
                        $scope.ForwardShow = true;
                        $scope.ReplyShow = false;
                        $scope.EditShow = false;
                        $scope.DeleteShow = false;
                        $scope.ViewShow = true;
                        document.getElementById("seItems").style.backgroundColor = "#dbdbdb";
                        break;
                    case "5":
                        $scope.ProcessShow = false;
                        $scope.PendingShow = false;
                        $scope.RestoreShow = true;
                        $scope.ForwardShow = true;
                        $scope.ReplyShow = true;
                        $scope.EditShow = false;
                        $scope.DeleteShow = true;
                        $scope.ViewShow = true;
                        document.getElementById("process").style.backgroundColor = "#dbdbdb";
                        break;
                    case "6":
                        $scope.ProcessShow = true;
                        $scope.PendingShow = true;
                        $scope.RestoreShow = false;
                        $scope.ForwardShow = true;
                        $scope.ReplyShow = true;
                        $scope.EditShow = false;
                        $scope.DeleteShow = false;
                        $scope.ViewShow = true;
                        document.getElementById("unk").style.backgroundColor = "#dbdbdb";
                        break;
                    default:
                }
            }

            // init button avilibilities.
            $scope.switchBox();

            //*****************************************<a> ng-click********************s
            $scope.getDatasInfo = function (item) {
                $scope.resetSearch();

                if (item == "inbox") {
                    $scope.mailType = "1";
                } else if (item == "pending") {
                    $scope.mailType = "2";
                } else if (item == "dra") {
                    $scope.mailType = "3";
                } else if (item == "seItems") {
                    $scope.mailType = "4";
                } else if (item == "process") {
                    $scope.mailType = "5";
                } else if (item = "unk") {
                    $scope.mailType = "6";
                }

                $scope.switchBox();

                $scope.queryMailBox();
            }

            $scope.queryMailBox = function () {
                var queryInput = {};
                queryInput.page = $scope.currentPage;
                queryInput.pageSize = $scope.itemsperpage;
                queryInput.subject = $scope.subject;
                queryInput.Content = $scope.Content;
                queryInput.from = $scope.from;
                queryInput.to = $scope.to;
                queryInput.start = $scope.mailFrom;
                queryInput.end = $scope.mailTo;
                queryInput.siteUseId = $scope.siteUseId;
                queryInput.customerNum = $scope.customerNum;
                queryInput.customerName = $scope.customerName;

                switch ($scope.mailType) {
                    case "1":
                        queryInput.category  = "customerNew";
                        break;
                    case "2":
                        queryInput.category = "pending";
                        break;
                    case "3":
                        queryInput.category = "draft";
                        break;
                    case "4":
                        queryInput.category = "sent";
                        break;
                    case "5":
                        queryInput.category = "processed";
                        break;
                    case "6":
                        queryInput.category = "unknow";
                        break;
                }

                mailProxy.queryMails(queryInput, function (list) {
                    if (list != null) {
                        $scope.conlist = list.mailList;
                        $scope.totalItems = list.listCount;
                        $scope.calculate($scope.currentPage, $scope.itemsperpage, $scope.totalItems);
                        $interval(function () {
                            $scope.unselectall();
                        }, 0, 1);
                    }
                })
            }

            $scope.searchMailBox = function () {
                var category = '';
                var writeBackElement;
                switch ($scope.mailType) {
                    case "1":
                        category = "CustomerNew";
                        writeBackElement = document.getElementById("inbox");
                        break;
                    case "2":
                        category = "Pending";
                        writeBackElement = document.getElementById("pending");
                        break;
                    case "3":
                        category = "Draft";
                        writeBackElement = document.getElementById("draft");
                        break;
                    case "4":
                        category = "Sent";
                        break;
                    case "5":
                        category = "Processed";
                        break;
                    case "6":
                        category = "Unknow";
                        writeBackElement = document.getElementById("unknow");
                        break;
                }

                var queryStr = $scope.buildSearchCondition(category);
                var orderby = "&$orderby=MailTime desc ";

                mailProxy.searchMailPagingSp($scope.currentPage, $scope.itemsperpage, queryStr, orderby, function (list) {
                    if (list != null) {
                        $scope.totalItems = list[0].count;
                        $scope.conlist = list[0].results;
                        $scope.calculate($scope.currentPage, $scope.itemsperpage, list[0].results.length);
                        if (writeBackElement) {
                            writeBackElement.innerHTML = list[0].count;
                        }

                        $scope.getMailCounts();
                        $interval(function () {
                            $scope.unselectall();
                        }, 0, 1);
                    }
                });
            }

            //*****************************************Get Inbox Datas********************s
            $scope.$on('MAIL_DATAS_REFRESH', function (e) {
                $scope.reFreshMailCount();
            });

            $scope.reFreshMailCount = function () {
                mailProxy.queryMailCount(function (cnt) {
                    $scope.mailCategoryCount = cnt;

                    var inbox = document.getElementById("inbox");
                    if (inbox != null && inbox != undefined) {
                        inbox.innerHTML = cnt.customerNew;
                    }

                    var unknow = document.getElementById("unknow");
                    if (unknow != null && unknow != undefined) {
                        unknow.innerHTML = cnt.unknow;
                    }

                    var unknow = document.getElementById("draft");
                    if (unknow != null && unknow != undefined) {
                        unknow.innerHTML = cnt.draft;
                    }

                    var unknow = document.getElementById("sentItems");
                    if (unknow != null && unknow != undefined) {
                        unknow.innerHTML = cnt.sent;
                    }

                    var processed = document.getElementById("processed");
                    if (processed != null && processed != undefined) {
                        processed.innerHTML = cnt.processed;
                    }

                    var pending = document.getElementById("pending");
                    if (pending != null && pending != undefined) {
                        pending.innerHTML = cnt.pending;
                    }

                    $scope.queryMailBox();

                }, function (err) {
                    alert(err);
                });
            }

            //get mail count
            $scope.getMailCounts = function () {

                mailProxy.getMailCountDistinct("CustomerNew", function (cnt) {
                    var inbox = document.getElementById("inbox");
                    if (inbox != null && inbox != undefined)
                    {
                        inbox.innerHTML = cnt;
                    }
                }, function (err) {
                    alert(err);
                    });

                mailProxy.getMailCountDistinct("Unknow", function (cnt) {
                    var unknow = document.getElementById("unknow");
                    if (unknow != null && unknow != undefined) {
                        unknow.innerHTML = cnt;
                    }
                }, function (err) {
                    alert(err);
                    });

                mailProxy.getMailCountDistinct("Draft", function (cnt) {
                    var draft = document.getElementById("draft");
                    if (draft != null && draft != undefined) {
                        draft.innerHTML = cnt;
                    }
                }, function (err) {
                    alert(err);
                    });

                mailProxy.getMailCountDistinct("Sent", function (cnt) {
                    var sentItems = document.getElementById("sentItems");
                    if (sentItems != null && sentItems != undefined) {
                        sentItems.innerHTML = cnt;
                    }
                }, function (err) {
                    alert(err);
                    });

                mailProxy.getMailCountDistinct("Processed", function (cnt) {
                    var processed = document.getElementById("processed");
                    if (processed != null && processed != undefined) {
                        processed.innerHTML = cnt;
                    }
                }, function (err) {
                    alert(err);
                    });

                mailProxy.getMailCountDistinct("Pending", function (cnt) {
                    var pending = document.getElementById("pending");
                    if (pending != null && pending != undefined) {
                        pending.innerHTML = cnt;
                    }
                }, function (err) {
                    alert(err);
                });

                //var draftQuery = "&$filter=(Category eq 'Draft')";
                //var unkQuery = "&$filter=(Category eq 'unknow')";

                //var mailBoxQuery = '';
                //if ($scope.currUser && $scope.searchSpecificUser) {
                //    mailBoxQuery = "and (MailBox eq '" + $scope.currUser.email + "')";
                //}

                //var queryStr = "&customerNum=" + $scope.customerNum + "&customerName=" + $scope.customerName;

                //mailProxy.getMailCount(draftQuery + mailBoxQuery + queryStr, function (list) {
                //    document.getElementById("draft").innerHTML = list[0].count;
                //}, function (err) {
                //    alert(err);
                //});

                //mailProxy.getMailCount(unkQuery + mailBoxQuery + queryStr, function (list) {
                //    document.getElementById("unknow").innerHTML = list[0].count;
                //}, function (err) {
                //    alert(err);
                //});
                //var pendingQuery = "&$filter=(Category eq 'Pending')";

                //mailProxy.getMailCount(pendingQuery + mailBoxQuery + queryStr, function (list) {
                //    document.getElementById("pending").innerHTML = list[0].count;
                //}, function (err) {
                //    alert(err);
                //});
            }

            $scope.init = function () {
                $scope.mailType = "1";

                // Initial call of function
                RefreshMail();

            }


            $scope.init();

            var __mailRefreshTimer;

            function RefreshMail() {
                // 1. Make request to server
                $scope.reFreshMailCount();
                clearInterval(__mailRefreshTimer);
                // 2. Schedule new request after 5 minutes (60000 miliseconds * 5)
                __mailRefreshTimer = setInterval(RefreshMail, 300000);
            }

            $rootScope.$on('$routeChangeStart', function () { clearInterval(__mailRefreshTimer); }); 

            //reset Search conditions
            $scope.resetSearch = function () {
                $scope.subject = "";
                $scope.Content = "";
                $scope.from = "";
                $scope.to = "";
                $scope.mailFrom = "";
                $scope.mailTo = "";
                $scope.siteUseId = "";
                if (!$scope.fixedCustomer) {
                    $scope.customerNum = "";
                }
                $scope.customerName = "";

            }

            //openfilter
            var isShow = 0; //0:hide;1:show
            $scope.openFilter = function () {
                $scope.resetSearch();
                if (isShow == 0) {
                    $("#divAgingSearch").show();
                    isShow = 1;
                } else if (isShow == 1) {
                    $("#divAgingSearch").hide();
                    isShow = 0;
                }
            }

            $scope.Process = function (mail) {
                mailProxy.updateMailCategory([mail.id], 'Processed', function (res) {
                    alert('success!');
                    $scope.queryMailBox();
                }, function (error) {
                    alert(error);
                })
            }

            //$scope.unknown = function (mail) {
            //    mailProxy.updateMailCategory([mail.id], 'unknown', function (res) {
            //        alert('success!');
            //        $scope.queryMailBox();
            //    }, function (error) {
            //        alert(error);
            //    })
            //}

            $scope.Pending = function (mail) {
                mailProxy.updateMailCategory([mail.id], 'Pending', function (res) {
                    $scope.queryMailBox();
                }, function (error) {
                    alert(error);
                })
            }

            $scope.PendingMultiple = function (mail) {
                var ids = [];
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        ids.push(rowItem.id);
                    }
                });

                if (ids == "" || ids == null) {
                    alert("Please select mail ! ");
                    return;
                }

                if (confirm("Pending all the selected mail ?")) {
                } else {
                    return;
                }

                mailProxy.updateMailCategory(ids, 'Pending', function (res) {
                    $scope.queryMailBox();
                }, function (error) {
                    alert(error);
                })
            }

            // Set multiple mail to processed
            $scope.ProcessMultiple = function () {

                var ids = [];
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        ids.push(rowItem.id);
                    }
                });

                if (ids == "" || ids == null) {
                    alert("Please select mail ! ");
                    return;
                }

                if (confirm("Process all the selected mail ?")) {
                } else {
                    return;
                }

                mailProxy.updateMailCategory(ids, 'Processed', function (res) {
                    $scope.queryMailBox();
                }, function (error) {
                    alert(error);
                })
            } //process

            $scope.RestoreMultiple = function () {

                var ids = [];
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        ids.push(rowItem.id);
                    }
                });

                if (ids == "" || ids == null) {
                    alert("Please select mail ! ");
                    return;
                }

                if (confirm("Resume all the selected mail ?")) {
                } else {
                    return;
                }

                mailProxy.updateMailCategory(ids, 'Unknow', function (res) {
                    $scope.queryMailBox();
                }, function (error) {
                    alert(error);
                })
            } //process

            $scope.Reply = function (mail) {
                $scope.mailInstanceService.replyMail(mail).then(function (result) {
                    $scope.queryMailBox();
                });
            }

            $scope.Forward = function (mail) {
                $scope.mailInstanceService.forwardMail(mail).then(function (result) {
                    $scope.queryMailBox();
                });
            }

            $scope.viewMail = function (mail) {
                var t = $scope.mailInstanceService.viewMail(mail);
                if (typeof t.then == "function") {
                    t.then(function (result) {
                        $scope.queryMailBox();
                    });
                }

            }

            $scope.newMail = function () {
                var t = $scope.mailInstanceService.newMail();
                if (typeof t.then == "function") {
                    t.then(function (result) {
                        $scope.queryMailBox();
                    });
                }
            }

            // Delete mail
            $scope.Delete = function (mailId) {
                mailProxy.deleteSelectedMail([mailId], function (res) {
                    $scope.queryMailBox();
                }, function (error) {
                    alert("Delete Error");
                });
            }

            // Delete multiple mails
            $scope.DeleteMultiple = function () {

                var ids = [];
                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (rowItem.id != 0) {
                        ids.push(rowItem.id);
                    }
                });

                if (ids == "" || ids == null) {
                    alert("Please select mail ! ");
                    return;
                }

                if (confirm("Delete all the selected mail ?")) {
                }
                else {
                    return;
                }

                mailProxy.deleteSelectedMail(ids, function (res) {
                    $scope.queryMailBox();
                }, function (error) {
                    alert("Delete Error");
                });
            }//Delete Selected Mail End

            rowselected = function (row) {
                if ($scope.gridApi.selection.getSelectedRows().length > 1) {
                    $scope.params[1] = "Only select one mail!";
                } else if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                    $scope.params[1] = row.entity;
                    //$scope.params[1] = row.entity.messageId;
                    //$scope.params[2] = row.entity.from;
                    //$scope.params[3] = row.entity.subject;
                    //$scope.params[4] = row.entity.createTime;
                    //$scope.params[5] = row.entity.id;
                    //$scope.params[6] = row.entity.to
                    //$scope.params[7] = row.entity.fileId;
                } else {
                    $scope.params[1] = "";
                    //$scope.params[2] = "";
                    //$scope.params[3] = "";
                    //$scope.params[4] = "";
                    //$scope.params[5] = "";
                    //$scope.params[6] = "";
                    //$scope.params[7] = "";
                }
                //               if (!row) {

                //               } else {

                //               }
            };

            $scope.defaultMailGrid = {
                data: 'conlist',
                //multiSelect: false,
                columnDefs: [
                    { field: 'from', displayName: 'From', width: '335' },
                    {
                        field: 'createTime', displayName: 'Date', width: '160',
                        cellTemplate: '<div class="ngCellText">{{row.entity.createTime | date:"yyyy-MM-dd HH:mm:ss"}}</div>'
                    },
                    {
                        field: 'subject', displayName: 'Title', width: '542',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">' +
                        '<a ng-click="grid.appScope.viewMail(row.entity)">{{row.entity.subject}}</a>' +
                        '</div>'
                    }
                    //,{
                    //    field: '""', displayName: 'Operation', width: '205',
                    //    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()" style="height:30px;vertical-align:middle">' +
                    //    '<a title="View" class="fa fa-list-alt" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.ViewShow" ng-click="grid.appScope.viewMail(row.entity)"></a>' +
                    //    '<a title="Process" class="fa fa-check" ng-show="grid.appScope.ProcessShow" style="line-height:28px; padding-left:15px;" ng-click="grid.appScope.Process(row.entity)"></a>' +
                    //    '<a title="Pending" class="fa fa-hourglass" ng-show="grid.appScope.PendingShow" style="line-height:28px; padding-left:15px;" ng-click="grid.appScope.Pending(row.entity)"></a>' +
                    //    '<a title="Forward" class="fa fa-share" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.ForwardShow" ng-click="grid.appScope.Forward(row.entity)"></a>' +
                    //    '<a title="Reply" class="fa fa-mail-reply" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.ReplyShow" ng-click="grid.appScope.Reply(row.entity)"></a>' +
                    //    '<a title="Edit" class="fa fa-pencil-square-o" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.EditShow" ng-click="grid.appScope.viewMail(row.entity)"></a>' +
                    //    '<a title="Delete" class="fa fa-times" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.DeleteShow" ng-click="grid.appScope.Delete(row.entity.id)"></a>' +
                    //    '</div>'
                    //}
                ]
            };

            $scope.registGridApi = function (gridApi) {
                //set gridApi on scope
                $scope.gridApi = gridApi;
                gridApi.selection.on.rowSelectionChanged($scope, rowselected);
                gridApi.selection.on.rowSelectionChangedBatch($scope, rowselected);
            }

            $scope.getMailGrid = function () {
                if ($scope.mailGrid) {
                    $scope.mailGrid.onRegisterApi = $scope.registGridApi;
                    return $scope.mailGrid;
                } else {
                    $scope.defaultMailGrid.onRegisterApi = $scope.registGridApi;
                    return $scope.defaultMailGrid;
                }
            }

            $scope.unselectall = function () {
                $scope.gridApi.selection.clearSelectedRows();
            }

        }])
    
    ;
