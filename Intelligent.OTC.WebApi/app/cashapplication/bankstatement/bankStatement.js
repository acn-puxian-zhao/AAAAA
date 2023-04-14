angular.module('app.cashapplication.bankstatement', ['ui.bootstrap'])
    .controller('bankStatementCtrl',
        ['$scope', '$uibModalInstance', '$filter', 'caHisDataProxy', 'baseDataProxy', 'caCommonProxy','bank',
            function ($scope, $uibModalInstance,$filter, caHisDataProxy, baseDataProxy, caCommonProxy, bank) {
                $scope.bank = bank;
                //console.log(bank);
                baseDataProxy.SysTypeDetail("088", function (list) {
                    $scope.matchStatusList = list;
                });

                baseDataProxy.SysTypeDetail("085", function (list) {
                    $scope.bstypeList = list;
                });

                baseDataProxy.SysTypeDetail("015", function (list) {
                    $scope.legalList = list;
                });

                baseDataProxy.SysTypeDetail("089", function (list) {
                    $scope.currencyList = list;
                });

                if (!$scope.bank.MATCH_STATUS){
                    $scope.bank.MATCH_STATUS = "-1";
                }

                if ($scope.bank.VALUE_DATE) {
                    var valueDateStr = $filter('date')($scope.bank.VALUE_DATE, 'yyyy-MM-dd');
                    $scope.valueDate = valueDateStr;
                    $("#valueDate").val(valueDateStr);
                }
                if ($scope.bank.PMTReceiveDate) {
                    var valueDateStr = $filter('date')($scope.bank.PMTReceiveDate, 'yyyy-MM-dd');
                    $scope.bank.PMTReceiveDate = valueDateStr;
                    console.log(valueDateStr);
                    console.log($scope.bank.PMTReceiveDate);
                }

                if (!$scope.bank.ISHISTORY) {
                    $scope.bank.ISHISTORY = 0;
                }
                $scope.$watch('$viewContentLoaded', function () {
                    if ($scope.bank.ISHISTORY == 0) {
                        $("#ishistoryNoLabel").click();
                    } else if ($scope.bank.ISHISTORY == 1) {
                        $("#ishistoryYesLabel").click();
                    }  
                }); 
                

                $scope.closeModal = function () {
                    $uibModalInstance.close();
                };

                $scope.isExistedTransactionNum = function () {
                    caHisDataProxy.isExistedTransactionNum($scope.bank.ID, $scope.bank.TRANSACTION_NUMBER, function (result) {
                        if (result > 0) {
                            alert("The transaction number existed.");
                            $scope.bank.TRANSACTION_NUMBER = "";
                            return;
                        }
                    })
                }

                $scope.changeHistory = function (value) {
                    $scope.bank.ISHISTORY = value;
                }

                $scope.save = function () {
                    let valueDateStr = $("#valueDate input").val();
                    if (!$scope.bank.TRANSACTION_NUMBER || !$scope.bank.TRANSACTION_AMOUNT 
                        || !$scope.bank.CURRENCY || !$scope.bank.Description || !valueDateStr
                        || !$scope.bank.LegalEntity
                        || !$scope.bank.BSTYPE) {
                        alert("Input canot be null");
                        return;
                    }
                    if (!$scope.bank.CURRENT_AMOUNT) {
                        $scope.bank.CURRENT_AMOUNT = 0;
                    }
                    if ($scope.bank.Description) {
                        if ($scope.bank.Description.length > 1000) {
                            alert("Description max length is 1000 chars.");
                            return;
                        }
                    };
                    /**
                    if ($scope.bank.ReceiptsMethod && !$scope.isRealNum($scope.bank.ReceiptsMethod)) {
                        $scope.bank.ReceiptsMethod = "";
                        alert("The ReceiptsMethod must be number!");
                        return;
                    }*/

                    /**
                    if ($scope.bank.BankAccountNumber && !$scope.isRealNum($scope.bank.BankAccountNumber)) {
                        $scope.bank.BankAccountNumber = "";
                        alert("The BankAccountNumber must be number!");
                        return;
                    }*/

                    if ($scope.bank.BankChargeFrom && $scope.bank.BankChargeTo) {
                        if ($scope.bank.BankChargeFrom > $scope.bank.BankChargeTo) {
                            alert("The Charge From cannot be less than the Charge To!");
                            return false;
                        }
                    } else if ((!$scope.bank.BankChargeFrom && $scope.bank.BankChargeTo)
                        || ($scope.bank.BankChargeFrom && !$scope.bank.BankChargeTo)) {
                        alert("Charge From and Charge To must not be empty at the same time.");
                        return false;
                    }

                    //校验amount
                    //校验时间
                    $scope.bank.VALUE_DATE = new Date(valueDateStr).format('yyyy-MM-dd');

                    caHisDataProxy.saveBankStatement($scope.bank,
                        function () {
                            alert("Save Success.");
                            $uibModalInstance.close();
                        },
                        function (res) {
                            alert(res);
                        });
                };

                $scope.isRealNum = function(val) {
                    // isNaN()函数 把空串 空格 以及NUll 按照0来处理 所以先去除
                    if (val === "" || val == null) {
                        return false;
                    }
                    if (!isNaN(val)) {
                        return true;
                    } else {
                        return false;
                    }
                }


            }])