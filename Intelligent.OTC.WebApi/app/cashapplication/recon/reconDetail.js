angular.module('app.reconDetail', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/reconDetail', {
                templateUrl: 'app/cashapplication/recon/reconDetail.tpl.html',
                controller: 'reconDetailCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('reconDetailCtrl',
        ['$scope', '$filter', '$interval', 'caHisDataProxy', 'caCommonProxy', 'modalService', '$location', 'APPSETTING',
            function ($scope, $filter, $interval, caHisDataProxy, caCommonProxy, modalService, $location, APPSETTING) {

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                // recon grid start
                $scope.reconDataList = {
                    multiSelect: true,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: false,
                    //enableCellEditOnFocus: true,
                    data: 'reconList',
                    columnDefs: [
                        {
                            name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            }},
                        {
                            field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '120', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey right'//自定义样式
                                } else {
                                    return 'right'
                                }
                            } },
                        {
                            field: 'transactioN_CURRENCY', displayName: 'Transaction Currency', width: '100', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'customeR_NUM', displayName: 'Customer Number', width: '120', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'customeR_NAME', displayName: 'Customer Name', width: '120', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey center'//自定义样式
                                } else {
                                    return 'center'
                                }
                            } },
                        {
                            field: 'grouP_NO', displayName: 'GroupNo', width: '120', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey center'//自定义样式
                                } else {
                                    return 'center'
                                }
                            } },
                        {
                            field: 'grouP_TYPE', displayName: 'GroupType', width: '100', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey center'//自定义样式
                                } else {
                                    return 'center'
                                }
                            } },
                        {
                            field: 'invoicE_NUM', displayName: 'Invoice Number', width: '120', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'invoicE_DUEDATE', displayName: 'Due Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey right'//自定义样式
                                } else {
                                    return 'right'
                                }
                            } },
                        {
                            field: 'invoicE_CURRENCY', displayName: 'Invoice Currency', width: '80', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },
                        {
                            field: 'invoicE_AMOUNT', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey right'//自定义样式
                                } else {
                                    return 'right'
                                }
                            } },
                        {
                            field: 'invoicE_SITEUSEID', displayName: 'SiteUseID', width: '120', cellClass: 'center', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },            
                        {
                            field: 'ebname', displayName: 'EBName', width: '120', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },            
                        {
                            field: 'hasVAT', displayName: 'Has VAT', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasVAT>0"><img src="~/../Content/images/Execute.png"></span>', cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                                if (row.entity.matcH_STATUS == '9' || row.entity.isClosed == 'true' || row.entity.isClosed) {
                                    return 'font_grey'//自定义样式
                                }
                            } },         
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.reconGridApi = gridApi;

                        $scope.reconGridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                            //行选中事件  
                            for (var i = 0; i < $scope.reconGridApi.grid.rows.length; i++) {
                                if ($scope.reconGridApi.grid.rows[i].entity.grouP_NO == row.entity.grouP_NO) {
                                    $scope.reconGridApi.grid.rows[i].isSelected = row.isSelected;
                                }
                            }
                        });  
                    }
                };

                $scope.reconGridInit = function (id) {
                    if (localStorage.getItem('isReconAdjustment') == true || localStorage.getItem('isReconAdjustment') == 'true') {
                        caHisDataProxy.getReconDetailsByBSIds(localStorage.getItem('reconAdjustIds'), function (result) {
                            if (null != result.dataRows && result.dataRows.length > 0) {
                                $scope.reconList = result.dataRows;
                            } else {
                                $scope.reconList = [];
                            }
                        }, function (error) {
                            alert(error);
                        });
                    } else {
                        caHisDataProxy.getReconDetails(id, function (result) {
                            if (null != result.dataRows && result.dataRows.length > 0) {
                                $scope.reconList = result.dataRows;
                            } else {
                                $scope.reconList = [];
                            }
                        }, function (error) {
                            alert(error);
                        });
                    }
                };

                $scope.reconGridInit(localStorage.getItem('reconTaskId'));
                // recon grid end

                // bank grid start
                $scope.sumBank = 0;

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
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'legalEntity', displayName: 'LegalEntity', width: '100', cellClass: 'center'},
                        { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '120' },
                        { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'currency', displayName: 'Currency', width: '120' },
                        { field: 'reF1', displayName: 'Description', width: '120' },
                        { field: 'customeR_NUM', displayName: 'Customer Number', width: '100', cellClass: 'center' },
                        { field: 'customeR_NAME', displayName: 'Customer Name', width: '200' },
                        { field: 'valuE_DATE', displayName: 'Value Date', width: '100', cellFilter: 'date:\'yyyy-MM-dd\'' }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.bankGridApi = gridApi;

                        $scope.bankGridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                            $scope.sumBank = 0;
                            //行选中事件  
                            if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                                var temp = $scope.bankGridApi.selection.getSelectedRows()[0].legalEntity;
                                for (var i = 0; i < $scope.bankGridApi.selection.getSelectedRows().length; i++) {
                                    if (temp != $scope.bankGridApi.selection.getSelectedRows()[i].legalEntity) {
                                        row.isSelected = false;
                                        alert("Can't select multiple legal entity!");
                                        return;
                                    }
                                }
                                for (var i = 0; i < $scope.bankGridApi.selection.getSelectedRows().length; i++) {
                                    $scope.sumBank = $scope.accAdd($scope.sumBank, $scope.bankGridApi.selection.getSelectedRows()[i].transactioN_AMOUNT);
                                }
                                $scope.difAr = $scope.accSub($scope.sumBank, $scope.sumAr);
                                $scope.difLocalAr = $scope.accSub($scope.sumBank, $scope.sumLocalAr);
                            }
                            
                            $scope.loadArByBank($scope.bankGridApi.selection.getSelectedRows());
                        }); 

                        $scope.bankGridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
                            $scope.sumBank = 0;
                            if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                                var temp = $scope.bankGridApi.selection.getSelectedRows()[0].legalEntity;
                                for (var i = 0; i < $scope.bankGridApi.selection.getSelectedRows().length; i++) {
                                    if (temp != $scope.bankGridApi.selection.getSelectedRows()[i].legalEntity) {
                                        $scope.bankGridApi.selection.clearSelectedRows();
                                        alert("Can't select multiple legal entity!");
                                        return;
                                    }
                                }
                                for (var i = 0; i < $scope.bankGridApi.selection.getSelectedRows().length; i++) {
                                    $scope.sumBank = $scope.accAdd($scope.sumBank, $scope.bankGridApi.selection.getSelectedRows()[i].transactioN_AMOUNT);
                                }
                                $scope.difAr = $scope.accSub($scope.sumBank, $scope.sumAr);
                                $scope.difLocalAr = $scope.accSub($scope.sumBank, $scope.sumLocalAr);
                            }

                            $scope.loadArByBank($scope.bankGridApi.selection.getSelectedRows());
                        });

                    }
                };

                $scope.reconBankGridInit = function () {
                    if (localStorage.getItem('isReconAdjustment') == true || localStorage.getItem('isReconAdjustment') == 'true') {
                        caHisDataProxy.getReconBankDetailsByBSIds(localStorage.getItem('reconAdjustIds'), function (result) {
                            if (null != result.dataRows && result.dataRows.length > 0) {
                                $scope.bankList = result.dataRows;
                            } else {
                                $scope.bankList = [];
                            }
                        }, function (error) {
                            alert(error);
                        });
                    } else {
                        caHisDataProxy.getReconBankDetails(localStorage.getItem('reconTaskId'), function (result) {
                            if (null != result.dataRows && result.dataRows.length > 0) {
                                $scope.bankList = result.dataRows;
                            } else {
                                $scope.bankList = [];
                            }
                        }, function (error) {
                            alert(error);
                        });
                    }
                    //$(".ui-grid-viewport .ng-isolate-scope").css("scroll-x", "hidden");
                    //$(".ui-grid-render-container .ng-isolate-scope .ui-grid-render-container-body").css("scroll-x", "hidden");
                    //$(".ui-grid-render-container .ng-isolate-scope .ui-grid-render-container-body").css("scroll-x", "show");
                };
                               
                // bank grid end

                // ar grid start
                $scope.sumAr = 0;
                $scope.sumLocalAr = 0;
                $scope.difAr = 0;
                $scope.difLocalAr = 0;

                $scope.arHisDataList = {
                    showGridFooter: true,
                    enableFullRowSelection: false, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                    //enableRowHeaderSelection: true, //是否显示选中checkbox框 ,default为true
                    enableSelectAll: true, // 选择所有checkbox是否可用，default为true;
                    enableSelectionBatchEvent: true, //default为true
                    multiSelect: true,// 是否可以选择多个,默认为true;
                    noUnselect: false,//default为false,选中后是否可以取消选中
                    enableFiltering: true,
                    data: 'arList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        //{ field: 'customeR_NUM', displayName: 'Customer Number', width: '120' },
                        { field: 'pmtCount', displayName: 'PMT Group', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.pmtCount>0"><img src="~/../Content/images/Execute.png"></span>' },
                        { field: 'hasVAT', displayName: 'Has VAT', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:25px;vertical-align:middle;text-align:center;display:block;" ng-if="row.entity.hasVAT>0"><img src="~/../Content/images/Execute.png"></span>' },
                        { field: 'siteUseId', displayName: 'SiteUseID', width: '80', cellClass: 'center' },
                        { field: 'invoicE_NUM', displayName: 'Invoice Number', width: '100', cellClass: 'center' },
                        { field: 'func_currency', displayName: 'Function Currency', width: '70', cellClass: 'center' },
                        { field: 'amt', displayName: 'Amount', width: '120', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'local_AMT', displayName: 'FXRate Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'duE_DATE', displayName: 'Due Date', width: '80', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                        { field: 'ebName', displayName: 'EBName', width: '150' },
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.arGridApi = gridApi;

                        $scope.arGridApi.selection.on.rowSelectionChanged($scope, function (row, event) {
                            //行选中事件  
                            if ($scope.arGridApi.selection.getSelectedRows().length > 0) {
                                $scope.sumAr = 0;
                                $scope.sumLocalAr = 0;
                                for (var i = 0; i < $scope.arGridApi.selection.getSelectedRows().length; i++) {
                                    $scope.sumAr = $scope.accAdd($scope.sumAr, $scope.arGridApi.selection.getSelectedRows()[i].amt);
                                    $scope.sumLocalAr = $scope.accAdd($scope.sumLocalAr, $scope.arGridApi.selection.getSelectedRows()[i].local_AMT);
                                }
                                $scope.difAr = $scope.accSub($scope.sumBank, $scope.sumAr);
                                $scope.difLocalAr = $scope.accSub($scope.sumBank, $scope.sumLocalAr);
                            }
                        }); 

                        $scope.arGridApi.selection.on.rowSelectionChangedBatch($scope, function (rows) {
                            $scope.sumAr = 0;
                            $scope.sumLocalAr = 0;
                            $scope.difAr = 0;
                            $scope.difLocalAr = 0;
                            //行选中事件  
                            if ($scope.arGridApi.selection.getSelectedRows().length > 0) {
                                for (var i = 0; i < $scope.arGridApi.selection.getSelectedRows().length; i++) {
                                    $scope.sumAr = $scope.accAdd($scope.sumAr, $scope.arGridApi.selection.getSelectedRows()[i].amt);
                                    $scope.sumLocalAr = $scope.accAdd($scope.sumLocalAr, $scope.arGridApi.selection.getSelectedRows()[i].local_AMT);
                                }
                                $scope.difAr = $scope.accSub($scope.sumBank, $scope.sumAr);
                                $scope.difLocalAr = $scope.accSub($scope.sumBank, $scope.sumLocalAr);
                            }
                        });
                    }
                };

                $scope.loadArByBank = function (rows) {
                    var customerList = [];
                    for (var i in rows) {
                        var customer = {};
                        customer.legalEntity = rows[i].legalEntity;
                        customer.customerNum = rows[i].customeR_NUM;
                        customerList.push(customer);
                    }
                    caHisDataProxy.getReconArDetails(customerList, function (result) {
                        if (null != result.dataRows && result.dataRows.length > 0) {
                            $scope.arList = result.dataRows;
                        } else {
                            $scope.arList = [];
                        }
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.groupExport = function () {
                    var rows = $scope.bankGridApi.selection.getSelectedRows();
                    var customerList = [];
                    for (var i in rows) {
                        var customer = {};
                        customer.legalEntity = rows[i].legalEntity;
                        customer.customerNum = rows[i].customeR_NUM;
                        customerList.push(customer);
                    }
                    caHisDataProxy.groupExport(customerList, function (result) {
                        window.open(APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + result, "_blank");
                    }, function (error) {
                        alert(error);
                    });
                };

                // ar grid end
                $scope.accAdd = function (arg1, arg2) {
                    var r1, r2, m;
                    try {
                        r1 = arg1.toString().split(".")[1].length
                    } catch (e) {
                        r1 = 0
                    }
                    try {
                        r2 = arg2.toString().split(".")[1].length
                    } catch (e) {
                        r2 = 0
                    }
                    m = Math.pow(10, Math.max(r1, r2));
                    return (arg1 * m + arg2 * m) / m;
                }

                $scope.accSub = function (arg1, arg2) {
                    var r1, r2, m;
                    try {
                        r1 = arg1.toString().split(".")[1].length
                    } catch (e) {
                        r1 = 0
                    }
                    try {
                        r2 = arg2.toString().split(".")[1].length
                    } catch (e) {
                        r2 = 0
                    }
                    m = Math.pow(10, Math.max(r1, r2));
                    return (arg1 * m - arg2 * m) / m;
                }

                $scope.ungroup = function () {
                    if (confirm("Ungroup these bankStatements ,continue ?")) {
                        if ($scope.reconGridApi.selection.getSelectedRows().length > 0) {
                            $scope.reconIds = [];
                            angular.forEach($scope.reconGridApi.selection.getSelectedRows(), function (rowItem) {
                                $scope.reconIds.push(rowItem.reconid);
                            });
                            caHisDataProxy.ungroup($scope.reconIds, function (result) {
                                alert("Operation success!");
                                $scope.reconGridInit(localStorage.getItem('reconTaskId'));
                                $scope.reconBankGridInit(localStorage.getItem('reconTaskId'));
                                $scope.arList = [];
                            }, function (error) {
                                alert(error);
                            });
                        } else {
                            alert('Please at least select one Item to Ungroup');
                        }
                    }                    
                };

                $scope.group = function () {
                    var data = {};
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0 && $scope.arGridApi.selection.getSelectedRows().length > 0) {
                        var msg = "";
                        if ($scope.sumBank == $scope.sumAr || $scope.sumBank == $scope.sumLocalAr) {
                            msg = "Group these bankStatements ,continue ?";
                        } else {
                            msg = "The sum amounts of Bank Statement and AR are not the same. Group anyway?";
                        }
                        if (confirm(msg)) {
                            data.taskId = localStorage.getItem('reconTaskId');
                            var bankIds = [];
                            var arIds = [];
                            angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                                bankIds.push(rowItem.id);
                            });
                            angular.forEach($scope.arGridApi.selection.getSelectedRows(), function (rowItem) {
                                arIds.push(rowItem.invoicE_NUM);
                            });
                            data.bankIds = bankIds;
                            data.arIds = arIds;
                            caHisDataProxy.group(data, function (result) {
                                alert("Operation success!");
                                $scope.reconGridInit(localStorage.getItem('reconTaskId'));
                                $scope.reconBankGridInit(localStorage.getItem('reconTaskId'));
                                $scope.arList = [];
                            }, function (error) {
                                alert(error);
                            });
                        }
                    } else {
                        alert('Please at least select one Item to group');
                    }
                };

                $scope.SendPmtDetailMail = function () {
                    $scope.bankIds = [];
                    $scope.bankIds.push(0);
                    if ($scope.bankGridApi.selection.getSelectedRows().length > 0) {
                        angular.forEach($scope.bankGridApi.selection.getSelectedRows(), function (rowItem) {
                            $scope.bankIds.push(rowItem.id);
                        });
                        caCommonProxy.SendPmtDetailMail($scope.bankIds, function (e) {
                            alert('Send Mail Task Successed! Records:' + e);
                        }, function (error) {
                            alert(error);
                        });
                    } else {
                        alert('Please at least select one item to unknown');
                    }
                };
				
                $scope.goback = function () {
                    $location.path("/ca/index");
                }

                $scope.toUploadPage = function () {
                    $location.path("/ca/upload");
                }

                $scope.toTaskListPage = function () {
                    $location.path("/ca/reconTask");
                }

                $scope.actiontask = function () {
                    $location.path("/ca/actiontask");
                }

                $scope.export = function () {
                    if (localStorage.getItem('isReconAdjustment') == true || localStorage.getItem('isReconAdjustment') == 'true') {
                        caHisDataProxy.exporReconResultByBankIds(localStorage.getItem('reconAdjustIds'), function (fileId) {
                            // return fieid;
                            window.location = APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + fileId;
                        }, function (err) {
                        });
                    } else {
                        caHisDataProxy.exporReconResultByReconId(localStorage.getItem('reconTaskId'));
                    }
                }
            }]);