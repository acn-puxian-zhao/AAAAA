angular.module('app.agentcustomer', ['ui.bootstrap'])

    .controller('agentCustomerCtrl', 
    // 'siteuseid','legalentity',
        ['$scope', '$uibModalInstance', 'caHisDataProxy','entity',
        //siteuseid,legalentity,
            function ($scope, $uibModalInstance, caHisDataProxy, entity) {
            var result = "";
            $scope.type = 1;
            $scope.alertMessage = "";
                var dataList = [];
                dataList[0] = entity;
            //分页容量下拉列表定义
            $scope.levelList = [
                { "id": 20, "levelName": '20' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' }
            ];

            $scope.bankGidOptions = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: false,
                data: 'bankList',
                columnDefs: [
                    { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'id', displayName: 'id', width: '120', visible: false },
                    {
                        field: 'matcH_STATUS_NAME', displayName: 'Match Status', width: '100'
                    },
                    { field: 'legalEntity', displayName: 'LegalEntity', width: '80', cellClass: 'center' },
                    { field: 'transactioN_NUMBER', displayName: 'Transaction Number', width: '100' },
                    { field: 'transactioN_AMOUNT', displayName: 'Transaction Amount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'currency', displayName: 'Currency', width: '60', cellClass: 'center' },
                    { field: 'valuE_DATE', displayName: 'Value Date', width: '80', cellFilter: 'date:\'yyyy-MM-dd\'', cellClass: 'center' },
                    { field: 'forwarD_NUM', displayName: 'Payer Number', width: '90', cellClass: 'center' },
                    { field: 'forwarD_NAME', displayName: 'Payer Name', width: '120' },
                    { field: 'customeR_NUM', displayName: 'Customer Number', width: '90', cellClass: 'center' },
                    { field: 'customeR_NAME', displayName: 'Customer Name', width: '120' },
                    //{ field: 'siteUseId', displayName: 'Site Use ID', width: '120' },
                    { field: 'reF1', displayName: 'Description', width: '120', enableCellEdit: false },
                    //{ field: 'referencE2', displayName: 'Reference 2', width: '120', enableCellEdit: false },
                    //{ field: 'referencE3', displayName: 'Reference 3', width: '120', enableCellEdit: false },
                    { field: 'bankChargeFrom', displayName: 'Charge From', width: '90', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                    { field: 'bankChargeTo', displayName: 'Charge To', width: '90', cellFilter: 'number:0', type: 'number', cellClass: 'right' },
                            //{
                        //    field: 'isFixedBankCharge', displayName: 'Charge Type', width: '120', cellFilter: 'mapChargeType'
                        //},
                        //{ field: 'updatE_DATE', displayName: 'Last Modified Time', width: '120', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' },
                        //{ field: 'creatE_DATE', displayName: 'Create Time', width: '120', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'' }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.bankGridApi = gridApi;

                    $scope.bankList = dataList;
                }
            };
                

            // customer grid start
            $scope.startIndex = 0;
            $scope.selectedLevel = 5000;  //下拉单页容量初始化
            $scope.itemsperpage = 5000;
            $scope.currentPage = 1; //当前页
            $scope.maxSize = 10; //分页显示的最大页  

            $scope.gridOptions = {
                multiSelect: false,
                enableFullRowSelection: true,
                enableFiltering: true,
                noUnselect: false,
                data: 'list',
                columnDefs: [
                    { name: 'RowNo', displayName: '', width: '40', cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'customerNum', displayName: 'Customer Number', width: '120', cellClass: 'center'  },
                    { field: 'customerName', displayName: 'Customer Name', width: '600' }
                    //{ field: 'menuregion', displayName: 'Region', width: '30%' }
 

                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            //Detail单页容量变化
 

                $scope.pageSizeChange = function (selectedLevelId) {
 

                    caHisDataProxy.getAgentCustomerDataDetails($scope.currentPage, selectedLevelId, entity, $scope.type, function (result) {
 

                    $scope.itemsperpage = selectedLevelId;
 

                    $scope.totalItems = result.listCount;
 

                    $scope.list = result.list;
 

                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                })
            };

            //Detail翻页
 

                $scope.pageChanged = function () {
 

                caHisDataProxy.getAgentCustomerDataDetails($scope.currentPage, $scope.itemsperpage, entity, $scope.type, function (result) {
 

                    $scope.totalItems = result.listCount;
 

                    $scope.list = result.list;
 

                    $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                }, function (error) {
                    alert(error);
                });
            };


            $scope.typeChanged = function () {  

                $scope.totalItems = 0;
                $scope.list = [];
                $scope.pageChanged();
            };

            $scope.pageChanged();
            // customer grid end

            

            $scope.cancel = function () {
                result = "cancel";
                $uibModalInstance.close(result);
            };

            $scope.submit = function () {
                var result = [];
                result.push('submit');

                var row = $scope.gridApi.selection.getSelectedRows()[0];

                if (!row) {
                    return;
                }

                if (entity.matcH_STATUS >= 4) {
                    alert("This operation is not available in the current state!");
                    $uibModalInstance.close(result);
                    return;
                }

                if (entity.forwarD_NUM != row.customerNum && entity.customeR_NUM != null && entity.customeR_NUM != '') {
                    
                    entity.customeR_NUM = ''
                    entity.customeR_NAME = ''
                    caHisDataProxy.deletePmtBs(entity.id, function () { });
                    entity.matcH_STATUS = 0;
                    entity.matcH_STATUS_NAME = 'Unknown';
                }

                if (row.customerNum == '' && row.customerName == '') {
                    entity.matcH_STATUS = 0;
                    entity.matcH_STATUS_NAME = 'Unknown';
                }

                entity.forwarD_NUM = row.customerNum;
                entity.forwarD_NAME = row.customerName;
                entity.siteUseId = "";

                caHisDataProxy.bankRowSave(entity,function () {
                        alert("save success!");
                        $uibModalInstance.close(result);                   
                });
            }

        }])
