angular.module('app.masterdata.ebsetting', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/admin/ebsetting', {
                templateUrl: 'app/masterdata/ebsetting/ebsetting-list.tpl.html',
                controller: 'ebsettingListCtrl',
                resolve: {
                    //首次加载第一页
                    statuslist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("005");
                    }],
                    custPaging: ['customerProxy', function (customerProxy) {
                        var now = new Date().format("yyyy-MM-dd");
                        return customerProxy.customerPaging(1, 20, "", "");
                    }],
                    internallist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("024");
                    }],
                    regionlist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("044");
                    }],
                    languagelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("013");
                    }],
                    legalEntitylist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("015");
                    }]
                }
            });
    }])


    //*****************************************header***************************s
    .controller('ebsettingListCtrl',
        ['$scope', '$interval', 'modalService', 'APPSETTING', 'siteProxy', 'ebsettingProxy', 'regionlist', 'legalEntitylist', 'languagelist',
            function ($scope, $interval, modalService, APPSETTING, siteProxy, ebsettingProxy, regionlist, legalEntitylist, languagelist) {

                siteProxy.GetLegalEntity('', function (list) {
                    $scope.legalEntityList = list;
                }, function (res) {
                    alert(res);
                });

                $scope.regionList = regionlist;

                $scope.selectedLevel = 20;  //下拉单页容量初始化
                $scope.itemsperpage = 20;
                $scope.currentPage = 1; //当前页
                $scope.maxSize = 10; //分页显示的最大页
                //分页容量下拉列表定义
                $scope.levelList = [
                    { "id": 20, "levelName": '20' },
                    { "id": 500, "levelName": '500' },
                    { "id": 1000, "levelName": '1000' },
                    { "id": 2000, "levelName": '2000' },
                    { "id": 5000, "levelName": '5000' }
                ];

                $scope.ebList = {
                    multiSelect: false,
                    enableFullRowSelection: false,
                    enableFiltering: true,
                    noUnselect: true,
                    columnDefs: [
                        { field: 'region', displayName: 'Region', width: '125' },
                        { field: 'legaL_ENTITY', displayName: 'Legal', width: '80' },
                        { field: 'eb', displayName: 'EB Name', width: '200' },
                        { field: 'crediT_TREM', displayName: 'Credit Term', width: '120' },
                        { field: 'collector', displayName: 'Collector', width: '100' },
                        { field: 'collectorEmail', displayName: 'Collector Email', width: '150' },
                        { field: 'contacT_LANGUAGENAME', displayName: 'Language', width: '100' },
                        { field: 'creditOfficer', displayName: 'Credit Officer', width: '140' },
                        { field: 'financialController', displayName: 'Financial Controller', width: '140' },
                        { field: 'csManager', displayName: 'CS Manager', width: '140' },
                        { field: 'financialManagers', displayName: 'Financial Manager', width: '140' },
                        { field: 'financeLeader', displayName: 'Finance Leader', width: '140' },
                        { field: 'localFinance', displayName: 'Local Finance', width: '140' },
                        { field: 'branchManager', displayName: 'Branch Manager', width: '140' }

                    ],
                    onRegisterApi: function (gridApi) {
                        $scope.gridApi = gridApi;
                    }
                };

                //单页容量变化
                $scope.pagesizechange = function (selectedLevelId) {
                    var index = $scope.currentPage;
                    ebsettingProxy.getEBSetting($scope.region, $scope.legalEntity, $scope.eb, $scope.collector, index, selectedLevelId, function (list) {
                        $scope.itemsperpage = selectedLevelId;
                        $scope.ebList.data = new Array();

                        $interval(function () {
                            $scope.ebList.data = list.dataRows;
                            $scope.totalItems = list.count;
                        }, 0, 1);

                    });
                };

                //翻页
                $scope.pageChanged = function () {
                    var index = $scope.currentPage;
                    ebsettingProxy.getEBSetting($scope.region, $scope.legalEntity, $scope.eb, $scope.collector, index, $scope.itemsperpage, function (list) {
                        $scope.ebList.data = new Array();
                        $interval(function () {
                            $scope.totalItems = list.count;
                            $scope.ebList.data = list.dataRows;
                        }, 0, 1);

                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.init = function () {
                    $scope.region = "";
                    $scope.legalEntity = "";
                    $scope.eb = "";
                    $scope.collector = "";
                    $scope.pageChanged();
                };

                $scope.init();

                $scope.searchEB = function () {
                    $scope.pageChanged();
                };

                $scope.resetSearch = function () {
                    $scope.region = "";
                    $scope.legalEntity = "";
                    $scope.eb = "";
                    $scope.collector = "";
                };

                $scope.addEBsetting = function () {
                    $scope.editModal({});
                };

                $scope.editEBsetting = function () {
                    if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                        $scope.editModal($scope.gridApi.selection.getSelectedRows()[0]);
                    } else {
                        alert("Please select one row.");
                        return;
                    }
                };

                $scope.DelEBsetting = function () {
                    if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                        var modalDefaults = {
                            templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                            controller: 'contactorReplaceDelConfirmCtrl',
                            windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            if (result == "Yes") {
                                var id = $scope.gridApi.selection.getSelectedRows()[0].id;
                                ebsettingProxy.deleteLegalEB(id, function () {
                                    $scope.pageChanged();
                                }, function (error) {
                                    alert(error);
                                });
                            }
                        });
                    } else {
                        alert("Please select one row.");
                        return;
                    }
                };

                $scope.editModal = function (row) {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/ebsetting/ebsetting-edit.tpl.html?2',
                        controller: 'ebsettingEditCtrl',
                        size: 'customSize',
                        resolve: {
                            cont: function () {
                                return row;
                            },
                            regionEditlist: function () {
                                return regionlist;
                            },
                            legalEntityEditlist: function () {
                                return legalEntitylist;
                            },
                            languageEditlist: function () {
                                return languagelist;
                            },
                            ebsettingEditProxy: function () {
                                return ebsettingProxy;
                            }
                        }, windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            $scope.pageChanged();
                        }
                    });
                };

                $scope.exportEBsetting = function () {
                    ebsettingProxy.downloadEBList($scope.region, $scope.legalEntity, $scope.eb, $scope.collector, function (fileId) {
                        if (fileId) {
                            window.open(APPSETTING['serverUrl'] + '/api/appFiles?fileId=' + fileId, "_blank");
                        }
                    }, function (error) {
                        alert(error);
                    });
                };

                $scope.importEBsetting = function () {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/ebsetting/ebsetting-import.tpl.html',
                        controller: 'ebsettingImportCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        
                    });;
                };

            }])


    .controller('ebsettingImportCtrl', ['$scope', 'FileUploader', 'APPSETTING', '$uibModalInstance',
        function ($scope, FileUploader, APPSETTING, $uibModalInstance) {
            var uploader = $scope.uploader = new FileUploader();

            uploader.filters.push({
                name: 'ebsettingReplaceFilter',
                fn: function (item, options) {
                    return this.queue.length < 10;
                }
            });

            // CALLBACKS
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                document.getElementById("overlay-container").style.display = "none";
                document.getElementById("overlay-container").style.flg = "";
                uploader.queue[3] = "";
                document.getElementById("importFile").value = "";
                alert(response);
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                document.getElementById("overlay-container").style.display = "none";
                document.getElementById("overlay-container").style.flg = "";
                alert(response);
            };

            $scope.importExcel = function () {
                if (uploader.queue[3] == null || uploader.queue[3] == "") {
                    alert("Please select File");
                    return;
                }
                document.getElementById("overlay-container").style.display = "block";
                document.getElementById("overlay-container").style.flg = "loading";
                uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/ebsetting/import';
                uploader.uploadAll();
            }
            $scope.close = function () {
                $uibModalInstance.close();
            };
            $scope.reset = function () {
                uploader.queue[3] = "";
                document.getElementById("importFile").value = "";
            };
        }])

    .controller('ebsettingEditCtrl', ['$scope', '$uibModalInstance', 'cont', 'ebsettingEditProxy', 'regionEditlist', 'legalEntityEditlist','languageEditlist',
        function ($scope, $uibModalInstance, cont, ebsettingEditProxy, regionEditlist, legalEntityEditlist, languageEditlist) {

            $scope.cont = cont;
            $scope.error = '';

            $scope.regionEditList = regionEditlist;
            $scope.legalEntityEditlist = legalEntityEditlist;
            $scope.languageEditlist = languageEditlist;
            $scope.TermList = [
                { "detailValue": '', "detailName": '' },
                { "detailValue": 'ALL', "detailName": 'ALL' },
                { "detailValue": 'PREPAY', "detailName": 'PREPAY' },
                { "detailValue": '有账期', "detailName": '有账期' }
            ];

            $scope.closeModal = function () {
                $uibModalInstance.close("No");
            };

            $scope.save = function () {

                if (!$scope.cont.region || !$scope.cont.legaL_ENTITY || !$scope.cont.eb || !$scope.cont.contacT_LANGUAGE || !$scope.cont.collector || !$scope.cont.collectorEmail) {
                    alert("* Input canot be null");
                    return;
                }

                ebsettingEditProxy.updateLegalEB($scope.cont,
                    function () {
                        $uibModalInstance.close("Yes");
                    },
                    function (res) {
                        alert(res);
                    });
            };

        }]);