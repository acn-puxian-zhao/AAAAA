angular.module('app.cashapplication.custAndBankCust', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/ca/bankcustomer', {
                templateUrl: 'app/cashapplication/custAndBankCust/custAndBankCust-list.tpl.html',
                controller: 'custAndBankCustListCtrl'
            });
    }])

    //*****************************************header***************************s
    .controller('custAndBankCustListCtrl',
        ['$scope', '$interval', 'custAndBankCustProxy', 'modalService',
        function ($scope, $interval, custAndBankCustProxy, modalService) {

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
            $scope.bankCustomerName = "";

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
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '10%' },
                    { field: 'customerNum', displayName: 'CustomerNum', width: '10%' },                   
                    { field: 'customerName', displayName: 'CustomerName', width: '30%' },
                    { field: 'localizeCustomerName', displayName: 'LocalizeCustomerName', width: '25%' },
                    { field: 'bankCustomerName', displayName: 'BankCustomerName', width: '25%' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            ////Detail单页容量变化
            $scope.pageSizeChange = function (selectedLevelId) {
                custAndBankCustProxy.getCustomerMapping($scope.currentPage, selectedLevelId, $scope.legalEntity, $scope.customerNum, $scope.bankCustomerName, function (result) {
                    $scope.itemsperpage = selectedLevelId;
                    $scope.totalItems = result.listCount;
                    $scope.dataList.data = result.list;
                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;

                    $scope.calculate($scope.currentPage, $scope.itemsperpage, result.list.length);
                })
            };

            //Detail翻页
            $scope.pageChanged = function () {
                custAndBankCustProxy.getCustomerMapping($scope.currentPage, $scope.itemsperpage, $scope.legalEntity, $scope.customerNum, $scope.bankCustomerName, function (result) {
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
                $scope.bankCustomerName = "";
                $scope.init();
            }

            $scope.init = function () {
                $scope.pageChanged();
            }

            $scope.add = function () {
                $scope.editModal({});
            };
           
            $scope.edit = function (row) {
               
                    $scope.editModal(row);
               
            };

            $scope.editModal = function (row) {

                var modalDefaults = {
                    templateUrl: 'app/cashapplication/custAndBankCust/custAndBankCust-edit.tpl.html?2',
                    controller: 'custAndBankCustEditCtrl',
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
                           
                            custAndBankCustProxy.deleteCustomerMapping(id, function () {
                                $scope.pageChanged();
                            }, function (error) {
                                alert(error);
                            })
                        }
                    });
               
            }


            $scope.export = function () {
                
                custAndBankCustProxy.export();
            };
        }])

    .controller('custAndBankCustEditCtrl', ['$scope', '$uibModalInstance','cont', 'custAndBankCustProxy',
        function ($scope, $uibModalInstance,cont, custAndBankCustProxy) {             
           
            $scope.cont = cont;
            $scope.error = '';


            $scope.closeModal = function () {
                $uibModalInstance.close();
            };

            $scope.searchCustomer = function () {

                num = $scope.cont.customerNum;
                legalEntity = $scope.cont.legalEntity;
                if (num && legalEntity) {
                    $scope.error = '';
                    custAndBankCustProxy.getCustomerName(num, legalEntity, function (result) {
                        if (result) {
                            $scope.cont.customerName = result.customerName;
                            $scope.cont.localizeCustomerName = result.localizeCustomerName;
                        } else {
                            $scope.error = 'customerNum Non-existent!';
                        }

                    }, function (error) {
                        alert(error);
                    });
                }

            };

            $scope.save = function () {

                if (!$scope.cont.customerNum || !$scope.cont.customerName || !$scope.cont.bankCustomerName || !$scope.cont.legalEntity ) {
                    alert("Input canot be null");
                    return;
                }

                custAndBankCustProxy.updateCustomer($scope.cont,
                    function () {
                        $uibModalInstance.close();
                    },
                    function (res) {
                        alert(res);
                    });
            };

        }])
    ;




