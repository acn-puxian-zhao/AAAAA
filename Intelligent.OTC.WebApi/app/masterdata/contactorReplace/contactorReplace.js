angular.module('app.masterdata.contactorreplace', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/admin/contactor/replace', {
                templateUrl: 'app/masterdata/contactorReplace/contactorreplace-list.tpl.html',
                controller: 'contactorReplaceListCtrl',
                resolve: {
                    //首次加载第一页
                    titles: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("051");
                    }]
                }
            });
    }])

    //*****************************************header***************************s
    .controller('contactorReplaceListCtrl',
    ['$scope', '$interval', 'contactReplaceProxy', 'modalService', 'titles',
        function ($scope, $interval, contactReplaceProxy, modalService, titles) {
            $scope.$parent.helloAngular = "OTC - Contactor Replace";

            $scope.titles = [
                { "detailValue": 'CS' },
                { "detailValue": 'Sales' }
            ];;

            $scope.contactorReplaceList = {
                multiSelect: false,
                enableFullRowSelection: false,
                enableFiltering: true,
                noUnselect: true,
                columnDefs: [
                    { name: 'RowNo', displayName: '', width: '40',  cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },
                    { field: 'customerNum', displayName: 'Customer No.', width: '200' },
                    { field: 'siteUseId', displayName: 'Site Use ID', width: '200' },
                    { field: 'title', displayName: 'Title', width: '200' },
                    { field: 'name', displayName: 'Name', width: '200' },
                    { field: 'changeTo', displayName: 'Change To', width: '200' },
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            $scope.initData = function () {
                contactReplaceProxy.findAll(function (list) {
                    if (list !== null) {
                        $scope.contactorReplaceList.data = list;
                    }
                }, function (error) {
                    alert(error);
                })
            };

            $scope.initData();

            $scope.add = function () {
                $scope.editModal({});
            };
           
            $scope.edit = function () {
                if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                    var row = $scope.gridApi.selection.getSelectedRows()[0];
                    $scope.editModal(row);
                } else {
                    alert("please select one record");
                }
            };

            $scope.editModal = function (row) {

                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactorReplace/contactorReplace-edit.tpl.html',
                    controller: 'contactorReplaceEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        titles: function () {
                            return $scope.titles;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function () {
                    $scope.initData();
                });
            };

            $scope.delete = function () {
                if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                        controller: 'contactorReplaceDelConfirmCtrl',
                        windowClass: 'modalDialog'
                    };
                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result == "Yes") {
                            var row = $scope.gridApi.selection.getSelectedRows()[0];
                            contactReplaceProxy.delete(row.id, function () {
                                $scope.initData();
                            }, function (error) {
                                alert(error);
                            })
                        }
                    });
                } else {
                    alert("please select one record");
                }
            }

            $scope.deleteAll = function () {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactorReplace/delConfirm.tpl.html',
                    controller: 'contactorReplaceDelConfirmCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    if (result == "Yes") {
                        var row = $scope.gridApi.selection.getSelectedRows()[0];
                        contactReplaceProxy.deleteAll(function () {
                            $scope.initData();
                        }, function (error) {
                            alert(error);
                        })
                    }
                });
            }

            $scope.import = function () {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactorReplace/contactorReplace-import.tpl.html',
                    controller: 'contactorReplaceImportCtrl',
                    windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    $scope.initData();
                });
            };

            $scope.export = function () {
                contactReplaceProxy.export(function (path) {
                        window.location = path;
                        alert("Export Successful!");
                    },function (res) {
                        alert(res);
                    });
            };

        }])

    .controller('contactorReplaceEditCtrl', ['$scope', '$uibModalInstance', 'cont', 'titles','contactReplaceProxy',
        function ($scope, $uibModalInstance, cont, titles, contactReplaceProxy) {

            $scope.titles = titles;
            $scope.cont = cont;

            $scope.closeModal = function () {
                $uibModalInstance.close();
            };

            $scope.save = function () {

                if ($scope.cont.customerNum &&
                    $scope.cont.siteUseId &&
                    $scope.cont.name &&
                    $scope.cont.title &&
                    $scope.cont.changeTo) {

                    contactReplaceProxy.update($scope.cont,
                        function () {
                            $uibModalInstance.close();
                        },
                        function (res) {
                            alert(res);
                        });
                } else {
                    alert("Input canot be null");
                }
            };

        }])

    .controller('contactorReplaceDelConfirmCtrl', ['$scope', '$uibModalInstance',
        function ($scope, $uibModalInstance) {
            $scope.confirm = function () {
                $uibModalInstance.close("Yes");
            }

            $scope.cancel = function () {
                $uibModalInstance.close("No");
            };
        }])

    .controller('contactorReplaceImportCtrl', ['$scope', 'FileUploader', 'APPSETTING', '$uibModalInstance',

        function ($scope, FileUploader, APPSETTING, $uibModalInstance) {
            var uploader = $scope.uploader = new FileUploader();

            uploader.filters.push({
                name: 'contactorReplaceFilter',
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
                uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/contactreplace/import';
                uploader.uploadAll();
            }
            $scope.close = function () {
                $uibModalInstance.close();
            };
            $scope.reset = function () {
                uploader.queue[3] = "";
                document.getElementById("importFile").value = "";
            };
        }

    ]);
    ;




