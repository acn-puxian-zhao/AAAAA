angular.module('app.cashapplication.customerAttribute', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/customerattribute', {
                templateUrl: 'app/cashapplication/customerAttribute/customerAttribute-list.tpl.html',
                controller: 'customerAttributeListCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('customerAttributeListCtrl',
        ['$scope', '$interval', 'customerAttributeProxy', 'modalService',
            function ($scope, $interval, customerAttributeProxy, modalService) {

            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' }
            ];

                $scope.legalEntity = "";
                $scope.customerNum = "";

            // bank grid start
            $scope.startIndex = 0;
            $scope.selectedLevel = 20;  //下拉单页容量初始化
            $scope.itemsperpage = 20;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页  

            $scope.dataList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: true,
                enableRowSelection: false,
                enableSelectAll: false,
                enableRowHeaderSelection :false,
                columnDefs: [
                    { name: 'RowNo', displayName: '', pinnedLeft: true, enableFiltering: false, enableColumnMenu: false, enableSorting: false, enableHiding: false, width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;" >{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'operation', displayName: '', width: '10%', pinnedRight: true, enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;"><a href="javascript: void (0);" ng-click="grid.appScope.edit(row.entity)">Edit</a> | <a href="javascript: void (0);" ng-click="grid.appScope.delete(row.entity.id)">Delete</a></span>' },                    
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '70' },
                    { field: 'customeR_NUM', displayName: 'Customer Num', width: '100' },
                    { field: 'func_Currency', displayName: 'Func Currency', width: '80' },
                    { field: 'isFixedBankCharge', displayName: 'isFixedBankCharge', width: '10%' },  
                    { field: 'bankChargeFrom', displayName: 'BankChargeFrom', width: '10%' },
                    { field: 'bankChargeTo', displayName: 'BankChargeTo', width: '10%' },
                    { field: 'isEntryAndWiteOff', displayName: 'isEntryAndWiteOff', width: '10%' },                                     
                    { field: 'isJumpBankStatement', displayName: 'isJumpBankStatement', width: '10%' },
                    { field: 'isJumpSiteUseId', displayName: 'isJumpSiteUseId', width: '10%' },
                    { field: 'isMustPMTDetail', displayName: 'isMustPMTDetail', width: '10%' },
                    { field: 'isMustSiteUseIdApply', displayName: 'isMustSiteUseIdApply', width: '10%' },
                    { field: 'isNeedRemittance', displayName: 'isNeedRemittance', width: '10%' },
                    { field: 'isNeedVat', displayName: 'isNeedVat', width: '100' },
                    { field: 'isFactoring', displayName: 'IsFactoring', width: '10%' },                   
                    { field: 'creatE_User', displayName: 'Create User', width: '100' },
                    { field: 'creatE_Date', displayName: 'Create Date', width: '10%', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                    { field: 'modifY_User', displayName: 'Modify User', width: '100' },
                    { field: 'modifY_Date', displayName: 'Modify Date', width: '10%', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' }

                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            ////Detail单页容量变化
            $scope.pageSizeChange = function (selectedLevelId) {
                customerAttributeProxy.getCustomerAttribute($scope.currentPage, selectedLevelId, $scope.legalEntity, $scope.customerNum, function (result) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = result.listCount;
                    $scope.dataList.data = result.list;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                    $scope.calculate($scope.currentPage, $scope.itemsperpage, result.list.length);
                })
            };

            //Detail翻页
            $scope.pageChanged = function () {
                customerAttributeProxy.getCustomerAttribute($scope.currentPage, $scope.itemsperpage, $scope.legalEntity, $scope.customerNum, function (result) {
                    $scope.totalItems = result.listCount;
                    $scope.dataList.data = result.list;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                    $scope.calculate($scope.currentPage, $scope.itemsperpage, result.list.length);
                }, function (error) {
                    alert(error);
                });
            };

                $scope.pageChanged();

                $scope.calculate = function (currentPage, itemsperpage, count) {
                    if (count == 0) {
                        $scope.fromItem = 0;
                    } else {
                        $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
                    }
                    $scope.toItem = (currentPage - 1) * itemsperpage + count;
                }

                $scope.resetSearch = function () {
                    $scope.legalEntity = "";
                    $scope.customerNum = "";
                    $scope.init();
                }

                $scope.init = function () {
                    $scope.pageChanged();
                }

            $scope.add = function () {
                $scope.editModal();
            };
           
            $scope.edit = function (row) {
               
                    $scope.editModal(row);
               
            };

            $scope.editModal = function (row) {

                var modalDefaults = {
                    templateUrl: 'app/cashapplication/customerAttribute/customerAttribute-edit.tpl.html?2',
                    controller: 'customerAttributeEditCtrl',
                    size: 'customSize',
                    resolve: {
                        cont: function () {
                            return row;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function () {
                    $scope.pageChanged();
                });
            };

            $scope.delete = function (id) {
                
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                           
                            customerAttributeProxy.deleteCustomerAttribute(id, function () {
                                $scope.pageChanged();
                            }, function (error) {
                                alert(error);
                            })
                        }
                    });
               
            }


            $scope.export = function () {
                
                customerAttributeProxy.export();
            };
        }])

    .controller('customerAttributeEditCtrl', ['$scope', '$uibModalInstance', 'cont', 'customerAttributeProxy','custAndBankCustProxy',
        function ($scope, $uibModalInstance, cont, customerAttributeProxy, custAndBankCustProxy) {             

            $scope.typeList = [
                { "id": 'Yes', "name": 'Yes' },
                { "id": 'No', "name": 'No' },
                { "id": 'Factoring', "name": 'Factoring' }
            ];

            $scope.error = false;
            $scope.cont = cont;
            if (cont == null) {
                $scope.cont = {};
                $scope.cont.isFixedBankCharge = false;
                $scope.cont.isJumpBankStatement = false;
                $scope.cont.isJumpSiteUseId = false;
                $scope.cont.isMustSiteUseIdApply = false;
                $scope.cont.isNeedVat = false;
                $scope.cont.isFactoring = false;
            }
           

            $scope.closeModal = function () {
                $uibModalInstance.close();
            };

            $scope.isFixed = function () {
                if ($scope.cont.isFixedBankCharge) {
                    $scope.readonlyFlag = true;
                    $scope.cont.bankChargeTo = $scope.cont.bankChargeFrom;
                } else {
                    $scope.readonlyFlag = false;
                }
            };

            $scope.synBankCharge = function () {
                if ($scope.cont.isFixedBankCharge) {
                    $scope.readonlyFlag = true;
                    $scope.cont.bankChargeTo = $scope.cont.bankChargeFrom;
                } 
            };


            $scope.save = function () {

                    num = $scope.cont.customeR_NUM;
                    legalEntity = $scope.cont.legalEntity;
                    if (num) {
                        custAndBankCustProxy.getCustomerName(num, legalEntity,function (result) {
                            if (!result) {
                                $scope.error = true;
                                alert("Please enter the correct customerNum");
                            } else {
                                customerAttributeProxy.updateCustomerAttribute($scope.cont,
                                    function () {
                                        $uibModalInstance.close();
                                    },
                                    function (res) {
                                        alert(res);
                                    });
                            }
                        }, function (error) {
                            alert(error);
                        });
                    } else {
                        $scope.error = true;
                        alert("Please enter the customerNum");
                    }


            };

        }])
    ;




