angular.module('app.hisdata',[])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/index', {
                templateUrl: 'app/cashapplication/hisdata/hisdata.tpl.html',
                controller: 'hisDataCtrl',
                resolve: {
                    //首次加载第一页
                    statusList: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("088");
                    }],bsTypeList: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("085");
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('hisDataCtrl',
        ['$scope', '$filter', '$interval', 'APPSETTING', 'caHisDataProxy', 'caCommonProxy', 'paymentDetailProxy', 'modalService', '$location', 'statusList','bsTypeList',
            function ($scope, $filter, $interval, APPSETTING, caHisDataProxy, caCommonProxy, paymentDetailProxy, modalService, $location, statusList, bsTypeList) {
                $scope.$parent.helloAngular = "OTC - Feedback Report";
                

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                $scope.yesNoList = [
                    { value: '', name: 'All' },
                    { value: '1', name: 'Yes' },
                    { value: '0', name: 'No' },
                ];

                $scope.active = 1;
                $scope.bsTypeList = [{ "detailValue": '', "detailName": '' }];
                for (i = 0; i < bsTypeList.length; i++) {
                    $scope.bsTypeList.push(bsTypeList[i]);
                }

                $scope.rebuildBankHisgrid = true;
                $scope.rebuildpmtgrid = true;
                $scope.rebuildptpgrid = true;

                $scope.tab1_transNumber = '';
                $scope.tab1_legalEntity = '';
                $scope.tab1_transcurrency = '';
                $scope.tab1_transCustomer = '';
                $scope.tab1_transaForward = '';
                $scope.tab1_bsType = '';

                //statusCheckBox
                $scope.selectedList = ["-1","0","2","4"];

                // bank grid start
                $scope.bankStartIndex = 0;
                $scope.bankSelectedLevel = 20;  //下拉单页容量初始化
                $scope.bankItemsperpage = 20;
                $scope.bankCurrentPage = 1; //当前页
                $scope.bankMaxSize = 10; //分页显示的最大页  


                $scope.statusselect = '';

                $scope.createbankHisDataGrid = function () {
                    $scope.bankHisDataList = {

                        showGridFooter: true,
                        enableFullRowSelection: false, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                        //enableRowHeaderSelection: true, //是否显示选中checkbox框 ,default为true
                        enableSelectAll: true, // 选择所有checkbox是否可用，default为true;
                        enableSelectionBatchEvent: true, //default为true
                        multiSelect: true,// 是否可以选择多个,默认为true;
                        noUnselect: false,//default为false,选中后是否可以取消选中
                        enableFiltering: true,
                        data: 'bankList',
                        columnDefs: [
                            { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'operation', displayName: '', width: '200', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:left;padding-left:10px;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.editBank(row.entity)">Edit</a> | <a href="javascript: void (0);" ng-click="grid.appScope.deleteBank(row.entity)">Delete</a>| <a href="javascript: void (0);" ng-click="grid.appScope.revert(row.entity)">Revert</a>| <a href="javascript: void (0);" ng-click="grid.appScope.deletePmtBsByBsId(row.entity)" ng-if="row.entity.matcH_STATUS<4">delete PMT</a></span>' },
                            {
                                field: 'hasfile', name: 'fileFlag', displayName: 'Attachment', width: '30', enableFiltering: false,
                                cellTemplate: '<a class="glyphicon glyphicon-paperclip" style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasfile==0" ng-click="grid.appScope.openBsFiles(row.entity)"></a>'
                                    + '<a class="glyphicon glyphicon-download-alt" style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasfile>0" ng-click="grid.appScope.openBsFiles(row.entity)"></a>'
                            },
                            { field: 'islocked', name: 'Lock', displayName: 'Lock', width: '30', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.islocked"><img src="~/../Content/images/HoldCustomer.png"></span>' },
                            { field: 'haspmtdetail', name: 'PMT', displayName: 'PMT', width: '30', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.haspmtdetail==2" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.haspmtdetail==1" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived_noar.png"></span>' },
                            { field: 'applY_STATUS', name: 'Post', displayName: 'Post', width: '30', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.applY_STATUS==\'1\'"><img src="~/../Content/images/Dunning_Reminder_actived.png" style="width:20px"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.applY_STATUS==\'2\'"><img src="~/../Content/images/Execute.png"></span>' },
                            { field: 'postMailFlag', name: 'Post Mail', displayName: 'Post Mail', width: '30', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.postMailFlag==\'1\'"><img src="~/../Content/images/My_mailbox_default.png" style="width:20px;cursor:pointer" ng-click="grid.appScope.openPostMailHis(row.entity.id)"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if= "row.entity.postMailFlag==\'2\'" > <img src="~/../Content/images/My_mailbox_actived.png" style="width:20px;cursor:pointer" ng-click="grid.appScope.openPostMailHis(row.entity.id)"></span>' },
                            { field: 'clearinG_STATUS', name: 'Clear', displayName: 'Clear', width: '30', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.clearinG_STATUS==\'1\'"><img src="~/../Content/images/Dunning_Reminder_actived.png" style="width:20px"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.clearinG_STATUS==\'2\'"><img src="~/../Content/images/Execute.png"></span>' },
                            { field: 'clearMailFlag', name: 'Clear Mail', displayName: 'Clear Mail', width: '30', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.clearMailFlag==\'1\'"><img src="~/../Content/images/My_mailbox_default.png" style="width:20px;cursor:pointer" ng-click="grid.appScope.openClearMailHis(row.entity.id)"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if= "row.entity.clearMailFlag==\'2\'" > <img src="~/../Content/images/My_mailbox_actived.png" style="width:20px;cursor:pointer" ng-click="grid.appScope.openClearMailHis(row.entity.id)"></span>' },
                            { field: 'id', displayName: 'id', width: '120', visible: false },
                            {
                                field: 'matcH_STATUS_NAME', displayName: 'Status', width: '90',
                                cellTemplate: '<div style="margin-top:6px;color:{{row.entity.statuscolor}}">{{row.entity.matcH_STATUS_NAME }}</div>'
                            },
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '50', cellClass: 'center', enableCellEdit: false },
                            { field: 'bstypename', displayName: 'Type', width: '80', enableCellEdit: false },
                            { field: 'transactioN_NUMBER', displayName: 'Transaction Inc', width: '100', enableCellEdit: false },
                            { field: 'currency', displayName: 'Currency', width: '50', cellClass: 'center', enableCellEdit: false  },
                            { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '90', enableCellEdit: false , cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'currenT_AMOUNT', displayName: 'Current Amount', width: '90', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'unClear_Amount', displayName: 'UnClear Amount', width: '90', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'valuE_DATE', displayName: 'Value Date', width: '80', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false  },
                            {
                                field: 'forwarD_NUM', displayName: 'Payer Number', width: '70', enableCellEdit: false,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewAgentCustomer(row.entity)">{{row.entity.forwarD_NUM ? row.entity.forwarD_NUM : "-" }}</a></div>',
                                cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                    if (row.entity.countCustomer == 0) {
                                        return 'bg_red'//自定义样式
                                    }
                                }
                            },
                            { field: 'forwarD_NAME', displayName: 'Payer Name', width: '140', enableCellEdit: false },
                            {
                                field: 'customeR_NUM', displayName: 'Customer Number', width: '70', enableCellEdit: false,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;"><a style="line-height:28px" ng-click="grid.appScope.viewPaymentCustomer(row.entity)">{{row.entity.customeR_NUM ? row.entity.customeR_NUM : (row.entity.countIdentify > 0 ? "*" : "-") }}</a></div>'
                            },
                            { field: 'customeR_NAME', displayName: 'Customer Name', width: '140', enableCellEdit: false  },
                            {
                                field: 'description', displayName: 'Description', width: '120', enableCellEdit: false, cellTooltip:
                                    function (row, col) {
                                        return row.entity.description;
                                    }
                            },
                            { field: 'bankChargeFrom', displayName: 'Charge From', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                            { field: 'bankChargeTo', displayName: 'Charge To', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                            { field: 'reF1', displayName: 'Ref1', width: '150', enableCellEdit: false },
                            {
                                field: 'comments', displayName: 'Comments', width: '120',
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;">'
                                    + '<a style="line-height:28px" ng-click="grid.appScope.multipleResult(row.entity)" ng-if="row.entity.comments==\'Multiple Possibilities\' && row.entity.matcH_STATUS==2">{{row.entity.comments}}</a>'
                                    + '<span style="line-height:28px" ng-if="row.entity.comments!=\'Multiple Possibilities\' || row.entity.matcH_STATUS!=2">{{row.entity.comments}}</span>'
                                    + '</div>'
                            },
                            {
                                field: 'groupNo', displayName: 'PMT GroupNo', width: '70', enableCellEdit: false,
                                cellTemplate: '<div style="height:30px;vertical-align:middle;text-align:center;" ng-if="row.entity.matcH_STATUS<4"><a style="line-height:28px" ng-click="grid.appScope.viewPmtDetail(row.entity)">{{row.entity.groupNo ? row.entity.groupNo : "-" }}</a></div>' + 
                                              '<div style="height:30px;vertical-align:middle;text-align:center;" ng-if="row.entity.matcH_STATUS>3"><span style="line-height:28px">{{row.entity.groupNo ? row.entity.groupNo : "-" }}</span></div>'
                            },
                            { field: 'pmtFileName', displayName: 'PMT File Name', width: '140', enableCellEdit: false},
                            { field: 'recoN_TIME', displayName: 'Recon Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'identifY_TIME', displayName: 'Identify Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'advisoR_TIME', displayName: 'Advisor Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'applY_TIME', displayName: 'Apply Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'clearinG_TIME', displayName: 'Clearing Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'creatE_USER', displayName: 'Create User', width: '100', enableCellEdit: false },
                            { field: 'creatE_DATE', displayName: 'Create Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'updatE_DATE', displayName: 'Last Modified Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.bankGridApi = gridApi;

                            $scope.bankPageChanged();
                        }
                    };
                };

                //Detail单页容量变化
                $scope.bankPageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getBankHisDataDetails($scope.statusselect, $scope.tab1_legalEntity, $scope.tab1_transNumber, $scope.tab1_transcurrency, $scope.tab1_transamount, $scope.tab1_transCustomer, $scope.tab1_transaForward, $scope.tab1_valueDateF, $scope.tab1_valueDateT, $scope.tab1_createDateF, $scope.tab1_createDateT, $scope.tab1_bsType, $scope.bankCurrentPage, selectedLevelId, function (result) {
                        $scope.bankItemsperpage = selectedLevelId;
                        $scope.bankTotalItems = result.count;
                        $scope.bankList = result.dataRows;
                        $scope.bankStartIndex = ($scope.currentPage - 1) * $scope.bankItemsperpage;

                        $scope.calculate($scope.bankCurrentPage, $scope.bankItemsperpage, result.dataRows.length);
                    }, function(error){
                        alert(error);
                    });
                };

                //Detail翻页
                $scope.bankPageChanged = function () {
                    caHisDataProxy.getBankHisDataDetails($scope.statusselect, $scope.tab1_legalEntity, $scope.tab1_transNumber, $scope.tab1_transcurrency, $scope.tab1_transamount, $scope.tab1_transCustomer, $scope.tab1_transaForward, $scope.tab1_valueDateF, $scope.tab1_valueDateT, $scope.tab1_createDateF, $scope.tab1_createDateT, $scope.tab1_bsType,$scope.bankCurrentPage, $scope.bankItemsperpage, function (result) {
                        $scope.bankTotalItems = result.count;
                        $scope.bankList = result.dataRows;
                        $scope.bankStartIndex = ($scope.bankCurrentPage - 1) * $scope.bankItemsperpage;
                        $scope.bankGridApi.selection.clearSelectedRows();

                        $scope.calculate($scope.bankCurrentPage, $scope.bankItemsperpage, result.dataRows.length);
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.calculate = function (currentPage, itemsperpage, count) {
                    if (count == 0) {
                        $scope.fromItem = 0;
                    } else {
                        $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
                    }
                    $scope.toItem = (currentPage - 1) * itemsperpage + count;
                }

                $scope.calculate_his = function (currentPage, itemsperpage, count) {
                    if (count == 0) {
                        $scope.fromItem_his = 0;
                    } else {
                        $scope.fromItem_his = (currentPage - 1) * itemsperpage + 1;
                    }
                    $scope.toItem_his = (currentPage - 1) * itemsperpage + count;
                }

                $scope.calculate_pmt = function (currentPage, itemsperpage, count) {
                    if (count == 0) {
                        $scope.fromItem_pmt = 0;
                    } else {
                        $scope.fromItem_pmt = (currentPage - 1) * itemsperpage + 1;
                    }
                    $scope.toItem_pmt = (currentPage - 1) * itemsperpage + count;
                }

                $scope.openPostMailHis = function (id) {
                    $scope.openMailHis(id, '006');
                }

                $scope.openClearMailHis = function (id) {
                    $scope.openMailHis(id,'008');
                }

                $scope.openMailHis = function (id, alerttype) {
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/camailalert.tpl.html',
                        controller: 'camailalertCtrl',
                        size: 'customSize',
                        resolve: {
                            id: function () {
                                return id;
                            },
                            alerttype: function () {
                                return alerttype;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function () {
                        $scope.bankPageChanged();
                    });

                }

                $scope.openBsFiles = function (bank) {
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/bankFile/bankFile-edit.tpl.html?id=1',
                        controller: 'bankFileCtrl',
                        size: 'customSize',
                        resolve: {
                            bank: function () {
                                return bank;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function () {
                        $scope.bankPageChanged();
                    });
                }

                // bank grid end

                // bank search start

                //查询条件展开、合上
                var bankShow = false;
                $scope.bankOpenFilter = function () {
                    bankShow = !bankShow;
                    if (bankShow) {
                        $("#bankDataSearch").show();
                        $("#mydropdownlist1").select2({
                            placeholder: "Select a State",
                            allowClear: true
                        });
                        $("#mydropdownlist1").on("change", function (e) {
                            var s = $("#mydropdownlist1").select2("val");
                            if (s) {
                                $scope.statusselect = $("#mydropdownlist1").select2("val").join(',');
                            } else {
                                $scope.statusselect = '';
                            }
                        });
                    } else {
                        $("#bankDataSearch").hide();
                    }
                };

                $scope.addBank = function () {
                    $scope.openBankEdit({});
                };

                $scope.editBank = function (entity) {
                    if (entity.matcH_STATUS == "9") {
                        alert("The Bank Statement has been closed and cannot be edited!");
                        return false;
                    }
                    //console.log("------------");
                    //console.log(entity);
                    var bank = {
                        ID: entity.id,
                        TRANSACTION_NUMBER: entity.transactioN_NUMBER,
                        TRANSACTION_AMOUNT: entity.transactioN_AMOUNT,
                        CURRENT_AMOUNT: entity.currenT_AMOUNT,
                        CURRENCY: entity.currency,
                        VALUE_DATE: entity.valuE_DATE,
                        ReceiptsMethod: entity.receiptsMethod,
                        BankAccountNumber: entity.bankAccountNumber,
                        MATCH_STATUS: entity.matcH_STATUS,
                        LegalEntity: entity.legalEntity,
                        BSTYPE: entity.bstype,
                        BankChargeFrom: entity.bankChargeFrom,
                        BankChargeTo: entity.bankChargeTo,
                        ISHISTORY: entity.ishistory,
                        Description: entity.description,
                        Comments: entity.comments,
                        PMTDetailFileName: entity.pmtDetailFileName,
                        PMTReceiveDate: entity.pmtReceiveDate
                    };
                    $scope.openBankEdit(bank);
                };

                $scope.openBankEdit = function (bank) {
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/bankstatement/bankStatement-edit.tpl.html?id=1',
                        controller: 'bankStatementCtrl',
                        size: 'customSize',
                        resolve: {
                            bank: function () {
                                return bank;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mmdlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function () {
                        $scope.bankPageChanged();
                        $scope.bankHisPageChanged();
                    });
                }

                $scope.deleteBank = function (entity) {
                    if (entity.matcH_STATUS == "9") {
                        alert("The Bank Statement has been closed and cannot be deleted!");
                        return false;
                    }
                    if (entity.applY_STATUS == "1" || entity.applY_STATUS == "2")
                    {
                        alert("Bank has been posted and cannot be deleted!");
                        return false;
                    }
                    if (entity.clearinG_STATUS == "1" || entity.clearinG_STATUS == "2")
                    {
                        alert("Bank has been posted and write-off, cannot be deleted!");
                        return false;
                    }
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            caHisDataProxy.deleteBankStatement(entity, function (result) {
                                alert("Delete Success.");
                                $scope.bankPageChanged();
                                $scope.bankHisPageChanged();
                            }, function (err) {
                                alert(err);
                            });
                        }
                    });
                    
                };

                $scope.revert = function (entity) {
                    if (confirm("are you sure to revert!")) {
                        caHisDataProxy.revertBankStatement(entity, function (result) {
                            alert(result);
                            $scope.bankPageChanged();
                        }, function (err) {
                            alert(err);
                        });
                    }
                    
                }

                $scope.deletePmtBsByBsId = function (entity) {
                    if (confirm("Are you sure to delete from pmt?")) {
                        caHisDataProxy.deletePmtBsByBsId(entity.id, function (result) {
                            alert("Operation success!");
                            $scope.bankPageChanged();
                        }, function (err) {
                            alert(err);
                        });
                    }
                    
                }

                $scope.addPMT = function () {
                    $scope.editPMTDetail("");
                };

                $scope.editPMT = function (row) {
                    if (row.isClosed == true) {
                        alert("The payment has been closed and cannot be edited!");
                        return false;
                    }
                    var id = row.id;
                    $scope.editPMTDetail(id);
                };

                $scope.editPMTDetail = function (id) {
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/paymentDetail/paymentDetail-edit.tpl.html?id=1',
                        controller: 'paymentCtrl',
                        size: 'customSize',
                        resolve: {
                            pmtID: function () {
                                return id;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function () {
                        $scope.PmtDetailPageChanged();
                    });
                };

                // bank search end

                // bank operation start

                // 查看Agent Customer列表
                $scope.viewAgentCustomer = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/agentcustomer/agentcustomer.tpl.html?id=4',
                        controller: 'agentCustomerCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
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
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.viewPmtDetail = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/pmtDetail/pmtDetail.tpl.html?id=2',
                        controller: 'pmtDetailCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.batchChangeINC = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/batchChangeINC.tpl.html',
                        controller: 'batchChangeINCCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        $scope.bankPageChanged();
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.batchManualClose = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/batchManualClose.tpl.html',
                        controller: 'batchManualCloseCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        $scope.bankPageChanged();
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.multipleResult = function (row) {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/multipleResult/multipleResult.tpl.html',
                        controller: 'multipleResultCtrl',
                        size: 'customSize',
                        resolve: {
                            entity: function () {
                                return row;
                            }
                        },
                        windowClass: 'modalDialog modalDialog_width_mxlg'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "submit") {
                            $scope.init();
                        }
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.init = function () {

                    $scope.statusselect = $scope.selected.join(',');
                    $scope.bankPageChanged();
                };

                

                $scope.resetSearch = function () {
                    $scope.tab1_transNumber = '';
                    $scope.tab1_legalEntity = '';
                    $scope.tab1_transamount = null;
                    $scope.tab1_transcurrency = '';
                    $scope.tab1_transCustomer = '';
                    $scope.tab1_transaForward = '';
                    $scope.tab1_bsType = '';
                    $scope.init();
                };

                //行编辑保存
                $scope.bankRowSave = function (row) {
                    caHisDataProxy.bankRowSave(row, function (result) {
                        $scope.bankPageChanged();
                    }, function (error) {
                        alert(error);
                    });
                };
                // bank operation end

                $scope.PostClear = function (type) {
                    //判断是否选中了记录
                    $scope.bankIds = [];
                    $scope.bankIds.push(type);
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caCommonProxy.PostClear($scope.bankIds, function (fileId) {
                            //刷新数据
                            $scope.bankPageChanged();
                            var files;
                            if (fileId.indexOf("&") > -1) {
                                var msg = fileId.split("&");
                                if (msg[0] == "" || msg[0] == "undefined") {
                                    if (type === '1') {
                                        alert('No data need to post.');
                                    } else {
                                        alert('No data need to clear.');
                                    }
                                    return false;
                                }
                                alert(msg[1]);
                                files = msg[0].split(";");
                            } else {
                                files = fileId.split(";");
                            }
                            // return fieid;

                            files.forEach((item, index, array) => {
                                if (item) {
                                    //下载文件(可能是多个)
                                    window.open(APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + item, "_blank");
                                } else {
                                    if (type === '1') {
                                        alert('No data need to post.');
                                    } else {
                                        alert('No data need to clear.');
                                    }
                                }
                            });
                        }, function (err) {
                        });
                    } else {
                        alert('Please at least select one item to unknown');
                    }

                };

                //---------------------------------------------------------------------------------------
                $scope.levelList_his = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                $scope.tab1_transNumber_his = '';
                $scope.tab1_legalEntity_his = '';
                $scope.tab1_transcurrency_his = '';
                $scope.tab1_transCustomer_his = '';
                $scope.tab1_transaForward_his = '';

                //statusCheckBox
                $scope.selectedList_his = ["-1", "0", "2", "4"];

                // bank grid start
                $scope.bankHisStartIndex = 0;
                $scope.bankHisSelectedLevel = 20;  //下拉单页容量初始化
                $scope.bankHisItemsperpage = 20;
                $scope.bankHisCurrentPage = 1; //当前页
                $scope.bankHisMaxSize = 10; //分页显示的最大页  

                $scope.statusHisselect = '';

                $scope.createbankHisHisDataGrid = function () {
                    $scope.bankHisHisDataList = {

                        showGridFooter: true,
                        enableFullRowSelection: false, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                        //enableRowHeaderSelection: true, //是否显示选中checkbox框 ,default为true
                        enableSelectAll: true, // 选择所有checkbox是否可用，default为true;
                        enableSelectionBatchEvent: true, //default为true
                        multiSelect: true,// 是否可以选择多个,默认为true;
                        noUnselect: false,//default为false,选中后是否可以取消选中
                        enableFiltering: true,
                        data: 'bankHisList',
                        columnDefs: [
                            { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'operation', displayName: '', width: '100', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:left;padding-left:10px;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.editBank(row.entity)">Edit</a> | <a href="javascript: void (0);" ng-click="grid.appScope.deleteBank(row.entity)">Delete</a></span>' },
                            {
                                name: 'fileFlag', displayName: 'Attachment', width: '35', enableFiltering: false,
                                cellTemplate: '<a class="glyphicon glyphicon-paperclip" style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasfile==0" ng-click="grid.appScope.openBsFiles(row.entity)"></a>'
                                    + '<a class="glyphicon glyphicon-download-alt" style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasfile>0" ng-click="grid.appScope.openBsFiles(row.entity)"></a>'
                            },
                            { name: 'Lock', displayName: 'Lock', width: '35', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.islocked"><img src="~/../Content/images/HoldCustomer.png"></span>' },
                            { name: 'PMT', displayName: 'PMT', width: '35', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.haspmtdetail" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },
                            { name: 'Post', displayName: 'Post', width: '35', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.applY_STATUS==\'1\'"><img src="~/../Content/images/Dunning_Reminder_actived.png" style="width:20px"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.applY_STATUS==\'2\'"><img src="~/../Content/images/Execute.png"></span>' },
                            { name: 'Clear', displayName: 'Clear', width: '35', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.clearinG_STATUS==\'1\'"><img src="~/../Content/images/Dunning_Reminder_actived.png" style="width:20px"></span><span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.clearinG_STATUS==\'2\'"><img src="~/../Content/images/Execute.png"></span>' },
                            { field: 'id', displayName: 'id', width: '120', visible: false },
                            {
                                field: 'matcH_STATUS_NAME', displayName: 'Status', width: '80',
                                cellTemplate: '<div style="margin-top:6px;color:{{row.entity.statuscolor}}">{{row.entity.matcH_STATUS_NAME }}</div>'
                            },
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '60', cellClass: 'center', enableCellEdit: false },
                            { field: 'transactioN_NUMBER', displayName: 'Transaction Inc', width: '100', enableCellEdit: false },
                            { field: 'currency', displayName: 'Currency', width: '50', cellClass: 'center', enableCellEdit: false },
                            { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '100', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'currenT_AMOUNT', displayName: 'Current Amount', width: '100', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'unClear_Amount', displayName: 'UnClear Amount', width: '100', enableCellEdit: false, cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'description', displayName: 'Description', width: '120', enableCellEdit: false },
                            { field: 'valuE_DATE', displayName: 'Value Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                            {
                                field: 'forwarD_NUM', displayName: 'Payer Number', width: '80', enableCellEdit: false
                            },
                            { field: 'forwarD_NAME', displayName: 'Payer Name', width: '140', enableCellEdit: false },
                            {
                                field: 'customeR_NUM', displayName: 'Customer Number', width: '80', enableCellEdit: false
                            },
                            { field: 'customeR_NAME', displayName: 'Customer Name', width: '140', enableCellEdit: false },

                            { field: 'bankChargeFrom', displayName: 'Charge From', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'bankChargeTo', displayName: 'Charge To', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'reF1', displayName: 'Ref1', width: '150', enableCellEdit: false },
                            { field: 'comments', displayName: 'Comments', width: '120' },
                            { field: 'recoN_TIME', displayName: 'Recon Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'identifY_TIME', displayName: 'Identify Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'advisoR_TIME', displayName: 'Advisor Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'applY_TIME', displayName: 'Apply Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'clearinG_TIME', displayName: 'Clearing Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'creatE_DATE', displayName: 'Create Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                            { field: 'updatE_DATE', displayName: 'Last Modified Time', width: '140', enableCellEdit: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.bankHisGridApi = gridApi;

                            $scope.statusHisselect = $scope.selected_his.join(',');
                            //$scope.bankHisPageChanged();
                            
                        }
                    };
                };

                //Detail单页容量变化
                $scope.bankHisPageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getBankHisHisDataDetails($scope.statusHisselect, $scope.tab1_legalEntity_his, $scope.tab1_transNumber_his, $scope.tab1_transcurrency_his, $scope.tab1_transamount_his, $scope.tab1_transCustomer_his, $scope.tab1_transaForward_his, $scope.tab1_valueDateF_his, $scope.tab1_valueDateT_his,$scope.bankHisCurrentPage, selectedLevelId, function (result) {
                        $scope.bankHisItemsperpage = selectedLevelId;
                        $scope.bankHisTotalItems = result.count;
                        $scope.bankHisList = result.dataRows;
                        $scope.bankHisStartIndex = ($scope.bankHisCurrentPage - 1) * $scope.bankHisItemsperpage;

                        $scope.calculate_his($scope.bankHisCurrentPage, $scope.bankHisItemsperpage, result.dataRows.length);
                    }, function (error) {
                            alert(error);
                    });
                };

                //Detail翻页
                $scope.bankHisPageChanged = function () {
                    caHisDataProxy.getBankHisHisDataDetails($scope.statusHisselect, $scope.tab1_legalEntity_his, $scope.tab1_transNumber_his, $scope.tab1_transcurrency_his, $scope.tab1_transamount_his, $scope.tab1_transCustomer_his, $scope.tab1_transaForward_his, $scope.tab1_valueDateF_his, $scope.tab1_valueDateT_his, $scope.bankHisCurrentPage, $scope.bankHisItemsperpage, function (result) {
                        $scope.bankHisTotalItems = result.count;
                        $scope.bankHisList = result.dataRows;
                        $scope.bankHisStartIndex = ($scope.bankHisCurrentPage - 1) * $scope.bankHisItemsperpage;
                        $scope.bankHisGridApi.selection.clearSelectedRows();

                        $scope.calculate_his($scope.bankHisCurrentPage, $scope.bankHisItemsperpage, result.dataRows.length);
                    }, function (error) {
                        alert(error);
                    });
                };


                //查询条件展开、合上
                var bankShow_his = false;
                $scope.bankOpenFilter_his = function () {
                    bankShow_his = !bankShow_his;
                    if (bankShow_his) {
                        $("#bankDataSearch_his").show();
                    } else {
                        $("#bankDataSearch_his").hide();
                    }
                };

                $scope.init_his = function () {

                    $scope.statusHisselect = $scope.selected_his.join(',');
                    $scope.bankHisPageChanged();
                };



                $scope.resetSearch_his = function () {
                    $scope.tab1_transNumber_his = '';
                    $scope.tab1_legalEntity_his = '';
                    $scope.tab1_transamount_his = null;
                    $scope.tab1_transcurrency_his = '';
                    $scope.tab1_transCustomer_his = '';
                    $scope.tab1_transaForward_his = '';
                    $scope.init_his();
                };


                let temp_his = statusList;//用一个变量来接受返回值，待处理完再赋值给$scope.list_his
                $scope.selected_his = [];// 用来保存选中的数据的值
                $scope.selectedobj_his = [];// 用来保存选中的数据
                $scope.unSelected_his = [];// 用来保存未选中的数据
                for (var i = 0; i < temp_his.length; i++) {
                    let ok = false;
                    $scope.selectedList_his.forEach(row => {
                        // 为选中的id添加check属性是true
                        if (row == temp_his[i].detailValue) {
                            temp_his[i].check = true;
                            $scope.selectedobj_his.push(temp_his[i]);
                            $scope.selected_his.push(row);
                            ok = true;
                        }
                    })
                    if (!ok) {
                        // 为没有选中的id添加check属性为false
                        temp_his[i].check = false;
                        $scope.unSelected_his.push(temp_his[i])
                    }
                }

                // 将选中的和未选中的全部放进一个list里面
                $scope.list_his = [...$scope.selectedobj_his, ...$scope.unSelected_his];

                if ($scope.selected_his.length == $scope.list_his.length) {
                    $scope.all_his = true; // 全选为true
                } else {
                    $scope.all_his = false; // 全选为false
                }


                // 点击选中事件
                $scope.updateAll_his = function (val) {
                    if (val) {
                        $scope.selected_his = [];
                        $scope.list_his.forEach(row => {
                            row.check = true;
                            $scope.selected_his.push(row.detailValue);
                        })
                    } else {
                        $scope.selected_his = [];

                        $scope.list_his.forEach(row => {
                            row.check = false;
                        })
                    }
                }

                // 复选框选中事件
                $scope.updateSelected_his = function ($event, id) {
                    let checkbox = $event.target;
                    let action = (checkbox.checked ? 'add' : 'delete');
                    if (action == 'add' && $scope.selected_his.indexOf(id) == -1) {
                        $scope.selected_his.push(id);
                        $scope.list_his.forEach(row => {
                            if (row.detailValue == id) {
                                row.check = true;
                            }
                        })
                    }
                    if (action == 'delete' && $scope.selected_his.indexOf(id) != -1) {
                        let idx = $scope.selected_his.indexOf(id);
                        $scope.selected_his.splice(idx, 1);
                        $scope.list_his.forEach(row => {
                            if (row.detailValue == id) {
                                row.check = false;
                            }
                        })
                    }

                    if ($scope.selected_his.length == $scope.list_his.length) {
                        $scope.all_his = true;
                    } else {
                        $scope.all_his = false;
                    }
                }



                // --------------------------------------------------bank grid end

                $scope.levelList_pmt = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];
                // PmtDetail grid start
                $scope.PmtDetailStartIndex = 0;
                $scope.PmtDetailSelectedLevel = 20;  //下拉单页容量初始化
                $scope.PmtDetailItemsperpage = 20;
                $scope.PmtDetailCurrentPage = 1; //当前页
                $scope.PmtDetailMaxSize = 10; //分页显示的最大页  

                $scope.tab2_groupNo = '';
                $scope.tab2_legalEntity = '';
                $scope.tab2_customerNum = '';
                $scope.tab2_currency = '';
                $scope.tab2_amount = '';
                $scope.tab2_transactionNumber = '';
                $scope.tab2_invoiceNum = '';
                $scope.tab2_valueDateF = '';
                $scope.tab2_valueDateT = '';
                $scope.tab2_hasBS = '';
                $scope.tab2_hasINV = '';
                $scope.tab2_hasMatched = '';
                $scope.tab2_isClosed = '0';



                //$scope.isCheckAll = 0;

                $scope.createPmtDetailHisDataGrid = function () {
                    $scope.PmtDetailHisDataList = {
                        showGridFooter: true,
                        enableFullRowSelection: true, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                        //enableRowHeaderSelection: true, //是否显示选中checkbox框 ,default为true
                        enableSelectAll: true, // 选择所有checkbox是否可用，default为true;
                        enableSelectionBatchEvent: true, //default为true
                        multiSelect: false,// 是否可以选择多个,默认为true;
                        noUnselect: false,//default为false,选中后是否可以取消选中
                        enableFiltering: true,
                        data: 'PmtDetailList',
                        columnDefs: [
                            {
                                field: 'willDelete', displayName: 'Del', width: '40', pinnedRight: true, enableFiltering: false,
                                //headerCellTemplate: '<div style="text-align:center">Is Send</div><div style="text-align:center"><input type="checkbox"   ng-checked="isCheckAll" ng-model="grid.appScope.isCheckAll" ng-click="grid.appScope.sendAddAll()" /></div>',
                                cellTemplate: '<div style="text-align:center"> <input type="checkbox" ng-checked="row.entity.willDelete" ng-model="row.entity.willDelete" /></div> '
                            },
                            { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'operation', displayName: '', width: '80', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.editPMT(row.entity)">Edit</a> | <a href="javascript: void (0);" ng-click="grid.appScope.deletePMTDetail(row.entity)">Delete</a></span>' },
                            { field: 'hasbs', name: 'BS', displayName: 'BS', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasbs>0" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },
                            { field: 'hasinv', name: 'Detail', displayName: 'Detail', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasinv>0" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },
                            { field: 'hasMatched', name: 'Matched', displayName: 'Matched', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasMatched>0" ><img style="height:20px;width:20px" src="~/../Content/images/SOA_actived.png"></span>' },                           
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '50' },
                            { field: 'valueDate', displayName: 'Value Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                            { field: 'receiveDate', displayName: 'Receive Date', width: '90', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                            { field: 'customerNum', displayName: 'Customer Number', width: '80', cellClass: 'center' },
                            { field: 'customerName', displayName: 'Customer Name', width: '140' },
                            { field: 'currency', displayName: 'Currency', width: '40', cellClass: 'center' },
                            { field: 'transactionAmount', displayName: 'Transaction Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'amount', displayName: 'Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'groupNo', displayName: 'Group No', width: '120', cellClass: 'center' },
                            { field: 'filename', displayName: 'File Name', width: '250', cellClass: 'left' },
                            { field: 'creatE_USER', displayName: 'Create User', width: '100' },
                            { field: 'creatE_DATE', displayName: 'Create Time', width: '140', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.PmtDetailGridApi = gridApi;
                            gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                                caHisDataProxy.getPmtDetailHisDataBsById(row.entity.id, function (result) {
                                    $scope.PmtDetailBsList = result;
                                });
                                caHisDataProxy.getPmtDetailHisDataDetailById(row.entity.id, function (result) {
                                    $scope.PmtDetailDetailList = result;
                                });
                            });
                        }
                    };
                    $scope.PmtDetailBSHisDataList = {
                        multiSelect: false,
                        enableFullRowSelection: true,
                        enableFiltering: true,
                        noUnselect: false,
                        data: 'PmtDetailBsList',
                        columnDefs: [
                            { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'transactionNumber', displayName: 'Transaction Inc', width: '130' },
                            { field: 'valueDate', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'currency', displayName: 'Currency', width: '80', cellClass: 'center' },
                            { field: 'amount', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'reF1', displayName: 'Description', width: '200' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.PmtDetailBSGridApi = gridApi;
                        }
                    };

                    $scope.PmtDetailDetailHisDataList = {
                        multiSelect: false,
                        enableFullRowSelection: true,
                        enableFiltering: true,
                        noUnselect: false,
                        data: 'PmtDetailDetailList',
                        columnDefs: [
                            { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'invIsClosed', name: 'invIsClosed', displayName: 'IS Closed', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.invIsClosed>0" ><img style="height:12px;width:12px" src="~/../Content/images/259.png"></span>' },
                            { field: 'siteUseId', displayName: 'SiteUseId', width: '100', cellClass: 'center' },
                            { field: 'invoiceNum', displayName: 'Invoice No', width: '120' },
                            { field: 'invoiceDate', displayName: 'Invoice Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'dueDate', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'currency', displayName: 'Currency', width: '80', cellClass: 'center' },
                            { field: 'amount', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.PmtDetailDetailGridApi = gridApi;
                        }
                    };
                    //$scope.PmtDetailPageChanged();
                };

                $scope.deletePMTDetail = function (row) {
                    if (row.isClosed == true) {
                        alert("The payment has been closed and cannot be deleted!");
                        return false;
                    }
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            paymentDetailProxy.deletePMTByID(row, function () {
                                $scope.PmtDetailPageChanged();
                            }, function (error) {
                                alert(error);
                            })
                        }
                    });
                }

                $scope.PmtDetailBatchDelete = function () {
                    //循环查找要删除的记录

                    $scope.deletePmts = [];
                    angular.forEach($scope.PmtDetailList, function (rowItem) {
                        if (rowItem.isClosed == true) {
                            alert(rowItemgroupNo + " is closed, can't be delete.");
                            return;
                        }
                        if (rowItem.willDelete == true) {
                            $scope.deletePmts.push(rowItem.id);
                        }
                    });
                    if ($scope.deletePmts.length == 0) {
                        alert("Please selected more than one row.");
                        return;
                    }
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            paymentDetailProxy.deletePMTByIDs($scope.deletePmts, function () {
                                $scope.PmtDetailPageChanged();
                            }, function (error) {
                                alert(error);
                            })
                        }
                    });

                };

                //Detail单页容量变化
                $scope.PmtDetailPageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getPmtDetailHisDataDetails($scope.tab2_groupNo, $scope.tab2_legalEntity, $scope.tab2_customerNum, $scope.tab2_currency,
                        $scope.tab2_amount, $scope.tab2_transactionNumber, $scope.tab2_invoiceNum, $scope.tab2_valueDateF, $scope.tab2_valueDateT,
                        $scope.tab2_createDateF, $scope.tab2_createDateT, $scope.tab2_isClosed, $scope.tab2_hasBS, $scope.tab2_hasMatched, $scope.tab2_hasINV, $scope.PmtDetailCurrentPage, selectedLevelId, function (result) {

                        $scope.PmtDetailItemsperpage = selectedLevelId;
                        $scope.PmtDetailTotalItems = result.count;
                        $scope.PmtDetailList = result.pmt;
                        if (result.count > 0) {
                            $scope.PmtDetailBsList = result.pmt[0].pmtBs;
                            $scope.PmtDetailDetailList = result.pmt[0].pmtDetail;
                        }
                        else {
                            $scope.PmtDetailBsList = [];
                            $scope.PmtDetailDetailList = [];
                        }
                        $scope.PmtDetailStartIndex = ($scope.currentPage - 1) * $scope.PmtDetailItemsperpage;
                        $scope.calculate_pmt($scope.PmtDetailCurrentPage, $scope.PmtDetailItemsperpage, result.pmt.length);
                    });
                };

                //Detail翻页
                $scope.PmtDetailPageChanged = function () {
                    caHisDataProxy.getPmtDetailHisDataDetails($scope.tab2_groupNo, $scope.tab2_legalEntity, $scope.tab2_customerNum, $scope.tab2_currency,
                        $scope.tab2_amount, $scope.tab2_transactionNumber, $scope.tab2_invoiceNum, $scope.tab2_valueDateF, $scope.tab2_valueDateT,
                        $scope.tab2_createDateF, $scope.tab2_createDateT, $scope.tab2_isClosed, $scope.tab2_hasBS, $scope.tab2_hasMatched, $scope.tab2_hasINV, $scope.PmtDetailCurrentPage, $scope.PmtDetailItemsperpage, function (result) {
                        $scope.PmtDetailTotalItems = result.count;
                        $scope.PmtDetailList = result.pmt;
                        if (result.count > 0) {
                            $scope.PmtDetailBsList = result.pmt[0].pmtBs;
                            $scope.PmtDetailDetailList = result.pmt[0].pmtDetail;
                        }
                        else {
                            $scope.PmtDetailBsList = [];
                            $scope.PmtDetailDetailList = [];
                        }
                        $scope.PmtDetailStartIndex = ($scope.PmtDetailCurrentPage - 1) * $scope.PmtDetailItemsperpage;

                        $scope.calculate_pmt($scope.PmtDetailCurrentPage, $scope.PmtDetailItemsperpage, result.pmt.length);
                    }, function (error) {
                        alert(error);
                        });

                    //$(".ui-grid-viewport .ng-isolate-scope").css("scroll", "hidden");
                    //$(".ui-grid-render-container .ng-isolate-scope .ui-grid-render-container-body").css("scroll", "hidden");
                    //$(".ui-grid-render-container .ng-isolate-scope .ui-grid-render-container-body").css("scroll", "show");
                };

                $scope.tab2_init = function () {
                    $interval($scope.PmtDetailPageChanged(),600,1);
                };

                $scope.tab2_resetSearch = function () {
                    $scope.tab2_groupNo = '';
                    $scope.tab2_legalEntity = '';
                    $scope.tab2_customerNum = '';
                    $scope.tab2_currency = '';
                    $scope.tab2_amount = '';
                    $scope.tab2_transactionNumber = '';
                    $scope.tab2_invoiceNum = '';
                    $scope.tab2_valueDateF = '';
                    $scope.tab2_valueDateT = '';
                    $scope.tab2_createDateF = '';
                    $scope.tab2_createDateT = '';
                    $scope.tab2_hasBS = '';
                    $scope.tab2_hasINV = '';
                    $scope.tab2_hasMatched = '';
                    $scope.tab2_isClosed = '0';
                    $scope.tab2_init();
                };

                // PmtDetail grid end

                // PmtDetail search start

                //查询条件展开、合上
                var PmtDetailShow = false;
                $scope.PmtDetailOpenFilter = function () {
                    PmtDetailShow = !PmtDetailShow;
                    if (PmtDetailShow) {
                        $("#PmtDetailDataSearch").show();
                    } else {
                        $("#PmtDetailDataSearch").hide();
                    }
                };

                // PmtDetail search end

                $scope.tab3_customerNum = '';
                $scope.tab3_legalEntity = '';
                $scope.tab3_customerCurrency = '';
                $scope.tab3_invCurrency = '';
                $scope.tab3_amt = '';
                $scope.tab3_localAmt = '';
                $scope.ptpDateF = '';
                $scope.ptpDateT = '';

                // ptp grid start
                $scope.ptpStartIndex = 0;
                $scope.ptpSelectedLevel = 20;  //下拉单页容量初始化
                $scope.ptpItemsperpage = 20;
                $scope.ptpCurrentPage = 1; //当前页
                $scope.ptpMaxSize = 10; //分页显示的最大页  

                $scope.createptpHisDataGrid = function () {
                    $scope.ptpHisDataList = {
                        showGridFooter: true,
                        multiSelect: false,
                        enableFullRowSelection: true,
                        enableFiltering: true,
                        noUnselect: false,
                        data: 'ptpList',
                        columnDefs: [
                            { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                            { field: 'legalEntity', displayName: 'LegalEntity', width: '120' },
                            { field: 'customeR_NUM', displayName: 'Customer Number', width: '150', cellClass: 'center' },
                            { field: 'customeR_NAME', displayName: 'Customer Name', width: '350' },
                            { field: 'ptP_DATE', displayName: 'PTP Date', width: '120', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                            { field: 'funC_CURRENCY', displayName: 'Func Currency', width: '120', cellClass: 'center' },
                            { field: 'inV_CURRENCY', displayName: 'Inv Currency', width: '120', cellClass: 'center' },
                            { field: 'amt', displayName: 'Amount', width: '130', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                            { field: 'local_AMT', displayName: 'FXRate Amount', width: '130', cellFilter: 'number:2', type: 'number', cellClass: 'right' }
                        ],
                        onRegisterApi: function (gridApi) {
                            $scope.ptpGridApi = gridApi;
                        }
                    };
                    //$scope.ptpPageChanged();
                };

                //Detail翻页
                $scope.ptpPageChanged = function () {
                    caHisDataProxy.getPtpHisDataDetails($scope.tab3_customerNum, $scope.tab3_legalEntity, $scope.tab3_customerCurrency, $scope.tab3_invCurrency, $scope.tab3_amt, $scope.tab3_localAmt, $scope.ptpDateF, $scope.ptpDateT, function (result) {
                        $scope.ptpTotalItems = result.count;
                        $scope.ptpList = result.dataRows;
                        $scope.ptpStartIndex = ($scope.ptpCurrentPage - 1) * $scope.ptpItemsperpage;

                    }, function (error) {
                        alert(error);
                    });
                    //$(".ui-grid-viewport .ng-isolate-scope").css("scroll", "hidden");
                    //$(".ui-grid-render-container .ng-isolate-scope .ui-grid-render-container-body").css("scroll", "hidden");
                    //$(".ui-grid-render-container .ng-isolate-scope .ui-grid-render-container-body").css("scroll", "show");
                };

                $scope.createbankHisDataGrid();
                $scope.createbankHisHisDataGrid();
                $scope.createPmtDetailHisDataGrid();
                $scope.createptpHisDataGrid();

                $scope.alertBankStatementHis = function () {
                    if ($scope.rebuildBankHisgrid === true) {
                        //$scope.createbankHisHisDataGrid();
                        $scope.bankHisPageChanged();
                        $scope.rebuildBankHisgrid = false;
                    }
                };
                $scope.alertPmtDetailhis = function () {
                    if ($scope.rebuildpmtgrid === true) {
                        //$scope.createPmtDetailHisDataGrid();
                        $scope.PmtDetailPageChanged();
                        $scope.rebuildpmtgrid = false;
                    }
                };
                $scope.alertptpHis = function () {
                    if ($scope.rebuildptpgrid === true) {
                        //$scope.createptpHisDataGrid();
                        $scope.ptpPageChanged();
                        $scope.rebuildptpgrid = true;
                    }
                };


                // ptp grid end

                // ptp search start

                //查询条件展开、合上
                var ptpShow = false;
                $scope.ptpOpenFilter = function () {
                    ptpShow = !ptpShow;
                    if (ptpShow) {
                        $("#ptpDataSearch").show();
                    } else {
                        $("#ptpDataSearch").hide();
                    }
                };

                // ptp search end

                // action operation start
                $scope.identifyCustomer = function () {
                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";
                    $scope.bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caHisDataProxy.identifyCustomer($scope.bankIds, function () {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            $scope.bankPageChanged();
                            alert('Operation Successed!');

                            $interval(function () {
                                $scope.bankPageChanged();
                                console.log(1);
                            }, 30000, 1);
                        }, function (error) {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            alert(error);
                        });
                    } else {
                        document.getElementById("overlay-container").style.display = "none";
                        document.getElementById("overlay-container").style.flg = "";
                        alert('Please at least select one Item to Identify');
                    }
                };

                $scope.unknownCashAdvisor = function () {
                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";
                    $scope.bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caHisDataProxy.unknownCashAdvisor($scope.bankIds, function () {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            $scope.bankPageChanged();
                            alert('Operation Successed!');
                        }, function (error) {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            alert(error);
                        });
                    } else {
                        document.getElementById("overlay-container").style.display = "none";
                        document.getElementById("overlay-container").style.flg = "";
                        alert('Please at least select one Item to Unknown');
                    }
                };

                $scope.pmtUnknownCashAdvisor = function () {
                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";
                    $scope.bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caHisDataProxy.pmtUnknownCashAdvisor($scope.bankIds, function () {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            $scope.bankPageChanged();
                            alert('Operation Successed!');
                        }, function (error) {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            alert(error);
                        });
                    } else {
                        document.getElementById("overlay-container").style.display = "none";
                        document.getElementById("overlay-container").style.flg = "";
                        alert('Please at least select one Item to Unknown');
                    }
                };

                $scope.recon = function () {
                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";
                    $scope.bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caHisDataProxy.recon($scope.bankIds, function () {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            $scope.bankPageChanged();
                            alert('Operation Successed!');
                        }, function (error) {
                            document.getElementById("overlay-container").style.display = "none";
                            document.getElementById("overlay-container").style.flg = "";
                            alert(error);
                        });
                    } else {
                        document.getElementById("overlay-container").style.display = "none";
                        document.getElementById("overlay-container").style.flg = "";
                        alert('Please at least select one item to unknown');
                    }
                };

                $scope.tab3_init = function () {
                    $scope.ptpPageChanged();
                };

                $scope.tab3_resetSearch = function () {
                    $scope.tab3_customerNum = '';
                    $scope.tab3_legalEntity = '';
                    $scope.tab3_customerCurrency = '';
                    $scope.tab3_invCurrency = '';
                    $scope.tab3_amt = '';
                    $scope.tab3_localAmt = '';
                    $scope.ptpDateF = '';
                    $scope.ptpDateT = '';
                    $scope.tab3_init();
                };

                // action operation end

                $scope.toUploadPage = function () {
                    $location.path("/ca/upload");
                };
                
                $scope.toIdentifyPage = function () {
                    $location.path("/ca/identifyCustomer");
                };

                
                $scope.toUnknownPage = function () {
                    var entity = $scope.bankGridApi.selection.getSelectedRows();
                    var unknownList = [];
                    angular.forEach(entity, function (row) {
                        if (row.matcH_STATUS < 2) {
                            unknownList.push(row);
                        }
                    });

                    if (unknownList.length == 0) {
                        alert("Please selectPlease select at least one correct status data");
                        return;
                    }
                    localStorage.setItem("entity", JSON.stringify(unknownList));
                    $location.path("/ca/unknownDetail");

                };

                $scope.toReconPage = function () {
                    $location.path("/ca/reconTask");
                };

                $scope.toAdjustmentPage = function () {
                    bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            bankIds.push(rowItem.id);
                        });
                        // 设置标识及传参
                        localStorage.setItem('isReconAdjustment', true);
                        localStorage.setItem('reconAdjustIds', bankIds);
                        $location.path("/ca/reconDetail");
                    } else {
                        alert('Please at least select one item to recon adjustment');
                    }
                };

                $scope.toTaskListPage = function () {
                    $location.path("/ca/reconTask");
                };

                $scope.actiontask = function () {
                    $location.path("/ca/actiontask");
                };

                $scope.Export = function () {
                    if ($scope.active === 1) {
                        caHisDataProxy.exportBankStatement($scope.statusselect, $scope.tab1_legalEntity, $scope.tab1_transNumber, $scope.tab1_transcurrency, $scope.tab1_transamount, $scope.tab1_transCustomer, $scope.tab1_transaForward, $scope.tab1_valueDateF, $scope.tab1_valueDateT, $scope.tab1_createDateF, $scope.tab1_createDateT, $scope.tab1_bsType);
                    } else if ($scope.active === 3){
                        caHisDataProxy.exportPmtDetail($scope.tab2_groupNo, $scope.tab2_legalEntity, $scope.tab2_customerNum, $scope.tab2_currency,
                            $scope.tab2_amount, $scope.tab2_transactionNumber, $scope.tab2_invoiceNum, $scope.tab2_valueDateF, $scope.tab2_valueDateT,
                            $scope.tab2_createDateF, $scope.tab2_createDateT, $scope.tab2_isClosed, $scope.tab2_hasBS, $scope.tab2_hasMatched, $scope.tab2_hasINV, $scope.PmtDetailCurrentPage, $scope.PmtDetailItemsperpage);
                    }
                };

                $scope.SendPmtDetailMail = function () {
                    $scope.bankIds = [];
                    $scope.bankIds.push(1);
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caCommonProxy.SendPmtDetailMail($scope.bankIds, function (e) {
                            $scope.bankPageChanged();
                            alert('Send Mail Task Finished! Records:' + e);
                        }, function (error) {
                            alert(error);
                        });
                    } else {
                        alert('Please at least select one item to unknown');
                    }
                };

                $scope.autoRecon = function () {
                    $scope.statusselect = $scope.selected.join(',');
                    document.getElementById("overlay-container").style.display = "block";
                    document.getElementById("overlay-container").style.flg = "loading";
                    caHisDataProxy.autoRecon($scope.statusselect, $scope.tab1_legalEntity, $scope.tab1_transNumber, $scope.tab1_transcurrency, $scope.tab1_transamount, $scope.tab1_transCustomer, $scope.tab1_transaForward, $scope.tab1_valueDateF, $scope.tab1_valueDateT, $scope.tab1_createDateF, $scope.tab1_createDateT, $scope.tab1_bsType, function () {
                        document.getElementById("overlay-container").style.display = "none";
                        document.getElementById("overlay-container").style.flg = "";
                        $scope.bankPageChanged();
                        alert('Auto Recon Started!');
                    }, function (error) {
                        document.getElementById("overlay-container").style.display = "none";
                        document.getElementById("overlay-container").style.flg = "";
                        alert(error);
                    });
                }



                let temp = statusList;//用一个变量来接受返回值，待处理完再赋值给$scope.list
                $scope.selected = [];// 用来保存选中的数据的值
                $scope.selectedobj = [];// 用来保存选中的数据
                $scope.unSelected = [];// 用来保存未选中的数据
                for (var i = 0; i < temp.length; i++) {
                    let ok = false;
                    $scope.selectedList.forEach(row => {
                        // 为选中的id添加check属性是true
                        if (row == temp[i].detailValue) {
                            temp[i].check = true;
                            $scope.selectedobj.push(temp[i]);
                            $scope.selected.push(row);
                            ok = true;
                        }
                    })
                    if (!ok) {
                        // 为没有选中的id添加check属性为false
                        temp[i].check = false;
                        $scope.unSelected.push(temp[i])
                    }
                }

                // 将选中的和未选中的全部放进一个list里面
                $scope.list = [...$scope.selectedobj, ...$scope.unSelected];

                if ($scope.selected.length == $scope.list.length) {
                    $scope.all = true; // 全选为true
                } else {
                    $scope.all = false; // 全选为false
                }


                // 点击选中事件
                $scope.updateAll = function (val) {
                    if (val) {
                        $scope.selected = [];
                        $scope.list.forEach(row => {
                            row.check = true;
                            $scope.selected.push(row.detailValue);
                        })
                    } else {
                        $scope.selected = [];

                        $scope.list.forEach(row => {
                            row.check = false;
                        })
                    }
                }

                // 复选框选中事件
                $scope.updateSelected = function ($event, id) {
                    let checkbox = $event.target;
                    let action = (checkbox.checked ? 'add' : 'delete');
                    if (action == 'add' && $scope.selected.indexOf(id) == -1) {
                        $scope.selected.push(id);
                        $scope.list.forEach(row => {
                            if (row.detailValue == id) {
                                row.check = true;
                            }
                        })
                    }
                    if (action == 'delete' && $scope.selected.indexOf(id) != -1) {
                        let idx = $scope.selected.indexOf(id);
                        $scope.selected.splice(idx, 1);
                        $scope.list.forEach(row => {
                            if (row.detailValue == id) {
                                row.check = false;
                            }
                        })
                    }

                    if ($scope.selected.length == $scope.list.length) {
                        $scope.all = true;
                    } else {
                        $scope.all = false;
                    }
                }

                //增加payment过滤条件
                $scope.updateSelectedIsClosed = function ($event) {
                    let checkbox = $event.target;
                    $scope.tab2_isClosed = (checkbox.checked ? '1' : '0');
                }

                $scope.reuploadPost = function () {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/reuploadPost.tpl.html',
                        controller: 'reuploadPostCtrl',
                        size: 'customSize',
                        resolve: {
                            
                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        $scope.bankPageChanged();
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.reuploadPostClear = function () {
                    //导入文件
                    var modalDefaults = {
                        templateUrl: 'app/cashapplication/hisdata/reuploadPostClear.tpl.html',
                        controller: 'reuploadPostClearCtrl',
                        size: 'customSize',
                        resolve: {

                        },
                        windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        $scope.bankPageChanged();
                    }, function (err) {
                        alert(err);
                    });
                };

                $scope.ignore = function () {

                    $scope.bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caHisDataProxy.ignore($scope.bankIds, function () {
                            $scope.bankPageChanged();
                            alert('Operation Successed!');
                        }, function (error) {
                            alert(error);
                        });
                    }
                };

                $scope.unlock = function () {

                    $scope.bankIds = [];
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caHisDataProxy.unlock($scope.bankIds, function () {
                            $scope.bankPageChanged();
                            alert('Operation Successed!');
                        }, function (error) {
                            alert(error);
                        });
                    }
                };

                $scope.batchDelete = function () {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            $scope.bankIds = [];
                            if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                                angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                                    $scope.bankIds.push(rowItem.id);
                                });
                                caHisDataProxy.batchDelete($scope.bankIds, function () {
                                    $scope.bankPageChanged();
                                    alert('Operation Successed!');
                                }, function (error) {
                                    alert(error);
                                });
                            }
                        }
                    });
                };

                $scope.init();

            }])
    .filter('mapChargeType', function () {
        return function (input) {
            if (input) {
                return 'Fixed Bank Charge';
            } else {
                return 'Range';
            }
        };
    })
    .filter('mapName', function () {
        return function (input) {
            if (!input) {
                return '-';
            } else {
                return input;
            }
        };
    });