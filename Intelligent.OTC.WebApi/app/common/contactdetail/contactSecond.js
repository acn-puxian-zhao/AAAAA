angular.module('app.common.contactdetailSecond', ['ui.bootstrap'])
    .controller('contactPtpSecondCtrl',
    ['$scope', 'id' , 'siteUseId', 'customerNo', 'isPartialPay', 'promiseDate', 'promiseAmount', 'ptpStatus', 'comments', '$uibModalInstance', 'contactProxy', 'collectorSoaProxy', 'baseDataProxy', 'PTPPaymentProxy',
        function ($scope, id, siteUseId, customerNo, isPartialPay, promiseDate, promiseAmount, ptpStatus, comments, $uibModalInstance, contactProxy, collectorSoaProxy, baseDataProxy, PTPPaymentProxy) {

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

            baseDataProxy.SysTypeDetail("010", function (list) {
                $scope.payMethodList = list;
            });

            baseDataProxy.SysTypeDetail("039", function (list) {
                $scope.ptpStatusList = list;
                //console.log(list);
            })

            $scope.promiseDate = new Date(promiseDate);
            $scope.partialpay = isPartialPay.toString();
            $scope.proamount = promiseAmount;
            $scope.ptpStatusV = ptpStatus;
            $scope.comss = comments;

            $scope.cancel = function () {
                result = "cancel";
                $uibModalInstance.close(result);
            };

            $scope.dateOptions = {
                //dateDisabled: disabled,
                formatYear: 'yy',
                maxDate: new Date(2099, 5, 22),
                //minDate: new Date(),
                startingDay: 1
            };
            
            $scope.openPromiseDate = function () {
                $scope.popupPromiseDate.opened = true;
            };
            $scope.popupPromiseDate = {
                opened: false
            };


            var ptpModel = {};
            $scope.submit = function () {
                if ($scope.promiseDate == "") {
                    alert("Please choose promise date !");
                    return;
                }
                else
                {
                    ptpModel.id = id;
                    ptpModel.promiseDate = $scope.promiseDate;
                    ptpModel.isPartialPay = $scope.partialpay;
                    ptpModel.promissAmount = $scope.proamount;
                    ptpModel.ptpStatus = $scope.ptpStatusV;
                    ptpModel.payer = $scope.payer;
                    ptpModel.paymentMethod = $scope.paymethod;
                    ptpModel.contact = $scope.contacter;
                    ptpModel.Comments = $scope.comss;
                    ptpModel.IsForwarder = $scope.IsForwarder;
                    PTPPaymentProxy.updatePTPPayment(ptpModel, function (res) {
                        alert(res);
                        result = "submit";
                        $uibModalInstance.close(result);
                    }, function (res) {
                        alert(res);
                    });
                }
                
            };

        }])
    