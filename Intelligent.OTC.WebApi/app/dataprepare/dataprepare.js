angular.module('app.dataprepare', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
        .when('/initaging', {
            templateUrl: 'app/dataprepare/dataprepare-list.tpl.html',
            controller: 'dataprepareListCtrl',
            resolve: {
                //首次加载第一页
                initAging: ['initagingProxy', function (initagingProxy) {
                    return initagingProxy.initagingPaging(1, 15, "");
                } ],
                bdHoldstatus: ['baseDataProxy', function (baseDataProxy) {
                    return baseDataProxy.SysTypeDetail("005");
                } ],
                lstRet: ['initagingProxy', function (initagingProxy) {
                    return initagingProxy.query({ para1: 1, para2: 2 });
                } ],
                lstSubmitDatasRet: ['periodProxy', function (periodProxy) {
                    return periodProxy.query({ reportType: "Aging" });
                } ],
                oneYearRet: ['periodProxy', function (periodProxy) {
                    return periodProxy.query({ reportType: "OneYearSales" });
                } ],
                strCurrentPer: ['periodProxy', function (periodProxy) {
                    return periodProxy.queryObject({ cur: '1' });
                } ],
                lstHisAreaRet: ['periodProxy', function (periodProxy) {
                    return periodProxy.hisAreaPeroidPaging(1, 15);
                }]

            }
        });
    } ])

