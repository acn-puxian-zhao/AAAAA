angular.module('app.common.mailtemplate', [])

    .controller('mailtemplatePickupCtrl', ['$scope', '$sce', 'baseDataProxy', 'mailTemplateProxy', 'modalService', '$uibModalInstance', 'custnum', 'siteUseId', 'templateTypes', function ($scope, $sce, baseDataProxy, mailTemplateProxy, modalService, $uibModalInstance, custnum, siteUseId, templateTypes) {

        baseDataProxy.SysTypeDetail("012", function (mts) {
            $scope.mailtypes = mts;
        });
        baseDataProxy.SysTypeDetail("013", function (langs) {
            $scope.maillanguages = langs;
        });
        mailTemplateProxy.getCusLang(custnum, siteUseId, function (res) {
            $scope.templanguage = res;
        }, function (r) {
        }
        );
        //mailTemplateProxy.all(function (res) {
        //    $scope.list = res;
        //});

        //$scope.templateTypes = templateTypes;

        //        $scope.searchTemplate = function (filterStr) {
        //            
        //            
        //        };
        //$scope.search = function () {
        //    var filterStr = '';
        //    var _English = '001';
        //    var _Chinese = '002';
        //    var _First = '001';
        //    var _Second = '002';
        //    var _Final = '003';
        //    if ($scope.language) {
        //        var language = "";
        //        switch ($scope.language) {
        //            case _English: language = "English";
        //                break;
        //            case _Chinese: language = "Chinese";
        //                break;
        //        }
        //        if (filterStr != "") {
        //            filterStr += "and (Language eq '" + language + "')";
        //        } else {
        //            filterStr += "&$filter=(Language eq '" + language + "')";
        //        }
        //    }

        //    if ($scope.type) {
        //        var type = "";
        //        switch ($scope.type) {
        //            case _First: type = "First Reminder";
        //                break;
        //            case _Second: type = "Second Reminder";
        //                break;
        //            case _Final: type = "Final Reminder";
        //        }
        //        if (filterStr != "") {
        //            filterStr += "and (Type eq '" + type + "')";
        //        } else {
        //            filterStr += "&$filter=(Type eq '" + type + "')";
        //        }
        //    }

        //    mailTemplateProxy.odataQuery(filterStr, function (res) {
        //        $scope.list = res;
        //    });
        //};

        //$scope.gridPickup = {
        //    data: 'list',
        //    multiSelect: false,
        //    enableFullRowSelection: true,
        //    columnDefs: [
        //                { field: 'subject', displayName: 'Subject' },
        //                {
        //                    field: 'type', displayName: 'Type',
        //                    cellTemplate: '<div hello="{valueMember: \'type\', basedata: \'grid.appScope.templateTypes\'}"></div>'
        //                },
        //                { field: 'mainBody', displayName: 'Body' },
        //                { field: 'creater', displayName: 'Operator' },
        //                { field: 'createDate', displayName: 'Update Date' }
        //            ], onRegisterApi: function (gridApi) {
        //                //set gridApi on scope
        //                $scope.gridApi = gridApi;
        //            }
        //};

        $scope.confirm = function () {
            //  var selectedId = $scope.gridApi.selection.getSelectedRows()[0].id;
            var selectedId = $scope.templanguage;

            if (selectedId) {
                $uibModalInstance.close(selectedId);
            }
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        //$scope.edit = function (row) {
        //    var modalDefaults = {
        //        templateUrl: 'app/masterdata/mailtemplate/mailtemplate-edit.tpl.html',
        //        controller: 'mailtemplateEditCtrl',
        //        resolve: {
        //            template: function () { return row; },
        //            modalOptions: function () {
        //                return {
        //                    headerText: 'Template Edit',
        //                    closeButtonText: 'Close',
        //                    actionButtonText: 'Confirm'
        //                }
        //            }
        //        },
        //        windowClass: 'modalDialog'
        //    };

        //    modalService.showModal(modalDefaults).then(function (result) {
        //        if (result != 'cancel') {
        //            mailTemplateProxy.all(function (res) {
        //                $scope.list = res;
        //            });
        //        }
        //    });
        //};
    }]);


