angular.module('app.unknownDetail', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/unknownDetail', {
                templateUrl: 'app/cashapplication/unknowAdvisor/unknownDetail.tpl.html',
                controller: 'unknownDetailCtrl'
            });
    }])


    //*****************************************header***************************s
    .controller('unknownDetailCtrl',
        ['$scope', '$filter', '$interval', 'APPSETTING', 'caHisDataProxy', 'mailProxy', 'modalService', '$location', '$sce', '$q',
            function ($scope, $filter, $interval, APPSETTING, caHisDataProxy, mailProxy, modalService, $location, $sce, $q) {

                $scope.goback = function () {
                    $location.path("/ca/index");
                }

                var result = "";
                $scope.type = 1;
                $scope.alertMessage = "";
                var entity = JSON.parse(localStorage.getItem("entity"));
                $scope.selected = entity[0];

                $scope.CaBankCustomerDto = {};

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                // bank grid start
                $scope.bankStartIndex = 0;
                $scope.bankSelectedLevel = 20;  //下拉单页容量初始化
                $scope.bankItemsperpage = 20;
                $scope.bankCurrentPage = 1; //当前页
                $scope.bankMaxSize = 10; //分页显示的最大页  

                $scope.isCheckAll = false;
                $scope.bankGidOptions = {
                    multiSelect: false,
                    enableFullRowSelection: true,
                    enableFiltering: true,
                    noUnselect: false,
                    enableCellEdit: false,
                    enableSelectAll: true,
                    enableRowSelection: true,
                    data: 'bankList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'id', displayName: 'id', width: '120', visible: false },
                        {
                            field: 'matcH_STATUS_NAME', displayName: 'Match Status', width: '120'
                        },
                        { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '100' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'currency', displayName: 'Currency', width: '80', cellClass: 'center' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        {
                            field: 'forwarD_NUM', displayName: 'Payer Number', width: '80', enableCellEdit: false,
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewAgentCustomer(row.entity)">{{row.entity.forwarD_NUM ? row.entity.forwarD_NUM : "-" }}</a></div>'
                        },
                        { field: 'forwarD_NAME', displayName: 'Payer Name', width: '80'  },
                        {
                            field: 'customeR_NUM', displayName: 'Customer Number', width: '80', enableCellEdit: false,
                            cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewPaymentCustomer(row.entity)">{{row.entity.customeR_NUM ? row.entity.customeR_NUM : (row.entity.countIdentify > 0 ? "*" : "-") }}</a></div>'
                        },
                        { field: 'customeR_NAME', displayName: 'Customer Number', width: '140' },
                        //{ field: 'siteUseId', displayName: 'Site Use ID', width: '120' },
                        //{ field: 'reF1', displayName: 'Description', width: '120', enableCellEdit: false },
                        //{ field: 'referencE2', displayName: 'Reference 2', width: '120', enableCellEdit: false },
                        //{ field: 'referencE3', displayName: 'Reference 3', width: '120', enableCellEdit: false },
                        { field: 'bankChargeFrom', displayName: 'Charge From', width: '100', cellClass: 'right' },
                        { field: 'bankChargeTo', displayName: 'Charge To', width: '100', cellClass: 'right' },
                        //{
                        //    field: 'isFixedBankCharge', displayName: 'Charge Type', width: '120', cellFilter: 'mapChargeType'
                        //},
                        { field: 'description', displayName: 'Description', width: '300'},
                        //{ field: 'updatE_DATE', displayName: 'Last Modified Time', width: '120', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                        //{ field: 'creatE_DATE', displayName: 'Create Time', width: '120', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                        {
                            field: 'needSendMail', displayName: 'Is Send', width: '40',  pinnedRight: true,
                            headerCellTemplate: '<div style="text-align:center">Is Send</div><div style="text-align:center"><input type="checkbox"   ng-checked="isCheckAll" ng-model="grid.appScope.isCheckAll" ng-click="grid.appScope.sendAddAll()" /></div>',
                            cellTemplate: '<div style="text-align:center"> <input type="checkbox" ng-checked="row.entity.needSendMail" ng-model="row.entity.needSendMail" /></div> '
                        },
                        { field: 'kong', displayName: '', width: '10', pinnedRight: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false}
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.bankGridApi = gridApi;

                        //$scope.bankList = $location.$$search.entity;
                        $scope.bankList = entity;

                        $scope.bankGridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                            //行选中事件
                            $scope.selected = row.entity;
                            $scope.arList = [];
                            $scope.amtTotal = '';
                            $scope.pageChanged();
                        });
                    }
                };

                
                // bank grid end

                // customer grid start
                $scope.startIndex = 0;
                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1; //当前页
                $scope.maxSize = 10; //分页显示的最大页  

                $scope.headIsChecked = false;

                $scope.gridOptions = {
                    multiSelect: false,
                    enableFullRowSelection: true,
                    enableFiltering: false,
                    noUnselect: false,
                    enableSorting: true,
                    showGridFooter: true,
                    data: 'list',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        
                        { field: 'customerNum', displayName: 'Customer Number', width: '18%', cellClass: 'center' },
                        { field: 'customerName', displayName: 'Customer Name', width: '41%' },                       
                        {
                            field: 'mailDate', displayName: 'Last Mail Date', width: '17%', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center',
                            cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"  style="text-align:center;margin-top:5px">' +
                                '<a ng-click="grid.appScope.showmail(row.entity.mailId)" ng-show="true" title="Show Mail">{{row.entity.mailDate | date:\'yyyy-MM-dd\'}}</a>' +
                                '</div>'
                        },{
                            field: 'needSendMail', displayName: 'Is Send', width: '44', enableFiltering: false, enableColumnMenu: false, cellTooltip: true, pinnedRight: true,
                            headerCellTemplate: '<div style="text-align:center">Is Send</div><div style="text-align:center"><input type="checkbox"  ng-click="grid.appScope.checkAll()" ng-model="grid.appScope.headIsChecked" /></div>',
                            cellTemplate: '<div style="text-align:center"> <input type="checkbox" ng-checked="row.entity.needSendMail" ng-model="row.entity.needSendMail" ng-click="grid.appScope.check(row.entity)" /></div> '
                        },
                        { field: 'kong', displayName: '', width: '10', pinnedRight: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false}

                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;

                        $scope.gridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                            //行选中事件
                            $scope.arPageChanged(row.entity,1);
                        });
                    }
                };

                //Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {


                    caHisDataProxy.getPaymentCustomerDataDetails($scope.currentPage, selectedLevelId, $scope.selected, $scope.type, function (result) {


                        $scope.itemsperpage = selectedLevelId;


                        $scope.totalItems = result.listCount;


                        $scope.list = result.list;


                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    })
                };

                //Detail翻页
                $scope.pageChanged = function () {


                    caHisDataProxy.getPaymentCustomerDataDetails($scope.currentPage, $scope.itemsperpage, $scope.selected, $scope.type, function (result) {


                        $scope.totalItems = result.listCount;


                        $scope.list = result.list;


                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.pageChanged();

                $scope.arGridOptions = {
                    multiSelect: false,
                    enableFullRowSelection: true,
                    enableFiltering: true,
                    noUnselect: false,
                    showGridFooter: true,
                    data: 'arList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        //{ field: 'menuregion', displayName: 'Menuregion', width: '120' },
                        //{ field: 'legalEntity', displayName: 'Legal Entity', width: '120' },
                        //{ field: 'customerNum', displayName: 'Customer Number', width: '120' },
                        { field: 'siteUseId', displayName: 'SiteUseID', width: '120', cellClass: 'center' },
                        { field: 'invoiceNum', displayName: 'Invoice Number', width: '120', cellClass: 'center' },
                        { field: 'invoiceDate', displayName: 'Invoice Date', width: '110', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false  },
                        { field: 'dueDate', displayName: 'Due Date', width: '110', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false  },
                        //{ field: 'customerCurrency', displayName: 'Customer Currency', width: '120' },
                        { field: 'invCurrency', displayName: 'Currency', width: '100', cellClass: 'center' },
                        { field: 'amt', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'localAmt', displayName: 'FXRate Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' }

                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.arGridApi = gridApi;

                    }
                };


                //Detail翻页
                $scope.arPageChanged = function (entity,type) {
                    caHisDataProxy.getArHisDataDetails(entity, type, function (result) {
                        $scope.arList = result.list;
                        $scope.amtTotal = result.amtTotal;
                    }, function (error) {
                        alert(error);
                    });
                };

                
                $scope.checkAll = function () {
                    var data = {};
                    data.needSendMail = !$scope.headIsChecked;
                    data.bankStatementId = $scope.selected.id;
                    
                    caHisDataProxy.changeNeedSendMailAll(data, function (result) {
                        $scope.pageChanged();
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.check = function (data) {

                    data.needSendMail = !data.needSendMail;
                    caHisDataProxy.changeNeedSendMail(data, function (result) {
                        $scope.pageChanged();
                    }, function (error) {
                        alert(error);
                    });
                };


                // ar grid end



                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

                $scope.submit = function () {
                    var result = [];
                    result.push('submit');

                    var row = $scope.gridApi.selection.getSelectedRows()[0];


                    entity.customeR_NUM = row.customerNum;


                    entity.customeR_NAME = row.customerName;

                    entity.matcH_STATUS = 2;

                    caHisDataProxy.bankRowSave(entity, function () {
                        alert("保存成功!");
                        $uibModalInstance.close(result);
                    });
                };


                $scope.sendMails = function () {
                    var sendBankList = [];
                    for (var i in $scope.bankList) {
                        if ($scope.bankList[i].needSendMail) {
                            sendBankList.push($scope.bankList[i]);
                        }
                    }

                    if (sendBankList.length == 0) {
                        alert("Please select one bankstatement.");
                        return;
                    } 

                    for (var i in sendBankList) {
                        $scope.CaBankCustomerDto.bank = sendBankList[i];
                        caHisDataProxy.SendMails($scope.CaBankCustomerDto, function (result) {
                        }, function (error) {
                            alert(error);
                        });

                        //caHisDataProxy.getPaymentCustomer(sendBankList[i].id, function (result) {
                        //        if (result) {
                        //            for (var j in result) {
                        //                if (result[j].needSendMail) {
                        //                    $scope.CaBankCustomerDto.bank = sendBankList[i];
                        //                    $scope.CaBankCustomerDto.customer = result[j];
                        //                    //console.log($scope.CaBankCustomerDto.customer.id);
                        //                    //$scope.sendMailDatiles($scope.CaBankCustomerDto);
                        //                    caHisDataProxy.SendMails($scope.CaBankCustomerDto);
                        //                }
                        //            }
                        //        } 
                        //    }, function (error) {
                        //        alert(error);
                        //    });
                       
                    }

                    alert("send success");
                };

                $scope.sendAddAll = function () {
                    if ($scope.isCheckAll) {
                        for (var i in $scope.bankList) {
                            $scope.bankList[i].needSendMail = false;
                        }
                    } else {
                        for (var i in $scope.bankList) {
                            $scope.bankList[i].needSendMail = true;
                        }
                    }

                };

                $scope.sendMailDatiles = function (data) {
                    caHisDataProxy.SendMails(data, function (result) {
                        
                    }, function (error) {
                        alert(error);
                    });
                }

                // 查看Agent Customer列表
                $scope.viewAgentCustomer = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/agentcustomer/agentcustomer.tpl.html?id=1',
                        controller: 'agentCustomerCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_xlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        //if (result == "submit") {
                        //    $scope.init();
                        //}
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.viewPaymentCustomer = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/paymentcustomer/paymentcustomer.tpl.html?id=2',
                        controller: 'paymentCustomerCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_xlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        //if (result == "submit") {
                        //    $scope.init();
                        //}
                    }, function (err) {
                        alert(err);
                    });
                };



                $scope.toUploadPage = function () {
                    $location.path("/ca/upload");
                }

                $scope.toTaskListPage = function () {
                    $location.path("/ca/reconTask");
                }

                $scope.actiontask = function () {
                    $location.path("/ca/actiontask");
                }

                $scope.showmail = function (mailid) {
                    mailProxy.queryObject({ messageId: mailid }, function (mailInstance) {
                        //mailType
                        mailInstance["title"] = "Mail View";
                        mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);
                        $scope.invoiceNums = "";

                        var modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return ""; },
                                siteuseId: function () { return ""; },
                                invoicenums: function () { return ""; },
                                instance: function () { return mailInstance },
                                mType: function () { return "" },
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
                    });
                };

                $scope.unknownMatchExport = function () {
                    caHisDataProxy.doExportUnknownDataByIds(entity, function (result) {
                        window.location = APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + result;
                    }, function (err) {
                            alert(err);
                    });
                }

            }])