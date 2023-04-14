angular.module('app.camailalert', ['ui.bootstrap'])

    .controller('camailalertCtrl',
        ['$scope', 'id', 'alerttype', 'caHisDataProxy', 'mailProxy', '$routeParams', 'modalService', '$interval', '$uibModalInstance', '$sce', '$q', 
            function ($scope, id, alerttype, caHisDataProxy, mailProxy, $routeParams, modalService, $interval, $uibModalInstance, $sce, $q) {

                $scope.id = id;
                $scope.alerttype = alerttype;

                $scope.setTtitle = function () {
                    if (alerttype == "006") {
                        $scope.title = "Mail - Post Confirm";
                    }
                    if (alerttype == "008") {
                        $scope.title = "Mail - Clear Confirm";
                    }
                };
                $scope.setTtitle();

                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                // grid start
                $scope.startIndex = 0;
                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1; //当前页
                $scope.maxSize = 10; //分页显示的最大页  
                $scope.totalItems = 0;

                $scope.mailAlertDataList = {
                    showGridFooter: true,
                    enableFullRowSelection: false, //是否点击行任意位置后选中,default为false,当为true时,checkbox可以显示但是不可选中
                    enableSelectAll: true, // 选择所有checkbox是否可用，default为true;
                    enableSelectionBatchEvent: true, //default为true
                    multiSelect: true,// 是否可以选择多个,默认为true;
                    noUnselect: false,//default为false,选中后是否可以取消选中
                    enableFiltering: true,
                    data: 'mailAlertList',
                    columnDefs: [
                        { name: 'RowNo', displayName: '', width: '40', enableFiltering: false, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                        { field: 'operation', displayName: '', width: '80', enableFiltering: false, enableSorting: false, enableHiding: false, enableColumnMenu: false, cellTemplate: '<span ng-if="row.entity.status==\'Initialized\'" style="line-height:30px;vertical-align:middle;text-align:left;padding-left:10px;display:block;"><a href="javascript:void(0);" ng-click="grid.appScope.cancelMail(row.entity.id)">Cancel</a></span>' },
                        { field: 'status', name: 'Status', displayName: 'Status', width: '120', enableCellEdit: false, enableFiltering: false },
                        { field: 'eid', name: 'EID', displayName: 'EID', width: '80', enableFiltering: false },
                        { field: 'transNumber', name: 'TransNumber', displayName: 'TransNumber', width: '90', enableFiltering: false },
                        { field: 'totitle', name: 'ToTitle', displayName: 'ToTitle', width: '100', enableFiltering: false },
                        { field: 'cctitle', name: 'CCTitle', displayName: 'CCTitle', width: '100', enableFiltering: false },
                        { field: 'mailto', name: 'MailTo', displayName: 'MailTo', width: '100', enableFiltering: false },
                        { field: 'mailcc', name: 'MailCC', displayName: 'MailCC', width: '100', enableFiltering: false },
                        { field: 'subject', name: 'Subject', displayName: 'Subject', width: '200', enableFiltering: false, cellTemplate: '<a href="javascript:void(0);" ng-click="grid.appScope.showMail(row.entity.messageId)">{{row.entity.subject}}</a>'},
                        { field: 'createTime', displayName: 'CreateTime', width: '120', enableCellEdit: false, enableFiltering: false , cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'sendTime', displayName: 'SendTime', width: '120', enableCellEdit: false, enableFiltering: false, cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', cellClass: 'center' },
                        { field: 'comment', name: 'Comment', displayName: 'Comment', width: '200', enableFiltering: false }
                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.mailGridApi = gridApi;

                        $scope.pageChanged();
                    }
                };

                //Detail单页容量变化
                $scope.pageSizeChange = function (selectedLevelId) {
                    caHisDataProxy.getCaMailAlertListbybsid($scope.id, $scope.alerttype, $scope.currentPage, $scope.itemsperpage, function (result) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.totalItems = result.count;
                        $scope.mailAlertList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                        $scope.calculate($scope.currentPage, $scope.itemsperpage, result.dataRows.length);
                    }, function (error) {
                        alert(error);
                    });
                };

                //Detail翻页
                $scope.pageChanged = function () {
                    caHisDataProxy.getCaMailAlertListbybsid($scope.id, $scope.alerttype, $scope.currentPage, $scope.itemsperpage, function (result) {
                        $scope.totalItems = result.count;
                        $scope.mailAlertList = result.dataRows;
                        $scope.startIndex = ($scope.currentPage - 1) * $scope.itemsperpage;
                        $scope.mailGridApi.selection.clearSelectedRows();
                        $scope.calculate($scope.currentPage, $scope.itemsperpage, result.dataRows.length);
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.showMail = function (mid) {
                    mailProxy.queryObject({ messageId: mid }, function (mailInstance) {
                        //mailType
                        mailInstance["title"] = "Mail View";
                        mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);

                        var modalDefaults = {
                            templateUrl: 'app/common/mail/mail-instance.tpl.html',
                            controller: 'mailInstanceCtrl',
                            size: 'customSize',
                            resolve: {
                                custnum: function () { return mailInstance.customerNum; },
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

                $scope.cancelMail = function (id){
                    caHisDataProxy.cancelCaMailAlertbyid(id, function (result) {
                        $scope.pageChanged();
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

                $scope.popup = {
                    opened: false
                };

                $scope.open = function () {
                    $scope.popup.opened = true;
                };

                var result = [];
                $scope.cancel = function () {
                    result = "cancel";
                    $uibModalInstance.close(result);
                };

                $scope.submit = function () {
                    taskProxy.saveTaskDispute(id, status, $scope.taskContent, function () {
                        result.push('submit');
                        $uibModalInstance.close(result);
                    });
                };

            }]);
