angular.module('app.masterdata.mailtemplate', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/admin/mailtemplate', {
                templateUrl: 'app/masterdata/mailtemplate/mailtemplate-list.tpl.html',
                controller: 'mailtemplateListCtrl'
            });
    }])

    .controller('mailtemplateListCtrl', ['$scope', '$sce', 'baseDataProxy', 'mailTemplateProxy', 'modalService', function ($scope, $sce, baseDataProxy, mailTemplateProxy, modalService) {
        $scope.$parent.helloAngular = "OTC - Mail Template";
        baseDataProxy.SysTypeDetail("012", function (mts) {
            $scope.mailtypes = mts;
        });
        baseDataProxy.SysTypeDetail("013", function (langs) {
            $scope.maillanguages = langs;
        });

        $scope.language = "";
        $scope.type = "";

        $scope.$on('search', function (e) {
            if (!$scope.language) $scope.language = "";
            if (!$scope.type) $scope.type = "";

            $scope.search();
        });

        $scope.search = function () {
            mailTemplateProxy.getMailTemplates($scope.language, $scope.type, function (res) {
                $scope.list = res;
            });
        };

        $scope.gridOptions = {
            multiSelect: false,
            enableFullRowSelection: true,
            data: 'list',
            columnDefs: [
                { name: 'subject' },
                { name: 'languageName'},
                { name: 'typeName' },
                { name: 'mainBody' },
                { name: 'creater' },
                { name: 'createDate' }
            ], onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            }
        };

        $scope.$on('edit', function (e) {
            $scope.edit();
        });

        $scope.edit = function () {
            var row = $scope.gridApi.selection.getSelectedRows()[0];
            var modalDefaults = {
                templateUrl: 'app/masterdata/mailtemplate/mailtemplate-edit.tpl.html',
                controller: 'mailtemplateEditCtrl',
                windowClass: 'modalDialog  width_70p',
                resolve: {
                    template: function () { return row; },
                    modalOptions: function () {
                        return {
                            headerText: 'Template Edit',
                            closeButtonText: 'Close',
                            actionButtonText: 'Confirm'
                        }
                    },
                    mts: function () { return $scope.mailtypes; },
                    langs: function () { return $scope.maillanguages; }
                }
            };

            modalService.showModal(modalDefaults, { }).then(function (result) {
                //                    if (result != 'cancel') {
                //                        mailTemplateProxy.all(function (res) {
                //                            $scope.list = res;
                //                        });
                //                    }
                $scope.search();
            });
        };

        $scope.$on('del', function (e) {
            $scope.Del();
        });

        $scope.Del = function () {
            var entity = $scope.gridApi.selection.getSelectedRows()[0];
            mailTemplateProxy.deleteTemplate(entity.id, function (res) {
                $scope.search();
                alert("Delete Success");
            }, function () {
                alert("Delete Error");
            });
        }

        $scope.$on('resetSearch', function (e) {
            $scope.resetSearch();
        });

        $scope.resetSearch = function () {
            filstr = "";
            $scope.language = "";
            $scope.type = "";
        }

        $scope.$on('new', function (e) {
            $scope.New();
        });

        $scope.New = function () {
            var modalDefaults = {
                templateUrl: 'app/masterdata/mailtemplate/mailtemplate-edit.tpl.html',
                controller: 'mailtemplateEditCtrl',
                windowClass: 'modalDialog width_70p',
                resolve: {
                    template: function () { return {}; },
                    modalOptions: function () {
                        return {
                            headerText: 'Template Edit',
                            closeButtonText: 'Close',
                            actionButtonText: 'Confirm'
                        }
                    },
                    mts: function () { return $scope.mailtypes; },
                    langs: function () {return $scope.maillanguages; }
                }
            }

            modalService.showModal(modalDefaults, { }).then(function () {

                $scope.search();
            });
        };

        $scope.search();
    } ])

    .controller('mailtemplateEditCtrl', ['$scope', '$sce', 'mailTemplateProxy', 'template', 'modalOptions', '$uibModalInstance', 'mts', 'langs', function ($scope, $sce, mailTemplateProxy, template, modalOptions, $uibModalInstance, mts, langs) {

        $scope.modalOptions = modalOptions;
        $scope.mailtypes = mts;
        $scope.maillanguages = langs;
        $scope.original = angular.copy(template);
        $scope.template = template;

        $scope.canRevert = function () {
            return true;
        };

        $scope.canSave = function () {
            return true;
        };

        $scope.revertChanges = function () {
            $scope.template = angular.copy($scope.original);
        };

        $scope.save = function () {
            if (!$scope.template.subject) return;
            if (!$scope.template.language) return;
            if (!$scope.template.type) return;
            if (!$scope.template.mainBody) return;

            if ($scope.template.subject == $scope.original.subject && $scope.template.language == $scope.original.language && $scope.template.type == $scope.original.type && $scope.template.mainBody == $scope.original.mainBody) {
                return;
            }

            mailTemplateProxy.saveTemplate($scope.template, function (res) {
                $uibModalInstance.close();
            }, function () {
                alert("Save Error");
            });
        };

        $scope.close = function () {
            $uibModalInstance.close();
        };

    } ]);