//*****************************************body***************************s
    .controller('dataprepareListCtrl',
    ['$scope', 'modalService', 'initAging', 'initagingProxy', 'baseDataProxy', 'siteProxy',
    '$http', 'bdHoldstatus', 'agingDownloadProxy', 'FileUploader', 'APPSETTING',
    'lstRet', 'collectionProxy', 'lstSubmitDatasRet', 'periodProxy', 'lstHisAreaRet',
    'strCurrentPer', 'oneYearRet', 
        function ($scope, modalService, initAging, initagingProxy, baseDataProxy, siteProxy,
        $http, bdHoldstatus, agingDownloadProxy, FileUploader, APPSETTING,
        lstRet, collectionProxy, lstSubmitDatasRet, periodProxy, lstHisAreaRet,
        strCurrentPer, oneYearRet) {
            $scope.$parent.helloAngular = "OTC - Data Preparation";
            //日期格式转换 llf
            Date.prototype.Format = function (fmt) { //author: llf 
                var o = {
                    "M+": this.getMonth() + 1, //月份 
                    "d+": this.getDate(), //日 
                    "H+": this.getHours(), //小时 
                    "m+": this.getMinutes(), //分 
                    "s+": this.getSeconds(), //秒 
                    "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
                    "S": this.getMilliseconds() //毫秒 
                };
                if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
                for (var k in o)
                    if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
                return fmt;
            }
            //日期控件定义 llf-start
            $scope.legalDateOptions = {
                //dateDisabled: disabled,
                formatYear: 'yy',
                maxDate: new Date(2099, 5, 22),
                //minDate: new Date(),
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

            $scope.openPromissDateHis = function () {
                $scope.popupPromissDateHis.opened = true;
            };
            $scope.popupPromissDateHis = {
                opened: false
            }; 

            $scope.today = function () {
                $scope.legalDate = new Date();
                $scope.legalDateHis = new Date();
            };

            $scope.today();
            //日期控件定义 llf-end     
           
            var strLegalDate = $scope.legalDate.Format("yyyyMMdd");           
          
            periodProxy.getLegalHisByDate(strLegalDate, function (list) {
                $scope.membersLegal = list;
            }, function (ex) { alert(ex) });

            var strHisDate = $scope.legalDateHis.Format("yyyyMMdd");
            periodProxy.getFileHisByDate(1, 15, strLegalDate, function (json) {        
                $scope.memHis = json.list;             
                $scope.hisareatotalItems = json.totalItems;
                $scope.hisTotalNum = json.totalItems;
                $scope.hisCalculate(json.list.length);
            }, function (ex) { alert(ex) });          
            
            //*************upload **********************s
            var uploader = $scope.uploader = new FileUploader({
                url: APPSETTING['serverUrl'] + '/api/Collection'
            });

            // FILTERS
            //上传文件，不同文件对应上传不同的队列，内容写在：angular-file-upload，这种写法也很无奈，第一任写法，我是第二任，如果再有增加文件上传的需求，请修改底层文件
            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    if (//item.name.toString().toUpperCase().split(".")[1] != "TXT"
                         item.name.toString().toUpperCase().split(".")[1] != "XLS"
                        //&& item.name.toString().toUpperCase().split(".")[1] != "ZIP"
                        && item.name.toString().toUpperCase().split(".")[1] != "CSV") {
                        alert("File format is not correct !");
                    }
                
                   
                    return this.queue.length < 100;
                }
            });

            //added by alex
            $scope.dropdownvalue = "Aging Report";
            $scope.updcolor = "#FFCC33";
            $scope.updCustcolor = "#FFCC33";
            $scope.updCHcolor = "#FFCC33";
            $scope.updCAcolor = "#FFCC33";
            $scope.updCATcolor = "#FFCC33";
            $scope.updVarcolor = "#FFCC33";
            $scope.submitcolor = "#FFCC33";
            $scope.dlhcolor = "#FFCC33";
            $scope.agingTotalNum = 0;
            $scope.oneYearTotalNum = 0;
            $scope.vatTotalNum = 0;
            $scope.hisTotalNum = 0;

            $scope.legalEntityList = lstRet;
            document.getElementById("currentPeriodTime").innerHTML = strCurrentPer;

            $scope.submitDatasList = lstSubmitDatasRet;
            $scope.oneYearSalesValueList = oneYearRet;

            var message = "";
            var indexNo = 1;
            var totalCount = 0;
            // CALLBACKS
            var totalFile = 0;
            var resType = 0;
            var accFile = 0;
            var invFile = 0;
            var invDetFile = 0;
            //var vatFile = 0;
            var uploadType = 0;
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                if (uploadType == 14) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[14] = "";
                    $scope.clearInfo("updConsignmentNumberFile");
                    uploadType = 0;
                }
                if (uploadType == 12) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[12] = "";
                    $scope.clearInfo("updATMCurrencyAmountFile");
                    uploadType = 0;
                }
                if (uploadType == 11) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[11] = "";
                    $scope.clearInfo("updTWCurrencyAmountFile");
                    uploadType = 0;
                }
                if (uploadType == 8) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[8] = "";
                    $scope.clearInfo("updCurrencyAmountFile");
                    uploadType = 0;
                }
                if (uploadType == 7) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[7] = "";
                    $scope.clearInfo("updCreditHoldFile");
                    uploadType = 0;
                }
                if (uploadType == 6) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[6] = "";
                    $scope.clearInfo("updSAPInvoiceFile");
                    uploadType = 0;
                }
                if (uploadType == 5) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[5] = "";
                    $scope.clearInfo("updVarFile");
                    uploadType = 0;
                }
                if (uploadType == 4)
                {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[4] = "";
                    $scope.clearInfo("updCustFile");
                    uploadType = 0;
                }
                if (uploadType == 3) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[3] = "";
                    $scope.clearInfo("vatFile");
                    uploadType = 0;
                }
                if (uploadType == 2) {
                    document.getElementById("overlay-container").style.display = "none";
                    alert(response);
                    uploader.queue[2] = "";
                    $scope.clearInfo("updInvoiceDetailFile");
                    uploadType = 0;
                }
                resType = fileItem.url.split('=')[1];
                if (resType != null)
                {
                    if (resType == 0)
                    {
                        accFile = response;
                    }
                    else if (resType == 1)
                    {
                        invFile = response;
                    }
                    //else if (resType == 2)
                    //{
                    //    invDetFile = response;
                    //}
                    //else if (resType == 3)
                    //{
                    //    vatFile = response;
                    //}
                }
                if (totalFile == 2) {
                    if (accFile != '0' && invFile != '0') {
                        periodProxy.uploadAg(accFile, invFile, function (list) {
                            message = message + list;
                            $scope.finalDo();
                            $scope.ReSearch();
                        }, function (ex) {
                            alert(ex); $scope.finalDo(); $scope.ReSearch();
                        });
                    }
                }
                //else if (totalFile == 3) {
                //    if (accFile != '0' && invFile != '0' && invDetFile != '0') {
                //        periodProxy.uploadAg(accFile, invFile, invDetFile, function (list) {
                //            message = message + list;
                //            $scope.finalDo();
                //            $scope.ReSearch();
                //        }, function (ex) { alert(ex); $scope.finalDo(); $scope.ReSearch(); });
                //    }
                //}
                //else if (totalFile == 4) {
                //    if (accFile != '0' && invFile != '0' && invDetFile != '0' && vatFile != '0') {
                //        periodProxy.uploadAg(accFile, invFile, invDetFile, vatFile, function (list) {
                //            message = message + list;
                //            $scope.finalDo();
                //            $scope.ReSearch();
                //        }, function (ex) { alert(ex); $scope.finalDo(); $scope.ReSearch(); });
                //    }
                //}
            };
            
            $scope.finalDo = function () {
                document.getElementById("overlay-container").style.display = "none";
                    alert(message);
                    //console.info('onSuccessItem', fileItem, message, status, headers);
                    totalFile = 0;
                    resType = 0;
                    accFile = 0;
                    invFile = 0;
                    invDetFile = 0;
                    //vatFile = 0;

                    $scope.search();

                    periodProxy.query({ reportType: "Aging" }, function (submitList) {
                        $scope.submitDatasList = submitList;
                    });

                    $scope.clearInfo("updAccountFile");
                    $scope.clearInfo("updInvoiceFile");              
                    //$scope.clearInfo("updInvoiceDetailFile");
                    $scope.clearInfo("updSAPInvoiceFile");
                    //$scope.clearInfo("vatFile");
                    $scope.clearInfo("updCustFile");
                    //$scope.clearInfo("updVarFile");
                    $scope.clearInfo("updCreditHoldFile");   
                    $scope.clearInfo("updCurrencyAmountFile");
                    $scope.clearInfo("updTWCurrencyAmountFile");
                    $scope.clearInfo("updATMCurrencyAmountFile"); 
                    $scope.clearInfo("updConsignmentNumberFile"); 
                    message = "";
            };
            $scope.ReSearch = function () {
                //Legal
                if ($scope.legalDate !== undefined && $scope.legalDate != "") {
                    var btnLegalDate = $scope.legalDate.Format("yyyyMMdd");
                    periodProxy.getLegalHisByDate(btnLegalDate, function (list) {
                        $scope.membersLegal = list;
                    }, function (ex) { alert(ex) });
                }
                //HIS
                var btnHisDate = $scope.legalDateHis.Format("yyyyMMdd");
                periodProxy.getFileHisByDate(1, 15, btnHisDate, function (json) {
                    $scope.memHis = json.list;
                    $scope.hisareatotalItems = json.totalItems;
                    $scope.hisTotalNum = json.totalItems;
                    $scope.hisCalculate(json.list.length);
                }, function (ex) { alert(ex) }); 
                //aging
                $scope.search();
                //invoice detail
                periodProxy.getSubmitWaitInvDet(1, 15, function (json) {
                    $scope.memInv = json.list;
                    $scope.oneyeartotalItems = json.totalItems;
                    $scope.oneYearTotalNum = json.totalItems;
                    $scope.oneYearCalculate(json.list.length);
                }, function (ex) { alert(ex) });  
                //vat
                periodProxy.getSubmitWaitVat(1, 15, function (json) {
                    $scope.memVat = json.list;
                    $scope.vattotalItems = json.totalItems;
                    $scope.vatTotalNum = json.totalItems;
                    $scope.vatCalculate(json.list.length);
                }, function (ex) { alert(ex) });
                //ConsigmentNumber
                //periodProxy.getSubmitWaitConsigmentNumber(1, 15, function (json) {
                //    $scope.memVat = json.list;
                //    $scope.vattotalItems = json.totalItems;
                //    $scope.vatTotalNum = json.totalItems;
                //    $scope.vatCalculate(json.list.length);
                //}, function (ex) { alert(ex) });
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                if (uploadType == 2) {
                    document.getElementById("overlay-container").style.display = "none";
                    //alert(response);
                    uploader.queue[2] = "";
                    $scope.clearInfo("updInvoiceDetailFile");
                    uploadType = 0;
                }
                if (uploadType == 3) {
                    document.getElementById("overlay-container").style.display = "none";
                    //alert(response);
                    uploader.queue[3] = "";
                    $scope.clearInfo("vatFile");
                    uploadType = 0;
                }
                if (uploadType == 5) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[5] = "";
                    $scope.clearInfo("updVarFile");
                    uploadType = 0;
                }
                if (uploadType == 6) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[6] = "";
                    $scope.clearInfo("updSAPInvoiceFile");
                    uploadType = 0;
                }
                if (uploadType == 7) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[7] = "";
                    $scope.clearInfo("updCreditHoldFile");
                    uploadType = 0;
                }
                if (uploadType == 8) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[8] = "";
                    $scope.clearInfo("updCurrencyAmountFile");
                    uploadType = 0;
                }
                if (uploadType == 11) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[11] = "";
                    $scope.clearInfo("updTWCurrencyAmountFile");
                    uploadType = 0;
                }
                if (uploadType == 12) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[12] = "";
                    $scope.clearInfo("updATMCurrencyAmountFile");
                    uploadType = 0;
                }
                if (uploadType == 14) {
                    document.getElementById("overlay-container").style.display = "none";
                    uploader.queue[14] = "";
                    $scope.clearInfo("updConsignmentNumberFile");
                    uploadType = 0;
                }
                alert(response);
                $scope.finalDo();
                $scope.ReSearch();               
            };

            $scope.getFileExtendName = function (str) {
                var d = /\.[^\.]+$/.exec(str);
                return d;
            }

            $scope.updateLevel = function () {

                if ($scope.checkIsDuringJobSchedule()) {
                    return;
                }

                messgeInfo = "";

                if (uploader.queue.length <= 0) {
                    alert("Please select the file you need to upload!");
                    return;
                } else if (uploader.queue[0] == "" && uploader.queue[1] == "" && uploader.queue[2] == "") {
                    alert("Please select the file you need to upload!");
                    return;
                } else if (uploader.queue[0] !== "" && uploader.queue[1] == "") {
                    alert("Account and Invoice files must be selected!");
                    return;
                } else if (uploader.queue[0] == "" && uploader.queue[1] !== "") {
                    alert("Account and Invoice files must be selected!");
                    return;
                } else if (uploader.queue[0] == "" && uploader.queue[1] == "") {
                    alert("Account and Invoice files must be selected!");
                    return;
                }
                //else if (uploader.queue[0] !== "" && uploader.queue[1] !== "" && uploader.queue[3] !== undefined && uploader.queue[3] !== "") {
                //    if (uploader.queue[2] !== undefined && uploader.queue[2] !== "") {
                //    }
                //    else {
                //        alert("upload vat must be selected Invoice Detail!");
                //        return;
                //    }
                //}

                //arrow-Account File 可以XLS(97-03)或者csv
                if (uploader.queue[0] !== "" && uploader.queue[0]._file.name.toString().toUpperCase().split(".")[1] != "XLS"
                    && uploader.queue[0]._file.name.toString().toUpperCase().split(".")[1] != "CSV") {
                    alert("Account File format is not correct, \nPlease select the '.xls' or '.csv' file !");
                    return;
                }

                //arrow-Invoice File 可以XLS(97-03)或者csv
                if (uploader.queue[1] !== "" && uploader.queue[1]._file.name.toString().toUpperCase().split(".")[1] != "XLS"
                    && uploader.queue[1]._file.name.toString().toUpperCase().split(".")[1] != "CSV") {
                    alert("Invoice File format is not correct, \nPlease select the '.xls' or '.csv' file !");
                    return;
                }
                
                document.getElementById("overlay-container").style.display = "block";

                //传2个文件                
                totalFile = 2;
                for (var i = 0; i < 2; i++) {
                    uploader.queue[i].url = APPSETTING['serverUrl'] + '/api/Collection' + '?levelFlag=' + i;
                } 

                indexNo = 1;
                message = "";
                totalCount = 0;

                uploader.uploadAll();
            };

            $scope.updateInvoiceDetail = function () {
                uploadType = 2;

                if (uploader.queue[2] == undefined || uploader.queue[2] == "" || uploader.queue[2] == "undefied") {
                    alert("Please select the invoice detail file you need to upload!");
                    return;
                }

                //InvoiceDetail File 
                if (uploader.queue[2] !== undefined) {
                    if (uploader.queue[2] !== "") {
                        var vatExendName = $scope.getFileExtendName(uploader.queue[2]._file.name.toString()).toString().toUpperCase().split(".")[1];
                        if (uploader.queue[2] !== "" && vatExendName != "CSV"
                            && vatExendName != "GZ"
                            && vatExendName != "TAR"
                            && vatExendName != "XLS") {
                            alert("Invoice detail file format is not correct, \nPlease select the '.csv' or '.gz' or '.tar' or '.xls' file !");
                            return;
                        }
                    }
                }
                document.getElementById("overlay-container").style.display = "block";

                if (uploader.queue[2] !== undefined && uploader.queue[2] != "") {

                    uploader.queue[2].url = APPSETTING['serverUrl'] + '/api/Customer/UploadInvoiceDetailOnly';
                }
                uploader.uploadAll();

            };

            $scope.updateSAPInvoice = function () {

                if ($scope.checkIsDuringJobSchedule()) {
                    return;
                }

                uploadType = 6;

                if (uploader.queue[6] == undefined || uploader.queue[6] == "" || uploader.queue[6] == "undefied") {
                    alert("Please select the sap invoice file you need to upload!");
                    return;
                }

                //SAP InvoiceDetail File 
                if (uploader.queue[6] !== undefined) {
                    if (uploader.queue[6] !== "") {
                        var vatExendName = $scope.getFileExtendName(uploader.queue[6]._file.name.toString()).toString().toUpperCase().split(".")[1];
                        if (uploader.queue[6] !== "" && vatExendName != "CSV"
                            && vatExendName != "GZ"
                            && vatExendName != "TAR"
                            && vatExendName != "XLS"
                            && vatExendName != "XLSX") {
                            alert("SAP Invoice file format is not correct, \nPlease select the '.csv' or '.gz' or '.tar' or '.xls' or '.xlsx' file !");
                            return;
                        }
                    }
                }
                document.getElementById("overlay-container").style.display = "block";

                if (uploader.queue[6] !== undefined && uploader.queue[6] != "") {

                    uploader.queue[6].url = APPSETTING['serverUrl'] + '/api/Customer/UploadSAPInvoiceOnly';
                }
                uploader.uploadAll();

            };

            //$scope.updateVat = function () {
            //    uploadType = 3;

            //    if (uploader.queue[3] == undefined || uploader.queue[3] == "" || uploader.queue[3] == "undefied" ) {
            //        alert("Please select the vat file you need to upload!");
            //        return;
            //    }

            //    //VAT File 
            //    if (uploader.queue[3] !== undefined) {
            //        if (uploader.queue[3] !== "") {
            //            var vatExendName = $scope.getFileExtendName(uploader.queue[3]._file.name.toString()).toString().toUpperCase().split(".")[1];
            //            if (uploader.queue[3] !== "" && vatExendName != "CSV"
            //                && vatExendName != "GZ"
            //                && vatExendName != "TAR"
            //                && vatExendName != "XLS") {
            //                alert("vat File format is not correct, \nPlease select the '.csv' or '.gz' or '.tar' or '.xls' file !");
            //                return;
            //            }
            //        }
            //    }
            //    document.getElementById("overlay-container").style.display = "block";

            //    if (uploader.queue[3] !== undefined && uploader.queue[3] != "") {
                   
            //        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Customer/UploadVatOnly';
            //    }
            //    uploader.uploadAll();

            //};

            $scope.updateVat = function () {

                uploadType = 3;
                if (uploader.queue[3] === null || uploader.queue[3] === "" || uploader.queue[3] === undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }
                if (uploader.queue[3]._file.name.toString().toUpperCase().split(".")[1] !== "XLS"
                    && uploader.queue[3]._file.name.toString().toUpperCase().split(".")[1] !== "XLSX") {
                    alert("File format is not correct, \nPlease select the '.xls' or '.xlsx' file !");
                    return;
                }

                document.getElementById("overlay-container").style.display = "block";
                document.getElementById("overlay-container").style.flg = "loading";

                uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/Customer/UploadVatOnly';
                uploader.uploadAll();
            };

            $scope.updateCust = function () {
                uploadType = 4;
                if (uploader.queue[4] == "" || uploader.queue[4] == "undefied" || uploader.queue[4] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }
             
                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[4] !== undefined && uploader.queue[4]!="") {                 
                    //totalFile = 4;
                    uploader.queue[4].url = APPSETTING['serverUrl'] + '/api/Customer/UploadCustomerLocalize';
                }
                uploader.uploadAll();
                
            };

            $scope.updateCreditHold = function () {
                uploadType = 7;
                if (uploader.queue[7] == "" || uploader.queue[7] == "undefied" || uploader.queue[7] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }

                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[7] !== undefined && uploader.queue[7] != "") {
                    uploader.queue[7].url = APPSETTING['serverUrl'] + '/api/Customer/UploadCreditHold';
                }
                uploader.uploadAll();

            };

            $scope.updateCurrencyAmount = function () {
                uploadType = 8;
                if (uploader.queue[8] == "" || uploader.queue[8] == "undefied" || uploader.queue[8] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }

                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[8] !== undefined && uploader.queue[8] != "") {
                    uploader.queue[8].url = APPSETTING['serverUrl'] + '/api/Customer/UploadCurrencyAmount';
                }
                uploader.uploadAll();

            };

            $scope.updateTWCurrencyAmount = function () {
                uploadType = 11;
                if (uploader.queue[11] == "" || uploader.queue[11] == "undefied" || uploader.queue[11] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }

                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[11] !== undefined && uploader.queue[11] != "") {
                    uploader.queue[11].url = APPSETTING['serverUrl'] + '/api/Customer/UploadTWCurrencyAmount';
                }
                uploader.uploadAll();

            };

            $scope.updateATMCurrencyAmount = function () {
                uploadType = 12;
                if (uploader.queue[12] == "" || uploader.queue[12] == "undefied" || uploader.queue[12] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }

                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[12] !== undefined && uploader.queue[12] != "") {
                    uploader.queue[12].url = APPSETTING['serverUrl'] + '/api/Customer/UploadATMCurrencyAmount';
                }
                uploader.uploadAll();

            };

            $scope.updateConsignmentNumber = function () {
                uploadType = 14;
                if (uploader.queue[14] == "" || uploader.queue[14] == "undefied" || uploader.queue[14] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }

                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[14] !== undefined && uploader.queue[14] != "") {
                    uploader.queue[14].url = APPSETTING['serverUrl'] + '/api/Customer/UploadConsignmentNumber';
                }
                uploader.uploadAll();

            };

            $scope.updateVar = function () {
                uploadType = 5;
                if (uploader.queue[5] == "" || uploader.queue[5] == "undefied" || uploader.queue[5] == undefined) {
                    alert("Please select the file you need to upload!");
                    return;
                }
                if (uploader.queue[5].file.name.toString().toUpperCase().split(".")[1] != "CSV" &&
                    uploader.queue[5].file.name.toString().toUpperCase().split(".")[1] != "XLSX") {
                    alert("File format is not correct! Please select .csv or .xlsx file.");
                    return;
                }
                document.getElementById("overlay-container").style.display = "block";
                if (uploader.queue[5] !== undefined && uploader.queue[5] != "") {
                    uploader.queue[5].url = APPSETTING['serverUrl'] + '/api/Customer/UploadVarData';
                }
                uploader.uploadAll();

            };


            $scope.selectedLevel = 15;          //下拉单页容量初始化(Confirm Area)
            $scope.selectedOneYearLevel = 15;   //下拉单页容量初始化(One Year Sales)
            $scope.selectedvatLevel = 15;
            $scope.selectedHisAreaLevel = 15;   //下拉单页容量初始化(His Area)
            $scope.itemsperpage = 15;           //对应页(Confirm Area)
            $scope.oneyearitemsperpage = 15;    //对应页(One Year Sales)
            $scope.vatitemsperpage = 15; 
            $scope.hisareaitemsperpage = 15;    //对应页(His Area)
            $scope.currentPage = 1;
            $scope.oneyearcurrentPage = 1;
            $scope.vatcurrentPage = 1;
            $scope.hisareacurrentPage = 1;
            $scope.maxSize = 10;
            $scope.oneyearmaxSize = 10;
            $scope.vatmaxSize = 10;
            $scope.hisareamaxSize = 10;
            var filstr = "";


            //单页容量下拉列表定义
            $scope.levelList = [
                            { "id": 15, "levelName": '15' },
                            { "id": 500, "levelName": '500' },
                            { "id": 1000, "levelName": '1000' },
                            { "id": 2000, "levelName": '2000' },
                            { "id": 5000, "levelName": '5000' }
            ];

            //footer items calculate
            $scope.agingCalculate = function (count) {
                if (count == 0) {
                    $scope.agingFromItem = 0;
                } else {
                    $scope.agingFromItem = ($scope.currentPage - 1) * $scope.itemsperpage + 1;
                }
                $scope.agingToItem = ($scope.currentPage - 1) * $scope.itemsperpage + count;
            }
            $scope.oneYearCalculate = function (count) {
                if (count == 0) {
                    $scope.oneYearFromItem = 0;
                } else {
                    $scope.oneYearFromItem = ($scope.oneyearcurrentPage - 1) * $scope.oneyearitemsperpage + 1;
                }
                $scope.oneYearToItem = ($scope.oneyearcurrentPage - 1) * $scope.oneyearitemsperpage + count;
            }
            $scope.vatCalculate = function (count) {
                if (count == 0) {
                    $scope.vatFromItem = 0;
                } else {
                    $scope.vatFromItem = ($scope.vatcurrentPage - 1) * $scope.vatitemsperpage + 1;
                }
                $scope.vatToItem = ($scope.vatcurrentPage - 1) * $scope.vatitemsperpage + count;
            }
            $scope.hisCalculate = function (count) {
                if (count == 0) {
                    $scope.hisFromItem = 0;
                } else {
                    $scope.hisFromItem = ($scope.hisareacurrentPage - 1) * $scope.hisareaitemsperpage + 1;
                }
                $scope.hisToItem = ($scope.hisareacurrentPage - 1) * $scope.hisareaitemsperpage + count;
            }

            //加载nggrid数据绑定
            //$scope.legalEntityCount = {
            //    data: 'legalEntityList',
            //    infiniteScrollUp: true,
            //    infiniteScrollDown: true,
            //    columnDefs: [
            //        { field: 'legalEntity', displayName: 'Legal Entity', width: '110' },//width: '130'
            //        { field: 'accTimes', displayName: 'Aging Report', width: '110' },// width: '130'
            //        { field: 'oneYearTimes', displayName: 'Invoice Detail File', width: '110' },// width: '130'
            //        { field: 'vatTimes', displayName: 'VAT File', width: '90' },//width: '130'
            //        { field: 'reportTime', displayName: 'Report Date', width: '150' }
            //    ],
            //    onRegisterApi: function (gridApi) {
            //        //set gridApi on scope
            //        $scope.gridApi = gridApi;
            //    }
            //};         

            $scope.dateStatusSearch = function () { 
                if( $scope.legalDate !== undefined && $scope.legalDate!="")
                {
                    var btnLegalDate = $scope.legalDate.Format("yyyyMMdd");
                    periodProxy.getLegalHisByDate(btnLegalDate, function (list) {
                        $scope.membersLegal = list;                
                    }, function (ex) { alert(ex) });
                 }
            };

            $scope.downloadHistorySearch = function () {
                var btnHisDate = $scope.legalDateHis.Format("yyyyMMdd");
                periodProxy.getFileHisByDate(1, 15, btnHisDate, function (json) {                  
                    $scope.memHis = json.list;
                    $scope.hisareatotalItems = json.totalItems;
                    $scope.hisTotalNum = json.totalItems;
                    $scope.hisCalculate(json.list.length);
                }, function (ex) { alert(ex) }); 
                //test-add
                //periodProxy.getLegalByDash(function (json1) {
                //    console.log(json1);
                  
                //}, function (ex) { alert(ex) }); 
                //test-end
            };
          
            $scope.legalEntityCount = {
                data: 'membersLegal',
                infiniteScrollUp: true,
                infiniteScrollDown: true,
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '100' },
                    {
                        field: 'stateAcc', displayName: 'Account Level', width: '150',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()" style="text-align:center">' +
                        '<img ng-show="row.entity.stateAcc == true" src="~/../Content/images/ready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateAcc,1)">' +
                        '<img ng-show="row.entity.stateAcc == false" src="~/../Content/images/notready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateAcc,1)"> </div>'
                    },
                    {
                        field: 'stateInv', displayName: 'Invoice Level', width: '150',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()" style="text-align:center">' +
                            '<img ng-show="row.entity.stateInv == true" src="~/../Content/images/ready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateInv,2)">' +
                            '<img ng-show="row.entity.stateInv == false" src="~/../Content/images/notready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateInv,2)"> </div>'
                    }
                    //},
                    //{
                    //    field: 'stateInvDet', displayName: 'Invoice Detail', width: '110',
                    //    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()" style="text-align:center">' +
                    //    '<img ng-show="row.entity.stateInvDet == true" src="~/../Content/images/ready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateInvDet,3)">' +
                    //    '<img ng-show="row.entity.stateInvDet == false" src="~/../Content/images/notready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateInvDet,3)"> </div>'
                    //},
                    //{
                    //    field: 'stateVat', displayName: 'VAT', width: '90',
                    //    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()" style="text-align:center">' +
                    //    '<img ng-show="row.entity.stateVat == true" src="~/../Content/images/ready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateVat,4)">' +
                    //    '<img ng-show="row.entity.stateVat == false" src="~/../Content/images/notready.png" ng-click="grid.appScope.downloadLegalNew(row.entity.legalEntity,row.entity.stateVat,4)"> </div>'
                    //}                
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };
            //清空（上传文件方法）-除了按钮清空，上传成功失败都要调用
            $scope.clearInfo = function (items) {
                if (uploader.queue.length > 0) {
                    if (items == "updAccountFile") {
                        uploader.queue[0] = "";
                    } else if (items == "updInvoiceFile") {
                        uploader.queue[1] = "";
                        //} else {
                        //    uploader.queue[2] = "";
                        //}
                    } else if (items == "updInvoiceDetailFile") {
                        uploader.queue[2] = "";
                    } else if (items == "vatFile") {
                        uploader.queue[3] = "";
                    } else if (items == "updCustFile") {
                        uploader.queue[4] = "";
                    } else if (items == "updVarFile")
                        uploader.queue[5] = "";
                    else if (items == "updSAPInvoiceFile")
                        uploader.queue[6] = "";
                    else if (items == "updCreditHoldFile")
                        uploader.queue[7] = "";
                    else if (items == "updCurrencyAmountFile")
                        uploader.queue[8] = "";
                    else if (items == "updTWCurrencyAmountFile")
                        uploader.queue[11] = "";
                    else if (items == "updATMCurrencyAmountFile")
                        uploader.queue[12] = "";
                    else if (items == "updConsignmentNumberFile")
                        uploader.queue[14] = "";
                    else {
                    }
                }
                messgeInfo = "";
                document.getElementById(items).value = "";
            };

            //*************upload **********************e

            $scope.isShow = true;
            $scope.isOneYearShow = false;
            $scope.isVat = false;

            $scope.showAging = function () {
                $scope.dropdownvalue = "Aging Report";
                $scope.isShow = true;
                $scope.isOneYearShow = false;
                $scope.isVat = false;
                
            };

            $scope.showInvoiceDetail = function () {
                $scope.dropdownvalue = "Invoice Detail File";
                $scope.isShow = false;
                $scope.isOneYearShow = true;
                $scope.isVat = false;

                periodProxy.getSubmitWaitInvDet(1, 15, function (json) {
                    $scope.memInv = json.list;
                    $scope.oneyeartotalItems = json.totalItems;
                    $scope.oneYearTotalNum = json.totalItems;
                    $scope.oneYearCalculate(json.list.length);
                }, function (ex) { alert(ex) });             

                //initagingProxy.initagingPagingOneYearList(1, 15, function (list) {
                //    $scope.oneYearList = list[0].results;
                //    $scope.oneyeartotalItems = list[0].count;
                //    $scope.oneYearTotalNum = list[0].count;
                //    $scope.oneYearCalculate(list[0].results.length);
                //});

                //第二页左上角
                //periodProxy.query({ reportType: "OneYearSales" }, function (list) {
                //    $scope.oneYearSalesValueList = list;
                //    if ($scope.oneYearSalesValueList.length > 0) {
                //        document.getElementById("oneYearSalesValue").innerHTML = $scope.oneYearSalesValueList[0].oneYearTimes;
                //        document.getElementById("reportDateValue").innerHTML = $scope.oneYearSalesValueList[0].reportTime;
                //    } else {
                //        document.getElementById("oneYearSalesValue").innerHTML = "";
                //        document.getElementById("reportDateValue").innerHTML = "";
                //    }
                //});

            };

            $scope.showVat = function () {
                $scope.dropdownvalue = "Vat File";
                $scope.isShow = false;
                $scope.isOneYearShow = false;
                $scope.isVat = true;

                periodProxy.getSubmitWaitVat(1, 15, function (json) {
                    $scope.memVat = json.list;
                    $scope.vattotalItems = json.totalItems;
                    $scope.vatTotalNum = json.totalItems;
                    $scope.vatCalculate(json.list.length);
                }, function (ex) { alert(ex) });
         
            };

            //加载nggrid数据绑定-back
            //$scope.oneYear = {
            //    data: 'oneYearList',
            //    columnDefs: [
            //        { field: 'billGroupCode', displayName: 'Factory Group Code' },
            //        { field: 'billGroupName', displayName: 'Group Name' },
            //        { field: 'oneYearSales', displayName: 'VAT', cellFilter: 'number:2', type: 'number' }
            //    ],
            //    onRegisterApi: function (gridApi) {
            //        //set gridApi on scope
            //        $scope.gridApi = gridApi;
            //    }
            //};

            //加载nggrid数据绑定-静态数据
            $scope.members = [
                {
                    invoiceDate: '2017/11/1', customerPO: '0718', manufacturer: 'ADI', 
                    partNumber: 'AD8608ARZ-REEL7', invoiceNumber: '605021059531', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '9930', unitResales: '0.943', nSB: '9363.99'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100015305.374/9', manufacturer: 'NXP',
                    partNumber: 'MC68340CAB25E', invoiceNumber: '605021059166', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '50', unitResales: '20.14822', nSB: '1007.411'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100017856.144/90', manufacturer: 'MICROCHIP',
                    partNumber: 'KSZ8081RNAIA-TR', invoiceNumber: '605021059678', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '930', unitResales: '0.4264', nSB: '396.552'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100017856.80/95', manufacturer: 'TI',
                    partNumber: 'CSD17307Q5A', invoiceNumber: '605021059833', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '2430', unitResales: '0.1189', nSB: '288.927'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100017856.81/124', manufacturer: 'MICROCHIP',
                    partNumber: 'MC68340CAB25E', invoiceNumber: '605021059166', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '50', unitResales: '20.14822', nSB: '1007.411'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100017856.144/90', manufacturer: 'MICROCHIP',
                    partNumber: 'MIC2940A-5.0WT', invoiceNumber: '605021059677', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '280', unitResales: '0.5658', nSB: '158.424'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100017890.102/46', manufacturer: 'JOHANSON',
                    partNumber: '302R29N150JV4E', invoiceNumber: '605021059360', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '11930', unitResales: '0.028864', nSB: '344.34752'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100017890.229/8', manufacturer: 'ONSEMI',
                    partNumber: 'BAT54CXV3T1G', invoiceNumber: '605021059565', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'USD', invoiceQty: '2930', unitResales: '0.07052', nSB: '206.6236'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '100070-953', manufacturer: 'FAIRRITE',
                    partNumber: '2643002402', invoiceNumber: '666020016256', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'CNY', invoiceQty: '10730', unitResales: '0.388598', nSB: '4169.65654'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '17-1', manufacturer: 'NEXPERIA',
                    partNumber: 'BZT52H-C13,115', invoiceNumber: '620020136921', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'CNY', invoiceQty: '503930', unitResales: '0.0714876', nSB: '36024.746268'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '170402066', manufacturer: 'ONSEMI',
                    partNumber: 'ULN2003ADR2G', invoiceNumber: '620020136858', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'CNY', invoiceQty: '9930', unitResales: '0.4064986', nSB: '4036.531098'
                },
                {
                    invoiceDate: '2017/11/1', customerPO: '170402066', manufacturer: 'VISHAY',
                    partNumber: 'TSAL6100', invoiceNumber: '620020136859', invoiceLineNumber: '1',
                    transactionCurrencyCode: 'CNY', invoiceQty: '15930', unitResales: '0.6447824', nSB: '10271.383632'
                },
            ];

            $scope.oneYear = {
                data: 'memInv',
                columnDefs: [
                    { field: 'invoiceDate', displayName: 'Invoice Date', width: '100' },
                    { field: 'customerPO', displayName: 'Customer PO #', width: '150' },
                    { field: 'manufacturer', displayName: 'Manufacturer', width: '150' },
                    { field: 'partNumber', displayName: 'Part Number', width: '150' },
                    { field: 'invoiceNumber', displayName: 'Invoice Number', width: '150' },
                    { field: 'invoiceLineNumber', displayName: 'Invoice Line Number', width: '150'},
                    { field: 'transactionCurrencyCode', displayName: 'Transaction Currency Code', width: '200' },
                    { field: 'invoiceQty', displayName: 'Invoice Qty', width: '100' },
                    { field: 'unitResales', displayName: 'Unit Resales', width: '100'},
                    { field: 'nsb', displayName: 'NSB', width: '100' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            $scope.vatGrid = {
                data: 'memVat',
                columnDefs: [
                    { field: 'trx_Number', displayName: 'TRX_NUMBER', width: '100' },
                    { field: 'lineNumber', displayName: 'LINE_NUMBER', width: '100' },
                    { field: 'salesOrder', displayName: 'SALES_ORDER', width: '120' },
                    { field: 'creationDate', displayName: 'CREATION_DATE', width: '150' },
                    { field: 'customerTrxId', displayName: 'CUSTOMER_TRX_ID', width: '150' },
                    { field: 'attributeCategory', displayName: 'ATTRIBUTE_CATEGORY', width: '180' },
                    { field: 'orgId', displayName: 'ORG_ID', width: '100' },
                    { field: 'vatInvoice', displayName: 'VAT_INVOICE', width: '100' },
                    { field: 'vatInvoiceDate', displayName: 'VAT_INV_DATE', width: '150' },
                    { field: 'vatInvoiceAmount', displayName: 'VAT_INV_AMT', width: '150' },
                    { field: 'vatTaxAmount', displayName: 'VAT_TAX_AMT', width: '150' },
                    { field: 'vatInvoiceTotalAmount', displayName: 'VAT_INV_TOTAL_AMT', width: '150' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            //Legal Entity DropDownList数据绑定
            siteProxy.Site("", function (legallist) {
                $scope.legallist = legallist;
            });
            //Status Entity DropDownList数据绑定
            $scope.bdHoldstatus = bdHoldstatus;

            ////One Year Sales 初期化数据赋值(ui-grid)
            //if (lstOneYearRet) {
            //    $scope.oneYearList = lstOneYearRet[0].results; //首次当前页数据
            //    $scope.oneyeartotalItems = lstOneYearRet[0].count; //查询结果初始化
            //}

            //Confirm Area 初期化数据赋值(ui-grid)
            if (initAging) {
                $scope.list = initAging[0].results; //首次当前页数据
                $scope.totalItems = initAging[0].count; //查询结果初始化
                $scope.agingTotalNum = initAging[0].count;
                $scope.agingCalculate(initAging[0].results.length);
            }

            //His Area 初期化数据赋值(ui-grid)
            //if (lstHisAreaRet) {
            //    $scope.hisAreaList = lstHisAreaRet[0].results;
            //    $scope.hisareatotalItems = lstHisAreaRet[0].count;
            //    $scope.hisTotalNum = lstHisAreaRet[0].count;
            //    $scope.hisCalculate(lstHisAreaRet[0].results.length);
            //}

            //nggrid数据绑定
            $scope.griddatas = {
                multiSelect: true,
                enableFullRowSelection: true,
                // enableRowSelection: true,
                // enableSelectAll: true,
                enableSorting: true,
                data: 'list',
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '100' },
                    { name: 'cc', displayName: 'Customer NO.', width: '150',
                        cellTemplate: '<div class="ui-grid-cell-contents ngCellText" ng-class="col.colIndex()">{{row.entity.customerNum}} &nbsp; <img ng-click="resetSearch();" ng-show="row.entity.cusExFlg == 0" src="~/../Content/images/259.png"></div>'
                    },
                    { field: 'customerName', displayName: 'Customer Name', width: '200' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '200' },
                //                    { field: 'customerClass', displayName: 'Customer Class', width: '70' },
                //                    { field: 'riskScore', displayName: 'Risk Score', width: '80' },
                    { field: 'totalAmt', displayName: 'Total A/R Balance', width: '120', cellFilter: 'number:2', type: 'number' },
                //                    { field: 'due90Amt', displayName: 'Over 90 days', width: '90', cellFilter: 'number' },
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '120', cellFilter: 'number:2', type: 'number' },
                    { field: 'isHoldFlg', displayName: 'Account Status', width: '120',
                        cellTemplate: '<div hello="{valueMember: \'isHoldFlg\', basedata: \'grid.appScope.bdHoldstatus\'}"></div>'
                    },
                    { field: 'operator', displayName: 'Operator', width: '120'
                        //cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">{{row.entity.collector}} <img ng-click="grid.appScope.showPop(row.entity);"  src="~/../Content/images/331.png"></div>'
                    }
                ]
            };

            $scope.search = function () {
                //组合过滤条件
                var filterStr = '';
                if ($scope.custCode) {
                    if (filterStr != "") {
                        filterStr += "and (contains(CustomerNum,'" + $scope.custCode + "'))";
                    } else {
                        filterStr += "&$filter=(contains(CustomerNum,'" + $scope.custCode + "'))";
                    }
                }
                if ($scope.custName) {
                    if (filterStr != "") {
                        filterStr += "and (contains(CustomerName,'" + $scope.custName + "'))"
                    } else {
                        filterStr += "&$filter=(contains(CustomerName,'" + $scope.custName + "'))";
                    }

                }
                if ($scope.class) {
                    if (filterStr != "") {
                        filterStr += "and (CustomerClass eq '" + $scope.class + "')";
                    } else {
                        filterStr += "&$filter=(CustomerClass eq '" + $scope.class + "')";
                    }
                }
                if ($scope.status) {
                    if (filterStr != "") {
                        filterStr += "and (IsHoldFlg eq '" + $scope.status + "')";
                    } else {
                        filterStr += "&$filter=(IsHoldFlg eq '" + $scope.status + "')";
                    }
                }
                if ($scope.legal) {
                    if (filterStr != "") {
                        filterStr += "and (LegalEntity eq '" + $scope.legal + "')";
                    } else {
                        filterStr += "&$filter=(LegalEntity eq '" + $scope.legal + "')";
                    }
                }
                if (document.getElementById("ckbMasterData").checked) {
                    if (filterStr != "") {
                        filterStr += "and (CusExFlg eq '0')";
                    } else {
                        filterStr += "&$filter=(CusExFlg eq '0')";
                    }
                }
                //                if (document.getElementById("ckbAccountData").checked) {
                //                    if (filterStr != "") {
                //                        filterStr += "and (AmtMappingFlg eq '2')";
                //                    } else {
                //                        filterStr += "&$filter=(AmtMappingFlg eq '2')";
                //                    }
                //                }
                if (document.getElementById("ckbInvoiceData").checked) {
                    if (filterStr != "") {
                        filterStr += "and (AmtMappingFlg eq '2')";
                    } else {
                        filterStr += "&$filter=(AmtMappingFlg eq '2')";
                    }
                }
                if (document.getElementById("ckbTotalAmountNotEqual").checked) {
                    if (filterStr != "") {
                        filterStr += "and (AmtMappingFlg eq '1')";
                    } else {
                        filterStr += "&$filter=(AmtMappingFlg eq '1')";
                    }
                }

                filstr = filterStr;

                $scope.currentPage = 1;
                initagingProxy.initagingPaging($scope.currentPage, $scope.itemsperpage, filterStr, function (list) {
                    if (list != null) {
                        //console.log(list[0])
                        $scope.totalItems = list[0].count;
                        $scope.list = list[0].results;
                        $scope.agingTotalNum = list[0].count;
                        $scope.agingCalculate(list[0].results.length);
                    }
                })

            };

            //单页容量变化
            $scope.pagesizechange = function (selectedLevelId) {
                var index = $scope.currentPage;
                initagingProxy.initagingPaging(index, selectedLevelId, filstr, function (list) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.list = list[0].results;
                    $scope.totalItems = list[0].count;
                    $scope.agingTotalNum = list[0].count;
                    $scope.agingCalculate(list[0].results.length);
                });
            };

            //翻页
            $scope.pageChanged = function () {
                var index = $scope.currentPage;
                initagingProxy.initagingPaging(index, $scope.itemsperpage, filstr, function (list) {
                    $scope.list = list[0].results;
                    $scope.totalItems = list[0].count;
                    $scope.agingTotalNum = list[0].count;
                    $scope.agingCalculate(list[0].results.length);
                }, function (error) {
                    alert(error);
                });
            };

            //单页容量变化(One Year Sales)
            $scope.oneyearpagesizechange = function (selectedOneYearLevelId) {
                var oyindex = $scope.oneyearcurrentPage;
                //initagingProxy.initagingPagingOneYearList($scope.oneyearcurrentPage, selectedOneYearLevelId, function (list) {
                //    $scope.oneyearitemsperpage = selectedOneYearLevelId;
                //    $scope.oneYearList = list[0].results;
                //    $scope.oneyeartotalItems = list[0].count;
                //    $scope.totalNum = list[0].count;
                //    $scope.calculate(list[0].results.length);
                //});

                periodProxy.getSubmitWaitInvDet(oyindex, selectedOneYearLevelId, function (json) {
                    $scope.oneyearitemsperpage = selectedOneYearLevelId;
                    $scope.memInv = json.list;
                    $scope.oneyeartotalItems = json.totalItems;
                    $scope.oneYearTotalNum = json.totalItems;
                    $scope.oneYearCalculate(json.list.length);
                }, function (ex) { alert(ex) });
            };

            //翻页(One Year Sales)-new invoce detail
            $scope.oneyearpageChanged = function (oneyearcurrentPage) {
                //initagingProxy.initagingPagingOneYearList(oneyearcurrentPage, $scope.oneyearitemsperpage, function (list) {
                //    $scope.oneYearList = list[0].results;
                //    $scope.oneyeartotalItems = list[0].count;
                //    $scope.oneYearTotalNum = list[0].count;
                //    $scope.oneYearCalculate(list[0].results.length);
                //}, function (error) {
                //    alert(error);
                //    });

                periodProxy.getSubmitWaitInvDet(oneyearcurrentPage, $scope.oneyearitemsperpage, function (json) {             
                    $scope.memInv = json.list;
                    $scope.oneyeartotalItems = json.totalItems;
                    $scope.oneYearTotalNum = json.totalItems;
                    $scope.oneYearCalculate(json.list.length);
                }, function (ex) { alert(ex) });
            };

            //单页容量变化(vat)
            $scope.vatpagesizechange = function (selectedvatLevelId) {
                var oyindex = $scope.vatcurrentPage;               
                periodProxy.getSubmitWaitVat(oyindex, selectedvatLevelId, function (json) {
                    $scope.vatitemsperpage = selectedvatLevelId;
                    $scope.memInv = json.list;
                    $scope.vattotalItems = json.totalItems;
                    $scope.vatTotalNum = json.totalItems;
                    $scope.vatCalculate(json.list.length);
                }, function (ex) { alert(ex) });
            };

            //翻页(vat)
            $scope.vatpageChanged = function (vatcurrentPage) {             
                periodProxy.getSubmitWaitVat(vatcurrentPage, $scope.vatitemsperpage, function (json) {
                    $scope.memInv = json.list;
                    $scope.vattotalItems = json.totalItems;
                    $scope.vatTotalNum = json.totalItems;
                    $scope.vatCalculate(json.list.length);
                }, function (ex) { alert(ex) });
            };

            //单页容量变化(His Area)
            $scope.hisareapagesizechange = function (selectedHisAreaLevelId) {
                //var index = $scope.hisareacurrentPage;
                //periodProxy.hisAreaPeroidPaging(index, selectedHisAreaLevelId, function (list) {
                //    $scope.hisareaitemsperpage = selectedHisAreaLevelId;
                //    $scope.hisAreaList = list[0].results;
                //    $scope.hisareatotalItems = list[0].count;
                //    $scope.hisTotalNum = list[0].count;
                //    $scope.hisCalculate(list[0].results.length);
                //});
                var index = $scope.hisareacurrentPage;
                var btnHisDate = $scope.legalDateHis.Format("yyyyMMdd");
                periodProxy.getFileHisByDate(index, selectedHisAreaLevelId, btnHisDate, function (json) {
                    $scope.hisareaitemsperpage = selectedHisAreaLevelId;
                    $scope.memHis = json.list;
                    $scope.hisareatotalItems = json.totalItems;
                    $scope.hisTotalNum = json.totalItems;
                    $scope.hisCalculate(json.list.length);
                }, function (ex) { alert(ex) }); 
            };

            //翻页(His Area)
            $scope.hisareapageChanged = function () {
                //var index = $scope.hisareacurrentPage;
                //periodProxy.hisAreaPeroidPaging(index, $scope.hisareaitemsperpage, function (list) {
                //    $scope.hisAreaList = list[0].results;
                //    $scope.hisareatotalItems = list[0].count;
                //    $scope.hisTotalNum = list[0].count;
                //    $scope.hisCalculate(list[0].results.length);
                //}, function (error) {
                //    alert(error);
                //    });

                var index = $scope.hisareacurrentPage;
                var btnHisDate = $scope.legalDateHis.Format("yyyyMMdd");
                periodProxy.getFileHisByDate(index, $scope.hisareaitemsperpage, btnHisDate, function (json) {                   
                    $scope.memHis = json.list;
                    $scope.hisareatotalItems = json.totalItems;
                    $scope.hisTotalNum = json.totalItems;
                    $scope.hisCalculate(json.list.length);
                }, function (ex) { alert(ex) }); 
            };

            

            //保存单行数据
            $scope.Save = function (entity) {
                entity.$update(function () {
                    alert("Save Success");
                }, function () {
                    alert("Save Error");
                });

            }

            //submit
            $scope.submit = function (items) {
                if (items == "Aging") {

                    if ($scope.checkIsDuringJobSchedule()) {
                        return;
                    }

                    initagingProxy.query({ custIds: '1' }, function () {
                        alert("Success!");
                        $scope.search();
                        initagingProxy.query({ para1: 1, para2: 2 }, function (legalList) {
                            $scope.legalEntityList = legalList;
                        });
                        $scope.ReSearch();
                    }, function (error) {
                        alert(error);
                        $scope.ReSearch();
                    })
                }else if (items == "Vat") {
                    initagingProxy.query({ custIds: '2' }, function () {
                        alert("Success!");
                        $scope.search();
                        initagingProxy.query({ para1: 1, para2: 2 }, function (legalList) {
                            $scope.legalEntityList = legalList;
                        });
                        $scope.ReSearch();
                    }, function (error) {
                        alert(error);
                        $scope.ReSearch();
                        })
                } else if (items == "InvoiceDetail") {
                    initagingProxy.query({ custIds: '3' }, function () {
                        alert("Success!");
                        $scope.search();
                        initagingProxy.query({ para1: 1, para2: 2 }, function (legalList) {
                            $scope.legalEntityList = legalList;
                        });
                        $scope.ReSearch();
                    }, function (error) {
                        alert(error);
                        $scope.ReSearch();
                    })
                } else if (items == "SAPInvoice") {

                    if ($scope.checkIsDuringJobSchedule()) {
                        return;
                    }

                    initagingProxy.query({ custIds: '6' }, function () {
                        alert("Success!");
                        $scope.search();
                        initagingProxy.query({ para1: 1, para2: 2 }, function (legalList) {
                            $scope.legalEntityList = legalList;
                        });
                        $scope.ReSearch();
                    }, function (error) {
                        alert(error);
                        $scope.ReSearch();
                    })
                }  else {
                    initagingProxy.query({ custIds: '2' }, function () {
                        alert("Success!");

                        //initagingProxy.initagingPagingOneYearList(1, 15, function (list) {
                        //    $scope.oneYearList = list[0].results;
                        //    $scope.oneyeartotalItems = list[0].count;
                        //    $scope.oneYearTotalNum = list[0].count;
                        //    $scope.oneYearCalculate(list[0].results.length);
                        //});

                        //第二页左上角部分
                        //periodProxy.query({ reportType: "OneYearSales" }, function (list) {
                        //    $scope.oneYearSalesValueList = list;
                        //    if ($scope.oneYearSalesValueList.length > 0) {
                        //        document.getElementById("oneYearSalesValue").innerHTML = $scope.oneYearSalesValueList[0].oneYearTimes;
                        //        document.getElementById("reportDateValue").innerHTML = $scope.oneYearSalesValueList[0].reportTime;
                        //    } else {
                        //        document.getElementById("oneYearSalesValue").innerHTML = "";
                        //        document.getElementById("reportDateValue").innerHTML = "";
                        //    }
                        //});

                        initagingProxy.query({ para1: 1, para2: 2 }, function (legalList) {
                            $scope.legalEntityList = legalList;
                        });

                        //periodProxy.hisAreaPeroidPaging(1, 15, function (hisList) {
                        //    $scope.hisAreaList = hisList[0].results;
                        //    $scope.hisareatotalItems = hisList[0].count;
                        //    $scope.hisTotalNum = hisList[0].count;
                        //    $scope.hisCalculate(hisList[0].results.length);
                        //});

                    }, function (error) {
                        alert(error);
                    })
                }
            };


            //加载nggrid数据绑定
            $scope.submitDatasCount = {
                data: 'submitDatasList',
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '130' },
                    { field: 'accTimes', displayName: 'Account Level', width: '131', cellFilter: 'number:2', type: 'number' },
                    { field: 'invTimes', displayName: 'Invoice Level', width: '131', cellFilter: 'number:2', type: 'number' },
                    { field: 'reportTime', displayName: 'Report Date', width: '150' }
                ],
                onRegisterApi: function (gridApi) {
                    //set gridApi on scope
                    $scope.gridApi = gridApi;
                }
            };

            //nggrid数据绑定-back
            //$scope.hisArea = {
            //    multiSelect: true,
            //    enableFullRowSelection: true,
            //    enableSorting: true,
            //    data: 'hisAreaList',
            //    columnDefs: [
            //        { field: 'period', displayName: 'Period', width: '275' },
            //        { name: 'fileType', displayName: 'Report Type', width: '220' },
            //        { field: 'operator', displayName: 'Operator', width: '220' },
            //        { field: 'operatorDate', displayName: 'Time', width: '220', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
            //        {
            //            field: 'downLoadFlg', displayName: 'Consolidated File', width: '163',
            //            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center">' +
            //                          '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity.downLoadFullName)" ng-show="row.entity.downLoadShowFlg == 1" title="DownLoad"></a>' +
            //                          //'<button ng-click="grid.appScope.download(row.entity.downLoadFullName)" ng-show="row.entity.downLoadShowFlg == 1">' +
            //                          //'DownLoad</button>　&nbsp;&nbsp;&nbsp;&nbsp;　' +
            //            //                                      '<img id="imgRefresh" ng-click="grid.appScope.resetSearch();" ng-show="row.entity.periodFlg == 1" src="~/../Content/images/refreshBg.png"></div>'
            //            //                                      '<button class="glyphicon glyphicon-refresh" style="height:24px" ng-click="grid.appScope.resetSearch();" ng-show="row.entity.periodFlg == 1"></button>' + 
            //                          '</div>'
            //        }
            //    ]
            //};
            $scope.membersHis = [
                {
                    legalEntity: 'A-Company',
                    fileType: 'Account Level File',
                    reportName:'2017-10-29-Account-yan.du-1123278.xls',
                    operator: 'yan.du',
                    operatorDate: '2017-10-29 11:23:27',
                    downLoadFlg: '605021059531'
                },
                {
                    legalEntity: 'A-Company',
                    fileType: 'Invoice Level File',
                    reportName: '2017-10-29-Invoice-yan.du-1125436.xls',
                    operator: 'yan.du',
                    operatorDate: '2017-10-29 11:25:43',
                    downLoadFlg: '605021059531'
                },
                {
                    legalEntity: 'A-Company',
                    fileType: 'Invoice Detail File',
                    reportName: '2017-10-29-InvoiceDetail-cindy.zhu-1511332.xls',
                    operator: 'cindy.zhu',
                    operatorDate: '2017-10-29 15:11:33',
                    downLoadFlg: '605021059531'
                },
                {
                    legalEntity: 'A-Company',
                    fileType: 'VAT',
                    reportName: '2017-10-29-VAT-cindy.zhu-1012556.xls',
                    operator: 'cindy.zhu',
                    operatorDate: '2017-10-29 10:12:55',
                    downLoadFlg: '605021059531'
                },
                {
                    legalEntity: 'B-Company',
                    fileType: 'Account Level File',
                    reportName: '2017-10-29-Account-yan.du-1244123.xls',
                    operator: 'yan.du',
                    operatorDate: '2017-10-29 12:44:12',
                    downLoadFlg: '605021059531'
                },
                {
                    legalEntity: 'B-Company',
                    fileType: 'Invoice Level File',
                    reportName: '2017-10-29-Invoice-yan.du-1245223.xls',
                    operator: 'yan.du',
                    operatorDate: '2017-10-29 12:45:22',
                    downLoadFlg: '605021059531'
                }
            ];
            //nggrid数据绑定
            $scope.hisArea = {
                multiSelect: true,
                enableFullRowSelection: true,
                enableSorting: true,
                data: 'memHis',//'membersHis',
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '160' },
                    { field: 'fileType', displayName: 'Report Type', width: '160' },
                    { field: 'reportName', displayName: 'Report Name', width: '280' },
                    { field: 'operator', displayName: 'Operator', width: '160' },
                    { field: 'operatorDate', displayName: 'Upload Time', width: '180', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                    {
                        field: 'downLoadFlg', displayName: 'Download', width: '150',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center">' +
                        '<a class="glyphicon glyphicon-download-alt" ng-click="grid.appScope.download(row.entity.downLoadFlg,row.entity.reportName)" ng-show="true" title="DownLoad"></a>' +                       
                        '</div>'
                    }
                ]
            };

            //download
            $scope.download = function (fullNamePath,filename) {
                if (fullNamePath == null || fullNamePath == "") {
                    alert("There is no need to download the file!");
                } else {    
                    periodProxy.GetFileFromWebApi(fullNamePath, function (data) {
                        if(data.byteLength > 0)
                        {
                            var blob = new Blob([data], {type: "application/vnd.ms-excel"});
                            var objectUrl = URL.createObjectURL(blob);
                            var aForExcel = $("<a><span class='forExcel'>下载excel</span></a>").attr("href",objectUrl);
                            aForExcel.attr("download", filename);
                            $("body").append(aForExcel);
                            $(".forExcel").click();
                            aForExcel.remove();
                        }
                        else
                        {
                            alert("File not find!");
                        }
                    }, function (ex) { alert(ex) });             
                    //window.location = fullNamePath;                  
                }
            };

            //下载最新的文件1.legal entity 2.filetype
            $scope.downloadLegalNew = function (legal, bol, type) {
                if (bol == false)
                {
                    alert("File not upload!");
                }
                else
                {               
                    periodProxy.getLegalNewFile(legal, type, function (result) {
                        if (result != null && result != "")
                        {
                            var res = result.split('^');
                            if (res[0] != null && res[0] != "" && res[1] != null && res[1] != "")
                            {
                                $scope.download(res[1], res[0]);
                            }
                        }
                        
                    }, function (ex) { alert(ex) });
                }               
            };

            $scope.Batch = function () {              
                    periodProxy.batch( function (result) {
                       
                    }, function (ex) { alert(ex) });
                
            };

            //清空过滤条件-已去掉
            $scope.resetSearch = function () {
                //document.getElementById("imgRefresh").src = "~/../Content/images/refresh.png";
                agingDownloadProxy.refreshDownloadFile(function (res) {
                    alert("Refresh Success!");
                    //periodProxy.hisAreaPeroidPaging(1, 15, function (hisList) {
                    //    $scope.hisAreaList = hisList[0].results;
                    //    $scope.hisareatotalItems = hisList[0].count;
                    //    $scope.hisTotalNum = hisList[0].count;
                    //    $scope.hisCalculate(hisList[0].results.length);
                    //});
                }, function (error) {
                    alert(error);
                    //document.getElementById("imgRefresh").src = "~/../Content/images/refreshBg.png";
                });
            }, function (error) {
                alert(error);
            };

            //area show
            $scope.show = function (type) {
                $($.parseHTML("#")).removeClass('ng-hide');
                if (type == "upload") {
                    $("#updAreaShow").hide();
                    $("#updAreaHide").show();
                    $("#tblUploadArea").show();
                    $scope.updcolor = "#FFCC33";
                } else if (type == "submit") {
                    $("#updSubmitComfirmShow").hide();
                    $("#updSubmitComfirmHide").show();
                    $("#tblSubmitComfirm").show();
                    $scope.submitcolor = "#FFCC33";
                } else if (type == "download") {
                    $("#downloadHisShow").hide();
                    $("#downloadHisHide").show();
                    $("#tblDownloadArea").show();
                    $scope.dlhcolor = "#FFCC33";
                } else if (type == 'uploadCust') {
                    $("#updCustAreaShow").hide();
                    $("#updCustAreaHide").show();
                    $("#tblUploadCust").show();
                    $scope.updCustcolor = "#FFCC33";
                } else if (type == 'uploadVar') {
                    $("#updVarAreaShow").hide();
                    $("#updVarAreaHide").show();
                    $("#tblUploadVar").show();
                    $scope.updVarcolor = "#FFCC33";
                } else if (type == 'uploadCreditHold') {
                    $("#updCreditHoldAreaShow").hide();
                    $("#updCreditHoldAreaHide").show();
                    $("#tblUploadCreditHold").show();
                    $scope.updCHcolor = "#FFCC33";
                } else if (type == 'uploadCurrencyAmount') {
                    $("#updCurrencyAmountAreaShow").hide();
                    $("#updCurrencyAmountAreaHide").show();
                    $("#tblUploadCurrencyAmount").show();
                    $scope.updCAcolor = "#FFCC33";
                } else if (type == 'uploadTWCurrencyAmount') {
                    $("#updTWCurrencyAmountAreaShow").hide();
                    $("#updTWCurrencyAmountAreaHide").show();
                    $("#tblUploadTWCurrencyAmount").show();
                    $scope.updCATcolor = "#FFCC33";
                } else if (type == 'uploadConsignmentNumber') {
                    $("#updConsignmentNumberAreaShow").hide();
                    $("#updConsignmentNumberAreaHide").show();
                    $("#tblUploadConsignmentNumber").show();
                    $scope.updCATcolor = "#FFCC33";
                }
            }
            //area hide
            $scope.hide = function (type) {
                $($.parseHTML("#")).addClass('ng-hide');
                if (type == "upload") {
                    $("#updAreaShow").show();
                    $("#updAreaHide").hide();
                    $("#tblUploadArea").hide();
                    $scope.updcolor = "#0072c6";
                } else if (type == "submit") {
                    $("#updSubmitComfirmShow").show();
                    $("#updSubmitComfirmHide").hide();
                    $("#tblSubmitComfirm").hide();
                    $scope.submitcolor = "#0072c6";
                } else if (type == "download") {
                    $("#downloadHisShow").show();
                    $("#downloadHisHide").hide();
                    $("#tblDownloadArea").hide();
                    $scope.dlhcolor = "#0072c6";
                } else if (type == 'uploadCust')
                {
                    $("#updCustAreaShow").show();
                    $("#updCustAreaHide").hide();
                    $("#tblUploadCust").hide();
                    $scope.updCustcolor = "#0072c6";
                } else if (type == 'uploadVar') {
                    $("#updVarAreaShow").show();
                    $("#updVarAreaHide").hide();
                    $("#tblUploadVar").hide();
                    $scope.updVarcolor = "#0072c6";
                } else if (type == 'uploadCreditHold') {
                    $("#updCreditHoldAreaShow").show();
                    $("#updCreditHoldAreaHide").hide();
                    $("#tblUploadCreditHold").hide();
                    $scope.updCHcolor = "#0072c6";

                } else if (type == 'uploadCurrencyAmount') {
                    $("#updCurrencyAmountAreaShow").show();
                    $("#updCurrencyAmountAreaHide").hide();
                    $("#tblUploadCurrencyAmount").hide();
                    $scope.updCAcolor = "#0072c6";

                } else if (type == 'uploadTWCurrencyAmount') {
                    $("#updTWCurrencyAmountAreaShow").show();
                    $("#updTWCurrencyAmountAreaHide").hide();
                    $("#tblUploadTWCurrencyAmount").hide();
                    $scope.updCATcolor = "#0072c6";
                } else if (type == 'uploadConsignmentNumber') {
                    $("#updConsignmentNumberAreaShow").show();
                    $("#updConsignmentNumberAreaHide").hide();
                    $("#tblUploadConsignmentNumber").hide();
                    $scope.updCATcolor = "#0072c6";
                }
            }

            //-----to check if during job schedule, add on 05-Jul-2019----
            $scope.checkIsDuringJobSchedule = function()
            {
            
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
            //----------

        } ])