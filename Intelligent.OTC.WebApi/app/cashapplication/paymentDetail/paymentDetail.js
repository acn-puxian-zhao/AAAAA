angular.module('app.cashapplication.payment', ['ui.bootstrap'])
    .controller('paymentCtrl',
        ['$scope', '$uibModalInstance', '$filter','modalService','paymentDetailProxy', 'baseDataProxy', 'caCommonProxy','pmtID',
            function ($scope, $uibModalInstance, $filter,modalService, paymentDetailProxy, baseDataProxy, caCommonProxy, pmtID) {
                baseDataProxy.SysTypeDetail("015", function (list) {
                    $scope.legalList = list;
                });
                baseDataProxy.SysTypeDetail("089", function (list) {
                    $scope.currencyList = list;
                });
                $scope.pmtID = pmtID;
                $scope.pmtTransactionNumbers = {};
                $scope.pmtInvoiceNumbers = {}
                $scope.NoBankFlag = false;
                if ($scope.pmtID != '') {
                    paymentDetailProxy.queryPMTByID($scope.pmtID, function (result) {
                        $scope.receiveDate = result.receiveDate;
                        $scope.pmt = {
                            ReceiveDate: result.receiveDate,
                            LegalEntity: result.legalEntity,
                            CustomerNumber: result.customerNum,
                            ValueDate: result.valueDate,
                            Currency:result.currency,
                            Amount: result.amount,
                            BankCharge: result.bankCharge,
                            ID: result.id
                        }
                        $scope.pmtBSList = result.pmtBs;
                        for (let i = 0, len = result.pmtBs.length; i < len; i++) {
                            let item = result.pmtBs[i].transactionNumber;
                            $scope.pmtTransactionNumbers[item] = "1";
                        }

                        if ($scope.pmtBSList == null || $scope.pmtBSList.length == 0) {
                            var bs = {
                                currency: result.currency,
                                amount: result.amount,
                                transactionAmount: result.transactionAmount,
                                valueDate: result.valueDate,
                                bankCharge: result.bankCharge
                            }
                            $scope.NoBankFlag = true;
                            $scope.pmtBSList.push(bs);
                        }

                        $scope.pmtDetailList = result.pmtDetail;
                        for (let i = 0, len = result.pmtDetail.length; i < len; i++) {
                            let item = result.pmtDetail[i].invoiceNum;
                            $scope.pmtInvoiceNumbers[item] = "1";
                        }

                        
                        $scope.customerName = result.customerName;
                    })
                } else {
                    $scope.pmt = {};
                    $scope.pmtBSList = [];
                    $scope.pmtDetailList = [];
                    caCommonProxy.getCARegionByCurrentUser(
                        function (result) {
                            $scope.pmt.menuregion = result;
                        },
                        function (error) {
                            alert(error);
                        }
                    );
                }   

                $scope.pmtBSEditGrid = {
                    enableFullRowSelection: false, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中                  
                    multiSelect: false,// 是否可以选择多个,默认为true;
                    noUnselect: false,//default为false,选中后是否可以取消选中
                    //   noUnselect: true,
                    data: 'pmtBSList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '5%', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },                       
                        {field: 'transactionNumber', displayName: 'Transaction Num', width: '20%'},
                        { field: 'valueDate', displayName: 'Date', width: '20%', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                        { field: 'currency', displayName: 'Currency', width: '10%', enableCellEdit: false },
                        { field: 'transactionAmount', displayName: 'Transaction Amount', width: '15%', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'amount', displayName: 'Clear Amount', width: '15%', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'bankCharge', displayName: 'Bank Charge', width: '15%', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'description', displayName: 'Description', width: '25%', enableCellEdit: false },
                        { field: 'operation', displayName: '', width: '5%', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.deletePMTBS(grid.renderContainers.body.visibleRowCache.indexOf(row), row.entity.transactionNumber)">delete</a></span>' }

                    ],
                    onRegisterApi: function (gridApi) {
                        //set gridApi on scope
                        $scope.pmtBSEditGridApi = gridApi;

                        gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                            //var rowIndex = grid.renderContainers.body.visibleRowCache.indexOf(row);
                            $scope.bs = {
                                transactionNumber: row.transactionNumber,
                                amount: row.amount
                            };                            
                        });

                    }
                };

                $scope.pmtDetailEditGrid = {
                    enableFullRowSelection: false, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                    enableRowHeaderSelection: false, //是否显示选中checkbox框 ,default为true                   
                    multiSelect: false,// 是否可以选择多个,默认为true;
                    noUnselect: true,//default为false,选中后是否可以取消选中
                    //   noUnselect: true,
                    data: 'pmtDetailList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '5%', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'siteUseId', displayName: 'Site Use Id', width: '10%', enableCellEdit: false },
                        { field: 'invoiceNum', displayName: 'Invoice Number', width: '10%' },
                        { field: 'dueDate', displayName: 'Date', width: '10%', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center', enableCellEdit: false },
                        { field: 'currency', displayName: 'Local Currency', width: '5%' },
                        { field: 'amount', displayName: 'Clear Amount', width: '15%', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'localCurrency', displayName: 'Inv Currency', width: '5%' },
                        { field: 'localAmount', displayName: 'Inv Amount', width: '15%', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                        { field: 'description', displayName: 'Description', width: '20%' },
                        { field: 'operation', displayName: '', width: '5%', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.deletePMTDetail(grid.renderContainers.body.visibleRowCache.indexOf(row), row.entity.invoiceNum)">delete</a></span>'}

                    ],
                    onRegisterApi: function (gridApi) {
                        //set gridApi on scope
                        $scope.pmtDetailEditGridApi = gridApi;

                        gridApi.selection.on.rowSelectionChanged($scope, function (row) {
                            //var rowIndex = grid.renderContainers.body.visibleRowCache.indexOf(row);
                            $scope.invoice = {
                                invoiceNumber: row.InvoiceNum,
                                amount: row.Amount
                            };
                        });
                    }
                };

                $scope.addPMTBS = function () {
                    var valueDateStr = $scope.valueDate;
                    $scope.bs.valueDate = new Date(valueDateStr).format('yyyy-MM-dd');
                    if (!$scope.bs.transactionNumber) {
                        if ($scope.NoBankFlag) {
                            alert("Cannot exist more than one Data without Transaction Number.");
                            return false;
                        } else {
                            if ($scope.pmtBSList.length > 0) {
                                alert("The Bank Statement info cannot be without Transaction Number.");
                                return false;
                            }
                            
                        }
                        $scope.pmt.Currency = $scope.bs.currency;
                        $scope.pmt.Amount = $scope.bs.amount;
                        $scope.pmt.ValueDate = $scope.bs.valueDate;
                        $scope.pmt.BankCharge = $scope.bs.bankCharge;
                        $scope.pmt.TransactionAmount = $scope.bs.transactionAmount;
                        $scope.NoBankFlag = true;

                    } else {
                        if ($scope.pmt.Currency && $scope.pmt.Amount && $scope.pmt.ValueDate) {
                            alert("The data without Transaction Number has been entered, no other Bank Statement winth Transaction Number can be entered.");
                            return false;
                        }
                        if ($scope.pmtTransactionNumbers.hasOwnProperty($scope.bs.transactionNumber)) {
                            alert("The bank statement info already exists.");
                            $scope.bs = {};
                            return false;
                        } else {
                            $scope.pmtTransactionNumbers[$scope.bs.transactionNumber] = 1;
                        }
                    } 

                    $scope.pmtBSList.push($scope.bs);
                    $scope.bs = {};
                }

                $scope.addPMTDetail = function () {

                    if ($scope.pmtInvoiceNumbers.hasOwnProperty($scope.invoice.invoiceNum)) {
                        alert("The invoice number already exists.");
                        $scope.invoice = {};
                        return false;
                    } else {
                        $scope.pmtInvoiceNumbers[$scope.invoice.invoiceNum] = 1;
                    }
                    $scope.pmtDetailList.push($scope.invoice);
                    $scope.invoice = {};

                }

                //根据customerNumber查customername
                $scope.getCustomerInfoByNo = function () {
                    if ($scope.pmt.CustomerNumber == null || $scope.pmt.CustomerNumber == "") {
                        $scope.customerName = "";
                        return false;
                    }

                    paymentDetailProxy.getCustomerByCustomerNum($scope.pmt.CustomerNumber, function (result) {
                        if (result) {
                            $scope.customerName = result.customerName;
                        } else {
                            alert("No customer item.");
                            $scope.pmt.CustomerNumber = "";
                            $scope.customerName = "";
                        }
                        
                    }, function (err) {
                            alert("No customer item.");
                            $scope.pmt.CustomerNumber = "";
                            $scope.customerName = "";
                    })
                };

                //根据bankNo查bank信息
                $scope.getBankInfoByTranINC = function () {
                    if ($scope.bs.transactionNumber == null || $scope.bs.transactionNumber == "") {
                        $scope.bs = {};
                        return false;
                    }

                    paymentDetailProxy.getBankStatementByTranINC($scope.bs.transactionNumber, function (result) {
                        if (result) {
                            $scope.bs.transactionNumber = result.transactioN_NUMBER;
                            $scope.bs.BANK_STATEMENT_ID = result.id;
                        } else {
                            alert("No bank statement item.");
                            $scope.bs = {};
                        }
                            
                    }, function (err) {
                            alert("No bank statement item.");
                            $scope.bs = {};
                    })
                };

                //根据invoiceNumber查invoice信息
                $scope.getInvoiceInfoByNo = function () {
                    if ($scope.invoice.invoiceNum == null || $scope.invoice.invoiceNum == "") {
                        $scope.invoice = {}
                        return false;
                    }
                    paymentDetailProxy.getInvoiceInfoByNum($scope.invoice.invoiceNum, function (result) {
                        if (result) {
                            $scope.invoice.invoiceNum = result.invoiceNum;
                            $scope.invoice.invoiceDate = result.invoiceDate;
                            $scope.invoice.dueDate = result.dueDate;
                            $scope.invoice.siteUseId = result.siteUseId;
                            $scope.invoice.legalEntity = result.legalEntity;
                            $scope.invoice.CUSTOMER_NUM = result.customeR_NUM;
                        } else {
                            alert("No invoice item.");
                            $scope.invoice = {};
                        }

                    }, function (err) {
                        alert("No invoice item.");
                        $scope.invoice = {};
                    })
                };

                $scope.deletePMTBS = function (index, transactionNum) {
                    if (transactionNum == null || transactionNum == "") {
                        $scope.pmt.Currency = "";
                        $scope.pmt.Amount = null;
                        $scope.pmt.ValueDate = null;
                        $scope.pmt.BankCharge = null;
                        $scope.NoBankFlag = false;
                    } else {
                        delete $scope.pmtTransactionNumbers[transactionNum];
                    }
                    $scope.pmtBSList.splice(index, 1);
                    
                }

                $scope.deletePMTDetail = function (index, invoiceNum) {
                    delete $scope.pmtInvoiceNumbers[invoiceNum];
                    $scope.pmtDetailList.splice(index, 1);
                }
                

                $scope.closeModal = function () {
                    $uibModalInstance.close();
                };

                $scope.save = function () {
                    if ($scope.NoBankFlag) {
                        $scope.pmtBSList.splice(0, 1);
                        $scope.NoBankFlag = false;
                    }

                    let params = {                     
                        CustomerNum: $scope.pmt.CustomerNumber,
                        ValueDate: $scope.pmt.ValueDate,
                        Currency: $scope.pmt.Currency,
                        Amount: $scope.pmt.Amount,
                        TransactionAmount: $scope.pmt.TransactionAmount,
                        BankCharge: $scope.pmt.BankCharge,
                        LegalEntity: $scope.pmt.LegalEntity,
                        ReceiveDate: $scope.receiveDate,
                        PmtBs: $scope.pmtBSList,
                        PmtDetail: $scope.pmtDetailList
                    }

                    if ($scope.pmt.ID) {
                        params.ID = $scope.pmt.ID;
                    }

                    paymentDetailProxy.savePMT(params, function (result) {
                        alert(result);
                        $uibModalInstance.close();
                    }, function (err) {
                        if (err.indexOf("deleteReconMsg:") >= 0) {
                            var rsp = err.substr(15);
                            var modalDefaults = {
                                templateUrl: 'app/cashapplication/paymentDetail/delConfirm.tpl.html',
                                controller: 'paymentDelConfirmCtrl',
                                resolve: {
                                    errmsg: function () {
                                        return rsp;
                                    }
                                },
                                windowClass: 'modalDialog'
                            };
                            modalService.showModal(modalDefaults, {}).then(function (result) {
                                if (result == "Yes") {
                                    params.deleteRecon = true;
                                    paymentDetailProxy.savePMT(params, function (result) {
                                        alert("Save Success.");
                                        $uibModalInstance.close();
                                    }, function (error) {
                                        alert(error);
                                    })
                                }
                            });
                        } else {
                            alert(err);
                        }
                    })
                };


            }])
    .controller('paymentDelConfirmCtrl', ['$scope', '$uibModalInstance', 'errmsg',
        function ($scope, $uibModalInstance, errmsg) {
            var errset = new Set(errmsg.split(";"));
            $scope.errmsgList = [...errset];
            $scope.confirm = function () {
                $uibModalInstance.close("Yes");
            }

            $scope.cancel = function () {
                $uibModalInstance.close("No");
            };
        }]);